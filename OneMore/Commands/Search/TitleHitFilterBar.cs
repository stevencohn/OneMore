//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	/// <summary>
	/// A single-select row of pill-shaped filter chips ("All N", "N n", "SG n", "S n", "P n")
	/// used by SearchTitleDialog to narrow its results by TitleHitLevel. Purely a local,
	/// in-memory filter over cards already fetched - selecting a chip never re-runs a search.
	/// </summary>
	internal sealed class TitleHitFilterBar : MoreUserControl
	{
		private const int PillHeight = 22;
		private const int PillGap = 6;
		private const int PillPadX = 9;
		private const int BadgeSize = 15;
		private const int BadgePadX = 5;   // horizontal padding either side of a multi-letter code (e.g. "SG")
		private const int BadgeGap = 5;
		private const int CornerRadius = 11;


		private sealed class Pill
		{
			public TitleHitLevel? Level;
			public string Label;
			public Rectangle Bounds;
			public int BadgeWidth;
		}


		private readonly List<Pill> pills = new();
		private TitleHitLevel? selectedLevel;
		private Pill hoveredPill;
		private Font font;


		public TitleHitFilterBar()
		{
			SetStyle(
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint, true);

			Height = 30;
		}


		public event EventHandler FilterChanged;


		/// <summary>
		/// The currently selected level, or null when "All" is selected.
		/// </summary>
		public TitleHitLevel? SelectedLevel => selectedLevel;


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (font == null) OnThemeChange();
		}


		public override void OnThemeChange()
		{
			base.OnThemeChange();
			font?.Dispose();
			font = new Font("Segoe UI", 8f, FontStyle.Bold, GraphicsUnit.Point);
			Invalidate();
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				font?.Dispose();
			}
			base.Dispose(disposing);
		}


		/// <summary>
		/// Rebuilds the pill list from live counts and resets the selection to "All". Hides the
		/// Notebook pill entirely when showNotebookChip is false (search scoped to one notebook,
		/// where the notebook itself isn't a meaningful match target).
		/// </summary>
		public void SetCounts(int all, IReadOnlyDictionary<TitleHitLevel, int> counts, bool showNotebookChip)
		{
			selectedLevel = null;
			hoveredPill = null;
			pills.Clear();
			pills.Add(new Pill { Level = null, Label = $"All {all}" });

			void AddPill(TitleHitLevel level)
			{
				counts.TryGetValue(level, out var n);
				pills.Add(new Pill
				{
					Level = level,
					Label = $"{TitleHitPalette.GetCode(level)} {n}"
				});
			}

			if (showNotebookChip) AddPill(TitleHitLevel.Notebook);
			AddPill(TitleHitLevel.SectionGroup);
			AddPill(TitleHitLevel.Section);
			AddPill(TitleHitLevel.Page);

			LayoutPills();
			Invalidate();
		}


		public void Clear()
		{
			pills.Clear();
			selectedLevel = null;
			hoveredPill = null;
			Invalidate();
		}


		private void LayoutPills()
		{
			var x = 2;
			var y = (Height - PillHeight) / 2;
			if (y < 0) y = 0;

			foreach (var pill in pills)
			{
				var textSize = TextRenderer.MeasureText(pill.Label, font ?? Font);
				var width = PillPadX * 2 + textSize.Width;

				if (pill.Level.HasValue)
				{
					var codeSize = TextRenderer.MeasureText(TitleHitPalette.GetCode(pill.Level.Value), font ?? Font);
					pill.BadgeWidth = Math.Max(BadgeSize, codeSize.Width + BadgePadX * 2);
					width += pill.BadgeWidth + BadgeGap;
				}

				pill.Bounds = new Rectangle(x, y, width, PillHeight);
				x += width + PillGap;
			}
		}


		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			LayoutPills();
			Invalidate();
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (pills.Count == 0) return;

			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			var activeBack = manager.GetColor("Highlight");
			var activeFore = manager.GetColor("HighlightText");
			var inactiveBack = manager.GetColor("ControlLight");
			var inactiveFore = manager.GetColor("ControlText");

			const TextFormatFlags flags =
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.NoPrefix |
				TextFormatFlags.SingleLine;

			foreach (var pill in pills)
			{
				var active = pill.Level == selectedLevel;

				using (var back = new SolidBrush(active ? activeBack : inactiveBack))
				{
					e.Graphics.FillRoundedRectangle(back, pill.Bounds, CornerRadius);
				}

				var fore = active ? activeFore : inactiveFore;
				var textX = pill.Bounds.X + PillPadX;

				if (pill.Level.HasValue)
				{
					var badgeRect = new Rectangle(
						textX, pill.Bounds.Y + (PillHeight - BadgeSize) / 2, pill.BadgeWidth, BadgeSize);

					using (var badgeBrush = new SolidBrush(TitleHitPalette.GetColor(pill.Level.Value, manager.DarkMode)))
					{
						e.Graphics.FillRoundedRectangle(badgeBrush, badgeRect, 3);
					}

					TextRenderer.DrawText(
						e.Graphics, TitleHitPalette.GetCode(pill.Level.Value), font ?? Font,
						badgeRect, TitleHitPalette.GetGlyphColor(manager.DarkMode), flags);

					textX += pill.BadgeWidth + BadgeGap;
				}

				var textRect = new Rectangle(
					textX, pill.Bounds.Y, pill.Bounds.Right - textX, PillHeight);

				TextRenderer.DrawText(e.Graphics, pill.Label, font ?? Font, textRect, fore,
					TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

				if (!active && pill == hoveredPill)
				{
					var outline = new Rectangle(
						pill.Bounds.X, pill.Bounds.Y, pill.Bounds.Width - 1, pill.Bounds.Height - 1);

					using var pen = new Pen(manager.ButtonHotBorder);
					e.Graphics.DrawRoundedRectangle(pen, outline, CornerRadius);
				}
			}
		}


		private Pill HitTest(Point location) =>
			pills.FirstOrDefault(p => p.Bounds.Contains(location));


		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			if (e.Button != MouseButtons.Left) return;

			var pill = HitTest(e.Location);
			if (pill == null || pill.Level == selectedLevel) return;

			selectedLevel = pill.Level;
			Invalidate();
			FilterChanged?.Invoke(this, EventArgs.Empty);
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var pill = HitTest(e.Location);
			Cursor = pill != null ? Cursors.Hand : Cursors.Default;

			if (pill != hoveredPill)
			{
				hoveredPill = pill;
				Invalidate();
			}
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			Cursor = Cursors.Default;

			if (hoveredPill != null)
			{
				hoveredPill = null;
				Invalidate();
			}
		}
	}
}
