//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// Extension of Button to allow forced system Hand cursor instead of default Forms Hand cursor.
	/// </summary>
	internal class MoreButton : Button
	{
		private IntPtr hcursor;


		public MoreButton()
			: base()
		{
			hcursor = IntPtr.Zero;
		}


		public void SetHandCursor()
		{
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
		}


		protected override void WndProc(ref Message msg)
		{
			if (hcursor != IntPtr.Zero &&
				msg.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero)
			{
				Native.SetCursor(hcursor);
				msg.Result = IntPtr.Zero; // indicate handled
				return;
			}

			base.WndProc(ref msg);
		}
	}
}
