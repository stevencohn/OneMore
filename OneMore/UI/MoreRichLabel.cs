//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.ComponentModel;
	using System.Windows.Forms;


	internal class MoreRichLabel : RichTextBox
	{
		private const int WM_SETFOCUS = 0x0007;
		private const int WM_KILLFOCUS = 0x0008;


		public MoreRichLabel()
			: base()
		{

		}


		/// <summary>
		/// Gets or sets whether text can be selected. Default is false.
		/// </summary>
		[DefaultValue(false)]
		public bool Selectable { get; set; } = false;


		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_SETFOCUS && !Selectable)
			{
				m.Msg = WM_KILLFOCUS;
			}
			base.WndProc(ref m);
		}
	}
}
