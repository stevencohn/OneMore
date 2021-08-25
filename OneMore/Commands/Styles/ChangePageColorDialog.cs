//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S4275 // Getters and setters should access the expected fields

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ChangePageColorDialog : UI.LocalizableForm
	{

		public ChangePageColorDialog(Color pageColor)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ChangePageColorDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"customLink",
					"themeIntroLabel",
					"currentLabel",
					"loadLink",
					"applyBox",
					"cancelButton",
					"okButton"
				});
			}

			InitializeCustomColor(pageColor);
			colorsBox.SelectColor(pageColor);

			var theme = new ThemeProvider().Theme;
			ThemeKey = theme.Key;
			themeLabel.Text = theme.Name;
		}


		public bool ApplyStyle => applyBox.Checked;

		public string ThemeKey { get; private set; }


		public string PageColor
		{
			get
			{
				if (colorsBox.Color.Equals(Color.White))
				{
					return "automatic";
				}

				return colorsBox.Color.ToRGBHtml();
			}
		}


		private void InitializeCustomColor(Color pageColor)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("pageTheme");
			if (settings != null && settings.Contains("customColor"))
			{
				try
				{
					colorsBox.SetCustomColor(ColorTranslator.FromHtml(settings["customColor"]));
					return;
				}
				catch (Exception exc)
				{
					logger.WriteLine("error reading saved custom color", exc);
				}
			}

			colorsBox.SetCustomColor(pageColor);
		}


		private void ChooseCustomColor(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var location = PointToScreen(customLink.Location);

			using (var dialog = new UI.MoreColorDialog(Resx.PageColorDialog_Text,
				location.X + customLink.Bounds.Location.X + customLink.Width,
				location.Y - 50))
			{
				dialog.Color = colorsBox.CustomColor;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					colorsBox.SetCustomColor(dialog.Color);

					var provider = new SettingsProvider();
					var settings = provider.GetCollection("pageTheme");
					settings.Add("customColor", dialog.Color.ToRGBHtml());
					provider.SetCollection(settings);
					provider.Save();
				}
			}
		}


		private void AnalyzeColorSelection(object sender, System.EventArgs e)
		{
			//var provider = new StyleProvider();
			//logger.WriteLine($"analyzing theme {provider.Key} (dark:{provider.Dark})");
		}


		private void LoadStyleTheme(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var theme = new LoadStylesCommand().LoadTheme();
			if (theme != null)
			{
				themeLabel.Text = theme.Name;
				ThemeKey = theme.Key;
			}
		}
	}
}
