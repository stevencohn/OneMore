//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.UI;
	using System.Drawing;
	using System.Drawing.Drawing2D;


	/// <summary>
	/// Draws small line-art glyphs distinguishing the four kinds of Favorite targets (page,
	/// section, section group, notebook), for use as a MoreListView.GetCellImage callback.
	/// Mirrors the icon choices already used by FavoritesMenu.MakeButton for the ribbon
	/// Favorites menu, redrawn as GDI+ shapes since imageMso isn't available outside the ribbon.
	/// </summary>
	internal static class FavoriteKindGlyphs
	{
		private const int GlyphSize = 14;

		private static Image pageGlyph;
		private static Image sectionGlyph;
		private static Image sectionGroupGlyph;
		private static Image notebookGlyph;


		/// <summary>
		/// Returns the cached glyph for the given favorite's kind (page, section, section
		/// group, or notebook), building it on first use.
		/// </summary>
		public static Image GetGlyph(Favorite favorite) =>
			!string.IsNullOrWhiteSpace(favorite.PageID)
				? pageGlyph ??= BuildPageGlyph()
				: favorite.Kind switch
				{
					"notebook" => notebookGlyph ??= BuildNotebookGlyph(),
					"sectiongroup" => sectionGroupGlyph ??= BuildSectionGroupGlyph(),
					_ => sectionGlyph ??= BuildSectionGlyph()
				};


		private static Bitmap NewCanvas(out Graphics g)
		{
			var bitmap = new Bitmap(GlyphSize, GlyphSize);
			g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			return bitmap;
		}


		private static Image BuildPageGlyph()
		{
			var bitmap = NewCanvas(out var g);
			using (g)
			{
				var color = ThemeManager.Instance.GetColor("ControlText");
				using var pen = new Pen(color);

				using var path = new GraphicsPath();
				path.AddLine(2, 1, 8, 1);
				path.AddLine(8, 1, 11, 4);
				path.AddLine(11, 4, 11, 13);
				path.AddLine(11, 13, 2, 13);
				path.CloseFigure();
				g.DrawPath(pen, path);

				g.DrawLine(pen, 4, 7, 9, 7);
				g.DrawLine(pen, 4, 10, 9, 10);
			}

			return bitmap;
		}


		private static Image BuildSectionGlyph()
		{
			var bitmap = NewCanvas(out var g);
			using (g)
			{
				var color = ThemeManager.Instance.GetColor("ControlText");
				using var pen = new Pen(color);
				g.DrawRectangle(pen, 2, 3, 9, 9);

				using var brush = new SolidBrush(color);
				g.FillRectangle(brush, 4, 1, 6, 3);
			}

			return bitmap;
		}


		private static Image BuildSectionGroupGlyph()
		{
			var bitmap = NewCanvas(out var g);
			using (g)
			{
				var color = ThemeManager.Instance.GetColor("ControlText");
				using var pen = new Pen(color);

				using var path = new GraphicsPath();
				path.AddLine(2, 4, 5, 4);
				path.AddLine(5, 4, 6, 2);
				path.AddLine(6, 2, 10, 2);
				path.AddLine(10, 2, 10, 4);
				path.AddLine(10, 4, 12, 4);
				path.AddLine(12, 4, 12, 12);
				path.AddLine(12, 12, 2, 12);
				path.CloseFigure();
				g.DrawPath(pen, path);
			}

			return bitmap;
		}


		private static Image BuildNotebookGlyph()
		{
			var bitmap = NewCanvas(out var g);
			using (g)
			{
				var color = ThemeManager.Instance.GetColor("ControlText");
				using var pen = new Pen(color);

				g.DrawRectangle(pen, 2, 1, 9, 12);
				g.DrawLine(pen, 5, 1, 5, 13);

				for (var y = 3; y <= 10; y += 3)
				{
					g.DrawLine(pen, 3, y, 4, y);
				}
			}

			return bitmap;
		}
	}
}
