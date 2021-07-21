//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using HtmlAgilityPack;
	using System;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class ImportWebCommand : Command
	{
		public ImportWebCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

			var baseUri = new Uri("https://github.com/stevencohn/OneMore/wiki");

			using (var client = new WebClient())
			{
				// download html

				var content = client.DownloadString(baseUri);
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
					// first update to add html to page

					page.AddHtmlContent(doc.DocumentNode.OuterHtml);
					await one.Update(page);

					// fetch page again so we can transform anchors to downloaded images

					page = one.GetPage(OneNote.PageDetail.All);

					var regex = new Regex(@"<a\s+href=""[^:]+://(onemore\.)", RegexOptions.Compiled);

					var runs = page.Root.DescendantNodes().OfType<XCData>()
						.Where(c => regex.IsMatch(c.Value))
						.Select(e => e.Parent)
						.ToList();

					// remove the "onemore." part of the host name from anchor URIs

					foreach (var run in runs)
					{
						var matches = regex.Matches(run.Value);
						if (matches.Count > 0)
						{
							// handle multiple anchors in cdata, backtracking to avoid index overlaps
							var text = run.Value;
							for (int i = matches.Count - 1; i >= 0; i--)
							{
								text = text.Remove(
									matches[i].Groups[1].Index, 
									matches[i].Groups[1].Length);
							}

							run.Value = text;
						}
					}

					// download and embed images
					var cmd = new GetImagesCommand();
					if (cmd.GetImages(page))
					{
						// second update to page
						await one.Update(page);
					}
				}
			}
		}
	}
}