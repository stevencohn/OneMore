//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;


	#region Shell interop

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

	#endregion Shell interop


	/// <summary>
	/// Prototype shell DelegateExecute handler for the onemore: protocol. Registered under
	/// HKCR\onemore\shell\open\command with a DelegateExecute value so the shell activates
	/// this class through COM (hosted in the add-in's dllhost.exe surrogate) instead of
	/// spawning OneMoreProtocolHandler.exe as a child of ONENOTE.EXE, which tightly controlled
	/// environments block. This version only logs what the shell hands it.
	/// </summary>
	[ComVisible(true)]
	[Guid(ClassId)]
	[ProgId("River.OneMoreAddIn.ProtocolExecuteHandler")]
	[ClassInterface(ClassInterfaceType.None)]
	public class ProtocolExecuteHandler :
		IExecuteCommand, IInitializeCommand, IObjectWithSelection, ICustomQueryInterface
	{
		public const string ClassId = "6F3A9B52-4C1D-4E8A-9D07-2B5E71C0A9F4";

		private const uint SigdnNormalDisplay = 0x00000000;
		private const uint SigdnUrl = 0x80068000;
		private const uint SigdnFileSysPath = 0x80058000;
		private const uint SigdnParsingName = 0x80028000;

		private readonly ILogger logger = Logger.Current;
		private string parameters;
		private string selection;


		public ProtocolExecuteHandler()
		{
			using var process = System.Diagnostics.Process.GetCurrentProcess();
			logger.WriteLine(
				$"{nameof(ProtocolExecuteHandler)} created in {process.ProcessName} pid={process.Id}");
		}


		// diagnostic: log every interface the shell asks for, then defer to default handling
		CustomQueryInterfaceResult ICustomQueryInterface.GetInterface(
			ref Guid iid, out IntPtr ppv)
		{
			ppv = IntPtr.Zero;
			logger.WriteLine($"{nameof(ProtocolExecuteHandler)}.QueryInterface {iid}");
			return CustomQueryInterfaceResult.NotHandled;
		}


		void IInitializeCommand.Initialize(string pszCommandName, IntPtr ppb)
		{
			logger.WriteLine(
				$"{nameof(ProtocolExecuteHandler)}.Initialize command=[{pszCommandName}] bag={ppb}");
		}


		void IObjectWithSelection.SetSelection(IShellItemArray psia)
		{
			try
			{
				var text = new StringBuilder();

				psia.GetCount(out var count);
				for (uint i = 0; i < count; i++)
				{
					psia.GetItemAt(i, out var item);
					text.Append($"[{i}] url=[{GetName(item, SigdnUrl)}]")
						.Append($" parsing=[{GetName(item, SigdnParsingName)}]")
						.Append($" path=[{GetName(item, SigdnFileSysPath)}]")
						.Append($" display=[{GetName(item, SigdnNormalDisplay)}] ");

					Marshal.ReleaseComObject(item);
				}

				selection = text.ToString();
				logger.WriteLine($"{nameof(ProtocolExecuteHandler)}.SetSelection count={count} {selection}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"{nameof(ProtocolExecuteHandler)}.SetSelection error", exc);
			}
		}


		void IObjectWithSelection.GetSelection(ref Guid riid, out IntPtr ppv)
		{
			ppv = IntPtr.Zero;
			throw new COMException("not implemented", unchecked((int)0x80004001)); // E_NOTIMPL
		}


		void IExecuteCommand.SetKeyState(uint grfKeyState)
		{
		}


		void IExecuteCommand.SetParameters(string pszParameters)
		{
			parameters = pszParameters;
			logger.WriteLine($"{nameof(ProtocolExecuteHandler)}.SetParameters [{pszParameters}]");
		}


		void IExecuteCommand.SetPosition(ShellPoint pt)
		{
		}


		void IExecuteCommand.SetShowWindow(int nShow)
		{
		}


		void IExecuteCommand.SetNoShowUI(bool fNoShowUI)
		{
		}


		void IExecuteCommand.SetDirectory(string pszDirectory)
		{
		}


		void IExecuteCommand.Execute()
		{
			// prototype: only log; real implementation will parse and invoke the command
			logger.WriteLine(
				$"{nameof(ProtocolExecuteHandler)}.Execute parameters=[{parameters}] selection=[{selection}]");
		}


		private static string GetName(IShellItem item, uint sigdn)
		{
			try
			{
				item.GetDisplayName(sigdn, out var ptr);
				if (ptr == IntPtr.Zero)
				{
					return string.Empty;
				}

				var name = Marshal.PtrToStringUni(ptr);
				Marshal.FreeCoTaskMem(ptr);
				return name;
			}
			catch (Exception exc)
			{
				return $"<{exc.GetType().Name} 0x{exc.HResult:X8}>";
			}
		}
	}
}
