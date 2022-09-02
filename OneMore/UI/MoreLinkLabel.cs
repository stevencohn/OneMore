//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Extension of LinkLabel to force system Hand cursor instead of default Forms Hand cursor.
	/// </summary>
	internal class MoreLinkLabel : LinkLabel
	{
		private readonly IntPtr hcursor;
		private Color fore;


		public MoreLinkLabel()
		{
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
			fore = Color.Empty;
		}


		[Description("Determines the color of the hyperlink when mouse is over it")]
		public Color HoverColor
		{
			get;
			set;
		} = Color.MediumOrchid;



		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			if (fore == Color.Empty)
			{
				fore = LinkColor;
			}

			LinkColor = VisitedLinkColor = HoverColor;
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			LinkColor = VisitedLinkColor = fore;
		}


		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero)
			{
				Native.SetCursor(hcursor);
				msg.Result = IntPtr.Zero; // indicate handled
				return;
			}

			base.WndProc(ref msg);
		}
	}
}
