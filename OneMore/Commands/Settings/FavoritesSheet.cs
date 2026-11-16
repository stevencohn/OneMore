//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands.Favorites;
	using Resx = Properties.Resources;


	internal partial class FavoritesSheet : SheetBase
	{

		private readonly bool shortcuts;
		private readonly IRibbonUI ribbon;


		public FavoritesSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = "FavoritesSheet";
			Title = Resx.word_Favorites;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"optionsBox=phrase_AdvancedOptions",
					"shortcutsBox",
					"dropLink",
					"dropLabel"
				});
			}

			shortcuts = provider.GetCollection(Name).Get<bool>("kbdshorts");
			shortcutsBox.Checked = shortcuts;

			// advanced options are only shown if experimental features are enabled
			optionsBox.Visible = provider.GetCollection(nameof(GeneralSheet))
				.Get("experimental", false);

			// "Advanced Options [pill]": the pill follows the caption text. A child can't draw
			// above the group box's top edge, so the pill is sized to the caption's line height
			// rather than its usual padding
			experimentalPill.AutoSize = false;
			experimentalPill.Text = Resx.phrase_Experimental;
			experimentalPill.Size = new System.Drawing.Size(
				experimentalPill.GetPreferredSize(System.Drawing.Size.Empty).Width,
				optionsBox.Font.Height + 2);

			int captionWidth;
			using (var g = optionsBox.CreateGraphics())
			{
				captionWidth = (int)g.MeasureString(optionsBox.Text, optionsBox.Font).Width;
			}

			const int gap = 8;
			experimentalPill.Location = new System.Drawing.Point(
				optionsBox.CaptionOffset + captionWidth + gap, 0);

			// clear the border line behind the pill, like it is behind the caption text
			optionsBox.TrailingInset = gap + experimentalPill.Width + 2;

			this.ribbon = ribbon;
		}


		private void DropSchema(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var result = UI.MoreMessageBox.ShowQuestion(this, Resx.FavoritesSheet_confirmDrop);
			if (result == System.Windows.Forms.DialogResult.Yes)
			{
				using var provider = new FavoritesProvider();
				if (provider.DropCatalog())
				{
					ribbon.SafeInvalidateControl(FavoritesMenu.MenuID);
					UI.MoreMessageBox.Show(this, Resx.FavoritesSheet_dropped);
				}
			}
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			settings.Add("kbdshorts", shortcutsBox.Checked);

			provider.SetCollection(settings);

			return false;
		}
	}
}
