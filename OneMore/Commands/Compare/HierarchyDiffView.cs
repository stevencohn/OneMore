//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Identifies which side(s) of a diff row are selected.
	/// </summary>
	internal enum DiffSide
	{
		None,
		Left,
		Right,
		Both
	}


	/// <summary>
	/// Owner-drawn, scrollable dual-column tree control that renders a paired hierarchy diff:
	/// a left (source) name/timestamp cell, a center "well" that selects both sides at once,
	/// and a right (target) name/timestamp cell, one row per DiffNode. Rows below the root are
	/// indented per depth level; orphaned rows (present on only one side) render the missing
	/// side as a dashed "not present" cell.
	/// </summary>
	internal class HierarchyDiffView : Panel, ILoadControl
	{
		private sealed class Row
		{
			public DiffNode Node;
			public int Depth;
			public bool HasChildren;
		}

		private const int RowHeight = 36;
		private const int IndentWidth = 18;
		private const int WellWidth = 44;
		private const int ChipHeight = 16;
		private const int ChipPadX = 5;
		private const int CaretWidth = 14;
		private const int WellDiameter = 16;

		private readonly List<Row> rows = new();
		private readonly HashSet<DiffNode> expanded = new();
		private readonly Font monoFont;
		private readonly Font chipFont;

		private DiffNode root;
		private DiffNode selectedNode;
		private DiffSide selectedSide = DiffSide.None;
		private bool darkMode;

		private Color backColor;
		private Color sameFore;
		private Color mutedFore;
		private Color warningBack;
		private Color warningFill;
		private Color orphanBack;
		private Color orphanFore;
		private Color selectedBack;
		private Color selectedBorder;
		private Color borderColor;
		private Color successColor;


		public HierarchyDiffView()
		{
			SetStyle(
				ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
				true);

			AutoScroll = true;
			TabStop = true;

			monoFont = new Font("Consolas", Font.SizeInPoints);
			chipFont = new Font("Segoe UI", 6.5f, FontStyle.Bold, GraphicsUnit.Point);
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				monoFont?.Dispose();
				chipFont?.Dispose();
			}

			base.Dispose(disposing);
		}


		/// <summary>
		/// Raised whenever the current selection changes, either by clicking a name (one
		/// side) or the center well (both sides).
		/// </summary>
		public event EventHandler SelectionChanged;


		/// <summary>
		/// Gets the node underlying the current selection, or null if nothing is selected.
		/// </summary>
		public DiffNode SelectedNode => selectedNode;


		/// <summary>
		/// Gets which side(s) of the current selection are selected.
		/// </summary>
		public DiffSide SelectedSide => selectedSide;


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;

			darkMode = manager.DarkMode;
			backColor = manager.GetColor("Window");
			sameFore = manager.GetColor("ControlText");
			mutedFore = manager.GetColor("GrayText");
			warningBack = manager.GetColor("CompareWarningBack");
			warningFill = manager.GetColor("CompareWarningFill");
			orphanBack = manager.GetColor("CompareOrphanBack");
			orphanFore = manager.GetColor("ErrorText");
			selectedBack = manager.GetColor("LinkHighlight");
			selectedBorder = manager.GetColor("HotTrack");
			borderColor = manager.GetColor("ButtonBorder");
			successColor = manager.GetColor("SuccessFill");

			BackColor = backColor;
			Invalidate();
		}


		/// <summary>
		/// Sets the diff tree to display, fully expanded, clearing any current selection.
		/// </summary>
		/// <param name="root">The root DiffNode returned by HierarchyDiffBuilder.Build</param>
		public void SetRoot(DiffNode root)
		{
			this.root = root;
			expanded.Clear();
			selectedNode = null;
			selectedSide = DiffSide.None;

			ExpandAll(root);
			Rebuild();
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}


		private void ExpandAll(DiffNode node)
		{
			if (node is null || node.Children.Count == 0)
			{
				return;
			}

			expanded.Add(node);
			foreach (var child in node.Children)
			{
				ExpandAll(child);
			}
		}


		private void Rebuild()
		{
			rows.Clear();
			if (root is not null)
			{
				Flatten(root, 0);
			}

			AutoScrollMinSize = new Size(0, rows.Count * RowHeight);
			Invalidate();
		}


		private void Flatten(DiffNode node, int depth)
		{
			rows.Add(new Row { Node = node, Depth = depth, HasChildren = node.Children.Count > 0 });

			if (node.Children.Count > 0 && expanded.Contains(node))
			{
				foreach (var child in node.Children)
				{
					Flatten(child, depth + 1);
				}
			}
		}


		private int GetColumnWidth()
		{
			return Math.Max(0, (ClientSize.Width - WellWidth) / 2);
		}


		private void Select(DiffNode node, DiffSide side)
		{
			selectedNode = node;
			selectedSide = side;
			Invalidate();
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}


		private void ToggleExpand(DiffNode node)
		{
			if (!expanded.Remove(node))
			{
				expanded.Add(node);
			}

			Rebuild();
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Mouse handling...

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Focus();

			var y = e.Y - AutoScrollPosition.Y;
			var index = y / RowHeight;
			if (index < 0 || index >= rows.Count)
			{
				return;
			}

			var row = rows[index];
			var node = row.Node;
			var colWidth = GetColumnWidth();
			var indent = row.Depth * IndentWidth;

			if (e.X < colWidth)
			{
				if (node.Status == DiffStatus.OrphanRight)
				{
					// left side is the "not present" cell; nothing to click
					return;
				}

				if (row.HasChildren && e.X >= indent && e.X < indent + CaretWidth)
				{
					ToggleExpand(node);
					return;
				}

				Select(node, DiffSide.Left);
			}
			else if (e.X < colWidth + WellWidth)
			{
				if (node.Status is DiffStatus.Same or DiffStatus.DifferentTimestamps)
				{
					Select(node, DiffSide.Both);
				}
			}
			else
			{
				if (node.Status == DiffStatus.OrphanLeft)
				{
					// right side is the "not present" cell; nothing to click
					return;
				}

				var rightX = e.X - (colWidth + WellWidth);
				if (row.HasChildren && rightX >= indent && rightX < indent + CaretWidth)
				{
					ToggleExpand(node);
					return;
				}

				Select(node, DiffSide.Right);
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Keyboard handling...

		protected override bool IsInputKey(Keys keyData)
		{
			return keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(keyData);
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			switch (e.KeyCode)
			{
				case Keys.Up:
					MoveSelection(-1);
					e.Handled = true;
					break;

				case Keys.Down:
					MoveSelection(1);
					e.Handled = true;
					break;

				case Keys.Left:
					MoveHorizontal(toRight: false);
					e.Handled = true;
					break;

				case Keys.Right:
					MoveHorizontal(toRight: true);
					e.Handled = true;
					break;
			}
		}


		// Up/Down: moves to the previous/next visible row, trying to keep the current
		// selection scope (left/right/both) and falling back to whichever side(s) actually
		// exist on the row landed on (e.g. an orphan row only has one side to select)
		private void MoveSelection(int delta)
		{
			if (rows.Count == 0)
			{
				return;
			}

			var currentIndex = selectedNode is null ? -1 : rows.FindIndex(r => r.Node == selectedNode);
			var desiredSide = selectedSide == DiffSide.None ? DiffSide.Both : selectedSide;

			int nextIndex;
			if (currentIndex < 0)
			{
				nextIndex = delta > 0 ? 0 : rows.Count - 1;
			}
			else
			{
				nextIndex = currentIndex + delta;
				if (nextIndex < 0 || nextIndex >= rows.Count)
				{
					return;
				}
			}

			SelectWithFallback(rows[nextIndex].Node, desiredSide);
			EnsureRowVisible(nextIndex);
		}


		// Left/Right: walks the selection across a single row - one side, to the center
		// well (both), to the other side - matching the center well's click behavior; a
		// direction with nowhere further to go (already at an extreme, or the well/other
		// side doesn't exist because the row is an orphan) is a no-op
		private void MoveHorizontal(bool toRight)
		{
			if (selectedNode is null)
			{
				return;
			}

			var node = selectedNode;
			var hasLeft = node.Status != DiffStatus.OrphanRight;
			var hasRight = node.Status != DiffStatus.OrphanLeft;
			var canBoth = hasLeft && hasRight;

			if (toRight)
			{
				if (selectedSide == DiffSide.Left && canBoth)
				{
					Select(node, DiffSide.Both);
				}
				else if (selectedSide == DiffSide.Both && hasRight)
				{
					Select(node, DiffSide.Right);
				}
			}
			else
			{
				if (selectedSide == DiffSide.Right && canBoth)
				{
					Select(node, DiffSide.Both);
				}
				else if (selectedSide == DiffSide.Both && hasLeft)
				{
					Select(node, DiffSide.Left);
				}
			}
		}


		// Selects the given node with the desired side/scope, substituting whichever
		// side(s) actually exist when the desired one doesn't (e.g. desired Both on an
		// orphan row falls back to that row's one existing side)
		private void SelectWithFallback(DiffNode node, DiffSide desired)
		{
			var hasLeft = node.Status != DiffStatus.OrphanRight;
			var hasRight = node.Status != DiffStatus.OrphanLeft;

			DiffSide side;
			if (desired == DiffSide.Left)
			{
				side = hasLeft ? DiffSide.Left : DiffSide.Right;
			}
			else if (desired == DiffSide.Right)
			{
				side = hasRight ? DiffSide.Right : DiffSide.Left;
			}
			else
			{
				side = hasLeft && hasRight ? DiffSide.Both : hasLeft ? DiffSide.Left : DiffSide.Right;
			}

			Select(node, side);
		}


		private void EnsureRowVisible(int index)
		{
			var top = index * RowHeight;
			var bottom = top + RowHeight;

			var scrollY = -AutoScrollPosition.Y;
			var viewHeight = ClientSize.Height;

			if (top < scrollY)
			{
				AutoScrollPosition = new Point(0, top);
			}
			else if (bottom > scrollY + viewHeight)
			{
				AutoScrollPosition = new Point(0, bottom - viewHeight);
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Drawing...

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(backColor);

			if (rows.Count == 0)
			{
				return;
			}

			var colWidth = GetColumnWidth();

			g.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);

			for (var i = 0; i < rows.Count; i++)
			{
				DrawRow(g, rows[i], i * RowHeight, colWidth);
			}
		}


		private void DrawRow(Graphics g, Row row, int top, int colWidth)
		{
			var node = row.Node;
			var indent = row.Depth * IndentWidth;

			var leftRect = new Rectangle(0, top, colWidth, RowHeight);
			var wellRect = new Rectangle(colWidth, top, WellWidth, RowHeight);
			var rightRect = new Rectangle(colWidth + WellWidth, top, colWidth, RowHeight);

			if (node.Status == DiffStatus.DifferentTimestamps)
			{
				using var brush = new SolidBrush(warningBack);
				g.FillRectangle(brush, leftRect);
				g.FillRectangle(brush, rightRect);
			}
			else if (node.Status == DiffStatus.OrphanLeft)
			{
				using var brush = new SolidBrush(orphanBack);
				g.FillRectangle(brush, rightRect);
			}
			else if (node.Status == DiffStatus.OrphanRight)
			{
				using var brush = new SolidBrush(orphanBack);
				g.FillRectangle(brush, leftRect);
			}

			var isSelected = ReferenceEquals(selectedNode, node);
			if (isSelected && selectedSide is DiffSide.Left or DiffSide.Both)
			{
				DrawSelectedOverlay(g, leftRect);
			}
			if (isSelected && selectedSide is DiffSide.Right or DiffSide.Both)
			{
				DrawSelectedOverlay(g, rightRect);
			}

			var showExpanded = expanded.Contains(node);

			if (node.Status == DiffStatus.OrphanRight)
			{
				DrawNotPresent(g, leftRect);
			}
			else
			{
				DrawCell(g, leftRect, indent, row.HasChildren, showExpanded, node.NodeType,
					node.Name, node.LeftModified);
			}

			if (node.Status == DiffStatus.OrphanLeft)
			{
				DrawNotPresent(g, rightRect);
			}
			else
			{
				DrawCell(g, rightRect, indent, row.HasChildren, showExpanded, node.NodeType,
					node.Name, node.RightModified);
			}

			DrawWell(g, wellRect, node, isSelected && selectedSide == DiffSide.Both);

			using var pen = new Pen(borderColor);
			g.DrawLine(pen, 0, top + RowHeight - 1, rightRect.Right, top + RowHeight - 1);
		}


		private void DrawCell(Graphics g, Rectangle rect, int indent, bool hasChildren,
			bool showExpanded, OneNote.NodeType nodeType, string name, DateTime? modified)
		{
			var x = rect.X + indent;

			if (hasChildren)
			{
				DrawCaret(g, x, rect.Y, showExpanded);
			}

			x += CaretWidth;

			var chipWidth = DrawTypeChip(g, x, rect.Y + (rect.Height - ChipHeight) / 2, nodeType);
			x += chipWidth + 6;

			using var brush = new SolidBrush(sameFore);
			var y = rect.Y + (rect.Height - Font.Height) / 2;
			g.DrawString(name, Font, brush, x, y);

			if (modified.HasValue)
			{
				var text = modified.Value.ToShortFriendlyString();
				var size = g.MeasureString(text, monoFont);
				using var timeBrush = new SolidBrush(mutedFore);
				g.DrawString(text, monoFont, timeBrush,
					rect.Right - size.Width - 10,
					rect.Y + (rect.Height - monoFont.Height) / 2);
			}
		}


		private void DrawNotPresent(Graphics g, Rectangle rect)
		{
			using var italic = new Font(Font, FontStyle.Italic);
			using var brush = new SolidBrush(orphanFore);
			var y = rect.Y + (rect.Height - italic.Height) / 2;
			g.DrawString(Resx.CompareDialog_notPresent, italic, brush, rect.X + 10, y);
		}


		private void DrawCaret(Graphics g, int x, int y, bool expandedFlag)
		{
			var cx = x + (CaretWidth / 2);
			var cy = y + (RowHeight / 2);

			using var brush = new SolidBrush(mutedFore);
			var points = expandedFlag
				? new[] { new Point(cx - 4, cy - 2), new Point(cx + 4, cy - 2), new Point(cx, cy + 4) }
				: new[] { new Point(cx - 2, cy - 4), new Point(cx - 2, cy + 4), new Point(cx + 4, cy) };

			g.FillPolygon(brush, points);
		}


		// Matches the [N]/[SG]/[S]/[P] type-chip badge used in the Search Titles results
		// card view (TitleHitPalette), for visual consistency between the two features.
		private int DrawTypeChip(Graphics g, int x, int y, OneNote.NodeType nodeType)
		{
			var level = ToHitLevel(nodeType);
			var code = TitleHitPalette.GetCode(level);

			// GDI+ (MeasureString/DrawString) throughout, not TextRenderer: TextRenderer is
			// GDI-based and does not reliably honor OnPaint's TranslateTransform, so once
			// scrolled the chip fill (GDI+) moved but its glyph (GDI) stayed put/vanished
			var codeSize = g.MeasureString(code, chipFont);
			var chipWidth = (int)Math.Max(ChipHeight, codeSize.Width + (ChipPadX * 2));
			var chipRect = new Rectangle(x, y, chipWidth, ChipHeight);

			using var brush = new SolidBrush(TitleHitPalette.GetColor(level, darkMode));
			g.FillRoundedRectangle(brush, chipRect, 3);

			using var textBrush = new SolidBrush(TitleHitPalette.GetGlyphColor(darkMode));
			g.DrawString(code, chipFont, textBrush,
				chipRect.X + ((chipRect.Width - codeSize.Width) / 2f),
				chipRect.Y + ((chipRect.Height - codeSize.Height) / 2f));

			return chipWidth;
		}


		private static TitleHitLevel ToHitLevel(OneNote.NodeType type)
		{
			return type switch
			{
				OneNote.NodeType.Notebook => TitleHitLevel.Notebook,
				OneNote.NodeType.SectionGroup => TitleHitLevel.SectionGroup,
				OneNote.NodeType.Section => TitleHitLevel.Section,
				_ => TitleHitLevel.Page
			};
		}


		private void DrawSelectedOverlay(Graphics g, Rectangle rect)
		{
			using var brush = new SolidBrush(selectedBack);
			g.FillRectangle(brush, rect);

			using var pen = new Pen(selectedBorder, 2);
			var inset = rect;
			inset.Inflate(-1, -1);
			g.DrawRectangle(pen, inset);
		}


		private void DrawWell(Graphics g, Rectangle rect, DiffNode node, bool bothSelected)
		{
			var wellRect = new Rectangle(
				rect.X + ((rect.Width - WellDiameter) / 2),
				rect.Y + ((rect.Height - WellDiameter) / 2),
				WellDiameter, WellDiameter);

			if (node.Status is DiffStatus.OrphanLeft or DiffStatus.OrphanRight)
			{
				// Dot (not Dash) + a round cap: long flat-capped dashes tile unevenly and
				// look chunky/choppy around a small-radius circle at this pen width
				using var pen = new Pen(orphanFore, 2f)
				{
					DashStyle = DashStyle.Dot,
					DashCap = DashCap.Round
				};
				g.DrawEllipse(pen, wellRect);
				return;
			}

			if (bothSelected)
			{
				using var brush = new SolidBrush(selectedBorder);
				g.FillEllipse(brush, wellRect);
				return;
			}

			if (node.Status == DiffStatus.DifferentTimestamps)
			{
				using var brush = new SolidBrush(warningFill);
				g.FillEllipse(brush, wellRect);
				return;
			}

			DrawEqualsGlyph(g, wellRect, successColor);
		}


		private static void DrawEqualsGlyph(Graphics g, Rectangle rect, Color color)
		{
			using var pen = new Pen(color, 2f);
			var cx = rect.X + (rect.Width / 2f);
			var cy = rect.Y + (rect.Height / 2f);
			var halfWidth = (rect.Width / 2f) - 3;

			g.DrawLine(pen, cx - halfWidth, cy - 3, cx + halfWidth, cy - 3);
			g.DrawLine(pen, cx - halfWidth, cy + 3, cx + halfWidth, cy + 3);
		}
	}
}
