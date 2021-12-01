//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Web.WebView2.Core;
	using Microsoft.Web.WebView2.WinForms;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Windows.Storage;
	using Windows.Storage.Streams;
	using Hap = HtmlAgilityPack;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ImportWebCommand : Command
	{
		private string address = null;
		private bool importImages = false;
		private ImportWebTarget target;


		public ImportWebCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				UIHelper.ShowInfo(Resx.NetwordConnectionUnavailable);
				return;
			}

			using (var dialog = new ImportWebDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				address = dialog.Address;
				target = dialog.Target;
				importImages = dialog.ImportImages;
			}

			if (importImages)
			{
				var progress = new UI.ProgressDialog(ImportImages);
				await progress.RunModeless();
				return;
			}

			await ImportHtml(address, target);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		// https://github.com/LanderVe/WPF_PDFDocument/blob/master/WPF_PDFDocument/WPF_PDFDocument.csproj
		// https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs
		// https://docs.microsoft.com/en-us/uwp/api/windows.data.pdf.pdfdocument.getpage?view=winrt-20348

		private async Task ImportImages(UI.ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			progress.SetMaximum(3);
			progress.SetMessage($"Importing {address}...");

			var pdfFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			await SingleThreaded.Invoke(async () =>
			{
				logger.WriteLine("automating edge");
				var source = new TaskCompletionSource<bool>();
				var webview = new WebView2();
				webview.NavigationCompleted += async (sender, args) =>
				{
					await Task.Delay(2000);
					logger.WriteLine("rendering pdf");
					await webview.CoreWebView2.PrintToPdfAsync(pdfFile).ConfigureAwait(true);
					webview.Dispose();
					logger.WriteLine("pdf done");
					source.SetResult(true);
				};

				var userDataFolder = Path.Combine(PathFactory.GetAppDataPath(), Resx.ProgramName);
				var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

				try
				{
					logger.WriteLine("ensuring core ready");
					await webview.EnsureCoreWebView2Async(env)
						.ContinueWith(t =>
						{
							if (t.IsFaulted)
							{
								logger.WriteLine(t.Exception.Message, t.Exception);
							}
						});

					logger.WriteLine($"navigating to {address}");
					webview.Source = new Uri(address);

					logger.WriteLine("awaiting completion");
					await source.Task;
				}
				catch (Exception exc)
				{
					logger.WriteLine(exc.Message, exc);
				}
			});

			if (token.IsCancellationRequested)
			{
				return;
			}

			if (!File.Exists(pdfFile))
			{
				logger.WriteLine($"PDF file not found, {pdfFile}");
				return;
			}

			// convert PDF pages to images...

			try
			{
				logger.WriteLine("rendering images");
				using (var one = new OneNote())
				{
					var page = target == ImportWebTarget.Append
						? one.GetPage()
						: await CreatePage(one,
							target == ImportWebTarget.ChildPage ? one.GetPage() : null, address);

					var ns = page.Namespace;
					var container = page.EnsureContentContainer();

					var file = await StorageFile.GetFileFromPathAsync(pdfFile);
					var doc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);

					progress.SetMaximum((int)doc.PageCount);

					for (int i = 0; i < doc.PageCount; i++)
					{
						progress.SetMessage($"Rasterizing image {i} of {doc.PageCount}");
						progress.Increment();

						//logger.WriteLine($"rasterizing page {i}");
						var pdfpage = doc.GetPage((uint)i);

						using (var stream = new InMemoryRandomAccessStream())
						{
							await pdfpage.RenderToStreamAsync(stream);

							using (var image = new Bitmap(stream.AsStream()))
							{
								var data = Convert.ToBase64String(
									(byte[])new ImageConverter().ConvertTo(image, typeof(byte[]))
									);

								container.Add(new XElement(ns + "OE",
									new XElement(ns + "Image",
										new XAttribute("format", "png"),
										new XElement(ns + "Size",
											new XAttribute("width", $"{image.Width}.0"),
											new XAttribute("height", $"{image.Height}.0")),
										new XElement(ns + "Data", data)
									)),
									new Paragraph(ns, " ")
								);
							}
						}
					}

					progress.SetMessage($"Updating page");
					await one.Update(page);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc.Message, exc);
			}

			logger.WriteTime("import complete");
			logger.End();
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private async Task ImportHtml(string address, ImportWebTarget target)
		{
			var baseUri = new Uri(address);

			logger.WriteLine($"importing web page {baseUri.AbsoluteUri}");
			logger.StartClock();

			string content;

			try
			{
				content = await DownloadWebContent(baseUri);
			}
			catch (Exception exc)
			{
				Giveup(exc.Messages());
				return;
			}

			if (string.IsNullOrEmpty(content))
			{
				Giveup(Resx.ImportWebCommand_BadUrl);
				logger.WriteLine("web page returned empty content");
				return;
			}

			var doc = ReplaceImagesWithAnchors(content, baseUri);
			if (doc == null)
			{
				Giveup(Resx.ImportWebCommand_BadUrl);
				return;
			}

			// Attempted to inline the css using the PreMailer nuget
			// but OneNote strips it all off anyway so, oh well
			//content = PreMailer.MoveCssInline(baseUri, doc.DocumentNode.OuterHtml,
			//	stripIdAndClassAttributes: true, removeComments: true).Html;

			using (var one = new OneNote())
			{
				Page page;

				if (target == ImportWebTarget.Append)
				{
					page = one.GetPage();
				}
				else
				{
					page = await CreatePage(one,
						target == ImportWebTarget.ChildPage ? one.GetPage() : null,
						doc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? address
						);
				}

				// add html to page and let OneNote rehydrate as it sees fit
				page.AddHtmlContent(doc.DocumentNode.OuterHtml);

				await one.Update(page);
				logger.WriteLine("pass 1 updated page with injected HTML");

				await HydrateImages(page, one);
			}

			logger.WriteTime("import web completed");
		}


		private void Giveup(string msg)
		{
			UIHelper.ShowInfo($"Cannot load web page.\n\n{msg}");
		}


		private async Task<string> DownloadWebContent(Uri uri)
		{
			try
			{
				using (var source = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
				{
					var client = HttpClientFactory.Create();
					using (var response = await client.GetAsync(uri, source.Token))
					{
						if (response.IsSuccessStatusCode)
						{
							return await response.Content.ReadAsStringAsync();
						}
					}
				}
			}
			catch (TaskCanceledException exc)
			{
				logger.WriteLine("timeout fetching web page", exc);
			}
			catch (Exception exc)
			{
				// for some reason, anything more than this will crash OneMore
				// seems that the exception is not fully instantiated at this point
				// so rethrow and let caller display aggregated message
				logger.WriteLine(exc.Message);
				throw;
			}

			return null;
		}


		private Hap.HtmlDocument ReplaceImagesWithAnchors(string content, Uri baseUri)
		{
			// use HtmlAgilityPack to normalize and clean up the HTML...

			var doc = new Hap.HtmlDocument();
			doc.LoadHtml(content);

			// convert img tags to anchor tags
			// OneNote will remove img tags but will keep anchors

			var body = doc.DocumentNode.SelectSingleNode("//body");
			if (body == null)
			{
				logger.WriteLine("no <body> found in content");
				return null;
			}

			var images = body.Descendants("img")
				.Where(e => !string.IsNullOrEmpty(e.GetAttributeValue("src", string.Empty)))
				.ToList();

			if (images.Count == 0)
			{
				return doc;
			}

			var oneUri = (new UriBuilder(baseUri) { Host = $"onemore.{baseUri.Host}" }).Uri;

			foreach (var image in images)
			{
				var src = image.GetAttributeValue("src", string.Empty);
				if (!string.IsNullOrEmpty(src))
				{
					src = new Uri(oneUri, src).AbsoluteUri;
					var anchor = Hap.HtmlNode.CreateNode($"<a href=\"{src}\">{src}</a>");
					image.ParentNode.ReplaceChild(anchor, image);
				}
			}

			return doc;
		}


		private async Task HydrateImages(Page page, OneNote one)
		{
			try
			{
				// fetch page again with hydrated html
				page = one.GetPage(page.PageId, OneNote.PageDetail.All);

				// transform anchors to downloaded images...

				var regex = new Regex(
					@"<a\s+href=""[^:]+://(onemore\.)[^:]+://(onemore\.)",
					RegexOptions.Compiled);

				// download and embed images
				var cmd = new GetImagesCommand(regex);
				if (cmd.GetImages(page))
				{
					// second update to page
					await one.Update(page);
					logger.WriteLine("pass 2 updated page with hydrated images");
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error hydrating images", exc);
			}
		}


		private async Task<Page> CreatePage(OneNote one, Page parent, string title)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);
			var page = one.GetPage(pageId);

			if (parent != null)
			{
				// get current section again after new page is created
				section = one.GetSection();

				var parentElement = section.Elements(parent.Namespace + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == parent.PageId);

				var childElement = section.Elements(parent.Namespace + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == pageId);

				if (childElement != parentElement.NextNode)
				{
					// move new page immediately after its original in the section
					childElement.Remove();
					parentElement.AddAfterSelf(childElement);
				}

				parentElement.GetAttributeValue("pageLevel", out var level, 1);
				var pageLevel = (level + 1).ToString();

				// must set level on the hierarchy entry and on the page itself
				childElement.SetAttributeValue("pageLevel", pageLevel);
				page.Root.SetAttributeValue("pageLevel", pageLevel);

				one.UpdateHierarchy(section);
			}

			await one.NavigateTo(pageId);

			page.Title = title;
			return page;
		}
	}
}
