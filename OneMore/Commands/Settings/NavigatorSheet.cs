//************************************************************************************************
// Copyright © 2023 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class NavigatorSheet : SheetBase
	{
		private const decimal Millisecond = 1000M;


		public NavigatorSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(NavigatorSheet);
			Title = Resx.NavigatorSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"depthLabel",
					"intervalLabel",
					"secLabel=word_Seconds",
					"corrallBox",
					"advancedGroup=phrase_AdvancedOptions",
					"disabledBox"
				});
			}

			var settings = provider.GetCollection(Name);

			depthBox.Value = settings.Get("depth", NavigationService.DefaultHistoryDepth);

			var interval = settings.Get("interval", NavigationService.DefaultPollingInterval);
			intervalBox.Value = interval / Millisecond * 2;

			if (Screen.AllScreens.Length == 1)
			{
				corrallBox.Text = $"{corrallBox.Text} ({Resx.NavigatorSheet_corrallBox_disabled})";
				corrallBox.Checked = true;
				corrallBox.Enabled = false;
			}
			else
			{
				corrallBox.Checked = settings.Get("corralled", false);
			}

			disabledBox.Checked = settings.Get("disabled", false);
		}


		public override bool CollectSettings()
		{
			// general...

			var settings = provider.GetCollection(Name);

			var updated = settings.Add("depth", (int)depthBox.Value);

			updated = settings.Add("interval", (int)(intervalBox.Value * Millisecond / 2)) || updated;

			updated = corrallBox.Checked
				? settings.Add("corralled", true) || updated
				: settings.Remove("corralled") || updated;

			updated = disabledBox.Checked
				? settings.Add("disabled", true) || updated
				: settings.Remove("disabled") || updated;

			if (updated)
			{
				provider.SetCollection(settings);
			}

			return updated;
		}
	}
}
