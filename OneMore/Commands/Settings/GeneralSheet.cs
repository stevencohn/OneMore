//************************************************************************************************
// Copyright © 2021 teven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class GeneralSheet : SheetBase
	{
		public GeneralSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "GeneralSheet";
			Title = Resx.GeneralSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"enablersBox",
					"checkUpdatesBox"
				});
			}

			var settings = provider.GetCollection(Name);

			enablersBox.Checked = settings.Get("enablers", true);
			checkUpdatesBox.Checked = settings.Get("checkUpdates", false);
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);

			var updated = false;
			if (settings.Add("enablers", enablersBox.Checked)) updated = true;
			if (settings.Add("checkUpdates", checkUpdatesBox.Checked)) updated = true;

			if (updated)
			{
				provider.SetCollection(settings);
				AddIn.EnablersEnabled = enablersBox.Checked;
				return true;
			}

			return false;
		}
	}
}
