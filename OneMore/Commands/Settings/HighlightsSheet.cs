//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = Properties.Resources;


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
					"themesGroup",
					"normalRadio",
					"fadedRadio",
					"deepRadio"
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


		public override bool CollectSettings()
		{
			string theme = "Normal";
			if (fadedRadio.Checked) theme = "Faded";
			else if (deepRadio.Checked) theme = "Deep";

			var settings = provider.GetCollection(Name);
			settings.Add("theme", theme);

			provider.SetCollection(settings);

			// restart not required
			return false;
		}


		private void ClickPicture(object sender, System.EventArgs e)
		{
			if (sender == fadedPicture)
			{
				fadedRadio.Checked = true;
			}
			else if (sender == deepPicture)
			{
				deepRadio.Checked = true;
			}
			else
			{
				normalRadio.Checked = true;
			}
		}
	}
}
