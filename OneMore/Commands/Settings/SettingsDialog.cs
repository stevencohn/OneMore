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


	internal partial class SettingsDialog : UI.MoreForm
	{
		public enum Sheets
		{
			General,
			Aliases,
			Colorizer,
			Colors,
			Context,
			Favorites,
			FileImport,
			Hashtags,
			Highlight,
			Images,
			Keyboard,
			Navigator,
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

				navTree.Nodes["generalNode"].Text = Resx.GeneralSheet_Title;
				navTree.Nodes["colorizerNode"].Text = Resx.ColorizerSheet_Title;
				navTree.Nodes["colorsNode"].Text = Resx.ColorsSheet_Title;
				navTree.Nodes["aliasNode"].Text = Resx.AliasSheet_Title;
				navTree.Nodes["contextNode"].Text = Resx.ContextMenuSheet_Title;
				navTree.Nodes["favoritesNode"].Text = Resx.word_Favorites;
				navTree.Nodes["fileImportNode"].Text = Resx.FileImportSheet_Title;
				navTree.Nodes["hashtagsNode"].Text = Resx.word_Hashtags;
				navTree.Nodes["highlightNode"].Text = Resx.HighlightsSheet_Title;
				navTree.Nodes["imagesNode"].Text = Resx.word_Images;
				navTree.Nodes["keyboardNode"].Text = Resx.SettingsDialog_keyboardNode_Text;
				navTree.Nodes["navigatorNode"].Text = Resx.word_Navigator;
				navTree.Nodes["pluginsNode"].Text = Resx.word_Plugins;
				navTree.Nodes["quickNotesNode"].Text = Resx.QuickNotesSheet_Title;
				navTree.Nodes["ribbonNode"].Text = Resx.RibbonBarSheet_Title;
				navTree.Nodes["searchNode"].Text = Resx.SearchEngineDialog_Text;
				navTree.Nodes["snippetsNode"].Text = Resx.word_Snippets;
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
					2 => new ColorsSheet(provider),
					3 => new AliasSheet(provider),
					4 => new ContextMenuSheet(provider),
					5 => new FavoritesSheet(provider, ribbon),
					6 => new FileImportSheet(provider),
					7 => new HashtagSheet(provider),
					8 => new HighlightsSheet(provider),
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

			contentPanel.SuspendLayout();
			contentPanel.Controls.Clear();
			contentPanel.Controls.Add(sheet);
			contentPanel.ResumeLayout();
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

			logger.WriteLine($"user settings saved, restart:{restart}");
		}
	}
}
