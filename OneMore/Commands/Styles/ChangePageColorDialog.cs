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
		private Theme theme;


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

			statusLabel.Text = string.Empty;

			InitializeCustomColor(pageColor);
			colorsBox.SelectColor(pageColor);

			theme = new ThemeProvider().Theme;
			themeLabel.Text = theme.Name;

			AnalyzeColorSelection(null, null);
		}


		public bool ApplyStyle => applyBox.Checked;

		public string ThemeKey => theme.Key;


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


		private void AnalyzeColorSelection(object sender, EventArgs e)
		{
			if (theme == null || colorsBox.SelectedItem == null)
			{
				statusLabel.Text = string.Empty;
				return;
			}

			var dark = colorsBox.Color.GetBrightness() < 0.5;

			statusLabel.Text = theme.Dark == dark
				? string.Empty
				: Resx.ChangePageColorDialog_Contrast;
		}


		private void LoadStyleTheme(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var thm = new LoadStylesCommand().LoadTheme();
			if (thm != null)
			{
				theme = thm;
				themeLabel.Text = thm.Name;

				AnalyzeColorSelection(sender, e);
			}
		}
	}
}
