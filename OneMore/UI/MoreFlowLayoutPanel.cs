//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	/// <summary>
	/// Reduce flickering
	/// </summary>
	internal class MoreFlowLayoutPanel : FlowLayoutPanel
	{
		public MoreFlowLayoutPanel()
		{
			DoubleBuffered = true;
		}


		protected override void WndProc(ref Message m)
		{
			const int WM_MOUSEWHEEL = 0x020A;
			const int WM_VSCROLL = 0x0115;

			base.WndProc(ref m);

			if (m.Msg == WM_MOUSEWHEEL ||
				m.Msg == WM_VSCROLL)
			{
				foreach (Control control in Controls)
				{
					if (control.Visible)
					{
						control.Refresh();
					}
				}
			}
		}
	}
}
