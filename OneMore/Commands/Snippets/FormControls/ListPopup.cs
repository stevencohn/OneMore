//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// A frameless popup showing a list of items to choose from, with a close row on top.
	/// </summary>
	internal sealed class ListPopup : FormControlPopup
	{
		private const int ListWidth = 220;
		private const int MaxVisibleItems = 10;

		private readonly ListBox listBox;
		private readonly ThemeManager theme = ThemeManager.Instance;
		private int hover = -1;


		public ListPopup(Point origin, IEnumerable<string> items)
			: base(origin)
		{
			listBox = new ListBox
			{
				BorderStyle = BorderStyle.None,
				DrawMode = DrawMode.OwnerDrawFixed,
				IntegralHeight = false,
				ItemHeight = S(26),
				BackColor = theme.GetColor("ListView"),
				ForeColor = theme.GetColor("ControlText")
			};

			listBox.DrawItem += DrawItem;
			listBox.MouseMove += HoverItem;
			listBox.MouseLeave += (s, e) => SetHover(-1);

			listBox.Items.AddRange(new List<string>(items).ToArray());

			var rows = Math.Max(1, Math.Min(listBox.Items.Count, MaxVisibleItems));
			var size = new Size(S(ListWidth), (rows * listBox.ItemHeight) + S(4));

			listBox.Bounds = new Rectangle(Padding.Left, Padding.Top + CloseRow, size.Width, size.Height);

			// a single click chooses; Enter chooses the highlighted item
			listBox.MouseUp += (s, e) =>
			{
				var index = listBox.IndexFromPoint(e.Location);
				if (e.Button == MouseButtons.Left && index >= 0)
				{
					Choose(index);
				}
			};

			listBox.KeyDown += (s, e) =>
			{
				if (e.KeyCode == Keys.Enter && listBox.SelectedIndex >= 0)
				{
					e.Handled = true;
					Choose(listBox.SelectedIndex);
				}
			};

			Controls.Add(listBox);
			Place(size);
		}


		/// <summary>
		/// Gets the item chosen by the user; valid when the dialog result is OK.
		/// </summary>
		public string SelectedItem { get; private set; }


		private void HoverItem(object sender, MouseEventArgs e)
		{
			SetHover(listBox.IndexFromPoint(e.Location));
		}


		private void SetHover(int index)
		{
			if (index != hover)
			{
				hover = index;
				listBox.Invalidate();
			}
		}


		private void DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0)
			{
				return;
			}

			var selected = (e.State & DrawItemState.Selected) != 0;

			Color back;
			Color fore;
			if (selected)
			{
				back = theme.GetColor("Highlight");
				fore = theme.GetColor("HighlightText");
			}
			else if (e.Index == hover)
			{
				back = theme.GetColor("HoverColor");
				fore = theme.GetColor("ControlText");
			}
			else
			{
				back = listBox.BackColor;
				fore = listBox.ForeColor;
			}

			using (var brush = new SolidBrush(back))
			{
				e.Graphics.FillRectangle(brush, e.Bounds);
			}

			var text = new Rectangle(
				e.Bounds.X + S(8), e.Bounds.Y, e.Bounds.Width - S(16), e.Bounds.Height);

			TextRenderer.DrawText(e.Graphics, (string)listBox.Items[e.Index], listBox.Font, text, fore,
				TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
				TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
		}


		private void Choose(int index)
		{
			SelectedItem = (string)listBox.Items[index];
			Dismiss(true);
		}


		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			listBox.Focus();
		}
	}
}
