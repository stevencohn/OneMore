﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;

	internal static class Native
	{

		// Constants...

		public const int DEVICECAPS_DESKTOPVERTRES = 117;
		public const int DEVICECAPS_DESKTOPHORZRES = 118;

		public const UInt32 LVM_FIRST = 0x1000;
		public const UInt32 LVM_SCROLL = (LVM_FIRST + 20);

		public const int LVS_OWNERDRAWFIXED = 0x0400;

		public const int HT_CAPTION = 0x2;

		public const int WM_DRAWITEM = 0x002B;
		public const int WM_HOTKEY = 0x312;
		public const int WM_HSCROLL = 0x114;
		public const int WM_VSCROLL = 0x115;
		public const int WM_MEASUREITEM = 0x002C;
		public const int WM_MOUSEWHEEL = 0x020A;
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int WM_PAINT = 0x000F;
		public const int WM_REFLECT = 0x2000;
		public const int WM_SHOWWINDOW = 0x0018;
		public const int WM_SETCURSOR = 0x0020;
		public const int WM_SETREDRAW = 11;
		public const int WM_SYSCOMMAND = 0x112;

		public const int MF_BYPOSITION = 0x400;

		public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
		public const uint WINEVENT_SKIPOWNTHREAD = 0x0001;
		public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
		public const uint WINEVENT_INCONTEXT = 0x0004;

		public const uint EVENT_SYSTEM_FOREGROUND = 3;
		public const uint EVENT_SYSTEM_MINIMIZESTART = 22;
		public const uint EVENT_SYSTEM_MINIMIZEEND = 23;

		public const int IDC_ARROW = 32512;
		public const int IDC_HAND = 32649;
		public const int IDC_SIZENS = 32645;

		public const int TVIF_STATE = 0x8;
		public const int TVIS_STATEIMAGEMASK = 0xF000;

		public const int TVM_SETITEMA = 0x110d;
		public const int TVM_SETITEM = 0x110d;
		public const int TVM_SETITEMW = 0x113f;

		public const int TVM_GETITEM = 0x110C;


		// LVS_OWNERDRAWFIXED: The owner window can paint ListView items in report view. 
		// The ListView control sends a WM_DRAWITEM message to paint each item. It does
		// not send separate messages for each subitem. 


		[Flags]
		public enum AssocF : uint
		{
			None = 0,
			Init_NoRemapCLSID = 0x1,
			Init_ByExeName = 0x2,
			Open_ByExeName = 0x2,
			Init_DefaultToStar = 0x4,
			Init_DefaultToFolder = 0x8,
			NoUserSettings = 0x10,
			NoTruncate = 0x20,
			Verify = 0x40,
			RemapRunDll = 0x80,
			NoFixUps = 0x100,
			IgnoreBaseClass = 0x200,
			Init_IgnoreUnknown = 0x400,
			Init_FixedProgId = 0x800,
			IsProtocol = 0x1000,
			InitForFile = 0x2000,
		}

		public enum AssocStr
		{
			Command = 1,
			Executable,
			FriendlyDocName,
			FriendlyAppName,
			NoOpen,
			ShellNewValue,
			DDECommand,
			DDEIfExec,
			DDEApplication,
			DDETopic,
			InfoTip,
			QuickTip,
			TileInfo,
			ContentType,
			DefaultIcon,
			ShellExtension,
			DropTarget,
			DelegateExecute,
			SupportedUriProtocols,
			// The values below ('Max' excluded) have been introduced in W10 1511
			ProgID,
			AppID,
			AppPublisher,
			AppIconReference,
			Max
		}


		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		public static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect,     // x-coordinate of upper-left corner
			int nTopRect,      // y-coordinate of upper-left corner
			int nRightRect,    // x-coordinate of lower-right corner
			int nBottomRect,   // y-coordinate of lower-right corner
			int nWidthEllipse, // width of ellipse
			int nHeightEllipse // height of ellipse
		);


		[StructLayout(LayoutKind.Sequential)]
		public struct DrawItemStruct
		{
			public int ctlType;
			public int ctlID;
			public int itemID;
			public int itemAction;
			public int itemState;
			public IntPtr hWndItem;
			public IntPtr hDC;
			public int rcLeft;
			public int rcTop;
			public int rcRight;
			public int rcBottom;
			public IntPtr itemData;
		}


		[StructLayout(LayoutKind.Sequential)]
		public struct Point
		{
			public int X;
			public int Y;
		}


		/// <summary>
		/// Specifies the position and size of a window
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct Rectangle
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}


		/// <summary>
		/// Specifies the attributes of a node in a TreeView
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct TreeItem
		{
			public int Mask;
			public IntPtr ItemHandle;
			public int State;
			public int StateMask;
			public IntPtr TextPtr;
			public int TextMax;
			public int Image;
			public int SelectedImage;
			public int Children;
			public IntPtr LParam;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Delegates...

		public delegate void WinEventDelegate(
			IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
			int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Functions...

		// https://docs.microsoft.com/en-us/windows/win32/api/shlwapi/nf-shlwapi-assocquerystringa
		[DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern uint AssocQueryString(
			AssocF flags, AssocStr str, string pszAssoc,
			string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);


		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out Point lpPoint);


		// https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getdevicecaps?redirectedfrom=MSDN
		[DllImport("gdi32.dll")]
		public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getforegroundwindow
		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getparent
		[DllImport("user32.dll")]
		public static extern IntPtr GetParent(IntPtr hWnd);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmenu
		[DllImport("user32.dll")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-iswindow
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindow(IntPtr hWnd);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadcursora		
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcursor
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SetCursor(IntPtr hCursor);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid
		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);


		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-insertmenua
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern bool InsertMenu(
			IntPtr hMenu, int wPosition, int wFlags, int wIDNewItem, string lpNewItem);


		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();


		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref TreeItem lParam);


		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);


		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);


		[DllImport("user32.dll")]
		public static extern bool SendMessage(IntPtr hWnd, UInt32 m, int wParam, int lParam);


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
