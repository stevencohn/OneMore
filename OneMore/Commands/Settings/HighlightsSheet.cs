//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


	internal partial class HighlightsSheet : SheetBase
	{
		// box metrics are in design pixels, authored at 150% (144 DPI) where the pictures
		// are DesignWidth wide; they are scaled by the ratio of the actual picture width
		private const int DesignWidth = 436;
		private const int BoxSpacing = 6;
		private const int BoxSize = 37;
		private const int BoxTop = 2;

		private readonly HighlightColorSchemes schemes;


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
					"extendBox",
					"normalRadio",
					"fadedRadio",
					"deepRadio",
					"modeLabel",
					"lightRadio",
					"darkRadio"
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

			schemes = new HighlightColorSchemes();

			backdrop.Paint += PaintBackdropBorder;
			normalPicture.Paint += PaintScheme;
			fadedPicture.Paint += PaintScheme;
			deepPicture.Paint += PaintScheme;

			extendBox.Checked = settings.Get<bool>("extended");

			// preview defaults to the Office theme; not persisted
			if (Office.IsBlackThemeEnabled())
			{
				darkRadio.Checked = true;
			}

			ApplyBackdrop();
		}


		public override bool CollectSettings()
		{
			string theme = "Normal";
			if (fadedRadio.Checked) theme = "Faded";
			else if (deepRadio.Checked) theme = "Deep";

			var settings = provider.GetCollection(Name);
			settings.Add("theme", theme);

			if (extendBox.Checked)
			{
				settings.Add("extended", extendBox.Checked);
			}
			else
			{
				settings.Remove("extended");
			}

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

		private bool Dark => darkRadio.Checked;


		private void ChangeMode(object sender, System.EventArgs e)
		{
			ApplyBackdrop();
		}


		// one backdrop behind all rows emulates a page of the selected color
		private void ApplyBackdrop()
		{
			var color = Dark ? ColorPalette.Theme(true)[0] : Color.White;
			backdrop.BackColor = color;
			normalPicture.BackColor = color;
			fadedPicture.BackColor = color;
			deepPicture.BackColor = color;
			DrawSchemes(null, null);
		}


		private void PaintBackdropBorder(object sender, PaintEventArgs e)
		{
			using (var pen = new Pen(ThemeManager.Instance.GetColor("ButtonBorder")))
			{
				e.Graphics.DrawRectangle(pen, 0, 0, backdrop.Width - 1, backdrop.Height - 1);
			}
		}


		private void DrawSchemes(object sender, System.EventArgs e)
		{
			normalPicture.Invalidate();
			fadedPicture.Invalidate();
			deepPicture.Invalidate();
		}


		private void PaintScheme(object sender, PaintEventArgs e)
		{
			var picture = (PictureBox)sender;

			var scheme = picture switch
			{
				var p when p == fadedPicture => schemes.GetScheme("Faded"),
				var p when p == deepPicture => schemes.GetScheme("Deep"),
				_ => schemes.GetScheme("Bright")
			};


			DrawScheme(e.Graphics, picture.Width, picture.BackColor, scheme.GetSet(Dark), extendBox.Checked, Dark);
		}


		private void DrawScheme(
			Graphics g, int width, Color backColor, HighlightColorSet scheme, bool extended, bool dark)
		{
			g.Clear(backColor);

			var scale = (float)width / DesignWidth;
			var size = (int)Math.Round(BoxSize * scale);
			var spacing = (int)Math.Round(BoxSpacing * scale);
			var top = (int)Math.Round(BoxTop * scale);

			var border = dark ? Pens.DimGray : Pens.LightGray;
			var fore = dark ? Color.White : Color.Black;

			var x = 0;
			foreach (var color in scheme.Colors)
			{
				DrawBox(g, x, top, size, color, border, fore);
				x += size + spacing;
			}

			if (extended)
			{
				foreach (var color in scheme.Extended)
				{
					DrawBox(g, x, top, size, color, border, fore);
					x += size + spacing;
				}
			}
		}


		private void DrawBox(
			Graphics g, int x, int top, int size, string color, Pen border, Color fore)
		{
			using (var brush = new SolidBrush(ColorTranslator.FromHtml(color)))
			{
				g.FillRectangle(brush, x, top, size, size);
				g.DrawRectangle(border, x, top, size - 1, size - 1);
			}

			TextRenderer.DrawText(g, "Aa", Font, new Rectangle(x, top, size, size), fore,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPadding);
		}
	}
}
