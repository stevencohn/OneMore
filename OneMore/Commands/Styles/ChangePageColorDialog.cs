//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ChangePageColorDialog : UI.LocalizableForm
	{

		public ChangePageColorDialog(Color pageColor, bool dark)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ChangePageColorDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"customLink",
					"cancelButton",
					"okButton"
				});
			}

			colorsBox.SetColor(pageColor);
		}


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
			logger.WriteLine("analyze");
		}
	}
}
