//************************************************************************************************
// Copyright © 2020 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SettingsDialog : LocalizableForm
	{
		private readonly Dictionary<int, SheetBase> sheets;
		private readonly SettingsProvider provider;


		public SettingsDialog()
		{
			InitializeComponent();

			provider = new SettingsProvider();

			sheets = new Dictionary<int, SheetBase>();

			navTree.SelectedNode = navTree.Nodes[0];
			navTree.Focus();
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 5));
			UIHelper.SetForegroundWindow(this);
		}


		private void Navigate(object sender, TreeViewEventArgs e)
		{
			SheetBase sheet;

			if (sheets.ContainsKey(e.Node.Index))
			{
				sheet = sheets[e.Node.Index];
			}
			else
			{
				switch (e.Node.Index)
				{
					case 0: sheet = new ContextMenuSheet(provider); break;
					case 1: sheet = new HighlightsSheet(provider); break;
					case 2: sheet = new RibbonBarSheet(provider); break;
					default: sheet = new SearchEngineSheet(provider); break;
				}

				sheets.Add(e.Node.Index, sheet);
			}

			headerLabel.Text = sheet.Title;

			contentPanel.Controls.Clear();
			contentPanel.Controls.Add(sheet);
		}


		private void OK(object sender, EventArgs e)
		{
			foreach (var sheet in sheets.Values)
			{
				sheet.CollectSettings();
			}

			provider.Save();

			UIHelper.ShowMessage(Resx.SettingsDialog_Restart);

			Logger.Current.WriteLine("User settings saved");
		}
	}
}
