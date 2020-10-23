

namespace River.OneMoreAddIn
{
	using System;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	internal static class FormExtensions
	{
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOMOVE = 0x0002;

		[DllImport("user32.dll")]
		static extern bool SetWindowPos (IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


		public static void ForceTopMost (this Form form)
		{
			SetWindowPos(form.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
		}
	}
}
