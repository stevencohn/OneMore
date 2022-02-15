//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class CrawlWebPageCommand : Command
	{
		private OneNote one;


		public CrawlWebPageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic))
			{
				var links = page.Root.DescendantNodes().OfType<XCData>()
					.Where(c => Regex.IsMatch(c.Value, $@"<a\s+href=""http[s]?://"));

				foreach (var link in links)
				{
					var wrapper = link.GetWrapper();
					var anchors = wrapper.Elements("a")
						.Where(e => e.Attribute("href") != null &&
							e.Attribute("href").Value.StartsWith("http"));

					foreach (var anchor in anchors)
					{
						logger.WriteLine($"a {anchor.Attribute("href").Value} ({anchor.Value})");
					}
				}
			}

			await Task.Yield();
		}
	}
}

