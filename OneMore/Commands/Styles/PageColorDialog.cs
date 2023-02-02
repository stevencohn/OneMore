//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
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
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("pageColor");
			if (settings == null)
			{
				omBox.Tag = color;
				customBox.Tag = color;
			}
			else
			{
				var setting = settings["omColor"];
				omBox.Tag = setting == null ? color : ColorTranslator.FromHtml(setting);

				setting = settings["customColor"];
				customBox.Tag = setting == null ? color : ColorTranslator.FromHtml(setting);
			}

			FillBox(omBox, (Color)omBox.Tag);
			FillBox(customBox, (Color)customBox.Tag);
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
				// use Tag as a cache for ApplyColor
				omBox.Tag = dialog.Color;

				var color = dialog.PaintColor;
				if (color.Equals(Color.Transparent))
				{
					color = SystemColors.Window;
				}

				FillBox(omBox, color);
			}
		}


		private void ChooseCustomColor(object sender, EventArgs e)
		{
			var location = PointToScreen(customBox.Location);

			using var dialog = new UI.MoreColorDialog(
				Resx.PageColorDialog_Text,
				// negative Left means right-justify window; see MoreColorDialog.HookProc!
				-(location.X + customBox.Width) - 6,
				location.Y + customBox.Height - 4
				);

			dialog.Color = (Color)customBox.Tag;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				// use Tag as a cache for ApplyCustomColor
				customBox.Tag = dialog.Color;
				FillBox(customBox, dialog.Color);
			}
		}


		private void ApplyColor(object sender, EventArgs e)
		{
			Color = (Color)omBox.Tag;

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("pageColor");
			settings.Add("omColor", Color.ToRGBHtml());
			provider.SetCollection(settings);
			provider.Save();

			DialogResult = DialogResult.OK;
			Close();
		}

		private void ApplyCustomColor(object sender, EventArgs e)
		{
			Color = (Color)customBox.Tag;

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("pageColor");
			settings.Add("customColor", Color.ToRGBHtml());
			provider.SetCollection(settings);
			provider.Save();

			DialogResult = DialogResult.OK;
			Close();
		}


		private void ApplyNoColor(object sender, EventArgs e)
		{
			Color = Color.Transparent;
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
