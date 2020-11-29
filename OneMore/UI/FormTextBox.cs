//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// This is a standard TextBox that intercepts pressing the Enter key so it does not also
	/// cause the default (accept) button of a dialog to be pressed. Instead, a press of the
	/// Enter key is projected in its own custom event, PressedEnter.
	/// </summary>
	internal class FormTextBox : TextBox
	{
		private const int WM_KEYDOWN = 256;


		/// <summary>
		/// The event fired when the Enter key is pressed in this TextBox
		/// </summary>
		public event EventHandler PressedEnter;


		public FormTextBox() : base()
		{

		}


		protected override bool ProcessCmdKey(ref Message m, Keys k)
		{
			if (m.Msg == WM_KEYDOWN && k == Keys.Enter)
			{
				if (PressedEnter != null)
				{
					PressedEnter(this, new EventArgs());
				}

				// stop further interpretation
				return true;
			}
			// else default handlers...
			return base.ProcessCmdKey(ref m, k);
		}
	}
}
