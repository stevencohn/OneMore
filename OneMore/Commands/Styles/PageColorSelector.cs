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
			public Swatch(string name, uint color)
			{
				Name = name;
				Color = Color.FromArgb((int)color);
				Paint = Color.Empty;
			}
		}


		private const int SwatchSize = 60;
		private const int SwatchMargin = 3;
		private static uint BorderRgb = 0xFF484644;             // light gray
		private static uint ActiveBorderRgb = 0xFFD42314;       // orangy red
		private static uint DarkBackgroundRgb = 0xFF292929;     // dark gray


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
			borderPen = new Pen(Color.FromArgb((int)BorderRgb));
			activePen = new Pen(Color.FromArgb((int)ActiveBorderRgb));

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
				Register("Black", 0xFFEEEEEE, 0xFF131313);
				Register("Gray", 0xFFCFCFCF, 0xFF303030);
				Register("Dark Teal", 0xFFB6DCDB, 0xFF1F4140);
				Register("Dark Olive Green", 0xFFD0E2C4, 0xFF2F4321);
				Register("Dark Slate Blue", 0xFFB6B6D1, 0xFF232338);
				Register("Midnight Blue", 0xFFBBC2D8, 0xFF23293D);
				Register("Saddle Brown", 0xFFE5CCB3, 0xFF473018);
				Register("Brown", 0xFFDAABAB, 0xFF401C1C);
				Register("Dark Purple", 0xFFE0C2D5, 0xFF412135);
				// light
				Register("White", 0xFF010101, 0xFFF3F3F3);
				Register("White Smoke", 0xFF111111, 0xFFF2F2F2);
				Register("Alice Blue", 0xFF003E75, 0xFFECF5FF);
				Register("Mint Cream", 0xFF00753A, 0xFFECFFF5);
				Register("Honeydew", 0xFF007500, 0xFFECFFEC);
				Register("Ivory", 0xFF757500, 0xFFFFFFEC);
				Register("Snow", 0xFF225151, 0xFFF0F9F9);
				Register("Seashell", 0xFF220E00, 0xFFFFF0E6);
				Register("Linen", 0xFF1B1005, 0xFFFCF3EB);
			}
			else
			{
				// light colors - explicit, exact color representations from ligth to dark
				// are applied to the page and rendered as specified...

				// light
				Register("White", 0xFFFFFFFF);
				Register("White Smoke", 0xFFF2F2F2);
				Register("Alice Blue", 0xFFECF5FF);
				Register("Mint Cream", 0xFFECFFF5);
				Register("Honeydew", 0xFFECFFEC);
				Register("Ivory", 0xFFFFFFEC);
				Register("Snow", 0xFFF0F9F9);
				Register("Seashell", 0xFFFFF0E6);
				Register("Linen", 0xFFFCF3EB);
				// dark
				Register("Black", 0xFF131313);
				Register("Gray", 0xFF303030);
				Register("Dark Teal", 0xFF1F4140);
				Register("Dark Olive Green", 0xFF2F4321);
				Register("Dark Slate Blue", 0xFF232338);
				Register("Midnight Blue", 0xFF23293D);
				Register("Saddle Brown", 0xFF473018);
				Register("Brown", 0xFF401C1C);
				Register("Dark Purple", 0xFF412135);
			}
		}


		private void Register(string name, uint color, uint? paint = null)
		{
			var swatch = new Swatch(name, color);
			swatch.Paint = paint == null ? swatch.Color : Color.FromArgb((int)paint);

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
			background = blackTheme ? Color.FromArgb((int)DarkBackgroundRgb) : BackColor;

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
