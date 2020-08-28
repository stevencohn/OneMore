//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Runtime.InteropServices;


	internal static class Native
	{

		// Constants...

		public const int DEVICECAPS_VERTRES = 10;
		public const int DEVICECAPS_DESKTOPVERTRES = 117;

		public const int WM_HOTKEY = 0x312;

		public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
		public const uint WINEVENT_SKIPOWNTHREAD = 0x0001;
		public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
		public const uint WINEVENT_INCONTEXT = 0x0004;

		public const uint EVENT_SYSTEM_FOREGROUND = 3;
		public const uint EVENT_SYSTEM_MINIMIZESTART = 22;
		public const uint EVENT_SYSTEM_MINIMIZEEND = 23;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Delegates...

		public delegate void WinEventDelegate(
			IntPtr hWinEventHook, uint eventType, IntPtr hwnd, 
			int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Functions...

		// https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getdevicecaps?redirectedfrom=MSDN
		[DllImport("gdi32.dll")]
		public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
		

		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getforegroundwindow
		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getparent
		[DllImport("user32.dll")]
		public static extern IntPtr GetParent(IntPtr hWnd);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid
		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
		[DllImport("user32", SetLastError = true)]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setprocessdpiaware
		[DllImport("user32.dll")]
		public static extern bool SetProcessDPIAware();


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook
		[DllImport("user32.dll")]
		public static extern IntPtr SetWinEventHook(
			uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
			WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterhotkey
		[DllImport("user32", SetLastError = true)]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	}
}
