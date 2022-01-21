//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System.Media;
	using System.Windows.Forms;


	/// <summary>
	/// Customization of DataGridView specifically for KeyboardSheet to capture and swallow
	/// invalid key sequences.
	/// </summary>
	internal class KeyboardGridView : DataGridView
	{

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
