//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Discovers all external hyperlinks on the current page, presents a dialog from which the
	/// user can select one or more link to import. Each hyperlink is imported as a separate page.
	/// </summary>
	internal class CrawlWebPageCommand : Command
	{
		private Page parentPage;
		private ImportWebCommand importer;
		private List<CrawlHyperlink> selections;
		private bool useTextTitles;
		private bool rewireParentLinks;


		public CrawlWebPageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(
				out parentPage, out var ns, OneNote.PageDetail.Selection);

			var candidates = GetHyperlinks(parentPage);
			if (!candidates.Any())
			{
				ShowError(Resx.CrawlWebCommand_NoHyperlinks);
				return;
			}

			using (var dialog = new CrawlWebPageDialog(candidates))
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				selections = dialog.GetSelectedHyperlinks();
				useTextTitles = dialog.UseTextTitles;
				rewireParentLinks = dialog.RewireParentLinks;
			}

			// reverse so we create subpages in correct order
			selections.Reverse();

			importer = new ImportWebCommand();
			importer.SetLogger(logger);

			var progress = new UI.ProgressDialog(DownloadSelectedSubpages);
			progress.SetMaximum(selections.Count);
			progress.RunModeless();
		}


		private List<CrawlHyperlink> GetHyperlinks(Page page)
		{
			var links = new List<CrawlHyperlink>();

			var range = new Models.SelectionRange(page);
			var runs = range.GetSelections(defaulToAnytIfNoRange: true);

			IEnumerable<XCData> cdatas;
			// special case when cursor is caret or on a hyperlink, no selection range
			if (range.Scope == SelectionScope.TextCursor ||
				range.Scope == SelectionScope.SpecialCursor)
			{
				cdatas = page.Root.DescendantNodes().OfType<XCData>()
					.Where(c => Regex.IsMatch(c.Value, $@"<a\s+href=""http[s]?://"));
			}
			else
			{
				cdatas = runs.DescendantNodes().OfType<XCData>()
					.Where(c => Regex.IsMatch(c.Value, $@"<a\s+href=""http[s]?://"));
			}

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
					var entry = links.Find(e =>
						e.Address == anchor.Address && e.Text == anchor.Text);

					if (entry == null)
					{
						//logger.WriteLine($"found {anchor.Address} ({anchor.Text})");

						links.Add(new CrawlHyperlink
						{
							CData = cdata,
							Address = anchor.Address,
							Text = anchor.Text,
							Order = links.Count + 1
						});
					}
					else
					{
						entry.RefCount++;
					}
				}
			}

			return links;
		}


		private async Task DownloadSelectedSubpages(
			UI.ProgressDialog progress, CancellationToken token)
		{
			var updated = false;

			await using var one = new OneNote();

			foreach (var selection in selections)
			{
				progress.SetMessage(selection.Address);
				//logger.WriteLine($"fetching {selection.Address}");

				var page = await importer.ImportSubpage(
					one, parentPage, new Uri(selection.Address),
					useTextTitles ? selection.Text : null,
					token);

				if (rewireParentLinks && page != null)
				{
					var pageUri = one.GetHyperlink(page.PageId, string.Empty);

					// redirect primary reference on parent page to our new subpage
					PatchCData(selection.CData, selection.Address, selection.Text, pageUri);

					// redirect duplicate references on parent page
					if (selection.RefCount > 1)
					{
						var ns = parentPage.Namespace;
						var regex = new Regex($@"<a\s+href=""{Regex.Escape(selection.Address)}""");

						var cdatas = parentPage.Root.Elements(ns + "Outline")
							.DescendantNodes().OfType<XCData>()
							.Where(d => d != selection.CData && regex.IsMatch(d.Value));

						foreach (var cdata in cdatas)
						{
							PatchCData(cdata, selection.Address, selection.Text, pageUri);
						}
					}

					updated = true;
				}

				progress.Increment();
			}

			if (updated)
			{
				await one.Update(parentPage);
				await one.NavigateTo(parentPage.PageId);
			}
		}


		private void PatchCData(XCData cdata, string address, string text, string pageUri)
		{
			var wrapper = cdata.GetWrapper();
			wrapper.Elements("a")
				.FirstOrDefault(e =>
					e.Attribute("href").Value == address &&
					e.Value == text)?
				.SetAttributeValue("href", pageUri);

			cdata.Value = wrapper.GetInnerXml();
		}
	}
}
