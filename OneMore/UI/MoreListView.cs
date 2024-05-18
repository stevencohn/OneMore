//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Drawing;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	/// <summary>
	/// A ListView in Detail mode that can host a user control in each cell of grid.
	/// </summary>
	internal class MoreListView : ListView
	{
		#region Private types

		private sealed class HostedControl
		{
			// the hosting item/subitem
			public IMoreHostItem Host { get; private set; }
			// column index of this item/subitem
			public int ColumnIndex { get; private set; }
			// the hosted user control
			public Control Control { get; private set; }
			public HostedControl(
				IMoreHostItem host, IMoreHostItem item, Control control, int columnIndex)
			{
				Host = host;
				Control = control;
				// commandeer Tag to point back up to hosting item
				control.Tag = item;
				ColumnIndex = columnIndex;
			}
		}

		/// <summary>
		/// For internal use only.
		/// </summary>
		public sealed class RegistrationEventArgs
		{
			public IMoreHostItem Item { get; private set; }
			public Control Control { get; private set; }
			public int ColumnIndex { get; private set; }
			public RegistrationEventArgs(IMoreHostItem item, Control control, int columnIndex)
			{
				Item = item;
				Control = control;
				ColumnIndex = columnIndex;
			}
		}

		/// <summary>
		/// For internal use only.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void RegistrationEventHandler(IMoreHostItem sender, RegistrationEventArgs e);

		#endregion Private types


		private const string UpGlyph = "△"; // ▲
		private const string DnGlyph = "▽"; // ▼

		private bool allowItemReorder;
		private SolidBrush sortedBrush;
		private SolidBrush highBackBrush;
		private SolidBrush highForeBrush;
		private readonly List<HostedControl> hostedControls;
		private readonly EventRouter router;


		/// <summary>
		/// Initialize a new (readonly) detail list view with default selection styling.
		/// </summary>
		public MoreListView()
		{
			View = View.Details;

			router = new EventRouter();
			hostedControls = new List<HostedControl>();
			highBackBrush = new SolidBrush(ColorTranslator.FromHtml("#D7C1FF"));
			highForeBrush = new SolidBrush(SystemColors.HighlightText);
			sortedBrush = new SolidBrush(BackColor);

			// TODO: ?
			FullRowSelect = true;

			// prevent flickering
			SetStyle(
				ControlStyles.DoubleBuffer |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.AllPaintingInWmPaint, true);

			var (_, yScaling) = UI.Scaling.GetScalingFactors();
			RowHeight = (int)(16 * yScaling);

			Click += RouteClick;

			router.Register(this, "Click");
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				router.Dispose();
				highBackBrush.Dispose();
				highForeBrush.Dispose();
				sortedBrush.Dispose();
			}
		}


		/// <summary>
		/// Gets or sets whether items can be reordered using drag and drop.
		/// </summary>
		[Category("Behavior")]
		[Description("Specifies whether items can be reordered using drag and drop")]
		public bool AllowItemReorder
		{
			get => allowItemReorder;

			set
			{
				allowItemReorder = value;
				if (value)
				{
					MouseDown += RouteMouseDown;
					DragDrop += RouteDragDrop;

					router.Register(this, "MouseDown");
					router.Register(this, "DragDrop");
				}
			}
		}


		/// <summary>
		/// Gets or set the padding around hosted controls in each cell of the list view.
		/// </summary>
		[Category("Appearance")]
		[Description("The padding around hosted controls")]
		public int ControlPadding { get; set; } = 2;


		/// <summary>
		/// Gets or sets the background fill color of selected rows
		/// </summary>
		public Color HighlightBackground
		{
			get => highBackBrush.Color;
			set => highBackBrush = new SolidBrush(value);
		}


		/// <summary>
		/// Gets or sets the foreground color of selected rows
		/// </summary>
		public Color HighlightForeground
		{
			get => highForeBrush.Color;
			set => highForeBrush = new SolidBrush(value);
		}


		/// <summary>
		/// Sets the row height in Details view
		/// This property appears in the Visual Studio Form Designer
		/// </summary>
		[Category("Appearance")]
		[Description("Sets height of rows in Details view, in pixels")]
		public int RowHeight { get; set; }


		/// <summary>
		/// Gets or set the background fill color of cells in the sorted column.
		/// </summary>
		public Color SortedBackground
		{
			get => sortedBrush.Color;
			set => sortedBrush = new SolidBrush(value);
		}


		/// <summary>
		/// Creates a new item with a hosted user control.
		/// </summary>
		/// <param name="control">The control to display in the first column of the item</param>
		/// <returns>The newly created item, already added to the list view</returns>
		public MoreHostedListViewItem AddHostedItem(Control control)
		{
			return AddHostedItem(string.Empty, control);
		}


		/// <summary>
		/// Creates a new item displaying standard text and optionally with a hosted control;
		/// the control will supercede the text.
		/// </summary>
		/// <param name="text">The text to display in the item.</param>
		/// <param name="control">The user control to host in the item</param>
		/// <returns>The newly created item, already added to the list view.</returns>
		public MoreHostedListViewItem AddHostedItem(string text, Control control = null)
		{
			var item = new MoreHostedListViewItem(text, control);
			item.Registering += new RegistrationEventHandler(RegisterHostedControl);
			Items.Add(item);

			if (control != null)
			{
				RegisterHostedControl(item, new RegistrationEventArgs(item, control, 0));
			}

			return item;
		}


		private void RegisterHostedControl(IMoreHostItem subitem, RegistrationEventArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => { RegisterHostedControl(subitem, e); }));
				return;
			}

			hostedControls.Add(new HostedControl(subitem, e.Item, e.Control, e.ColumnIndex));
			Controls.Add(e.Control);
		}


		public void EnableItemEventBubbling()
		{
			router.Register(this, Items, "Click");

			if (allowItemReorder)
			{
				router.Register(this, Items, "MouseDown");
				router.Register(this, Items, "DragDrop");
			}
		}


		/// <summary>
		/// Returns a collection of selected T objects from the specified column in the list view.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="columnIndex"></param>
		/// <returns></returns>
		public IEnumerable<T> GetSelectedItems<T>(int columnIndex)
		{
			if (SelectedItems.Count == 0)
			{
				yield return default;
			}

			for (int i = 0; i < SelectedItems.Count; i++)
			{
				if (SelectedItems[i] is MoreHostedListViewItem item &&
					item.Control is T thing)
				{
					yield return thing;
				}
			}
		}


		/// <summary>
		/// Select the specified item if it is not already selected. If it is not selected
		/// then other selected items will be deselected before this item is selected.
		/// </summary>
		/// <param name="item">The item to select</param>
		/// <remarks>
		/// This should be used when you want to programmaticaly for a selection of the
		/// row when the user interacts with a hosted control, like a Button or LinkLabel
		/// which would not normally select that row automatically.
		/// </remarks>
		public void SelectIf(ListViewItem item)
		{
			if (!item.Selected)
			{
				SelectedItems.Clear();
				item.Selected = true;
			}
		}

		#region Routed events
		private void RouteClick(object sender, EventArgs e)
		{
			var point = PointToClient(Control.MousePosition);
			var item = GetItemAt(point.X, point.Y);
			if (item != null)
			{
				SelectImplicitly(item);
			}
		}


		private void SelectImplicitly(ListViewItem item)
		{
			if (ModifierKeys.HasFlag(Keys.Control))
			{
				// ctlr-click individuals
				item.Selected = !item.Selected;
				if (item.Selected)
				{
					FocusedItem = item;
				}
			}
			else if (ModifierKeys.HasFlag(Keys.Shift))
			{
				// shift-click range
				if (SelectedIndices.Count > 0)
				{
					if (SelectedIndices[0] == item.Index)
					{
						item.Selected = true;
						FocusedItem = item;
					}
					else
					{
						(int first, int last) = SelectedIndices[0] < item.Index
							? (SelectedIndices[0], item.Index)
							: (item.Index, SelectedIndices[SelectedIndices.Count - 1]);

						BeginUpdate();
						SelectedItems.Clear();
						for (var i = first; i <= last; i++)
						{
							Items[i].Selected = true;
						}

						FocusedItem = SelectedItems[first];
						EndUpdate();
					}
				}
				else
				{
					item.Selected = true;
					FocusedItem = item;
				}
			}
			else
			{
				// single-click
				BeginUpdate();
				SelectedItems.Clear();
				item.Selected = !item.Selected;
				if (item.Selected)
				{
					FocusedItem = item;
				}
				EndUpdate();
			}

			Focus();
		}


		private void RouteMouseDown(object sender, MouseEventArgs e)
		{
			Logger.Current.Verbose($"drag start for {sender.GetType().FullName}");
			DoDragDrop(sender, DragDropEffects.Move);
		}


		private void RouteDragDrop(object sender, DragEventArgs e)
		{
			Logger.Current.Verbose($"drop");
		}
		#endregion Routed events

		#region Overrides

		protected override CreateParams CreateParams
		{
			get
			{
				var p = base.CreateParams;
				p.Style |= Native.LVS_OWNERDRAWFIXED;
				return p;
			}
		}


		protected override void OnColumnClick(ColumnClickEventArgs e)
		{
			base.OnColumnClick(e);
			if (Columns[e.Column] is MoreColumnHeader column && column.Sortable)
			{
				// clear glyph from another sorted column
				if (column.SortOrder == SortOrder.None)
				{
					foreach (var header in Columns)
					{
						if (header is MoreColumnHeader mch)
						{
							if (mch.SortOrder != SortOrder.None)
							{
								mch.SortOrder = SortOrder.None;
								mch.Text = mch.Title;
							}
						}
					}
				}

				// show/hide glyph in this column
				if (column.SortOrder == SortOrder.None)
				{
					column.SortOrder = Sorting = SortOrder.Ascending;
					column.Text = $"{column.Title} {UpGlyph}";
				}
				else if (column.SortOrder == SortOrder.Ascending)
				{
					column.SortOrder = Sorting = SortOrder.Descending;
					column.Text = $"{column.Title} {DnGlyph}";
				}
				else
				{
					column.SortOrder = Sorting = SortOrder.None;
					column.Text = column.Title;
				}

				// sort
				if (e.Column == 0)
				{
					ListViewItemSorter = Items[0] is IMoreHostedValue
						? new ItemValueComparer(Sorting)
						: new ItemTextComparer(Sorting);
				}
				else
				{
					ListViewItemSorter = Items[0].SubItems[e.Column] is IMoreHostedValue
						? new SubItemValueComparer(e.Column, Sorting)
						: new SubItemTextComparer(e.Column, Sorting);
				}
			}
		}


		protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
		{
			if (e.NewWidth < 50)
			{
				e.NewWidth = 50;
				e.Cancel = true;
			}

			base.OnColumnWidthChanging(e);
		}


		protected override void OnMouseDown(MouseEventArgs e)
		{
			// ensure left of clicked subitem is visible (TODO: R-t-L languages?)
			var test = HitTest(e.X, e.Y);
			if (test.SubItem != null)
			{
				if (test.SubItem.Bounds.Left < 0)
				{
					Native.SendMessage(Handle, Native.LVM_SCROLL, test.SubItem.Bounds.Left, 0);
				}
			}
		}


		protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
		{
			base.OnItemSelectionChanged(e);
			if (e.Item is IMoreHostItem host && host.Control is IChameleon item)
			{
				if (e.IsSelected)
				{
					item.ApplyBackground(highBackBrush.Color);
				}
				else
				{
					item.ResetBackground();
				}
			}
		}


		protected override void WndProc(ref Message m)
		{
			// WM_MEASUREITEM and WM_DRAWITEM are sent to the parent control rather than to the
			// ListView itself. They come here as WM_REFLECT + WM_MEASUREITEM and
			// WM_REFLECT + WM_DRAWITEM. They are sent from Control.WmOwnerDraw() to
			// Control.ReflectMessageInternal()

			// called when the ListView becomes visible
			if (m.Msg == Native.WM_SHOWWINDOW)
			{
				Debug.Assert(View == View.Details, "MoreListView supports only Details view");
				Debug.Assert(!OwnerDraw, "Do not set OwnerDraw=true in MoreListView");
			}
			// called once when the ListView is created, but only in Details view
			else if (m.Msg == Native.WM_REFLECT + Native.WM_MEASUREITEM)
			{
				// overwrite itemHeight, the fifth integer in MeasureItemStruct
				Marshal.WriteInt32(m.LParam + 4 * sizeof(int), RowHeight);
				m.Result = (IntPtr)1;
			}
			// called for each ListViewItem to be drawn
			else if (m.Msg == Native.WM_REFLECT + Native.WM_DRAWITEM)
			{
				PaintItem(ref m);
			}
			else if (m.Msg == Native.WM_PAINT)
			{
				BoundHostedControls();
			}
			else if (
				m.Msg == Native.WM_HSCROLL ||
				m.Msg == Native.WM_VSCROLL ||
				m.Msg == Native.WM_MOUSEWHEEL)
			{
				Focus();
			}

			base.WndProc(ref m);
		}


		private void PaintItem(ref Message m)
		{
			var draw = (Native.DrawItemStruct)m.GetLParam(typeof(Native.DrawItemStruct));
			var item = Items[draw.itemID];

			if (!item.SubItems.Cast<object>().Any(e => e is IMoreHostItem mhi && mhi.Control != null))
			{
				//Logger.Current.Verbose($"PaintItem {draw.itemID} unhosted");
				return;
			}

			//Logger.Current.Verbose($"PaintItem {draw.itemID}...");

			using var g = Graphics.FromHdc(draw.hDC);

			var backColor = item.BackColor;
			if (item.Selected)
			{
				backColor = highBackBrush.Color;
			}
			else if (!Enabled)
			{
				backColor = SystemColors.Control;
			}

			// erase the background of the entire row
			using var backBrush = new SolidBrush(backColor);
			g.FillRectangle(backBrush, item.Bounds);

			for (int i = 0; i < item.SubItems.Count; i++)
			{
				var subitem = item.SubItems[i];

				//Logger.Current.Verbose($".. subitem {i} subitem.text=[{subitem.Text}] " +
				//	$"hosted={subitem is IMoreHostItem hx && hx.Control != null}");

				if (string.IsNullOrWhiteSpace(subitem.Text))
				{
					// nothing to see here
					continue;
				}

				if (subitem is IMoreHostItem host && host.Control != null)
				{
					// presume hosted control overlays text, is prioritized over text
					continue;
				}

				// SubItems[0].Bounds contains the entire row, rather than the first column only
				var bounds = (i > 0) ? subitem.Bounds : item.GetBounds(ItemBoundsPortion.Label);

				// can use item.ForeColor instead of subitem.ForeColor to
				// get the same behaviour as without OwnerDraw
				var foreColor = subitem.ForeColor;
				if (!Enabled)
				{
					foreColor = SystemColors.ControlText;
				}
				else if (item.Selected)
				{
					foreColor = HighlightForeground;
				}

				var flags = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis |
					TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;

				switch (Columns[i].TextAlign)
				{
					case HorizontalAlignment.Center: flags |= TextFormatFlags.HorizontalCenter; break;
					case HorizontalAlignment.Right: flags |= TextFormatFlags.Right; break;
				}

				TextRenderer.DrawText(g, subitem.Text, subitem.Font, bounds, foreColor, flags);
			}
		}


		private void BoundHostedControls()
		{
			//Logger.Current.Verbose($"PaintHostedControls [{Name}]");

			foreach (var hosted in hostedControls)
			{
				var bounds = hosted.Host.Bounds;

				var header = Columns[hosted.ColumnIndex] as MoreColumnHeader;
				if (header != null)
					bounds.Width = header.Width;

				// is control within viewing client rectangle
				// could be <0 if scrolled up or >0 if scrolled down out of sight
				if (bounds.Y >= 0 && bounds.Y < ClientRectangle.Height)
				{
					//Logger.Current.Verbose($".. header visible name={header?.Name}");

					hosted.Control.Visible = true;
					if (header != null && header.AutoSizeItems)
					{
						hosted.Control.Bounds = new Rectangle(
							bounds.X + ControlPadding,
							bounds.Y + ControlPadding,
							bounds.Width - (2 * ControlPadding),
							bounds.Height - (2 * ControlPadding));

						//var text = hosted.Control is HistoryControl hist ? hist.Text : string.Empty;
						//Logger.Current.Verbose($".. bounds={hosted.Control.Bounds} client={ClientRectangle} [{text}]");
					}
					else
					{
						var x = bounds.X + ControlPadding;
						if (hosted.Host.Alignment == ContentAlignment.BottomCenter ||
							hosted.Host.Alignment == ContentAlignment.MiddleCenter ||
							hosted.Host.Alignment == ContentAlignment.TopCenter)
						{
							x += (bounds.Width - hosted.Control.Width) / 2;
						}
						else if (
							hosted.Host.Alignment == ContentAlignment.BottomRight ||
							hosted.Host.Alignment == ContentAlignment.MiddleRight ||
							hosted.Host.Alignment == ContentAlignment.TopRight)
						{
							x = bounds.X + bounds.Width - ControlPadding - hosted.Control.Width;
						}

						hosted.Control.Location = new Point(x, bounds.Y + ControlPadding);

						//var text = hosted.Control is HistoryControl hist ? hist.Text : string.Empty;
						//Logger.Current.Verbose($".. headless bounds={hosted.Control.Bounds} client={ClientRectangle} [{text}]");
					}
				}
				else
				{
					//Logger.Current.Verbose($".. header invisible");
					hosted.Control.Visible = false;
				}
			}
		}
		#endregion Overrides

		#region DragDrop Overrides
		protected override void OnItemDrag(ItemDragEventArgs e)
		{
			base.OnItemDrag(e);
			if (!allowItemReorder)
			{
				return;
			}

			DoDragDrop(nameof(MoreListView), DragDropEffects.Move);
		}


		protected override void OnDragOver(DragEventArgs drgevent)
		{
			if (!allowItemReorder)
			{
				drgevent.Effect = DragDropEffects.None;
				return;
			}

			if (!drgevent.Data.GetDataPresent(DataFormats.Text))
			{
				drgevent.Effect = DragDropEffects.None;
				return;
			}

			var cp = PointToClient(new Point(drgevent.X, drgevent.Y));
			var hoverItem = GetItemAt(cp.X, cp.Y);
			if (hoverItem == null)
			{
				drgevent.Effect = DragDropEffects.None;
				return;
			}

			foreach (ListViewItem moveItem in SelectedItems)
			{
				if (moveItem.Index == hoverItem.Index)
				{
					drgevent.Effect = DragDropEffects.None;
					hoverItem.EnsureVisible();
					return;
				}
			}

			base.OnDragOver(drgevent);
			var text = (string)drgevent.Data.GetData(typeof(string));
			if (text.CompareTo(nameof(MoreListView)) == 0)
			{
				drgevent.Effect = DragDropEffects.Move;
				hoverItem.EnsureVisible();
			}
			else
			{
				drgevent.Effect = DragDropEffects.None;
			}
		}


		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			base.OnDragDrop(drgevent);
			if (!allowItemReorder)
			{
				return;
			}

			if (SelectedItems.Count == 0)
			{
				return;
			}

			var cp = PointToClient(new Point(drgevent.X, drgevent.Y));
			var dragToItem = GetItemAt(cp.X, cp.Y);
			if (dragToItem == null)
			{
				return;
			}

			var dropIndex = dragToItem.Index;
			if (dropIndex > SelectedItems[0].Index)
			{
				dropIndex++;
			}

			var insertItems = new ArrayList(SelectedItems.Count);
			foreach (ListViewItem item in SelectedItems)
			{
				insertItems.Add(item.Clone());
			}

			for (int i = insertItems.Count - 1; i >= 0; i--)
			{
				ListViewItem insertItem = (ListViewItem)insertItems[i];
				Items.Insert(dropIndex, insertItem);
			}

			foreach (ListViewItem removeItem in SelectedItems)
			{
				Items.Remove(removeItem);
			}
		}


		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			base.OnDragEnter(drgevent);
			if (!allowItemReorder)
			{
				drgevent.Effect = DragDropEffects.None;
				return;
			}

			if (!drgevent.Data.GetDataPresent(DataFormats.Text))
			{
				drgevent.Effect = DragDropEffects.None;
				return;
			}

			base.OnDragEnter(drgevent);
			var text = (string)drgevent.Data.GetData(typeof(string));
			if (text.CompareTo(nameof(MoreListView)) == 0)
			{
				drgevent.Effect = DragDropEffects.Move;
			}
			else
			{
				drgevent.Effect = DragDropEffects.None;
			}
		}
		#endregion DragDrop Overrides

		#region Comparers
		private abstract class ComparerBase : IComparer
		{
			protected SortOrder Order { get; set; } = SortOrder.Ascending;
			protected abstract string GetValueOf(object obj);
			public int Compare(object x, object y)
			{
				var xvalue = GetValueOf(x);
				var yvalue = GetValueOf(y);
				int result;
				if (Decimal.TryParse(xvalue, out decimal xnum) &&
					Decimal.TryParse(yvalue, out decimal ynum))
				{
					result = Decimal.Compare(xnum, ynum);
				}
				else if (DateTime.TryParse(xvalue, out DateTime xdate) &&
					DateTime.TryParse(yvalue, out DateTime ydate))
				{
					result = DateTime.Compare(xdate, ydate);
				}
				else
				{
					result = String.Compare(xvalue, yvalue);
				}

				return Order == SortOrder.Descending ? -result : result;
			}
		}
		private sealed class ItemTextComparer : ComparerBase
		{
			public ItemTextComparer()
				: this(SortOrder.Ascending)
			{
			}

			public ItemTextComparer(SortOrder order)
			{
				Order = order;
			}

			protected override string GetValueOf(object obj) => ((ListViewItem)obj).Text;
		}
		private sealed class ItemValueComparer : ComparerBase
		{
			public ItemValueComparer()
				: this(SortOrder.Ascending)
			{
			}

			public ItemValueComparer(SortOrder order)
			{
				Order = order;
			}

			protected override string GetValueOf(object obj) => ((IMoreHostedValue)obj).Value;
		}
		private sealed class SubItemTextComparer : ComparerBase
		{
			private readonly int index;
			public SubItemTextComparer()
				: this(0, SortOrder.Ascending)
			{
			}

			public SubItemTextComparer(int col, SortOrder order)
			{
				index = col;
				Order = order;
			}

			protected override string GetValueOf(object obj) =>
				((ListViewItem)obj).SubItems[index].Text;
		}
		private sealed class SubItemValueComparer : ComparerBase
		{
			private readonly int index;
			public SubItemValueComparer()
				: this(0, SortOrder.Ascending)
			{
			}

			public SubItemValueComparer(int col, SortOrder order)
			{
				index = col;
				Order = order;
			}

			protected override string GetValueOf(object obj) =>
				((IMoreHostedValue)((ListViewItem)obj).SubItems[index]).Value;

		}
		#endregion Comparers
	}


	//===========================================================================================
	// Supporting classes
	//===========================================================================================

	/// <summary>
	/// Declares the members that must be implemented by a hosted control to support
	/// value retrieval. For example, a ProgressBar would expose its Value property as
	/// ImoreHostedValue.Value.
	/// </summary>
	internal interface IMoreHostedValue
	{
		string Value { get; }
	}


	/// <summary>
	/// Provides callback methods for MoreHostedListViewItem implementors to react when an item
	/// is selected or deselected, so that it can change the backcolor of itself as needed.
	/// </summary>
	internal interface IChameleon
	{
		void ApplyBackground(Color color);
		void ResetBackground();
	}


	/// <summary>
	/// Base class provided common members inherited by custom column headers. Inheritors
	/// may extend this with their own "templating" information such as a list of images
	/// to display in a drop-down ComboBox.
	/// </summary>
	/// <typeparam name="T">The value type represented by items in this column</typeparam>
	internal class MoreColumnHeader<T> : ColumnHeader
	{
		public MoreColumnHeader()
		{
		}

		public MoreColumnHeader(T value)
			: this()
		{
			Value = value;
			Text = value.ToString();
			Title = Text;
		}

		public MoreColumnHeader(T value, int width)
			: this(value)
		{
			Width = width;
		}

		/// <summary>
		/// Gets or sets whether values in this column are resized according to the column width.
		/// </summary>
		public bool AutoSizeItems { get; set; }

		/// <summary>
		/// Gets or sets whether this column can be sorted by clicking its header.
		/// </summary>
		public bool Sortable { get; set; } = false;

		/// <summary>
		/// The current sort state of the column.
		/// </summary>
		public SortOrder SortOrder { get; set; }

		/// <summary>
		/// The display title of the column; this is the normal Text value without a
		/// sorting glyph.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// TBD
		/// </summary>
		public T Value { get; private set; }
	}


	/// <summary>
	/// The standard text based column header.
	/// </summary>
	internal class MoreColumnHeader : MoreColumnHeader<string>
	{
		public MoreColumnHeader()
			: base()
		{
		}

		public MoreColumnHeader(string value)
			: base(value)
		{
		}

		public MoreColumnHeader(string value, int width)
			: base(value, width)
		{
		}
	}


	/// <summary>
	/// Common interface shared between items and subitems to make it easier for the
	/// main MoreListView class to handle both equally.
	/// </summary>
	internal interface IMoreHostItem
	{
		ContentAlignment Alignment { get; }

		Rectangle Bounds { get; }

		Control Control { get; }

		object Tag { get; }

		string Text { get; }
	}


	/// <summary>
	/// An item or row added to the list view and supports hosted controls.
	/// </summary>
	internal class MoreHostedListViewItem : ListViewItem, IMoreHostItem
	{
		public MoreHostedListViewItem()
			: base()
		{
		}

		public MoreHostedListViewItem(string text, Control control)
			: base(text)
		{
			Control = control;
		}

		/// <summary>
		/// Adds a new subitem to this item with standard display text.
		/// </summary>
		/// <param name="text">Display text of the subitem</param>
		/// <returns>The new subitem, already added to the item's SubItems collection.</returns>
		public MoreHostedListViewSubItem AddHostedSubItem(string text)
		{
			return AddHostedSubItem(text, null);
		}

		/// <summary>
		/// Adds a new subitem to this item with a given hosted control and no text.
		/// </summary>
		/// <param name="control">The control to host in the subitem</param>
		/// <returns>The new subitem, already added to the item's SubItems collection</returns>
		public MoreHostedListViewSubItem AddHostedSubItem(Control control)
		{
			return AddHostedSubItem(String.Empty, control);
		}

		/// <summary>
		/// Adds a new subitem to this item with a given hosted control.
		/// </summary>
		/// <param name="text">Display text of the subitem. Will be ignored if control is given.</param>
		/// <param name="control">The control to host in the subitem</param>
		/// <returns>The new subitem, already added to the item's SubItems collection</returns>
		public MoreHostedListViewSubItem AddHostedSubItem(string text, Control control)
		{
			var subitem = new MoreHostedListViewSubItem(this, text, control);
			if (control != null && Registering != null)
			{
				Registering(subitem,
					new MoreListView.RegistrationEventArgs(this, control, SubItems.Count));
			}
			SubItems.Add(subitem);
			return subitem;
		}

		/// <summary>
		/// For internal use only.
		/// </summary>
		public event MoreListView.RegistrationEventHandler Registering;

		/// <summary>
		/// Gets or set the alignment of the control in the item.
		/// </summary>
		public ContentAlignment Alignment { get; set; }

		/// <summary>
		/// Gets the control hosted by this item.
		/// </summary>
		public Control Control { get; private set; }
	}


	/// <summary>
	/// A subitem of a ListView item that supports hosted controls.
	/// </summary>
	internal class MoreHostedListViewSubItem : ListViewItem.ListViewSubItem, IMoreHostItem
	{
		public MoreHostedListViewSubItem()
			: base()
		{
		}

		public MoreHostedListViewSubItem(ListViewItem owner, string text)
			: base(owner, text)
		{
		}

		public MoreHostedListViewSubItem(
			ListViewItem owner, string text, Color foreColor, Color backColor, Font font)
			: base(owner, text, foreColor, backColor, font)
		{
		}

		public MoreHostedListViewSubItem(ListViewItem owner, string text, Control control)
			: this(owner, text)
		{
			Control = control;
		}

		public MoreHostedListViewSubItem(ListViewItem owner, string text, Control control,
			Color foreColor, Color backColor, Font font)
			: base(owner, text, foreColor, backColor, font)
		{
			Control = control;
		}

		/// <summary>
		/// Gets or set the alignment of the control in the subitem.
		/// </summary>
		public ContentAlignment Alignment { get; set; }

		/// <summary>
		/// Gets the control hosted by this subitem.
		/// </summary>
		public Control Control { get; private set; }
	}
}
