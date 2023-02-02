//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class PageColorDialog : UI.LocalizableForm
	{
		public PageColorDialog()
		{
			InitializeComponent();
		}

		public PageColorDialog(Color color)
			: this()
		{
			FillBox(omBox, Color.Wheat);
			FillBox(customBox, Color.DarkSlateBlue);
		}


		public Color Color { get; private set; }


		public string ThemeKey { get; private set; }


		private void FillBox(PictureBox box, Color color)
		{
			box.Image = new Bitmap(box.Width, box.Height);
			using var g = Graphics.FromImage(box.Image);
			g.Clear(color);
			g.DrawRectangle(Pens.DarkGray, 0, 0, box.Width - 1, box.Height - 1);

			// TODO: resx
			var label = box == omBox
				? "Click here to select a pre-defined color"
				: "Click here to change your own custom color";

			var fbrush = color.GetBrightness() < 0.5 ? Brushes.White : Brushes.Black;
			var size = g.MeasureString(label, Font);
			g.DrawString(label, Font, fbrush, 8, box.Height / 2 - size.Height / 2);
		}


		private void ChooseColor(object sender, EventArgs e)
		{
			var location = PointToScreen(omBox.Location);

			using var dialog = new PageColorSelector();
			dialog.Left = location.X + omBox.Width - dialog.Width - 10;
			dialog.Top = location.Y + omBox.Height - 2;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				FillBox(omBox, dialog.Color);
			}
		}


		private void ChooseCustomColor(object sender, EventArgs e)
		{
			var location = PointToScreen(customBox.Location);

			using var dialog = new UI.MoreColorDialog(
				Resx.PageColorDialog_Text,
				// negative Left means right-justify window; see MoreColorDialog.HookProc!
				-(location.X + customBox.Width - 10),
				location.Y + customBox.Height - 4
				);

			dialog.Color = Color.Blue; //CustomColor;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				FillBox(customBox, dialog.Color);

				//var provider = new SettingsProvider();
				//var settings = provider.GetCollection("pageTheme");
				//settings.Add("customColor", dialog.Color.ToRGBHtml());
				//provider.SetCollection(settings);
				//provider.Save();
			}

		}


		private void ApplyColor(object sender, EventArgs e)
		{

		}

		private void ApplyCustomColor(object sender, EventArgs e)
		{

		}

		private void ApplyNoColor(object sender, EventArgs e)
		{

		}
	}
}
