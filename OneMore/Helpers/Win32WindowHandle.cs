//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;


	internal class Win32WindowHandle : IWin32Window
	{
		private readonly IntPtr handle;


		public Win32WindowHandle (IntPtr windowHandle)
		{
			handle = windowHandle;
		}


		public IntPtr Handle => handle;
	}
}
