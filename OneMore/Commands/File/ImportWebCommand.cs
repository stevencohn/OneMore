//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
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
	using Win = System.Windows;


	internal class ImportWebCommand : Command
	{
		private sealed class WebPageInfo
		{
			public string Content;
			public string Title;
		}

		private string address = null;
		private bool importImages = false;
		private bool experimental = false;
		private ImportWebTarget target;
		private ProgressDialog progress;


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
				experimental = dialog.Experimental;
			}

			if (importImages)
			{
				progress = new ProgressDialog(ImportImages);
				await progress.RunModeless();
				return;
			}

			using (var source = new CancellationTokenSource())
			{
				using (progress = new ProgressDialog(source))
				{
					progress.SetMaximum(5);
					progress.SetMessage($"Importing {address}...");

					progress.StartTimer();
					var result = progress.ShowDialog(owner);
					if (result == DialogResult.Cancel)
					{
						return;
					}

					await ImportHtml(address, target);
				}
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		// https://github.com/LanderVe/WPF_PDFDocument/blob/master/WPF_PDFDocument/WPF_PDFDocument.csproj
		// https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs
		// https://docs.microsoft.com/en-us/uwp/api/windows.data.pdf.pdfdocument.getpage?view=winrt-20348

		private async Task ImportImages(ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			progress.SetMaximum(4);
			progress.SetMessage($"Importing {address}...");

			var pdfFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			// WebView2 needs to run in an STA thread
			await SingleThreaded.Invoke(() =>
			{
				// WebView2 needs a message pump so host in its own invisible worker dialog
				using (var form = new WebViewWorkerDialog(
					new WebViewWorker(async (webview) =>
					{
						webview.Source = new Uri(address);
						progress.Increment();
						await Task.Yield();
						return true;
					}),
					new WebViewWorker(async (webview) =>
					{
						progress.Increment();
						await Task.Delay(2000);
						await webview.CoreWebView2.PrintToPdfAsync(pdfFile);
						progress.Increment();
						return true;
					})))
				{
					form.ShowDialog(progress);
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
			logger.WriteLine("rendering images");

			try
			{
				Page page = null;
				using (var one = new OneNote())
				{
					page = target == ImportWebTarget.Append
						? one.GetPage()
						: await CreatePage(one,
							target == ImportWebTarget.ChildPage ? one.GetPage() : null, address);
				}

				var ns = page.Namespace;
				var container = page.EnsureContentContainer();

				var file = await StorageFile.GetFileFromPathAsync(pdfFile);
				var doc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);

				await file.DeleteAsync();

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

				using (var one = new OneNote())
				{
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

			WebPageInfo info;

			try
			{
				info = experimental
					? await DownloadWebContent_Experimental(baseUri)
					: await DownloadWebContent(baseUri);
			}
			catch (Exception exc)
			{
				Giveup(exc.Messages());
				return;
			}

			if (string.IsNullOrEmpty(info.Content))
			{
				Giveup(Resx.ImportWebCommand_BadUrl);
				logger.WriteLine("web page returned empty content");
				return;
			}

			var doc = ReplaceImagesWithAnchors(info.Content, baseUri);
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
					if (string.IsNullOrEmpty(info.Title))
					{
						info.Title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;
					}

					page = await CreatePage(one,
						target == ImportWebTarget.ChildPage ? one.GetPage() : null,
						info.Title ?? address
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


		private async Task<WebPageInfo> DownloadWebContent_Experimental(Uri uri)
		{
			// copy the HTML of the entire web page to the clipboard
			// and return the text of the page <title>
			const string javascript =
@"var range = document.createRange();
range.selectNodeContents(document.body);
var selection = window.getSelection();
selection.removeAllRanges();
selection.addRange(range);
document.execCommand('copy');
document.getElementsByTagName('title')[0].innerText;";

			string content = null;
			string title = null;

			// WebView2 needs to run in an STA thread
			await SingleThreaded.Invoke(() =>
			{
				// WebView2 needs a message pump so host in its own invisible worker dialog
				using (var form = new WebViewWorkerDialog(
					startup:
					new WebViewWorker(async (webview) =>
					{
						logger.WriteLine($"starting up webview with {uri}");
						webview.Source = uri;
						await Task.Yield();
						return true;
					}),
					work:
					new WebViewWorker(async (webview) =>
					{
						logger.WriteLine("getting webview content");
						await Task.Delay(200);

						title = await webview
							.ExecuteScriptAsync(javascript);

						await Task.Delay(100);

						logger.WriteLine($"title=[{content}]");

						//// unescape all escape chars in string and remove outer quotes
						//content = Regex.Unescape(content);
						//content = content.Substring(1, content.Length - 2);

						if (Win.Clipboard.ContainsText(Win.TextDataFormat.Html))
						{
							content = Win.Clipboard.GetText(Win.TextDataFormat.Html);
							var index = content.IndexOf(
								"<html", StringComparison.InvariantCultureIgnoreCase);

							if (index > 0)
							{
								content = content.Substring(index);
							}
						}
						else
						{
							content = null;
						}

						//logger.WriteLine($"content=[{content}]");

						await Task.Yield();
						return true;
					})))
				{
					form.ShowDialog();
				}
			});

			if (title != null && title.Length > 1 &&
				title[0] == '"' && title[title.Length - 1] == '"')
			{
				title = title.Substring(1, title.Length - 2);
			}

			return new WebPageInfo
			{
				Content = content,
				Title = title
			};
		}


		private async Task<WebPageInfo> DownloadWebContent(Uri uri)
		{
			var info = new WebPageInfo();

			try
			{
				using (var source = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
				{
					var client = HttpClientFactory.Create();
					using (var response = await client.GetAsync(uri, source.Token))
					{
						if (response.IsSuccessStatusCode)
						{
							info.Content = await response.Content.ReadAsStringAsync();
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

			return info;
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
