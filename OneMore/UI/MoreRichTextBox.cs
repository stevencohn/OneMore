//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	/// <summary>
	/// A RichTextBox that scrolls content live while the scrollbar thumb is being dragged.
	/// The underlying Win32 RichEdit control only responds to SB_THUMBPOSITION (mouse-up)
	/// and ignores SB_THUMBTRACK (drag), so content would otherwise only jump on release.
	/// This override rewrites each SB_THUMBTRACK message as SB_THUMBPOSITION at the current
	/// tracking position, forcing RichEdit to scroll in real-time.
	/// </summary>
	internal class MoreRichTextBox : RichTextBox
	{
		private const int SB_HORZ = 0;
		private const int SB_VERT = 1;
		private const int SB_THUMBPOSITION = 4;
		private const int SB_THUMBTRACK = 5;
		private const uint SIF_TRACKPOS = 0x10;


		[DllImport("user32.dll")]
		private static extern bool GetScrollInfo(System.IntPtr hwnd, int bar, ref SCROLLINFO si);


		[StructLayout(LayoutKind.Sequential)]
		private struct SCROLLINFO
		{
			public uint cbSize;
			public uint fMask;
			public int nMin;
			public int nMax;
			public int nPage;
			public int nPos;
			public int nTrackPos;
		}


		protected override void WndProc(ref Message m)
		{
			if ((m.Msg == Native.WM_VSCROLL || m.Msg == Native.WM_HSCROLL) &&
				(m.WParam.ToInt32() & 0xFFFF) == SB_THUMBTRACK)
			{
				var si = new SCROLLINFO
				{
					cbSize = (uint)Marshal.SizeOf<SCROLLINFO>(),
					fMask = SIF_TRACKPOS
				};

				var bar = m.Msg == Native.WM_VSCROLL ? SB_VERT : SB_HORZ;
				if (GetScrollInfo(m.HWnd, bar, ref si))
				{
					m.WParam = new System.IntPtr((si.nTrackPos << 16) | SB_THUMBPOSITION);
				}
			}

			base.WndProc(ref m);
		}
	}
}
