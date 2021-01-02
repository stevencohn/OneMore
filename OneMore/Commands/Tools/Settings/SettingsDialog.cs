//************************************************************************************************
// Copyright © 2020 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SettingsDialog : UI.LocalizableForm
	{
		public enum Sheets
		{
			Context,
			Favorites,
			Highlight,
			Ribbon,
			Search,
			Snippets
		}

		private readonly Dictionary<int, SheetBase> sheets;
		private readonly SettingsProvider provider;
		private readonly IRibbonUI ribbon;


		public SettingsDialog(IRibbonUI ribbon)
		{
			InitializeComponent();

			VerticalOffset = 4;

			if (NeedsLocalizing())
			{
				Text = Resx.SettingsDialog_Text;

				Localize(new string[]
				{
					"okButton",
					"cancelButton"
				});

				navTree.Nodes["contextNode"].Text = Resx.SettingsDialog_contextNode_Text;
				navTree.Nodes["favoritesNode"].Text = Resx.SettingsDialog_favoritesNode_Text;
				navTree.Nodes["highlightNode"].Text = Resx.SettingsDialog_highlightNode_Text;
				navTree.Nodes["ribbonNode"].Text = Resx.SettingsDialog_ribbonNode_Text;
				navTree.Nodes["searchNode"].Text = Resx.SettingsDialog_searchNode_Text;
				navTree.Nodes["snippetsNode"].Text = "Snippets"; // translate Resx.SettingsDialog_searchNode_Text;
			}

			this.ribbon = ribbon;
			provider = new SettingsProvider();
			sheets = new Dictionary<int, SheetBase>();

			navTree.SelectedNode = navTree.Nodes[0];
			navTree.Focus();
		}


		public void ActivateSheet(Sheets sheet)
		{
			var index = (int)sheet;
			if (index > 0 && index < navTree.Nodes.Count)
			{
				navTree.SelectedNode = navTree.Nodes[index];
			}
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
					case 1: sheet = new FavoritesSheet(provider, ribbon); break;
					case 2: sheet = new HighlightsSheet(provider); break;
					case 3: sheet = new RibbonBarSheet(provider); break;
					case 4: sheet = new SearchEngineSheet(provider); break;
					default: sheet = new SnippetsSheet(provider, ribbon); break;
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

			Logger.Current.WriteLine("user settings saved");
		}
	}
}
