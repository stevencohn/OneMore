//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	/// <summary>
	/// A plain owner-drawn ListView with OneMore theming (header and selection colors,
	/// correct in both Light and Dark mode), fill-to-width columns, and manual (non-OLE)
	/// drag/drop reordering with a live insertion-line indicator.
	/// </summary>
	/// <remarks>
	/// Unlike MoreListView, this never hosts a child control per cell - it only customizes
	/// painting (DrawSubItem), so input handling (click, double-click, multi-select) stays
	/// fully native, and it has none of MoreListView's "a row with no hosted control never
	/// gets painted" failure mode. Its drag/drop is implemented via plain mouse tracking
	/// rather than AllowDrop/DoDragDrop (OLE), which requires an STA thread; this add-in's
	/// COM surrogate runs in MTA (see docs\developers\TechNote - COM Surrogate.htm), so a
	/// mouse-tracked drag avoids that requirement entirely.
	/// </remarks>
	internal class MoreListView : ListView
	{
		/// <summary>
		/// Per-cell rendering overrides returned by GetCellStyle.
		/// </summary>
		public readonly struct CellStyle
		{
			public static readonly CellStyle Default = new(0, false, null);

			public CellStyle(int indent, bool muted, string foreColorKey = null)
			{
				Indent = indent;
				Muted = muted;
				ForeColorKey = foreColorKey;
			}

			/// <summary>
			/// Extra left padding, in pixels, used to visually nest a row under another.
			/// </summary>
			public int Indent { get; }

			/// <summary>
			/// True to render this cell's text in a muted (gray, italic) style, e.g. for
			/// placeholder/hint text.
			/// </summary>
			public bool Muted { get; }

			/// <summary>
			/// Optional ThemeManager color key overriding the normal/muted foreground color,
			/// e.g. "ErrorText". Ignored while the row is selected.
			/// </summary>
			public string ForeColorKey { get; }
		}


		/// <summary>
		/// Raised after an item has been moved via drag/drop, once the ListView's own
		/// Items collection has already been updated to reflect the new position.
		/// </summary>
		public sealed class ItemMovedEventArgs : EventArgs
		{
			public ItemMovedEventArgs(ListViewItem item, ListViewItem precedingItem, bool movedToEnd)
			{
				Item = item;
				PrecedingItem = precedingItem;
				MovedToEnd = movedToEnd;
			}

			/// <summary>
			/// The item that was moved.
			/// </summary>
			public ListViewItem Item { get; }

			/// <summary>
			/// The item now immediately preceding the moved item, or null if it is now the
			/// first item in the list.
			/// </summary>
			public ListViewItem PrecedingItem { get; }

			/// <summary>
			/// True if the item was dropped into the empty space below the last row, rather
			/// than next to a specific item. Kept distinct from inspecting PrecedingItem
			/// because "dropped below everything" should mean a fixed, unambiguous target
			/// (e.g. root) regardless of what row happens to be last.
			/// </summary>
			public bool MovedToEnd { get; }
		}


		private const int MinimumColumnWidth = 50;

		private readonly ThemeManager manager;
		private float[] columnProportions;
		private bool resizingColumns;

		// manual (non-OLE) drag/drop state
		private bool isDragging;
		private ListViewItem dragItem;
		private Point dragStartPoint;
		private int insertionIndex = -1;
		private bool insertAtEnd;


		public MoreListView()
		{
			manager = ThemeManager.Instance;

			View = View.Details;
			FullRowSelect = true;
			HideSelection = false;
			OwnerDraw = true;

			// prevent flickering, especially while dragging an item
			SetStyle(
				ControlStyles.DoubleBuffer |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.AllPaintingInWmPaint, true);

			// Never explicitly assign BackColor at design time: the VS designer would bake
			// the (always-light) snapshot into the consumer's InitializeComponent the next
			// time its Designer.cs is opened and saved (ShouldSerializeBackColor/ResetBackColor
			// are NOT honored for Control.BackColor on a subclass, confirmed empirically).
			if (!ThemeManager.IsDesignTime)
			{
				BackColor = manager.GetColor("ListView");
			}

			DrawColumnHeader += OnDrawColumnHeader;
			DrawItem += OnDrawItem;
			DrawSubItem += OnDrawSubItem;
			Resize += (s, e) => ResizeColumns();
			ColumnWidthChanged += OnColumnWidthChanged;
			MouseDown += OnMouseDown;
			MouseMove += OnMouseMove;
			MouseUp += OnMouseUp;
		}


		/// <summary>
		/// .NET's ListView never forwards BackColor to the native control, so owner-drawing
		/// only colors the rows themselves; the blank area below the last item is otherwise
		/// left painted with the OS default (white). LVM_SETBKCOLOR fixes that fill.
		/// </summary>
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			Native.SendMessage(Handle, Native.LVM_SETBKCOLOR, 0, ColorTranslator.ToWin32(BackColor));
		}


		protected override void OnBackColorChanged(EventArgs e)
		{
			base.OnBackColorChanged(e);
			if (IsHandleCreated)
			{
				Native.SendMessage(Handle, Native.LVM_SETBKCOLOR, 0, ColorTranslator.ToWin32(BackColor));
			}
		}


		/// <summary>
		/// Gets or sets a callback that supplies per-cell rendering overrides (indent,
		/// muted styling). When null, every cell renders with CellStyle.Default.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<ListViewItem, int, CellStyle> GetCellStyle { get; set; }


		/// <summary>
		/// Gets or sets the ThemeManager color key used to fill the background of a
		/// selected row. Defaults to "Highlight" (the normal, strong selection color);
		/// override with something subtler, e.g. "LinkHighlight", when rows host their
		/// own checkbox/icon glyphs that need to stay legible against the selection fill.
		/// </summary>
		public string SelectedBackColorKey { get; set; } = "Highlight";


		/// <summary>
		/// Gets or sets the ThemeManager color key used for a selected row's text.
		/// Defaults to "HighlightText" (matching the default "Highlight" background).
		/// </summary>
		public string SelectedForeColorKey { get; set; } = "HighlightText";


		/// <summary>
		/// Gets or sets a callback that supplies an optional leading icon to draw at the
		/// start of a cell, before its text, shifting the text right to make room. When
		/// null, or when the callback returns null for a given cell, no icon is drawn.
		/// Unlike SmallImageList/ImageIndex, this is invoked on every paint, so it can
		/// return a different image depending on the item's current Selected state.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<ListViewItem, int, Image> GetCellImage { get; set; }


		/// <summary>
		/// Gets or sets a predicate determining whether the given item can be dragged.
		/// When null, no item is draggable.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<ListViewItem, bool> CanDragItem { get; set; }


		/// <summary>
		/// Gets or sets a predicate determining whether the given item is an "anchor" row
		/// (e.g. a group header) that cannot itself be reordered and onto which a drop
		/// always means "insert immediately after this row", regardless of where within
		/// the row the cursor is. When null, no item is treated as an anchor.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<ListViewItem, bool> IsInsertionAnchor { get; set; }


		/// <summary>
		/// Raised after a drag/drop reorder completes.
		/// </summary>
		public event EventHandler<ItemMovedEventArgs> ItemMoved;


		/// <summary>
		/// Sets the proportional width of each column (e.g. 0.4f, 0.6f for a 40/60 split)
		/// and immediately applies it. The values don't need to sum to 1; only their
		/// relative magnitude matters. Must be called after all columns have been added.
		/// </summary>
		public void SetColumnProportions(params float[] proportions)
		{
			columnProportions = proportions;
			ResizeColumns();
		}


		private void ResizeColumns()
		{
			var width = ClientSize.Width;
			if (width <= 0 || columnProportions == null || Columns.Count != columnProportions.Length)
			{
				return;
			}

			var total = columnProportions.Sum();
			if (total <= 0)
			{
				return;
			}

			resizingColumns = true;

			var used = 0;
			for (var i = 0; i < columnProportions.Length - 1; i++)
			{
				var columnWidth = (int)(width * (columnProportions[i] / total));
				Columns[i].Width = columnWidth;
				used += columnWidth;
			}

			Columns[columnProportions.Length - 1].Width = Math.Max(MinimumColumnWidth, width - used);

			resizingColumns = false;
		}


		/// <summary>
		/// Keeps all columns summing to the full client width whenever the user drags a
		/// column divider: the last column is recalculated to fill whatever's left, so
		/// there's never a gap or overflow after it.
		/// </summary>
		private void OnColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			if (resizingColumns || e.ColumnIndex == Columns.Count - 1)
			{
				return;
			}

			var width = ClientSize.Width;
			if (width <= 0)
			{
				return;
			}

			var used = 0;
			for (var i = 0; i < Columns.Count - 1; i++)
			{
				used += Columns[i].Width;
			}

			resizingColumns = true;
			Columns[Columns.Count - 1].Width = Math.Max(MinimumColumnWidth, width - used);
			resizingColumns = false;
		}


		private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			using var backBrush = new SolidBrush(manager.GetColor("ControlDarkDark"));
			e.Graphics.FillRectangle(backBrush, e.Bounds);

			var bounds = e.Bounds;
			bounds.X += 4;

			TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, bounds,
				manager.GetColor("DarkText"),
				TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
		}


		private void OnDrawItem(object sender, DrawListViewItemEventArgs e)
		{
			// no-op; OnDrawSubItem (below) handles all rendering for every column
		}


		private void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			var selected = e.Item.Selected;
			var style = GetCellStyle?.Invoke(e.Item, e.ColumnIndex) ?? CellStyle.Default;

			using (var backBrush = new SolidBrush(manager.GetColor(selected ? SelectedBackColorKey : "ListView")))
			{
				e.Graphics.FillRectangle(backBrush, e.Bounds);
			}

			var foreColor = selected
				? manager.GetColor(SelectedForeColorKey)
				: style.ForeColorKey != null
					? manager.GetColor(style.ForeColorKey)
					: style.Muted ? manager.GetColor("GrayText") : manager.GetColor("ControlText");

			var bounds = e.Bounds;
			if (style.Indent > 0)
			{
				bounds.X += style.Indent;
				bounds.Width -= style.Indent;
			}

			var image = GetCellImage?.Invoke(e.Item, e.ColumnIndex);
			if (image != null)
			{
				e.Graphics.DrawImage(image, bounds.X + 2, bounds.Top + ((bounds.Height - image.Height) / 2));

				var advance = image.Width + 6;
				bounds.X += advance;
				bounds.Width -= advance;
			}

			const TextFormatFlags flags =
				TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;

			if (style.Muted)
			{
				using var italic = new Font(e.Item.Font, FontStyle.Italic);
				TextRenderer.DrawText(e.Graphics, e.SubItem.Text, italic, bounds, foreColor, flags);
			}
			else
			{
				TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, bounds, foreColor, flags);
			}

			if (isDragging)
			{
				using var lineBrush = new SolidBrush(manager.GetColor("Highlight"));
				if (e.ItemIndex == insertionIndex)
				{
					e.Graphics.FillRectangle(lineBrush, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, 2);
				}
				else if (insertAtEnd && e.ItemIndex == Items.Count - 1)
				{
					e.Graphics.FillRectangle(lineBrush, e.Bounds.Left, e.Bounds.Bottom - 2, e.Bounds.Width, 2);
				}
			}
		}


		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			var hit = HitTest(e.Location);
			dragItem = hit.Item != null && (CanDragItem?.Invoke(hit.Item) ?? false) ? hit.Item : null;
			dragStartPoint = e.Location;
		}


		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (dragItem == null || e.Button != MouseButtons.Left)
			{
				return;
			}

			if (!isDragging)
			{
				var size = SystemInformation.DragSize;
				if (Math.Abs(e.X - dragStartPoint.X) < size.Width &&
					Math.Abs(e.Y - dragStartPoint.Y) < size.Height)
				{
					return;
				}

				isDragging = true;
				Cursor = Cursors.Hand;
			}

			ComputeInsertionPoint(e.Location);
			Invalidate();
		}


		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			if (isDragging)
			{
				PerformDrop();
			}

			isDragging = false;
			dragItem = null;
			insertionIndex = -1;
			insertAtEnd = false;
			Cursor = Cursors.Default;
			Invalidate();
		}


		private void ComputeInsertionPoint(Point point)
		{
			insertAtEnd = false;
			insertionIndex = -1;

			var hit = HitTest(point);
			if (hit.Item == null)
			{
				var lastItem = Items.Count > 0 ? Items[Items.Count - 1] : null;
				if (lastItem != null && point.Y >= lastItem.Bounds.Bottom)
				{
					insertionIndex = Items.Count;
					insertAtEnd = true;
				}

				return;
			}

			if (IsInsertionAnchor?.Invoke(hit.Item) ?? false)
			{
				// anchor rows are never reordered; dropping anywhere on one always means
				// "become its first child"
				insertionIndex = hit.Item.Index + 1;
				return;
			}

			var midY = hit.Item.Bounds.Top + (hit.Item.Bounds.Height / 2);
			insertionIndex = point.Y < midY ? hit.Item.Index : hit.Item.Index + 1;
		}


		private void PerformDrop()
		{
			if (insertionIndex < 0 || dragItem == null)
			{
				return;
			}

			var precedingItem = !insertAtEnd && insertionIndex > 0 ? Items[insertionIndex - 1] : null;

			var originalIndex = dragItem.Index;
			var adjustedIndex = insertionIndex > originalIndex ? insertionIndex - 1 : insertionIndex;

			BeginUpdate();
			Items.Remove(dragItem);
			Items.Insert(adjustedIndex, dragItem);
			EndUpdate();

			dragItem.Selected = true;

			ItemMoved?.Invoke(this, new ItemMovedEventArgs(dragItem, precedingItem, insertAtEnd));
		}
	}
}
