//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Drawing;
	using System.Windows.Forms;


	internal class MoreCheckedListBox : CheckedListBox
	{

		public MoreCheckedListBox()
			: base()
		{
			CheckOnClick = true;
		}


		private ThemeProvider Theme => ThemeProvider.Instance;


		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (Items.Count == 0)
			{
				return;
			}

			var g = e.Graphics;
			var iconWidth = this.Scaled(16);

			using var fill = new SolidBrush(BackColor);
			g.FillRectangle(fill, 0, e.Bounds.Y, e.Bounds.Width, iconWidth);

			using var pen = new Pen(Theme.Control);
			g.DrawRectangle(pen, 0, e.Bounds.Y + this.Scaled(1), this.Scaled(12), this.Scaled(12));

			if (CheckedIndices.Contains(e.Index))
			{
				using var brush = new SolidBrush(Theme.Control);
				g.FillRectangle(brush, this.Scaled(2), e.Bounds.Y + this.Scaled(3), this.Scaled(9), this.Scaled(9));
			}

			var size = g.MeasureString(Text, Font);

			using var forebrush = new SolidBrush(ForeColor);

			g.DrawString(Items[e.Index].ToString(), Font, forebrush,
				new Rectangle(iconWidth, // standard icon size
					e.Bounds.Y,
					e.Bounds.Width - iconWidth,
					(int)size.Height),
				new StringFormat
				{
					Trimming = StringTrimming.EllipsisCharacter,
					FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
				});
		}


		/// <summary>
		/// Intercepts the keyboard input, [Enter] confirms a selection and [Esc] cancels it.
		/// </summary>
		/// <param name="e">The Key event arguments</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				// Enact selection.
				//((CheckedComboBox.Dropdown)Parent).OnDeactivate(new CCBoxEventArgs(null, true));
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Escape)
			{
				// Cancel selection.
				//((CheckedComboBox.Dropdown)Parent).OnDeactivate(new CCBoxEventArgs(null, false));
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Delete)
			{
				// Delete unckecks all, [Shift + Delete] checks all.
				for (int i = 0; i < Items.Count; i++)
				{
					//SetItemChecked(i, e.Shift);
				}
				e.Handled = true;
			}
			// If no Enter or Esc keys presses, let the base class handle it.
			base.OnKeyDown(e);
		}
	}
}
