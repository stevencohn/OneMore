//************************************************************************************************
// Copyright © 2020 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class HighlightsSheet : SheetBase
	{
		public HighlightsSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "HighlightsSheet";
			Title = Resx.HighlightsSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox"
				});
			}

			var settings = provider.GetCollection(Name);
			if (settings.Get<string>("theme") == "Faded")
			{
				fadedRadio.Checked = true;
			}
		}


		public override void CollectSettings()
		{
			string theme = normalRadio.Checked ? "Normal" : "Faded";

			var settings = provider.GetCollection(Name);
			settings.Add("theme", theme);

			provider.SetCollection(settings);
		}
	}
}
