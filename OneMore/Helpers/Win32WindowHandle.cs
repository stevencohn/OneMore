//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;


	internal class Win32WindowHandle : IWin32Window
	{
		public Win32WindowHandle(IntPtr windowHandle)
		{
			Handle = windowHandle;
		}


		public IntPtr Handle { get; }
	}
}
