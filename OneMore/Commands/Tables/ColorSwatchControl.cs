//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// A small owner-drawn color swatch used by ThemeColorRow. Paints a checkerboard
	/// "unset" pattern when Color is empty, a palette sample when Color is TableTheme's
	/// special multi-color "Rainbow" sentinel, otherwise a solid fill.
	/// </summary>
	internal sealed class ColorSwatchControl : Control
	{
		private const int TileSize = 5;

		private Color color = Color.Empty;


		public ColorSwatchControl()
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer, true);

			Cursor = Cursors.Hand;
		}


		public Color Color
		{
			get => color;
			set
			{
				color = value;
				Invalidate();
			}
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var manager = ThemeManager.Instance;
			var g = e.Graphics;
			var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

			if (color.IsEmpty)
			{
				PaintCheckerboard(g, bounds, manager);
			}
			else if (color == TableTheme.Rainbow)
			{
				PaintMultiColor(g, bounds);
			}
			else
			{
				using var brush = new SolidBrush(color);
				g.FillRectangle(brush, bounds);
			}

			using var pen = new Pen(manager.GetColor("ButtonBorder"));
			g.DrawRectangle(pen, bounds);
		}


		private static void PaintCheckerboard(Graphics g, Rectangle bounds, ThemeManager manager)
		{
			using var light = new SolidBrush(manager.GetColor("Window"));
			using var dark = new SolidBrush(manager.GetColor("ButtonFace"));

			g.FillRectangle(light, bounds);

			var rowToggle = false;
			for (var y = bounds.Top; y < bounds.Bottom; y += TileSize)
			{
				var toggle = rowToggle;
				for (var x = bounds.Left; x < bounds.Right; x += TileSize)
				{
					if (toggle)
					{
						var w = Math.Min(TileSize, bounds.Right - x);
						var h = Math.Min(TileSize, bounds.Bottom - y);
						g.FillRectangle(dark, x, y, w, h);
					}

					toggle = !toggle;
				}

				rowToggle = !rowToggle;
			}
		}


		/// <summary>
		/// TableTheme.Rainbow is a sentinel value (see TableThemePainter) meaning "paint the
		/// special multi-color pattern here", not a real color - filling with it literally
		/// would render as an almost-invisible ~7%-alpha tint, indistinguishable from
		/// "unset" at a glance. Show a sample of the same palette TableThemePainter itself
		/// uses instead, so this area reads as "set, and multi-colored" rather than blank.
		/// </summary>
		private static void PaintMultiColor(Graphics g, Rectangle bounds)
		{
			var swatchColors = TableTheme.MediumColorNames;
			var stripeWidth = Math.Max(1, bounds.Width / swatchColors.Length);

			for (var i = 0; i < swatchColors.Length; i++)
			{
				using var brush = new SolidBrush(ColorTranslator.FromHtml(swatchColors[i]));
				var x = bounds.Left + (i * stripeWidth);
				var w = i == swatchColors.Length - 1 ? bounds.Right - x : stripeWidth;
				g.FillRectangle(brush, x, bounds.Top, w, bounds.Height);
			}
		}
	}
}
