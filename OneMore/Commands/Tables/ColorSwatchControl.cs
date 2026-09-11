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
	/// "unset" pattern when Color is empty, otherwise a solid fill.
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
	}
}
