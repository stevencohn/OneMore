//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreProtocolHandler
{
	using System;
	using System.Runtime.InteropServices;
	using System.Threading;


	/// <summary>
	/// Hosts the shell DelegateExecute handler for the onemore: protocol as an out-of-process
	/// COM local server. The protocol is registered with a DelegateExecute value naming this
	/// class, and the class is registered with LocalServer32 pointing at this exe, so the shell
	/// asks COM to activate it. COM's launcher starts this exe with -Embedding, which makes
	/// the new process a child of the COM service, not of ONENOTE.EXE. That is what lets
	/// onemore: links work where Defender ASR blocks Office from creating child processes.
	/// </summary>
	internal static class ComServer
	{
		public const string ClassId = "6F3A9B52-4C1D-4E8A-9D07-2B5E71C0A9F4";

		private const uint ClsctxLocalServer = 4;
		private const uint RegclsMultipleUse = 1;

		// how long to wait for the shell to activate us, and to finish after the call
		private static readonly TimeSpan ActivationTimeout = TimeSpan.FromSeconds(15);
		private static readonly TimeSpan ReturnGrace = TimeSpan.FromMilliseconds(500);

		private static readonly ManualResetEvent done = new(false);


		[DllImport("ole32.dll")]
		private static extern int CoRegisterClassObject(
			ref Guid rclsid,
			[MarshalAs(UnmanagedType.IUnknown)] object pUnk,
			uint dwClsContext, uint flags, out uint lpdwRegister);

		[DllImport("ole32.dll")]
		private static extern int CoRevokeClassObject(uint dwRegister);


		/// <summary>
		/// Registers the class factory and serves a single activation, then exits.
		/// </summary>
		public static void Run()
		{
			var clsid = new Guid(ClassId);
			var hr = CoRegisterClassObject(
				ref clsid, new ClassFactory(), ClsctxLocalServer, RegclsMultipleUse, out var cookie);

			if (hr != 0)
			{
				Program.Log($"CoRegisterClassObject failed 0x{hr:X8}");
				return;
			}

			if (!done.WaitOne(ActivationTimeout))
			{
				Program.Log("timed out waiting for activation");
			}

			// let the shell's call return before the process goes away
			Thread.Sleep(ReturnGrace);
			CoRevokeClassObject(cookie);
		}


		/// <summary>
		/// Signals that the request was handled so the server can exit.
		/// </summary>
		public static void Complete()
		{
			done.Set();
		}
	}


	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	internal class ClassFactory : IClassFactory
	{
		private const int ClassENoAggregation = unchecked((int)0x80040110);


		public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
		{
			ppvObject = IntPtr.Zero;
			if (pUnkOuter != IntPtr.Zero)
			{
				return ClassENoAggregation;
			}

			var unknown = Marshal.GetIUnknownForObject(new CommandHandler());
			try
			{
				return Marshal.QueryInterface(unknown, ref riid, out ppvObject);
			}
			finally
			{
				Marshal.Release(unknown);
			}
		}


		public int LockServer(bool fLock)
		{
			return 0;
		}
	}


	/// <summary>
	/// The shell hands the onemore:// URL to SetSelection as a shell item and then calls
	/// Execute, where it is forwarded to the add-in over the named pipe.
	/// </summary>
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	internal class CommandHandler : IExecuteCommand, IInitializeCommand, IObjectWithSelection
	{
		private const uint SigdnUrl = 0x80068000;
		private const uint SigdnParsingName = 0x80028000;

		private string uri;


		void IObjectWithSelection.SetSelection(IShellItemArray psia)
		{
			try
			{
				psia.GetCount(out var count);
				if (count > 0)
				{
					psia.GetItemAt(0, out var item);
					try
					{
						uri = GetName(item, SigdnUrl);
						if (string.IsNullOrEmpty(uri))
						{
							uri = GetName(item, SigdnParsingName);
						}
					}
					finally
					{
						Marshal.ReleaseComObject(item);
					}
				}
			}
			catch (Exception exc)
			{
				Program.Log(exc);
			}
		}


		void IObjectWithSelection.GetSelection(ref Guid riid, out IntPtr ppv)
		{
			ppv = IntPtr.Zero;
			throw new COMException("not implemented", unchecked((int)0x80004001)); // E_NOTIMPL
		}


		void IExecuteCommand.Execute()
		{
			try
			{
				// only the scheme is checked here; the add-in's CommandService applies
				// full validation to the request, same as for the exe command path
				if (string.IsNullOrEmpty(uri) ||
					!uri.StartsWith("onemore://", StringComparison.OrdinalIgnoreCase))
				{
					Program.Log($"rejected request '{uri}'");
					return;
				}

				Program.SendCommand(uri);
			}
			catch (Exception exc)
			{
				Program.Log(exc);
			}
			finally
			{
				ComServer.Complete();
			}
		}


		// the remaining interface members are required by the shell but unused...

		void IInitializeCommand.Initialize(string pszCommandName, IntPtr ppb)
		{
		}

		void IExecuteCommand.SetKeyState(uint grfKeyState)
		{
		}

		void IExecuteCommand.SetParameters(string pszParameters)
		{
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
			catch (Exception)
			{
				// the format isn't supported for this item
				return string.Empty;
			}
		}
	}
}
