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
					ribbon.InvalidateControl(FavoritesMenu.MenuID);
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
