//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class CrawlWebPageCommand : Command
	{

		private OneNote one;
		private ImportWebCommand importer;


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

				importer = new ImportWebCommand();
				importer.SetLogger(logger);

				if (await DownloadSelectedSubpages(page, hyperlinks))
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
					.Where(e => e.Attribute("href") != null)
					.Select(e => new
					{
						Address = e.Attribute("href").Value,
						Text = e.Value
					})
					.Where(a => a.Address.StartsWith("http") &&
						Uri.IsWellFormedUriString(a.Address, UriKind.Absolute));

				foreach (var anchor in anchors)
				{
					// entries are unique
					if (!links.Any(e => e.Address == anchor.Address && e.Text == anchor.Text))
					{
						logger.WriteLine($"found {anchor.Address} ({anchor.Text})");

						links.Add(new CrawlHyperlink
						{
							CData = cdata,
							Address = anchor.Address,
							Text = anchor.Text,
							Order = links.Count + 1
						});
					}
				}
			}

			return links;
		}


		private async Task<bool> DownloadSelectedSubpages(Page parent, List<CrawlHyperlink> links)
		{
			var updated = false;
			foreach (var link in links)
			{
				logger.WriteLine($"fetching {link.Address}");

				var page = await importer.ImportSubpage(one, parent, new Uri(link.Address));

				if (page != null)
				{
					var wrapper = link.CData.GetWrapper();

					wrapper.Elements("a")
						.FirstOrDefault(e =>
							e.Attribute("href").Value == link.Address &&
							e.Value == link.Text)?
						.SetAttributeValue("href", one.GetHyperlink(page.PageId, string.Empty));

					link.CData.Value = wrapper.GetInnerXml();
					updated = true;
				}
			}

			return updated;
		}
	}
}

