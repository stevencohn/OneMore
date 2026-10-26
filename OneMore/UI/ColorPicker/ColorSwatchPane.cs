//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// The left side of MoreColorPicker: Automatic, theme, standard and recent swatches. Swatches
	/// are painted in light-mode or dark-mode variants but always report the light-mode color.
	/// </summary>
	internal sealed class ColorSwatchPane : Control
	{
		private const int Cell = 22;
		private const int Step = 26;
		private const int GridStep = Cell; // rows of the gradient grid touch, as in OneNote
		private const int Pad = 12;

		private sealed class Swatch
		{
			public Rectangle Bounds;
			public Color Light;
			public Color Paint;
			public bool Border = true;
		}

		private readonly ThemeManager manager;
		private readonly ToolTip toolTip;
		private readonly List<Swatch> swatches = new();
		private readonly List<(string Text, Point Location)> headers = new();
		private Rectangle automaticBounds;
		private Font headerFont;
		private List<Color> recents = new();
		private bool dark;
		private bool showAutomatic = true;
		private bool automaticSelected;
		private bool automaticHot;
		private int selected = -1;
		private int hot = -1;
		private float scale = 1f;
		private int recentBottom;
		private float? fixedScale;
		

		public ColorSwatchPane()
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

			manager = ThemeManager.Instance;
			toolTip = new ToolTip();
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				headerFont?.Dispose();
				toolTip.Dispose();
			}

			base.Dispose(disposing);
		}


		/// <summary>
		/// Raised with the light-mode color of a clicked swatch
		/// </summary>
		public event Action<Color> ColorPicked;

		public event Action AutomaticPicked;


		/// <summary>
		/// Gets the size needed to show all swatches at the current DPI
		/// </summary>
		public Size GetContentSize()
		{
			UpdateScale();
			return new Size(S((Pad * 2) + (Step * (ColorPalette.Columns - 1)) + Cell), S(332));
		}


		/// <summary>
		/// Sets the scale factor to lay out with, so this pane agrees with its host's layout
		/// rather than measuring the window DPI itself
		/// </summary>
		public new float Scale
		{
			set => fixedScale = value;
		}


		/// <summary>
		/// Gets the Y of the bottom of the Recent Colors row, in this control's coordinates
		/// </summary>
		public int RecentBottom => recentBottom;


		/// <summary>
		/// Gets the X of the right edge of the swatch columns, in this control's coordinates
		/// </summary>
		public int ContentRight => S(Pad + (Step * (ColorPalette.Columns - 1)) + Cell);


		public void Initialize(bool dark, bool showAutomatic, List<Color> recents)
		{
			this.dark = dark;
			this.showAutomatic = showAutomatic;
			this.recents = recents;
			Rebuild();
		}


		/// <summary>
		/// Selects the swatch matching the given color or, if automatic, the Automatic row;
		/// otherwise clears the selection
		/// </summary>
		public void Select(Color color, bool automatic)
		{
			automaticSelected = automatic;
			selected = -1;

			if (!automatic)
			{
				for (var i = 0; i < swatches.Count; i++)
				{
					if (ColorMath.SameRgb(swatches[i].Light, color))
					{
						selected = i;
						break;
					}
				}
			}

			Invalidate();
		}


		private int S(int value) => (int)Math.Round(value * scale);


		private void UpdateScale()
		{
			scale = fixedScale ?? (Scaling.GetDpi(this) / 96f);
		}


		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			Rebuild();
		}


		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			headerFont?.Dispose();
			headerFont = null;
			Invalidate();
		}


		private void Rebuild()
		{
			UpdateScale();
			swatches.Clear();
			headers.Clear();

			var x0 = S(Pad);
			var cell = S(Cell);
			var step = S(Step);
			var gridStep = S(GridStep);

			automaticBounds = showAutomatic
				? new Rectangle(0, S(6), S(280), S(26))
				: Rectangle.Empty;

			var y = showAutomatic ? S(40) : S(10);
			headers.Add((Resx.MoreColorPicker_themeColors, new Point(x0, y)));

			y += S(24);
			AddRow(ColorPalette.Theme(false), ColorPalette.Theme(dark), x0, y, cell, step);

			y += cell + S(10);
			var light = ColorPalette.Grid(false);
			var paint = ColorPalette.Grid(dark);
			for (var row = 0; row < ColorPalette.GridRows; row++)
			{
				var rowLight = new Color[ColorPalette.Columns];
				var rowPaint = new Color[ColorPalette.Columns];
				Array.Copy(light, row * ColorPalette.Columns, rowLight, 0, ColorPalette.Columns);
				Array.Copy(paint, row * ColorPalette.Columns, rowPaint, 0, ColorPalette.Columns);
				AddRow(rowLight, rowPaint, x0, y + (row * gridStep), cell, step, false);
			}

			y += (ColorPalette.GridRows * gridStep) + S(10);
			headers.Add((Resx.MoreColorPicker_standardColors, new Point(x0, y)));

			y += S(24);
			AddRow(ColorPalette.Standard(false), ColorPalette.Standard(dark), x0, y, cell, step);

			y += cell + S(14);
			headers.Add((Resx.MoreColorPicker_recentColors, new Point(x0, y)));

			y += S(24);
			var recentLight = recents.ToArray();
			var recentPaint = new Color[recentLight.Length];
			for (var i = 0; i < recentLight.Length; i++)
			{
				recentPaint[i] = dark ? ColorPalette.ToDark(recentLight[i]) : recentLight[i];
			}

			AddRow(recentLight, recentPaint, x0, y, cell, step);
			recentBottom = y + cell;

			selected = -1;
			hot = -1;
			Invalidate();
		}


		private void AddRow(
			Color[] light, Color[] paint, int x, int y, int cell, int step, bool border = true)
		{
			for (var i = 0; i < light.Length && i < ColorPalette.Columns; i++)
			{
				swatches.Add(new Swatch
				{
					Bounds = new Rectangle(x + (i * step), y, cell, cell),
					Light = light[i],
					Paint = paint[i],
					Border = border
				});
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			var back = manager.GetColor("Window");
			var fore = manager.GetColor("WindowText");
			var border = manager.GetColor("ButtonBorder");
			var accent = manager.GetColor("HotTrack");

			g.Clear(back);

			headerFont ??= new Font(Font, FontStyle.Bold);

			using var textBrush = new SolidBrush(fore);
			using var borderPen = new Pen(border);

			if (showAutomatic)
			{
				if (automaticHot || automaticSelected)
				{
					using var pen = new Pen(accent, automaticSelected ? 2 : 1);
					g.DrawRectangle(pen, Rectangle.Inflate(automaticBounds, -S(4), 0));
				}

				var box = new Rectangle(S(Pad), automaticBounds.Top + S(5), S(16), S(16));
				using (var brush = new SolidBrush(dark ? Color.White : Color.Black))
				{
					g.FillRectangle(brush, box);
				}

				g.DrawRectangle(borderPen, box);
				g.DrawString(Resx.MoreColorPicker_automatic, Font, textBrush,
					box.Right + S(8), automaticBounds.Top + S(5));
			}

			foreach (var (text, location) in headers)
			{
				g.DrawString(text, headerFont, textBrush, location.X, location.Y);
			}

			for (var i = 0; i < swatches.Count; i++)
			{
				var swatch = swatches[i];

				using (var brush = new SolidBrush(swatch.Paint))
				{
					g.FillRectangle(brush, swatch.Bounds);
				}

				if (i != selected && i != hot && swatch.Border)
				{
					g.DrawRectangle(borderPen, swatch.Bounds);
				}
			}

			// highlights are drawn after all fills so the next touching grid row
			// doesn't paint over their bottom edge
			if (hot >= 0 && hot != selected)
			{
				using var pen = new Pen(accent);
				g.DrawRectangle(pen, swatches[hot].Bounds);
			}

			if (selected >= 0)
			{
				using var pen = new Pen(accent, 2);
				// drawn inside the cell
				g.DrawRectangle(pen, Rectangle.Inflate(swatches[selected].Bounds, -1, -1));
			}
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var index = HitTest(e.Location);
			var auto = index < 0 && automaticBounds.Contains(e.Location);

			if (index != hot || auto != automaticHot)
			{
				hot = index;
				automaticHot = auto;
				Cursor = index >= 0 || auto ? Cursors.Hand : Cursors.Default;

				toolTip.SetToolTip(this, index >= 0
					? ColorMath.ToHex(swatches[index].Light)
					: auto ? Resx.MoreColorPicker_automatic : string.Empty);

				Invalidate();
			}
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			hot = -1;
			automaticHot = false;
			Invalidate();
		}


		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);

			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			var index = HitTest(e.Location);
			if (index >= 0)
			{
				ColorPicked?.Invoke(swatches[index].Light);
			}
			else if (automaticBounds.Contains(e.Location))
			{
				AutomaticPicked?.Invoke();
			}
		}


		private int HitTest(Point point)
		{
			for (var i = 0; i < swatches.Count; i++)
			{
				if (swatches[i].Bounds.Contains(point))
				{
					return i;
				}
			}

			return -1;
		}
	}
}
