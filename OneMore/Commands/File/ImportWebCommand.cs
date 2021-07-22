//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Hap = HtmlAgilityPack;


	internal class ImportWebCommand : Command
	{
		public ImportWebCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			string address = null;
			ContentCreation creation;
			using (var dialog = new ImportWebDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				address = dialog.Address;
				creation = dialog.Creation;
			}

			var baseUri = new Uri(address);

			logger.WriteLine($"importing web page {baseUri.AbsoluteUri}");
			logger.StartClock();

			// download html
			string content = await DownloadWebContent(baseUri);

			if (string.IsNullOrEmpty(content))
			{
				logger.WriteLine("web page returned empty content");
				return;
			}

			var doc = ReplaceImagesWithAnchors(content, baseUri);
			if (doc == null)
			{
				return;
			}

			using (var one = new OneNote(out _, out _))
			{
				Page page;
				switch (creation)
				{
					case ContentCreation.Append:
						page = one.GetPage();
						break;

					case ContentCreation.ChildPage:
						page = await CreatePage(one, one.GetPage(),
							doc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? address);
						break;

					default:
						page = await CreatePage(one, null,
							doc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? address);
						break;
				}

				// add html to page and let OneNote rehydrate as it sees fit
				page.AddHtmlContent(doc.DocumentNode.OuterHtml);

				await one.Update(page);
				logger.WriteLine("pass 1 updated page with injected HTML");

				await HydrateImages(page, one);
			}

			logger.WriteTime("import web completed");
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
				logger.WriteLine("error fetching web page", exc);
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