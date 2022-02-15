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
	using System.Windows.Forms;
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
				var links = GetHyperlinks(page);
				if (!links.Any())
				{
					return;
				}

				List<CrawlHyperlink> hyperlinks = null;
				using (var dialog = new CrawlWebPageDialog(links))
				{
					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					hyperlinks = dialog.GetSelectedHyperlinks();
				}

				if (await GetSelectedSubpages(page, hyperlinks))
				{
					await one.Update(page);
				}
			}
		}


		private List<CrawlHyperlink> GetHyperlinks(Page page)
		{
			var links = new List<CrawlHyperlink>();

			var cdatas = page.Root.DescendantNodes().OfType<XCData>()
				.Where(c => Regex.IsMatch(c.Value, $@"<a\s+href=""http[s]?://"));

			foreach (var cdata in cdatas)
			{
				var wrapper = cdata.GetWrapper();
				var anchors = wrapper.Elements("a")
					.Where(e =>
						e.Attribute("href") != null &&
						e.Attribute("href").Value.StartsWith("http"));

				foreach (var anchor in anchors)
				{
					logger.WriteLine($"a {anchor.Attribute("href").Value} ({anchor.Value})");

					var address = anchor.Attribute("href").Value;
					var text = anchor.Value;

					if (!links.Any(e => e.Address == address && e.Text == text))
					{
						links.Add(new CrawlHyperlink
						{
							CData = cdata,
							Address = address,
							Text = text,
							Order = links.Count + 1
						});
					}
				}
			}

			return links;
		}


		private async Task<bool> GetSelectedSubpages(Page page, List<CrawlHyperlink> links)
		{
			await Task.Yield();
			return false;
		}
	}
}

