//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	internal partial class PageColorSelector : Form
	{
		private sealed class Swatch
		{
			public readonly string Name;
			public Color Color;
			public Color Paint;
			public Rectangle Bounds;
			public Swatch(string name, int color)
			{
				Name = name;
				Color = ColorHelper.FromRgb(color);
				Paint = Color.Empty;
			}
		}

		private const int BorderRgb = 0x484644;             // light gray
		private const int ActiveBorderRgb = 0xD42314;       // orangy red
		private const int DarkBackgroundRgb = 0x292929;     // dark gray

		private const int SwatchSize = 60;
		private const int SwatchMargin = 3;

		private List<Swatch> palette;
		private Swatch active;
		private Color background;
		private readonly bool blackTheme;
		private readonly Pen borderPen;
		private readonly Pen activePen;


		public PageColorSelector()
		{
			InitializeComponent();

			Width = SwatchMargin + ((SwatchSize + SwatchMargin) * 3);
			var paletteHeight = SwatchMargin + ((SwatchSize + SwatchMargin) * 6);
			Height = paletteHeight + buttonPanel.Height;
			paletteBox.Image = new Bitmap(Width, paletteHeight);

			// disposed in Dispose()
			borderPen = new Pen(ColorHelper.FromRgb(BorderRgb));
			activePen = new Pen(ColorHelper.FromRgb(ActiveBorderRgb));

			blackTheme = Office.IsBlackThemeEnabled();
			LoadPalette();
			DrawPalette();
		}


		protected override CreateParams CreateParams
		{
			get
			{
				// force a drop-shadow on the window
				const int CS_DROPSHADOW = 0x20000;
				CreateParams cp = base.CreateParams;
				cp.ClassStyle |= CS_DROPSHADOW;
				return cp;
			}
		}


		private void LoadPalette()
		{
			palette = new List<Swatch>();

			if (blackTheme)
			{
				// dark colors - the color:param is applied to the page but dynamically inverted
				// by OneNote; the rendered color follows a conversion algorithm. The paint:param
				// is used to render the swatch, showing how it will appear when applied to a page
				// in dark mode...

				// dark
				Register("Black", 0xEEEEEE, 0x131313);
				Register("Gray", 0xCFCFCF, 0x303030);
				Register("Dark Teal", 0xB6DCDB, 0x1F4140);
				Register("Dark Olive Green", 0xD0E2C4, 0x2F4321);
				Register("Dark Slate Blue", 0xB6B6D1, 0x232338);
				Register("Midnight Blue", 0xBBC2D8, 0x23293D);
				Register("Saddle Brown", 0xE5CCB3, 0x473018);
				Register("Brown", 0xDAABAB, 0x401C1C);
				Register("Dark Purple", 0xE0C2D5, 0x412135);
				// light
				Register("White", 0x010101, 0xF3F3F3);
				Register("White Smoke", 0x111111, 0xF2F2F2);
				Register("Alice Blue", 0x003E75, 0xECF5FF);
				Register("Mint Cream", 0x00753A, 0xECFFF5);
				Register("Honeydew", 0x007500, 0xECFFEC);
				Register("Ivory", 0x757500, 0xFFFFEC);
				Register("Snow", 0x225151, 0xF0F9F9);
				Register("Seashell", 0x220E00, 0xFFF0E6);
				Register("Linen", 0x1B1005, 0xFCF3EB);
			}
			else
			{
				// light colors - explicit, exact color representations from ligth to dark
				// are applied to the page and rendered as specified...

				// light
				Register("White", 0xFFFFFF);
				Register("White Smoke", 0xF2F2F2);
				Register("Alice Blue", 0xECF5FF);
				Register("Mint Cream", 0xECFFF5);
				Register("Honeydew", 0xECFFEC);
				Register("Ivory", 0xFFFFEC);
				Register("Snow", 0xF0F9F9);
				Register("Seashell", 0xFFF0E6);
				Register("Linen", 0xFCF3EB);
				// dark
				Register("Black", 0x131313);
				Register("Gray", 0x303030);
				Register("Dark Teal", 0x1F4140);
				Register("Dark Olive Green", 0x2F4321);
				Register("Dark Slate Blue", 0x232338);
				Register("Midnight Blue", 0x23293D);
				Register("Saddle Brown", 0x473018);
				Register("Brown", 0x401C1C);
				Register("Dark Purple", 0x412135);
			}
		}


		private void Register(string name, int color, int? paint = null)
		{
			var swatch = new Swatch(name, color);
			swatch.Paint = paint == null ? swatch.Color : ColorHelper.FromRgb((int)paint);

			var row = palette.Count / 3;
			var col = palette.Count == 0 ? 0 : palette.Count % 3;

			swatch.Bounds = new Rectangle(
				SwatchMargin + (col * (SwatchSize + SwatchMargin)),
				SwatchMargin + (row * (SwatchSize + SwatchMargin)),
				SwatchSize, SwatchSize
				);

			palette.Add(swatch);
		}


		private void DrawPalette()
		{
			background = blackTheme ? ColorHelper.FromRgb(DarkBackgroundRgb) : BackColor;

			using var g = Graphics.FromImage(paletteBox.Image);
			g.Clear(background);

			for (int i = 0; i < palette.Count; i++)
			{
				var swatch = palette[i];

				using var brush = new SolidBrush(
					swatch.Paint.IsEmpty ? swatch.Color : swatch.Paint);

				g.FillRectangle(brush, swatch.Bounds);
				g.DrawRectangle(borderPen, swatch.Bounds);
			}

			paletteBox.Invalidate();
		}


		public Color Color { get; private set; }
		public Color PaintColor { get; private set; }


		private void HitTest(object sender, MouseEventArgs e)
		{
			var swatch = palette.FirstOrDefault(s => s.Bounds.Contains(e.Location));
			if (swatch != active)
			{
				using var g = paletteBox.CreateGraphics();

				if (active != null)
				{
					g.DrawRectangle(borderPen, active.Bounds);
				}

				if (swatch != null)
				{
					g.DrawRectangle(activePen, swatch.Bounds);
					tooltip.Show(swatch.Name, paletteBox, e.Location, 2000);
				}

				active = swatch;
			}
		}


		private void ChooseColor(object sender, MouseEventArgs e)
		{
			var swatch = palette.FirstOrDefault(s => s.Bounds.Contains(e.Location));
			if (swatch != null)
			{
				Color = swatch.Color;
				PaintColor = swatch.Paint;
				DialogResult = DialogResult.OK;
				Close();
			}
		}


		private void ChooseNoColor(object sender, EventArgs e)
		{
			Color = Color.Transparent;
			PaintColor = Color.Transparent;
			DialogResult = DialogResult.OK;
			Close();
		}


		private void CheckEscape(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}
	}
}
