//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Lists the links that RepairUrlsCommand repaired, highlighted or left alone, and goes to
	/// the page that holds a chosen link.
	/// </summary>
	internal partial class RepairUrlsResultsDialog : MoreForm
	{
		private const string HeaderShading = "#DEEBF6";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";

		private readonly IReadOnlyList<RepairUrlsResult> results;
		private readonly string sectionID;
		private bool copying;


		/// <param name="results">The links to list</param>
		/// <param name="sectionID">
		/// The section where the command was run, to hold the page made by the Copy button
		/// </param>
		public RepairUrlsResultsDialog(IReadOnlyList<RepairUrlsResult> results, string sectionID)
		{
			InitializeComponent();
			RememberSize = true;

			this.results = results;
			this.sectionID = sectionID;

			if (NeedsLocalizing())
			{
				Text = Resx.RepairUrlsResultsDialog_Text;

				Localize(new string[]
				{
					"filterLabel=word_Filter",
					"copyButton",
					"goButton=word_Go",
					"closeButton=word_Close"
				});

				statusColumn.Text = Resx.word_Status;
				linkColumn.Text = Resx.RepairUrlsResultsDialog_linkColumn;
				pageColumn.Text = Resx.word_Page;
				detailColumn.Text = Resx.RepairUrlsResultsDialog_detailColumn;
			}

			listView.SetColumnProportions(0.14f, 0.26f, 0.22f, 0.38f);

			summaryLabel.Text = string.Format(Resx.RepairUrlsResultsDialog_summaryMsg,
				Count(RepairUrlsOutcome.Repaired),
				Count(RepairUrlsOutcome.Highlighted),
				Count(RepairUrlsOutcome.Unchanged));

			filterBox.Items.AddRange(new object[]
			{
				Resx.word_All,
				OutcomeText(RepairUrlsOutcome.Repaired),
				OutcomeText(RepairUrlsOutcome.Highlighted),
				OutcomeText(RepairUrlsOutcome.Unchanged)
			});

			// also populates the list
			filterBox.SelectedIndex = 0;
		}


		private int Count(RepairUrlsOutcome outcome)
		{
			return results.Count(r => r.Outcome == outcome);
		}


		internal static string OutcomeText(RepairUrlsOutcome outcome)
		{
			return outcome switch
			{
				RepairUrlsOutcome.Repaired => Resx.RepairUrlsResultsDialog_repaired,
				RepairUrlsOutcome.Highlighted => Resx.RepairUrlsResultsDialog_highlighted,
				_ => Resx.RepairUrlsResultsDialog_unchanged
			};
		}


		private void FilterResults(object sender, EventArgs e)
		{
			// index 0 is All; the others follow the order of RepairUrlsOutcome
			var index = filterBox.SelectedIndex;

			listView.BeginUpdate();
			listView.Items.Clear();

			foreach (var result in results)
			{
				if (index <= 0 || (int)result.Outcome == index - 1)
				{
					var item = new ListViewItem(OutcomeText(result.Outcome)) { Tag = result };
					item.SubItems.Add(result.LinkText);
					item.SubItems.Add(result.PageTitle);
					item.SubItems.Add(result.Detail);
					listView.Items.Add(item);
				}
			}

			listView.EndUpdate();

			if (listView.Items.Count > 0)
			{
				listView.Items[0].Selected = true;
			}

			goButton.Enabled = listView.Items.Count > 0;
			copyButton.Enabled = !copying && listView.Items.Count > 0;
		}


		// a modeless form is not closed by the DialogResult of a button, only a modal one is
		private void CloseDialog(object sender, EventArgs e)
		{
			Close();
		}


		/// <summary>
		/// Copies the links now listed, in the order listed, to a new page in the section where
		/// the command was run, so the user can work through them later.
		/// </summary>
		private async void CopyToPage(object sender, EventArgs e)
		{
			if (copying || listView.Items.Count == 0 || string.IsNullOrEmpty(sectionID))
			{
				return;
			}

			copying = true;
			copyButton.Enabled = false;

			try
			{
				var rows = listView.Items.Cast<ListViewItem>()
					.Select(i => i.Tag)
					.OfType<RepairUrlsResult>()
					.ToList();

				await using var one = new OneNote();
				one.CreatePage(sectionID, out var pageId);

				var page = await one.GetPage(pageId);
				page.Title = Resx.RepairUrlsResultsDialog_pageTitle;

				var ns = page.Namespace;
				PageNamespace.Set(ns);

				var table = await BuildTable(one, ns, rows);

				var container = page.EnsureContentContainer();
				container.Add(
					new Paragraph($"{DateTime.Now.ToShortFriendlyString()} {summaryLabel.Text}"),
					new Paragraph(string.Empty),
					new Paragraph(table.Root),
					new Paragraph(string.Empty)
					);

				if (await one.Update(page))
				{
					await one.NavigateTo(pageId);
				}
				else
				{
					Logger.Current.WriteLine("could not save the Repair URLs results page");
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error copying Repair URLs results to a page", exc);
			}
			finally
			{
				copying = false;
				copyButton.Enabled = listView.Items.Count > 0;
			}
		}


		private async Task<Table> BuildTable(
			OneNote one, XNamespace ns, IReadOnlyList<RepairUrlsResult> rows)
		{
			var table = new Table(ns, 1, 4)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 90);
			table.SetColumnWidth(1, 220);
			table.SetColumnWidth(2, 200);
			table.SetColumnWidth(3, 360);

			var header = table[0];
			header.SetShading(HeaderShading);
			header[0].SetContent(new Paragraph(Resx.word_Status).SetStyle(HeaderCss));
			header[1].SetContent(new Paragraph(Resx.RepairUrlsResultsDialog_linkColumn).SetStyle(HeaderCss));
			header[2].SetContent(new Paragraph(Resx.word_Page).SetStyle(HeaderCss));
			header[3].SetContent(new Paragraph(Resx.RepairUrlsResultsDialog_detailColumn).SetStyle(HeaderCss));

			// many links share a page, and each link to a page is a call into OneNote
			var pageLinks = new Dictionary<string, string>();

			foreach (var result in rows)
			{
				var row = table.AddRow();
				row[0].SetContent(WebUtility.HtmlEncode(OutcomeText(result.Outcome)));
				row[1].SetContent(WebUtility.HtmlEncode(result.LinkText));

				var title = WebUtility.HtmlEncode(result.PageTitle);
				if (!string.IsNullOrEmpty(result.PageID))
				{
					if (!pageLinks.TryGetValue(result.PageID, out var link))
					{
						link = await one.GetHyperlinkWithRetry(result.PageID, string.Empty);
						pageLinks.Add(result.PageID, link);
					}

					if (!string.IsNullOrEmpty(link))
					{
						title = $"<a href=\"{link}\">{title}</a>";
					}
				}

				row[2].SetContent(title);
				row[3].SetContent(WebUtility.HtmlEncode(result.Detail));
			}

			return table;
		}


		private async void GoToPage(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0 ||
				listView.SelectedItems[0].Tag is not RepairUrlsResult result ||
				string.IsNullOrEmpty(result.PageID))
			{
				return;
			}

			try
			{
				await using var one = new OneNote();
				await one.NavigateTo(result.PageID);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error going to page [{result.PageTitle}]", exc);
			}
		}
	}
}
