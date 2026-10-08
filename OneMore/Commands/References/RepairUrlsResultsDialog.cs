//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Lists the links that RepairUrlsCommand repaired, highlighted or left alone, and goes to
	/// the page that holds a chosen link.
	/// </summary>
	internal partial class RepairUrlsResultsDialog : MoreForm
	{
		private readonly IReadOnlyList<RepairUrlsResult> results;


		public RepairUrlsResultsDialog(IReadOnlyList<RepairUrlsResult> results)
		{
			InitializeComponent();
			RememberSize = true;

			this.results = results;

			if (NeedsLocalizing())
			{
				Text = Resx.RepairUrlsResultsDialog_Text;

				Localize(new string[]
				{
					"filterLabel=word_Filter",
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
		}


		// a modeless form is not closed by the DialogResult of a button, only a modal one is
		private void CloseDialog(object sender, EventArgs e)
		{
			Close();
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
