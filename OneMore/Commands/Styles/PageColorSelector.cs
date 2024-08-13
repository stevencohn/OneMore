//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	internal partial class PageColorSelector : Form
	{

		private const int BorderRgb = 0x484644;             // light gray
		private const int ActiveBorderRgb = 0xD42314;       // orangy red
		private const int DarkBackgroundRgb = 0x292929;     // dark gray

		private readonly Pen activePen;
		private readonly Pen borderPen;
		private readonly PageColors palette;
		private Swatch active;
		private Color background;


		public PageColorSelector()
		{
			InitializeComponent();

			Width = Swatch.Margin + ((Swatch.Size + Swatch.Margin) * 3);
			var paletteHeight = Swatch.Margin + ((Swatch.Size + Swatch.Margin) * 6);
			Height = paletteHeight + buttonPanel.Height;
			paletteBox.Image = new Bitmap(Width, paletteHeight);

			// disposed in Dispose()
			borderPen = new Pen(ColorHelper.FromRgb(BorderRgb));
			activePen = new Pen(ColorHelper.FromRgb(ActiveBorderRgb));

			palette = new PageColors();
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


		private void DrawPalette()
		{
			background = Office.IsBlackThemeEnabled()
				? ColorHelper.FromRgb(DarkBackgroundRgb)
				: BackColor;

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
			var swatch = palette.Find(s => s.Bounds.Contains(e.Location));
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
					tooltip.Show(
						$"{swatch.Name} ({swatch.Color.ToRGBHtml()})",
						paletteBox, e.Location, 2000);
				}

				active = swatch;
			}
		}


		private void ChooseColor(object sender, MouseEventArgs e)
		{
			var swatch = palette.Find(s => s.Bounds.Contains(e.Location));
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
