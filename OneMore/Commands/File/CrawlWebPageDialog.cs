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
	}
}
