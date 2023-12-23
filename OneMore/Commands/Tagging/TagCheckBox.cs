//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;


	internal class TagCheckBox : CheckBox
	{
		private readonly IntPtr hcursor;


		public TagCheckBox()
		{
			Appearance = Appearance.Button;
			AutoSize = true;
			Margin = new Padding(4);
			Padding = new Padding(0);
			Cursor = Cursors.Hand;
			UseVisualStyleBackColor = true;

			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
		}


		public TagCheckBox(string text) : this()
		{
			Name = text;
			Tag = text;
			Text = text;
		}


		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero)
			{
				Native.SetCursor(hcursor);
				m.Result = IntPtr.Zero; // indicate handled
				return;
			}

			base.WndProc(ref m);
		}
	}
}
