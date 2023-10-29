//************************************************************************************************
// Copyright © 2020 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class SettingsDialog : UI.LocalizableForm
	{
		public enum Sheets
		{
			General,
			Aliases,
			Colorizer,
			Context,
			Favorites,
			FileImport,
			Hashtags,
			Highlight,
			Images,
			Keyboard,
			Navigator,
			Lines,
			Plugins,
			QuickNotes,
			Ribbon,
			Search,
			Snippets
		}

		private readonly Dictionary<int, SheetBase> sheets;
		private readonly SettingsProvider provider;
		private readonly IRibbonUI ribbon;
		private bool restart;


		public SettingsDialog(IRibbonUI ribbon)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SettingsDialog_Text;

				Localize(new string[]
				{
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});

				navTree.Nodes["generalNode"].Text = Resx.SettingsDialog_generalNode_Text;
				navTree.Nodes["colorizerNode"].Text = Resx.ColorizeSheet_Title;
				navTree.Nodes["aliasNode"].Text = Resx.SettingsDialog_aliasNode_Text;
				navTree.Nodes["contextNode"].Text = Resx.SettingsDialog_contextNode_Text;
				navTree.Nodes["favoritesNode"].Text = Resx.word_Favorites;
				navTree.Nodes["fileImportNode"].Text = Resx.FileImportSheet_Title;
				navTree.Nodes["hashtagsNode"].Text = Resx.SettingsDialog_hashtagsNode_Text;
				navTree.Nodes["highlightNode"].Text = Resx.SettingsDialog_highlightNode_Text;
				navTree.Nodes["linesNode"].Text = Resx.SettingsDialog_linesNode_Text;
				navTree.Nodes["imagesNode"].Text = Resx.SettingsDialog_imagesNode_Text;
				navTree.Nodes["keyboardNode"].Text = Resx.SettingsDialog_keyboardNode_Text;
				navTree.Nodes["navigatorNode"].Text = Resx.SettingsDialog_navigatorNode_Text;
				navTree.Nodes["pluginsNode"].Text = Resx.SettingsDialog_pluginsNode_Text;
				navTree.Nodes["quickNotesNode"].Text = Resx.SettingsDialog_quickNotesNode_Text;
				navTree.Nodes["ribbonNode"].Text = Resx.SettingsDialog_ribbonNode_Text;
				navTree.Nodes["searchNode"].Text = Resx.SettingsDialog_searchNode_Text;
				navTree.Nodes["snippetsNode"].Text = Resx.SettingsDialog_snippetshNode_Text;
			}

			this.ribbon = ribbon;
			provider = new SettingsProvider();
			sheets = new Dictionary<int, SheetBase>();

			navTree.SelectedNode = navTree.Nodes[0];
			navTree.Focus();

			restart = false;
		}


		private void InitializeLoad(object sender, EventArgs e)
		{
			// width and height will be correct at this point; otherwise,
			// we would need to calculate them based on screen scaling
			MinimumSize = new System.Drawing.Size(Width, Height);
			FormBorderStyle = FormBorderStyle.Sizable;
		}


		public bool RestartNeeded => restart;


		public void ActivateSheet(Sheets sheet)
		{
			var index = (int)sheet;
			if (index > 0 && index < navTree.Nodes.Count)
			{
				navTree.SelectedNode = navTree.Nodes[index];
			}
		}


		private async void Navigate(object sender, TreeViewEventArgs e)
		{
			SheetBase sheet;

			if (sheets.ContainsKey(e.Node.Index))
			{
				sheet = sheets[e.Node.Index];
			}
			else
			{
				sheet = e.Node.Index switch
				{
					0 => new GeneralSheet(provider),
					1 => new ColorizerSheet(provider),
					2 => new AliasSheet(provider),
					3 => new ContextMenuSheet(provider),
					4 => new FavoritesSheet(provider, ribbon),
					5 => new FileImportSheet(provider),
					6 => new HashtagSheet(provider),
					7 => new HighlightsSheet(provider),
					8 => new LinesSheet(provider),
					9 => new ImagesSheet(provider),
					10 => new KeyboardSheet(provider, ribbon),
					11 => new NavigatorSheet(provider),
					12 => await PluginsSheet.Create(provider, ribbon),
					13 => new QuickNotesSheet(provider),
					14 => new RibbonBarSheet(provider),
					15 => new SearchEngineSheet(provider),
					_ => new SnippetsSheet(provider, ribbon),
				};
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
				if (sheet.CollectSettings())
				{
					restart = true;
				}
			}

			provider.Save();

			Logger.Current.WriteLine($"user settings saved, restart:{restart}");
		}
	}
}
