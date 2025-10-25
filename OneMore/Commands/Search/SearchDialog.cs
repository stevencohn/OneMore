//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class SearchDialog : MoreForm
	{
		private readonly bool experimental;

		public SearchDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Search;

				Localize(new string[]
				{
					"textTab=word_Search",
					"actionTab"
				});
			}

			DefaultControl = textTab;
			ElevatedWithOneNote = true;

			var provider = new SettingsProvider();
			experimental = provider.GetCollection("GeneralSheet").Get<bool>("experimental");
			if (!experimental)
			{
				tabControl.TabPages.Remove(textTab);
			}
		}


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }


		protected override void OnShown(EventArgs e)
		{
			// base method must be called to complete the EvelatedWithOneNote procedure
			base.OnShown(e);

			// force focus on textSheet's tabIndex(0) control
			if (experimental)
			{
				actionSheet.Focus();
				return;
			}

			textSheet.Focus();
		}


		private void ClosingSearch(object sender, SearchCloseEventArgs e)
		{
			if (e.DialogResult == DialogResult.OK && sender == actionSheet)
			{
				CopySelections = actionSheet.CopySelections;
				SelectedPages = actionSheet.SelectedPages;
			}

			DialogResult = e.DialogResult;
			Close();
		}


		private void TabSelected(object sender, EventArgs e)
		{
			if (tabControl.SelectedIndex == 0)
			{
				textSheet.Focus();
			}
			else
			{
				actionSheet.Focus();
			}
		}
	}
}
