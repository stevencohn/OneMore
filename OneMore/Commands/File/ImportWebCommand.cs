//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using HtmlAgilityPack;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;

	internal class ImportWebCommand : Command
	{
		public ImportWebCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			logger.StartClock();

			var baseUri = new Uri("https://github.com/stevencohn/OneMore/wiki");
			var client = HttpClientFactory.Create();

			// download html

			string content = null;

			try
			{
				using (var source = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
				{
					using (var response = await client.GetAsync(baseUri, source.Token))
					{
						if (response.IsSuccessStatusCode)
						{
							content = await response.Content.ReadAsStringAsync();
						}
					}
				}
			}
			catch (TaskCanceledException exc)
			{
				logger.WriteLine("timeout fetching web page", exc);
				return;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error fetching web page", exc);
				return;
			}

			if (string.IsNullOrEmpty(content))
			{
				logger.WriteLine("web page return empty content");
				return;
			}

			var doc = new HtmlDocument();
			doc.LoadHtml(content);

			// convert img tags to anchor tags
			// OneNote will remove img tags but will keep anchors

			var body = doc.DocumentNode.SelectSingleNode("//body");
			var images = body.Descendants("img")
				.Where(e => !string.IsNullOrEmpty(e.GetAttributeValue("src", string.Empty)))
				.ToList();

			baseUri = (new UriBuilder(baseUri) { Host = $"onemore.{baseUri.Host}" }).Uri;

			foreach (var image in images)
			{
				var src = image.GetAttributeValue("src", string.Empty);
				if (!string.IsNullOrEmpty(src))
				{
					src = new Uri(baseUri, src).AbsoluteUri;
					var anchor = HtmlNode.CreateNode($"<a href=\"{src}\">{src}</a>");
					image.ParentNode.ReplaceChild(anchor, image);
				}
			}

			using (var one = new OneNote(out var page, out var ns))
			{
				// add html to page and let OneNote rehydrate as it sees fit
				page.AddHtmlContent(doc.DocumentNode.OuterHtml);

				await one.Update(page);
				logger.WriteLine("pass 1 updated page with injected HTML");

				try
				{
					// fetch page again with hydrated html
					page = one.GetPage(OneNote.PageDetail.All);

					// transform anchors to downloaded images...

					var regex = new Regex(
						@"<a\s+href=""[^:]+://(onemore\.)[^:]+://(onemore\.)",
						RegexOptions.Compiled);

					// download and embed images
					var cmd = new GetImagesCommand(regex);
					if (cmd.GetImages(page, ns))
					{
						// second update to page
						await one.Update(page);
						logger.WriteLine("pass 2 updated page with hydrated images");
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine(exc);
				}
			}

			logger.WriteTime("import web completed");
		}
	}
}