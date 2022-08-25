//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class CrawlWebPageDialog : UI.LocalizableForm
	{

		public CrawlWebPageDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.CrawlWebPageDialog_Text;

				Localize(new string[]
				{
					"introBox",
					"selectLabel",
					"unselectLabel",
					"useTextBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public CrawlWebPageDialog(List<CrawlHyperlink> links)
			: this()
		{
			gridView.DataSource = new SortableBindingList<CrawlHyperlink>(links);
		}


		public bool UseTextTitles => useTextBox.Checked;


		public List<CrawlHyperlink> GetSelectedHyperlinks()
		{
			return ((SortableBindingList<CrawlHyperlink>)gridView.DataSource)
				.Where(e => e.Selected)
				.ToList();
		}


		private void DirtyStateChanged(object sender, EventArgs e)
		{
			if (gridView.CurrentCell is DataGridViewCheckBoxCell)
			{
				gridView.CommitEdit(DataGridViewDataErrorContexts.Commit);

				var data = (SortableBindingList<CrawlHyperlink>)gridView.DataSource;
				okButton.Enabled = data.Any(d => d.Selected);
			}
		}

		private void GridResize(object sender, EventArgs e)
		{
			var width = 0;
			foreach (DataGridViewColumn column in gridView.Columns)
			{
				width += column.Width;
			}

			var adjusted = gridView.Width - SystemInformation.VerticalScrollBarWidth;
			if (width != adjusted)
			{
				var diff = (adjusted - width) / 2;
				gridView.Columns[1].Width += diff;
				gridView.Columns[2].Width += diff;
			}
		}

		private void SelectAllItems(object sender, LinkLabelLinkClickedEventArgs ea)
		{
			var data = (SortableBindingList<CrawlHyperlink>)gridView.DataSource;
			data.Where(e => !e.Selected).ForEach(e => e.Selected = true);

			gridView.Invalidate();
			okButton.Enabled = data.Any(d => d.Selected);
		}


		private void SelectNoneItems(object sender, LinkLabelLinkClickedEventArgs ea)
		{
			var data = (SortableBindingList<CrawlHyperlink>)gridView.DataSource;
			data.Where(e => e.Selected).ForEach(e => e.Selected = false);

			gridView.Invalidate();
			okButton.Enabled = data.Any(d => d.Selected);
		}
	}
}
