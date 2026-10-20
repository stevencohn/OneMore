//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001  // Type is not CLS-compliant

namespace OneMoreProtocolHandler
{
	using System;
	using System.Runtime.InteropServices;


	[ComImport]
	[Guid("00000001-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IClassFactory
	{
		[PreserveSig]
		int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);

		[PreserveSig]
		int LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
	}


	[ComImport]
	[Guid("7F9185B0-CB92-43C5-80A9-92277A4F7B54")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IExecuteCommand
	{
		void SetKeyState(uint grfKeyState);
		void SetParameters([MarshalAs(UnmanagedType.LPWStr)] string pszParameters);
		void SetPosition(ShellPoint pt);
		void SetShowWindow(int nShow);
		void SetNoShowUI([MarshalAs(UnmanagedType.Bool)] bool fNoShowUI);
		void SetDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDirectory);
		void Execute();
	}


	[ComImport]
	[Guid("85075ACF-231F-40EA-9610-D26B7B58F638")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IInitializeCommand
	{
		void Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszCommandName, IntPtr ppb);
	}


	[ComImport]
	[Guid("1C9CD5BB-98E9-4491-A60F-31AACC72B83C")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IObjectWithSelection
	{
		void SetSelection(IShellItemArray psia);
		void GetSelection(ref Guid riid, out IntPtr ppv);
	}


	[ComImport]
	[Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellItemArray
	{
		void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppvOut);
		void GetPropertyStore(int flags, ref Guid riid, out IntPtr ppv);
		void GetPropertyDescriptionList(IntPtr keyType, ref Guid riid, out IntPtr ppv);
		void GetAttributes(int attribFlags, uint sfgaoMask, out uint psfgaoAttribs);
		void GetCount(out uint pdwNumItems);
		void GetItemAt(uint dwIndex, out IShellItem ppsi);
		void EnumItems(out IntPtr ppenumShellItems);
	}


	[ComImport]
	[Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellItem
	{
		void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
		void GetParent(out IShellItem ppsi);
		void GetDisplayName(uint sigdnName, out IntPtr ppszName);
		void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
		void Compare(IShellItem psi, uint hint, out int piOrder);
	}


	[StructLayout(LayoutKind.Sequential)]
	internal struct ShellPoint
	{
		public int X;
		public int Y;
	}
}
