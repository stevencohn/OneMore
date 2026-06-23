//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// A plain owner-drawn ListView in large-icon view, with OneMore theming (selection
	/// colors, correct in both Light and Dark mode), used to show a flowing grid of
	/// square images, each optionally captioned with a small label below it.
	/// </summary>
	internal class MoreIconListView : ListView
	{
		private const int LabelPaddingTop = 2;
		private readonly ThemeManager manager;


		public MoreIconListView()
		{
			manager = ThemeManager.Instance;

			View = View.LargeIcon;
			MultiSelect = true;
			HideSelection = false;
			OwnerDraw = true;

			SetStyle(
				ControlStyles.DoubleBuffer |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.AllPaintingInWmPaint, true);

			// Never explicitly assign BackColor at design time: the VS designer would bake
			// the (always-light) snapshot into the consumer's InitializeComponent the next
			// time its Designer.cs is opened and saved (see MoreListView for more detail).
			if (!ThemeManager.IsDesignTime)
			{
				BackColor = manager.GetColor("ListView");
			}

			DrawItem += OnDrawItem;
		}


		/// <summary>
		/// .NET's ListView never forwards BackColor to the native control, so owner-drawing
		/// only colors the items themselves; the blank area around them is otherwise left
		/// painted with the OS default (white). LVM_SETBKCOLOR fixes that fill.
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
		/// Gets or sets a callback that supplies the image to draw, centered, for a given
		/// item. When null, or when the callback returns null for a given item, nothing
		/// is drawn beyond the themed background fill.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<ListViewItem, Image> GetItemImage { get; set; }


		/// <summary>
		/// Gets or sets a callback that supplies a small caption to draw below the image
		/// for a given item. When null, or when the callback returns null or empty for a
		/// given item, no label is drawn. Deliberately separate from ListViewItem.Text,
		/// which would make the native LargeIcon view re-measure tile layout based on
		/// label content - see EmojiDialog.DoTabSelected for why that's fragile here.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<ListViewItem, string> GetItemLabel { get; set; }


		private void OnDrawItem(object sender, DrawListViewItemEventArgs e)
		{
			var selected = e.Item.Selected;

			using (var backBrush = new SolidBrush(manager.GetColor(selected ? "Highlight" : "ListView")))
			{
				e.Graphics.FillRectangle(backBrush, e.Bounds);
			}

			var imageBottom = e.Bounds.Top;

			var image = GetItemImage?.Invoke(e.Item);
			if (image != null)
			{
				var top = e.Bounds.Top + LabelPaddingTop;
				e.Graphics.DrawImage(image, e.Bounds.Left + ((e.Bounds.Width - image.Width) / 2), top);
				imageBottom = top + image.Height;
			}

			var label = GetItemLabel?.Invoke(e.Item);
			if (!string.IsNullOrEmpty(label))
			{
				var labelBounds = new Rectangle(
					e.Bounds.Left, imageBottom, e.Bounds.Width, e.Bounds.Bottom - imageBottom);

				using var font = new Font(e.Item.Font.FontFamily, e.Item.Font.Size * 0.75f);
				var foreColor = manager.GetColor(selected ? "HighlightText" : "GrayText");

				TextRenderer.DrawText(e.Graphics, label, font, labelBounds, foreColor,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.Top |
					TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
			}
		}
	}
}
