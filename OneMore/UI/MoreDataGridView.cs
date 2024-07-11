//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Media;
	using System.Windows.Forms;


	/// <summary>
	/// Customization of DataGridView specifically for KeyboardSheet to capture and swallow
	/// invalid key sequences.
	/// </summary>
	internal class MoreDataGridView : DataGridView, ILoadControl, IThemedControl
	{
		public string ThemedBack { get; set; }

		public string ThemedFore { get; set; }


		public void ApplyTheme(ThemeManager manager)
		{
			// let OnLoad handle it
		}


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;

			var back = manager.GetColor(
				string.IsNullOrWhiteSpace(ThemedBack) ? "Control" : ThemedBack);

			var fore = manager.GetColor(
				string.IsNullOrWhiteSpace(ThemedFore) ? "ControlText" : ThemedFore);

			var frame = manager.GetColor("WindowFrame");

			BackgroundColor = back; // manager.GetColor("ControlDark");
			ForeColor = fore;
			GridColor = frame;
			BorderStyle = BorderStyle.FixedSingle;
			ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
			CellBorderStyle = DataGridViewCellBorderStyle.Single;

			EnableHeadersVisualStyles = false;
			ColumnHeadersDefaultCellStyle.BackColor = manager.GetColor("ControlDarkDark");
			ColumnHeadersDefaultCellStyle.ForeColor = manager.GetColor("DarkText");

			RowsDefaultCellStyle.BackColor = back;
			RowsDefaultCellStyle.ForeColor = fore;
			RowHeadersDefaultCellStyle.BackColor = back;
			RowHeadersDefaultCellStyle.ForeColor = fore;

			DefaultCellStyle.BackColor = back;
			DefaultCellStyle.ForeColor = fore;
			DefaultCellStyle.SelectionBackColor = manager.GetColor("Highlight");
			DefaultCellStyle.SelectionForeColor = manager.GetColor("HighlightText");
		}


		/// <summary>
		/// Filters out the Ctrl+Shift+C sequence within the DataGridView. This sequence
		/// seems to be handled by DataGridView and attempts to access the clipboard which
		/// will raise an STA exception since OneNote runs in MTA mode
		/// </summary>
		/// <param name="keyData">The Keys flag to check</param>
		/// <returns>True to continue processing the key sequence</returns>
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData.HasFlag(Keys.Control) &&
				keyData.HasFlag(Keys.Shift) &&
				keyData.HasFlag(Keys.C))
			{
				SystemSounds.Exclamation.Play();
				return false;
			}

			return base.ProcessDialogKey(keyData);
		}


		/// <summary>
		/// Moves the first selected row down in the grid, also updating is position in the
		/// given BindingList.
		/// </summary>
		/// <typeparam name="T">Type of the items in the BindingList</typeparam>
		/// <param name="items">The BindingList of items to modify</param>
		public void MoveSelectedItemDown<T>(BindingList<T> items)
		{
			if (SelectedCells.Count == 0)
			{
				return;
			}

			int rowIndex = SelectedCells[0].RowIndex;
			if (rowIndex < items.Count - 1)
			{
				var lastDisplayed = Math.Max(0, Rows.Cast<DataGridViewRow>()
					.Last(row => row.Displayed)
					.Index - 1);

				// clear all selections and must set CurrentCell to null; reset below
				ClearSelection();
				CurrentCell = null;

				var item = items[rowIndex];
				items.RemoveAt(rowIndex);
				items.Insert(++rowIndex, item);

				if (rowIndex > lastDisplayed)
				{
					FirstDisplayedScrollingRowIndex++;
				}

				Rows[rowIndex].Selected = true;
				CurrentCell = Rows[rowIndex].Cells[0];
			}
		}


		/// <summary>
		/// Moves the first selected row up in the grid, also updating is position in the
		/// given BindingList.
		/// </summary>
		/// <typeparam name="T">Type of the items in the BindingList</typeparam>
		/// <param name="items">The BindingList of items to modify</param>
		public void MoveSelectedItemUp<T>(BindingList<T> items)
		{
			if (SelectedCells.Count == 0)
			{
				return;
			}

			int rowIndex = SelectedCells[0].RowIndex;
			if (rowIndex > 0 && rowIndex < items.Count)
			{
				var firstDisplayed = Math.Max(0, Rows.Cast<DataGridViewRow>()
					.First(row => row.Displayed)
					.Index);

				// clear all selections and must set CurrentCell to null; reset below
				ClearSelection();
				CurrentCell = null;

				var item = items[rowIndex];
				items.RemoveAt(rowIndex);
				items.Insert(--rowIndex, item);

				if (rowIndex < firstDisplayed)
				{
					FirstDisplayedScrollingRowIndex--;
				}

				Rows[rowIndex].Selected = true;
				CurrentCell = Rows[rowIndex].Cells[0];
			}
		}
	}
}
