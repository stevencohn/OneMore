//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;


	internal class Win32WindowHandle : IWin32Window
	{
		private IntPtr handle;


		public Win32WindowHandle (IntPtr windowHandle)
		{
			handle = windowHandle;
		}


		public IntPtr Handle
		{
			get
			{
				return handle;
			}
		}
	}
}
