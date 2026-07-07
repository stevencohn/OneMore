//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn;
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	/// <summary>
	/// A MoreListView that renders each row with a custom checkbox glyph and handles
	/// mouse-click and Space-key toggling internally. Subscribe to CheckChanged to react
	/// to user-driven state changes without wiring up GetCellImage or input handlers yourself.
	/// </summary>
	internal sealed class MoreCheckList : MoreListView
	{
		private const int GlyphSize = 14;

		private Image checkedGlyph;
		private Image uncheckedGlyph;
		private Image checkedDisabledGlyph;
		private Image uncheckedDisabledGlyph;


		public sealed class CheckChangedEventArgs : EventArgs
		{
			public ListViewItem Item { get; }

			public CheckChangedEventArgs(ListViewItem item)
			{
				Item = item;
			}
		}


		public event EventHandler<CheckChangedEventArgs> CheckChanged;


		/// <summary>
		/// Gets or sets a predicate determining whether the given item's checkbox can be
		/// toggled by the user. When null, every item can be toggled.
		/// </summary>
		public Func<ListViewItem, bool> CanToggleItem { get; set; }


		public MoreCheckList()
		{
			GetCellImage = GetGlyph;
			MouseClick += OnMouseClick;
			KeyDown += OnKeyDown;
			Disposed += (s, e) =>
			{
				checkedGlyph?.Dispose();
				uncheckedGlyph?.Dispose();
				checkedDisabledGlyph?.Dispose();
				uncheckedDisabledGlyph?.Dispose();
			};
		}


		private Image GetGlyph(ListViewItem item, int column)
		{
			var enabled = CanToggleItem?.Invoke(item) ?? true;

			if (item.Checked)
			{
				return enabled
					? checkedGlyph ??= BuildGlyph(true, true)
					: checkedDisabledGlyph ??= BuildGlyph(true, false);
			}

			return enabled
				? uncheckedGlyph ??= BuildGlyph(false, true)
				: uncheckedDisabledGlyph ??= BuildGlyph(false, false);
		}


		private static Image BuildGlyph(bool isChecked, bool enabled)
		{
			var bitmap = new Bitmap(GlyphSize, GlyphSize);
			using var g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;

			var boxColor = ThemeManager.Instance.GetColor(enabled ? "Highlight" : "GrayText");
			using var pen = new Pen(boxColor);
			g.DrawRoundedRectangle(pen, new Rectangle(0, 0, GlyphSize - 1, GlyphSize - 1), 2);

			if (isChecked)
			{
				using var fillBrush = new SolidBrush(boxColor);
				g.FillRoundedRectangle(fillBrush, new Rectangle(2, 2, GlyphSize - 4, GlyphSize - 4), 2);
			}

			return bitmap;
		}


		private void OnMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			var hit = HitTest(e.Location);
			if (hit.Item != null && (CanToggleItem?.Invoke(hit.Item) ?? true))
			{
				ToggleItem(hit.Item);
			}
		}


		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space && SelectedItems.Count > 0 &&
				(CanToggleItem?.Invoke(SelectedItems[0]) ?? true))
			{
				ToggleItem(SelectedItems[0]);
				e.Handled = true;
			}
		}


		private void ToggleItem(ListViewItem item)
		{
			item.Checked = !item.Checked;
			Invalidate(item.Bounds);
			CheckChanged?.Invoke(this, new CheckChangedEventArgs(item));
		}
	}
}
