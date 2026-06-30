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
			Snippets,
			TableThemes,
			Variables
		}

		private readonly Dictionary<int, SheetBase> sheets;
		private readonly SettingsProvider provider;
		private readonly IRibbonUI ribbon;
		private readonly UI.MoreLinkLabel[] navLinks;
		private int activeIndex;
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

				generalLink.Text = Resx.GeneralSheet_Title;
				colorizerLink.Text = Resx.ColorizerSheet_Title;
				colorsLink.Text = Resx.ColorsSheet_Title;
				aliasLink.Text = Resx.AliasSheet_Title;
				contextLink.Text = Resx.ContextMenuSheet_Title;
				favoritesLink.Text = Resx.word_Favorites;
				fileImportLink.Text = Resx.FileImportSheet_Title;
				hashtagsLink.Text = Resx.word_Hashtags;
				highlightLink.Text = Resx.HighlightsSheet_Title;
				imagesLink.Text = Resx.word_Images;
				keyboardLink.Text = Resx.SettingsDialog_keyboardNode_Text;
				navigatorLink.Text = Resx.word_Navigator;
				pluginsLink.Text = Resx.word_Plugins;
				quickNotesLink.Text = Resx.QuickNotesSheet_Title;
				ribbonLink.Text = Resx.RibbonBarSheet_Title;
				searchLink.Text = Resx.SearchEngineSheet_Text;
				snippetsLink.Text = Resx.word_Snippets;
				tableThemesLink.Text = Resx.TableThemesSheet_Title;
				variablesLink.Text = Resx.VariablesSheet_Title;
			}

			this.ribbon = ribbon;
			provider = new SettingsProvider();
			sheets = new Dictionary<int, SheetBase>();

			navLinks = new[]
			{
				generalLink, colorizerLink, colorsLink, aliasLink, contextLink,
				favoritesLink, fileImportLink, hashtagsLink, highlightLink, imagesLink,
				keyboardLink, navigatorLink, pluginsLink, quickNotesLink, ribbonLink,
				searchLink, snippetsLink, tableThemesLink, variablesLink
			};

			activeIndex = 0;
			navLinks[0].Active = true;
			Navigate(0);
			navLinks[0].Focus();

			restart = false;
		}


		private void InitializeLoad(object sender, EventArgs e)
		{
			// Sheets use AutoScaleMode.Font (SizeF 9,20); the dialog uses AutoScaleMode.Dpi.
			// At sub-design DPIs the dialog shrinks faster than the sheets, making contentPanel
			// too small for HashtagSheet and TableThemesSheet (content minimum 816w x 560h).
			// Grow the form by the gap so no content is clipped on open.
			var fontScale = Font.Height / 20.0f;
			var dw = Math.Max(0, (int)(816 * fontScale) - contentPanel.Width);
			var dh = Math.Max(0, (int)(560 * fontScale) - contentPanel.Height);
			if (dw > 0 || dh > 0)
			{
				Width += dw;
				Height += dh + 40; // 40 is a fudge factor to accomodate RDP
			}

			MinimumSize = new System.Drawing.Size(Width, Height);
			FormBorderStyle = FormBorderStyle.Sizable;
			LayoutNavLinks();
		}


		// the designer bakes each link's Height/Top as raw pixels authored at this
		// dialog's design-time DPI (150%, see AutoScaleDimensions); WinForms rescales
		// those pixel values for whatever DPI the dialog actually opens at, and the
		// rounding in that conversion can leave a row a couple pixels shorter than its
		// own (separately rescaled) font needs, clipping the bottom of the text - most
		// visible on lower-DPI screens since that's a scale-down. Deriving the row
		// height from the link's actual runtime Font instead keeps every row sized to
		// fit its own text regardless of DPI.
		private void LayoutNavLinks()
		{
			var rowHeight = navLinks[0].Font.Height + 2;
			var gap = rowHeight / 2;
			var y = navLinks[0].Top;

			foreach (var link in navLinks)
			{
				link.Height = rowHeight;
				link.Top = y;
				y += rowHeight + gap;
			}
		}


		public bool RestartNeeded => restart;


		public void ActivateSheet(Sheets sheet)
		{
			var index = (int)sheet;
			if (index > 0 && index < navLinks.Length)
			{
				ActivateLink(index);
			}
		}


		private void DoLinkClicked(object sender, EventArgs e)
		{
			if (sender is UI.MoreLinkLabel link && link.Tag is int index)
			{
				ActivateLink(index);
			}
		}


		private void DoLinkKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down && activeIndex < navLinks.Length - 1)
			{
				ActivateLink(activeIndex + 1);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up && activeIndex > 0)
			{
				ActivateLink(activeIndex - 1);
				e.Handled = true;
			}
		}


		private void ActivateLink(int index)
		{
			if (index == activeIndex)
			{
				return;
			}

			navLinks[activeIndex].Active = false;
			navLinks[index].Active = true;
			activeIndex = index;
			navLinks[index].Focus();

			Navigate(index);
		}


		private async void Navigate(int index)
		{
			SheetBase sheet;

			if (sheets.ContainsKey(index))
			{
				sheet = sheets[index];
			}
			else
			{
				sheet = index switch
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
					16 => new SnippetsSheet(provider, ribbon),
					17 => new TableThemesSheet(provider, ribbon),
					_ => new VariablesSheet(provider)
				};

				sheets.Add(index, sheet);
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
