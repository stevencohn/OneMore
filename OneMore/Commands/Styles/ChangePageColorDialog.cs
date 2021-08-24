//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S4275 // Getters and setters should access the expected fields

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
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

			colorsBox.SetColor(pageColor);

			themeLabel.Text = new ThemeProvider().Theme.Key;
		}


		public bool ApplyStyle => applyBox.Checked;


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
					colorsBox.SetColor(dialog.Color);
				}
			}
		}


		private void AnalyzeColorSelection(object sender, System.EventArgs e)
		{
			//var provider = new StyleProvider();
			//logger.WriteLine($"analyzing theme {provider.Key} (dark:{provider.Dark})");
		}

		private async void LoadStyleTheme(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var loader = new LoadStylesCommand();
			await loader.Execute();

			themeLabel.Text = loader.Theme.Name;
		}
	}
}
