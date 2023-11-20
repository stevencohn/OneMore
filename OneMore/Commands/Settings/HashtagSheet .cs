//************************************************************************************************
// Copyright © 2023 teven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using Resx = Properties.Resources;


	internal partial class HashtagSheet : SheetBase
	{

		public HashtagSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(HashtagSheet);
			Title = Resx.HashtagSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"intervalLabel",
					"minLabel=word_Minutes",
					"advancedGroup=phrase_AdvancedOptions",
					"disabledBox"
				});
			}

			var settings = provider.GetCollection(Name);

			intervalBox.Value = settings.Get("interval", HashtagService.DefaultPollingInterval);

			disabledBox.Checked = settings.Get("disabled", false);
		}


		public override bool CollectSettings()
		{
			// general...

			var settings = provider.GetCollection(Name);

			var updated = settings.Add("interval", (int)intervalBox.Value);

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
