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
					"introBox",
					"themeGroup"
				});
			}

			var settings = provider.GetCollection(Name);
			var theme = settings.Get<string>("theme");
			if (theme == "Faded")
			{
				fadedRadio.Checked = true;
			}
			else if (theme == "Deep")
			{
				deepRadio.Checked = true;
			}
		}


		public override void CollectSettings()
		{
			string theme = "Normal";
			if (fadedRadio.Checked) theme = "Faded";
			else if (deepRadio.Checked) theme = "Deep";

			var settings = provider.GetCollection(Name);
			settings.Add("theme", theme);

			provider.SetCollection(settings);
		}
	}
}
