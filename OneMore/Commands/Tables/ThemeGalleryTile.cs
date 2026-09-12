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
	/// One clickable thumbnail in a ThemeGalleryPopup: a small table-swatch preview (same
	/// TableThemePainter used by the ribbon gallery's own TileFactory.MakeTableTile, at the
	/// same 70x60 size) with a hover highlight. Raises Click (inherited) when picked.
	/// </summary>
	internal sealed class ThemeGalleryTile : Control
	{
		private const int ImageWidth = 70;
		private const int ImageHeight = 60;
		private const int Padding = 5;

		private readonly Image image;
		private bool hovering;


		public ThemeGalleryTile(TableTheme theme)
		{
			Theme = theme;

			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer, true);

			Cursor = Cursors.Hand;
			Size = new Size(ImageWidth + (Padding * 2), ImageHeight + (Padding * 2));

			image = new Bitmap(ImageWidth, ImageHeight);
			var painter = new TableThemePainter(
				image,
				new Rectangle(3, 3, ImageWidth - 6, ImageHeight - 6),
				ThemeManager.Instance.GetColor("Window"));
			painter.Paint(theme);
		}


		public TableTheme Theme { get; }


		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			hovering = true;
			Invalidate();
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			hovering = false;
			Invalidate();
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var manager = ThemeManager.Instance;
			var g = e.Graphics;

			if (hovering)
			{
				using var brush = new SolidBrush(manager.GetColor("LinkHighlight"));
				g.FillRectangle(brush, 0, 0, Width, Height);
			}

			g.DrawImage(image, Padding, Padding);

			using var pen = new Pen(manager.GetColor("ButtonBorder"));
			g.DrawRectangle(pen, Padding, Padding, ImageWidth - 1, ImageHeight - 1);

			if (hovering)
			{
				using var hoverPen = new Pen(manager.GetColor("HotTrack"));
				g.DrawRectangle(hoverPen, 0, 0, Width - 1, Height - 1);
			}
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				image.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
