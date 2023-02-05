﻿//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class PageColorDialog : UI.LocalizableForm
	{
		private Theme theme;
		private Color pageColor;
		private bool optionsAvailable;
		private readonly bool darkMode;


		public PageColorDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.PageColorDialog_Text;

				Localize(new string[]
				{
					"noLabel",
					"expander.Title",
					"currentThemeLabel",
					"loadThemeLink",
					"applyThemeBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			VerticalOffset = -(Height / 3);
			Height -= optionsPanel.Height;
			optionsPanel.Height = 0;
			optionsAvailable = true;

			darkMode = Office.IsBlackThemeEnabled();
			statusLabel.Text = string.Empty;
		}


		/// <summary>
		/// Called to set the current page color
		/// </summary>
		/// <param name="color">
		/// The current color of the page;
		/// If the page color is "automatic" then this should be Color.Transparent
		/// </param>
		public PageColorDialog(Color color)
			: this()
		{
			pageColor = color;
			Color customColor, omColor;

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("pageColor");

			var customPaint = pageColor.Equals(Color.Transparent)
				? darkMode ? BasicColors.BlackSmoke : Color.White
				: pageColor;

			// prioritize MRU, otherwise default to page color
			var setting = settings["customColor"];
			if (setting != null)
			{
				customPaint = ColorHelper.FromHtml(setting);
			}

			customBox.Tag = customPaint;
			customColor = customPaint;

			if (pageColor.Equals(Color.Transparent))
				noButton.Checked = true;
			else
				customButton.Checked = true;

			// prioritize MRU, otherwise default to page color
			setting = settings["omColor"];
			if (setting == null)
			{
				var colors = new PageColors();
				var index = colors.FindIndex(s => s.Color.Equals(pageColor));
				if (index >= 0)
				{
					omBox.Tag = omColor = colors[index].Color;
					omButton.Checked = true;
				}
				else
				{
					omBox.Tag = omColor = colors[0].Color;
				}
			}
			else
			{
				omColor = ColorHelper.FromHtml(setting);
				omBox.Tag = omColor;
				omButton.Checked = omColor.Equals(pageColor);
			}

			FillBox(omBox, EstimateOneNoteColor((Color)omBox.Tag));
			FillBox(customBox, EstimateOneNoteColor((Color)customBox.Tag));

			TipBox(omBox, omColor);
			TipBox(customBox, customColor);
		}


		public PageColorDialog(Color color, string themeName, bool useTheme)
			: this(color)
		{
			pageColor = color;
			Color customColor, omColor;

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("pageColor");

			var customPaint = pageColor.Equals(Color.Transparent)
				? darkMode ? BasicColors.BlackSmoke : Color.White
				: pageColor;

			// prioritize MRU, otherwise default to page color
			var setting = settings["customColor"];
			if (setting != null)
			{
				customPaint = ColorHelper.FromHtml(setting);
			}

			customBox.Tag = customPaint;
			customColor = customPaint;

			if (pageColor.Equals(Color.Transparent))
				noButton.Checked = true;
			else
				customButton.Checked = true;

			// prioritize MRU, otherwise default to page color
			setting = settings["omColor"];
			if (setting == null)
			{
				var colors = new PageColors();
				var index = colors.FindIndex(s => s.Color.Equals(pageColor));
				if (index >= 0)
				{
					omBox.Tag = omColor = colors[index].Color;
					omButton.Checked = true;
				}
				else
				{
					omBox.Tag = omColor = colors[0].Color;
				}
			}
			else
			{
				omColor = ColorHelper.FromHtml(setting);
				omBox.Tag = omColor;
				omButton.Checked = omColor.Equals(pageColor);
			}

			FillBox(omBox, EstimateOneNoteColor((Color)omBox.Tag));
			FillBox(customBox, EstimateOneNoteColor((Color)customBox.Tag));

			TipBox(omBox, omColor);
			TipBox(customBox, customColor);

			if (themeName != null)
			{
				currentThemeLabel.Text =
					string.Format(Resx.PageColorDialog_currentThemeLabel_Text, themeName);
			}
		}


		/// <summary>
		/// Called to set the preferred color for a style theme
		/// </summary>
		/// <param name="color"></param>
		/// <param name="setColor"></param>
		/// <param name="themeName"></param>
		public PageColorDialog(Color color, bool setColor, string themeName = null)
			: this()
		{
			pageColor = color;

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("pageColor");
			if (settings == null)
			{
				omBox.Tag = pageColor;
				customBox.Tag = pageColor;
			}
			else
			{
				var setting = settings["omColor"];
				omBox.Tag = setting == null ? pageColor : ColorTranslator.FromHtml(setting);

				setting = settings["customColor"];
				customBox.Tag = setting == null ? pageColor : ColorTranslator.FromHtml(setting);
			}

			FillBox(omBox, EstimateOneNoteColor((Color)omBox.Tag));
			FillBox(customBox, EstimateOneNoteColor((Color)customBox.Tag));

			// find cloest match...

			if (pageColor.Equals(Color.Transparent))
			{
				noButton.Checked = true;
			}
			else if (pageColor.Equals((Color)customBox.Tag))
			{
				customButton.Checked = true;
			}
			else if (new PageColors().Contains(pageColor))
			{
				omButton.Checked = true;
			}
			else
			{
				//noButton.Checked = true;
			}

			if (themeName != null)
			{
				currentThemeLabel.Text =
					string.Format(Resx.PageColorDialog_currentThemeLabel_Text, themeName);
			}
		}


		public bool ApplyStyle => expander.Expanded && applyThemeBox.Checked;


		public Color Color { get; private set; }


		public string ThemeKey => theme.Key;


		private Color EstimateOneNoteColor(Color color)
		{
			return darkMode ? color.Invert() : color;
		}


		public void HideOptions()
		{
			Height -= expander.Height - statusLabel.Height;
			expander.Visible = false;
			statusLabel.Visible = false;
			optionsAvailable = false;
		}


		private void FillBox(PictureBox box, Color color)
		{
			box.Image?.Dispose();
			box.Image = new Bitmap(box.Width, box.Height);
			using var g = Graphics.FromImage(box.Image);
			g.Clear(color);
			g.DrawRectangle(Pens.DarkGray, 0, 0, box.Width - 1, box.Height - 1);

			// TODO: resx
			var label = box == omBox
				? Resx.PageColorDialog_omLabel
				: Resx.PageColorDialog_customLabel;

			var fbrush = color.GetBrightness() < 0.5 ? Brushes.White : Brushes.Black;
			var size = g.MeasureString(label, Font);
			g.DrawString(label, Font, fbrush, 8, box.Height / 2 - size.Height / 2);
		}


		private void TipBox(PictureBox box, Color color)
		{
			tooltip.SetToolTip(box,
				color.Equals(Color.Transparent) ? StyleBase.Automatic : color.ToRGBHtml());
		}


		private void ChooseColor(object sender, EventArgs e)
		{
			omButton.Checked = true;

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
				TipBox(omBox, color);

				CheckContrast();
			}
		}


		private void ChooseCustomColor(object sender, EventArgs e)
		{
			customButton.Checked = true;

			var location = PointToScreen(customBox.Location);

			using var dialog = new UI.MoreColorDialog(
				Resx.PageColorDialog_Text,
				location.X + 30,
				location.Y + customBox.Height - 4
				);

			dialog.Color = (Color)customBox.Tag;
			dialog.FullOpen = true;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				// use Tag as a cache for ApplyCustomColor
				customBox.Tag = dialog.Color;

				var estimate = EstimateOneNoteColor(dialog.Color);
				FillBox(customBox, estimate);
				TipBox(customBox, estimate);

				CheckContrast();
			}
		}


		private void ChooseNoColor(object sender, EventArgs e)
		{
			noButton.Checked = true;
			CheckContrast();
		}


		private void ChangeColorOption(object sender, EventArgs e)
		{
			CheckContrast();
		}



		private void LoadTheme(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var candidate = new LoadStylesCommand().LoadTheme();
			if (candidate != null)
			{
				theme = candidate;
				currentThemeLabel.Text =
					string.Format(Resx.PageColorDialog_currentThemeLabel_Text, theme.Name);

				CheckContrast();
			}
		}


		private void CheckContrast()
		{
			if (theme == null || !optionsAvailable)
			{
				statusLabel.Text = string.Empty;
				return;
			}

			var color = omButton.Checked
				? (Color)omBox.Tag
				: customButton.Checked ? (Color)customBox.Tag : pageColor;

			var dark = color.GetBrightness() < 0.5;

			statusLabel.Text = theme.Dark == dark
				? string.Empty
				: Resx.PageColorDialog_contrastWarning;
		}


		private void Apply(object sender, EventArgs e)
		{
			if (omButton.Checked)
			{
				Color = (Color)omBox.Tag;
				Save("omColor");
			}
			else if (customButton.Checked)
			{
				Color = (Color)customBox.Tag;
				Save("customColor");
			}
			else
			{
				Color = Color.Transparent;
			}
		}


		private void Save(string key)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("pageColor");
			settings.Add(key, Color.ToRGBHtml());
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
