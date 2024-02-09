//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
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
			ColumnHeadersDefaultCellStyle.ForeColor = fore;

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
	}
}
