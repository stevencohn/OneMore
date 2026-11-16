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
		private const float PenWidth = 1.5f;

		private static Color builtWith;

		private static Image pageGlyph;
		private static Image openPageGlyph;
		private static Image sectionGlyph;
		private static Image openSectionGlyph;
		private static Image sectionGroupGlyph;
		private static Image openSectionGroupGlyph;
		private static Image notebookGlyph;


		/// <summary>
		/// Returns the cached glyph for the given favorite's kind (page, section, section
		/// group, or notebook), building it on first use.
		/// </summary>
		/// <param name="favorite">The favorite to draw</param>
		/// <param name="isOpen">True if the favorite is already showing in an OneNote window;
		/// only a page is drawn differently, as a solid page rather than an outline</param>
		public static Image GetGlyph(Favorite favorite, bool isOpen = false)
		{
			DiscardIfThemeChanged();
			return Pick(favorite, isOpen);
		}


		// glyphs are drawn in the theme's text color and cached for the life of the process, so
		// start over if the theme changed, e.g. dark to light, since the old color may be unreadable
		private static void DiscardIfThemeChanged()
		{
			var color = ThemeManager.Instance.GetColor("ControlText");
			if (color == builtWith)
			{
				return;
			}

			foreach (var glyph in new[]
			{
				pageGlyph, openPageGlyph, sectionGlyph, openSectionGlyph,
				sectionGroupGlyph, openSectionGroupGlyph, notebookGlyph
			})
			{
				glyph?.Dispose();
			}

			pageGlyph = openPageGlyph = sectionGlyph = openSectionGlyph =
				sectionGroupGlyph = openSectionGroupGlyph = notebookGlyph = null;

			Logger.Current.Verbose($"favorite glyphs drawn in {color}");
			builtWith = color;
		}


		private static Image Pick(Favorite favorite, bool isOpen) =>
			!string.IsNullOrWhiteSpace(favorite.PageID)
				? isOpen
					? openPageGlyph ??= BuildOpenPageGlyph()
					: pageGlyph ??= BuildPageGlyph()
				: favorite.Kind switch
				{
					"notebook" => notebookGlyph ??= BuildNotebookGlyph(),
					"sectiongroup" => isOpen
						? openSectionGroupGlyph ??= BuildSectionGroupGlyph(true)
						: sectionGroupGlyph ??= BuildSectionGroupGlyph(false),
					_ => isOpen
						? openSectionGlyph ??= BuildSectionGlyph(true)
						: sectionGlyph ??= BuildSectionGlyph(false)
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
				using var pen = new Pen(color, PenWidth);

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


		// the page silhouette filled solid, with its text lines cut out so the row shows through
		private static Image BuildOpenPageGlyph()
		{
			var bitmap = NewCanvas(out var g);
			using (g)
			{
				var color = ThemeManager.Instance.GetColor("ControlText");
				using var pen = new Pen(color, PenWidth);
				using var brush = new SolidBrush(color);

				using var path = new GraphicsPath();
				path.AddLine(2, 1, 8, 1);
				path.AddLine(8, 1, 11, 4);
				path.AddLine(11, 4, 11, 13);
				path.AddLine(11, 13, 2, 13);
				path.CloseFigure();
				g.FillPath(brush, path);
				g.DrawPath(pen, path);

				g.SmoothingMode = SmoothingMode.None;
				g.CompositingMode = CompositingMode.SourceCopy;
				using var cut = new Pen(Color.Transparent);
				g.DrawLine(cut, 4, 7, 9, 7);
				g.DrawLine(cut, 4, 10, 9, 10);
			}

			return bitmap;
		}


		// open is drawn solid, like the open page
		private static Image BuildSectionGlyph(bool open)
		{
			var bitmap = NewCanvas(out var g);
			using (g)
			{
				var color = ThemeManager.Instance.GetColor("ControlText");
				using var pen = new Pen(color, PenWidth);
				using var brush = new SolidBrush(color);

				if (open)
				{
					g.FillRectangle(brush, 2, 3, 10, 10);
				}
				else
				{
					g.DrawRectangle(pen, 2, 3, 9, 9);
				}

				g.FillRectangle(brush, 4, 1, 6, 3);
			}

			return bitmap;
		}


		private static Image BuildSectionGroupGlyph(bool open)
		{
			var bitmap = NewCanvas(out var g);
			using (g)
			{
				var color = ThemeManager.Instance.GetColor("ControlText");
				using var pen = new Pen(color, PenWidth);

				using var path = new GraphicsPath();
				path.AddLine(2, 4, 5, 4);
				path.AddLine(5, 4, 6, 2);
				path.AddLine(6, 2, 10, 2);
				path.AddLine(10, 2, 10, 4);
				path.AddLine(10, 4, 12, 4);
				path.AddLine(12, 4, 12, 12);
				path.AddLine(12, 12, 2, 12);
				path.CloseFigure();

				if (open)
				{
					using var brush = new SolidBrush(color);
					g.FillPath(brush, path);
				}

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
				using var pen = new Pen(color, PenWidth);

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
