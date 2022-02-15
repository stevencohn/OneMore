//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class CrawlWebPageCommand : Command
	{
		private sealed class Hyperlink
		{
			public string Address;
			public string Text;
		}

		private OneNote one;


		public CrawlWebPageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic))
			{
				var links = GetHyperlinks(page);
				if (!links.Any())
				{
					return;
				}
			}

			await Task.Yield();
		}


		private IEnumerable<Hyperlink> GetHyperlinks(Page page)
		{
			var links = new List<Hyperlink>();

			var hrefs = page.Root.DescendantNodes().OfType<XCData>()
				.Where(c => Regex.IsMatch(c.Value, $@"<a\s+href=""http[s]?://"));

			foreach (var href in hrefs)
			{
				var wrapper = href.GetWrapper();
				var anchors = wrapper.Elements("a")
					.Where(e => e.Attribute("href") != null &&
						e.Attribute("href").Value.StartsWith("http"));

				foreach (var anchor in anchors)
				{
					logger.WriteLine($"a {anchor.Attribute("href").Value} ({anchor.Value})");

					links.Add(new Hyperlink
					{
						Address = anchor.Attribute("href").Value,
						Text = anchor.Value
					});
				}
			}

			return links;
		}
	}
}

