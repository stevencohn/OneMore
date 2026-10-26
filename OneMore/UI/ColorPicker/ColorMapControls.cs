//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	/// <summary>
	/// The rainbow map of hue (horizontal) and saturation (vertical, saturated at top)
	/// </summary>
	internal sealed class SpectrumControl : Control
	{
		private Bitmap map;
		private double hue;
		private double saturation;


		public SpectrumControl()
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

			Cursor = Cursors.Cross;
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				map?.Dispose();
			}

			base.Dispose(disposing);
		}


		/// <summary>
		/// Raised when the user picks a hue (0-360) and saturation (0-1)
		/// </summary>
		public event Action<double, double> Picked;


		public void SetPosition(double hue, double saturation)
		{
			this.hue = hue;
			this.saturation = saturation;
			Invalidate();
		}


		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			map?.Dispose();
			map = null;
		}


		private Bitmap BuildMap()
		{
			var bitmap = new Bitmap(Math.Max(1, Width), Math.Max(1, Height));
			var xmax = Math.Max(1, Width - 1);
			var ymax = Math.Max(1, Height - 1);

			for (var x = 0; x < Width; x++)
			{
				var h = 360.0 * x / xmax;
				for (var y = 0; y < Height; y++)
				{
					bitmap.SetPixel(x, y, ColorMath.FromHsv(h, 1.0 - ((double)y / ymax), 1.0));
				}
			}

			return bitmap;
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			map ??= BuildMap();
			var g = e.Graphics;
			g.DrawImageUnscaled(map, 0, 0);

			var x = (int)Math.Round(hue / 360.0 * Math.Max(1, Width - 1));
			var y = (int)Math.Round((1.0 - saturation) * Math.Max(1, Height - 1));
			var r = Math.Max(4, Width / 40);

			g.SmoothingMode = SmoothingMode.AntiAlias;
			using (var pen = new Pen(Color.Black, 2))
			{
				g.DrawEllipse(pen, x - r, y - r, r * 2, r * 2);
			}

			using (var pen = new Pen(Color.White))
			{
				g.DrawEllipse(pen, x - r, y - r, r * 2, r * 2);
			}
		}


		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Pick(e.Location, e.Button);
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			Pick(e.Location, e.Button);
		}


		private void Pick(Point point, MouseButtons buttons)
		{
			if (buttons != MouseButtons.Left)
			{
				return;
			}

			var x = Math.Max(0, Math.Min(Width - 1, point.X));
			var y = Math.Max(0, Math.Min(Height - 1, point.Y));

			hue = 360.0 * x / Math.Max(1, Width - 1);
			saturation = 1.0 - ((double)y / Math.Max(1, Height - 1));
			Invalidate();
			Picked?.Invoke(hue, saturation);
		}
	}


	/// <summary>
	/// A vertical brightness (HSV value) slider painted as a gradient from the full-brightness
	/// color at the top to black, with a pointer on the right
	/// </summary>
	internal sealed class BrightnessControl : Control
	{
		private double hue;
		private double saturation;
		private double value;


		public BrightnessControl()
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

			Cursor = Cursors.Hand;
		}


		/// <summary>
		/// Raised when the user picks a brightness (0-1)
		/// </summary>
		public event Action<double> Picked;


		public void SetColor(double hue, double saturation, double value)
		{
			this.hue = hue;
			this.saturation = saturation;
			this.value = value;
			Invalidate();
		}


		private Rectangle Bar => new(0, 0, Width - Pointer, Height - 1);

		private int Pointer => Math.Max(8, Width * 2 / 5);


		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			var manager = ThemeManager.Instance;
			g.Clear(BackColor);

			var bar = Bar;
			if (bar.Height <= 0 || bar.Width <= 0)
			{
				return;
			}

			using (var brush = new LinearGradientBrush(
				new Rectangle(bar.Left, bar.Top - 1, bar.Width, bar.Height + 2),
				ColorMath.FromHsv(hue, saturation, 1.0), Color.Black, 90f))
			{
				g.FillRectangle(brush, bar);
			}

			using (var pen = new Pen(manager.GetColor("ButtonBorder")))
			{
				g.DrawRectangle(pen, bar);
			}

			var y = (int)Math.Round((1.0 - value) * bar.Height);
			var tip = bar.Right + 2;
			using var fill = new SolidBrush(manager.GetColor("WindowText"));
			g.FillPolygon(fill, new[]
			{
				new Point(tip, y),
				new Point(Width - 1, y - Pointer / 2 + 1),
				new Point(Width - 1, y + Pointer / 2 - 1)
			});
		}


		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Pick(e.Y, e.Button);
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			Pick(e.Y, e.Button);
		}


		private void Pick(int y, MouseButtons buttons)
		{
			if (buttons != MouseButtons.Left)
			{
				return;
			}

			var height = Math.Max(1, Bar.Height);
			value = 1.0 - (Math.Max(0, Math.Min(height, y)) / (double)height);
			Invalidate();
			Picked?.Invoke(value);
		}
	}


	/// <summary>
	/// A small owner-drawn button showing a double chevron: pointing right (expand) when
	/// collapsed, left (collapse) when expanded
	/// </summary>
	internal sealed class ExpandGlyph : Control
	{
		private bool expanded;
		private bool hot;


		public ExpandGlyph()
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
				ControlStyles.Selectable, true);

			Cursor = Cursors.Hand;
			TabStop = true;
		}


		public bool Expanded
		{
			get => expanded;
			set
			{
				expanded = value;
				Invalidate();
			}
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			var manager = ThemeManager.Instance;
			g.Clear(BackColor);

			if (hot || Focused)
			{
				using var border = new Pen(manager.GetColor(hot ? "HotTrack" : "ButtonBorder"));
				g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
			}

			g.SmoothingMode = SmoothingMode.AntiAlias;
			using var pen = new Pen(manager.GetColor("WindowText"), Math.Max(1f, Height / 12f))
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round,
				LineJoin = LineJoin.Round
			};

			// two chevrons centered in the control
			var arm = Height * 0.28f;
			var gap = arm * 1.1f;
			var cx = Width / 2f;
			var cy = Height / 2f;
			var dir = expanded ? -1f : 1f;

			for (var i = 0; i < 2; i++)
			{
				var tip = cx + (dir * ((i - 0.5f) * gap)) + (dir * arm / 2f);
				g.DrawLines(pen, new[]
				{
					new PointF(tip - (dir * arm), cy - arm),
					new PointF(tip, cy),
					new PointF(tip - (dir * arm), cy + arm)
				});
			}
		}


		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			hot = true;
			Invalidate();
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			hot = false;
			Invalidate();
		}


		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}


		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			Invalidate();
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Space)
			{
				e.Handled = true;
				OnClick(EventArgs.Empty);
			}
		}
	}
}
