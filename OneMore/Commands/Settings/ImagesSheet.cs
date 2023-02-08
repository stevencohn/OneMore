//************************************************************************************************
// Copyright © 2023 teven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = Properties.Resources;


	internal partial class ImagesSheet : SheetBase
	{

		public ImagesSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "ImagesSheet";
			Title = Resx.LinesSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"plantGroup",
					"plantAfterBox",
					"plantCollapseBox"
				});
			}

			var settings = provider.GetCollection(Name);

			plantAfterBox.Checked = settings.Get("plantAfter", false);
			plantCollapseBox.Checked = settings.Get("plantCollapsed", false);
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			settings.Add("plantAfter", plantAfterBox.Checked.ToString());
			settings.Add("plantCollapsed", plantCollapseBox.Checked.ToString());
			provider.SetCollection(settings);

			return false;
		}
	}
}
