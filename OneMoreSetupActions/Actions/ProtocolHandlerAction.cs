//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.IO;
	using System.Reflection;


	/// <summary>
	/// How the shell activates the onemore:// protocol.
	/// </summary>
	internal enum ProtocolMode
	{
		/// <summary>COM when the Defender ASR rule blocking Office child processes is in effect, else Exe</summary>
		Auto,

		/// <summary>The shell runs OneMoreProtocolHandler.exe as a command</summary>
		Exe,

		/// <summary>The shell activates OneMoreProtocolHandler.exe as an out-of-process COM server</summary>
		Com
	}


	/// <summary>
	/// Registers or unregisters a Windows shell handler for the onemore:// protocol that
	/// is used to send signals via names pipes to the OneMore addin.
	/// </summary>
	internal class ProtocolHandlerAction : CustomAction
	{
		private const string ProtocolHandler = "OneMoreProtocolHandler.exe";

		// CLSID of OneMoreProtocolHandler.CommandHandler (see ComServer.ClassId)
		private const string DelegateClsid = "{6F3A9B52-4C1D-4E8A-9D07-2B5E71C0A9F4}";

		// where the requested mode is saved; the 64-bit view, to match the installer's search
		private const string ModeKeyPath = @"SOFTWARE\River\OneMoreAddIn";
		private const string ModeValueName = "ProtocolMode";

		private readonly ProtocolMode mode;


		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="mode">
		/// Com registers OneMoreProtocolHandler.exe as an out-of-process COM server for a
		/// DelegateExecute handler instead of a shell command, for environments that block
		/// Office applications from creating child processes. Auto chooses Com only when
		/// Defender's ASR rule for that is in effect.
		/// </param>
		public ProtocolHandlerAction(
			Logger logger, Stepper stepper, ProtocolMode mode = ProtocolMode.Auto)
			: base(logger, stepper)
		{
			this.mode = mode;
		}


		/// <summary>
		/// Registers OneMoreProtocolHandler.exe as the Windows shell handler for the
		/// onemore:// URL protocol under HKLM\Software\Classes\onemore.
		/// </summary>
		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("ProtocolHandlerAction.Install ---");

			/*
			[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\onemore]
			@="URL:OneMore Protocol Handler"
			"URL Protocol"=""

			[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\onemore\shell\open\command]
			@="C:\\Github\\OneMore\\bin\\Debug\\OneMoreProtocolHandler.exe"
			*/

			// this approach is probably a bit of overkill, but want to ensure ability to debug
			// and diagnose any issues and make it as fault-tolerant as possible...

			var classesPath = @"Software\Classes";
			var onemorePath = "onemore";
			var path = $@"{classesPath}\{onemorePath}";

			logger.WriteLine($@"step {stepper.Step()}: opening HKLM:\{path}");
			var hive = Registry.LocalMachine;

			var parent = hive.OpenSubKey(path,
				RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryHelper.WriteRights);

			if (parent == null)
			{
				logger.WriteLine($@"step {stepper.Step()}: creating HKLM:\{path}");
				try
				{
					parent = hive.CreateSubKey(path, true);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error creating onemore parent key");
					logger.WriteLine(exc);
					return FAILURE;
				}

				if (parent == null)
				{
					logger.WriteLine("could not create onemore parent key, unknown reason");
					return FAILURE;
				}
			}

			logger.WriteLine($@"step {stepper.Step()}: setting properties of HKLM:\{path}");
			parent.SetValue(string.Empty, "URL:OneMore Protocol Handler");
			parent.SetValue("URL Protocol", string.Empty);

			var cmdpath = @"shell\open\command";
			logger.WriteLine($@"step {stepper.Step()}: opening HKLM:\{path}\{cmdpath}");

			var key = parent.OpenSubKey(cmdpath,
				RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryHelper.WriteRights);

			if (key == null)
			{
				logger.WriteLine($@"step {stepper.Step()}: creating HKLM:\{path}\{cmdpath}");
				try
				{
					key = parent.CreateSubKey(cmdpath, true);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error creating onemore command");
					logger.WriteLine(exc);
					return FAILURE;
				}

				if (key == null)
				{
					logger.WriteLine("could not create onemore command, unknown reason");
					return FAILURE;
				}
			}

			logger.WriteLine($@"step {stepper.Step()}: setting properties of HKLM:\{path}\{cmdpath}");

			var exe = Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				ProtocolHandler);

			var cmd = $"\"{exe}\" %1 %2 %3 %4 %5";

			key.SetValue(string.Empty, cmd);

			// The shell uses DelegateExecute exclusively when present and does not fall back
			// to the command, so only write it after the COM class is registered, and always
			// remove it otherwise.
			if (ResolveCom() && RegisterDelegate(exe))
			{
				logger.WriteLine($"step {stepper.Step()}: setting DelegateExecute {DelegateClsid}");
				key.SetValue("DelegateExecute", DelegateClsid);
			}
			else
			{
				logger.WriteLine($"step {stepper.Step()}: using {ProtocolHandler} command");
				key.DeleteValue("DelegateExecute", false);
				UnregisterDelegate();
			}

			key.Dispose();

			parent.Dispose();

			SaveMode();

			// confirm
			var verified = SUCCESS;
			using (key = hive.OpenSubKey($@"{path}\{cmdpath}", false))
			{
				if (key != null)
				{
					var value = key.GetValue(string.Empty, string.Empty) as string;
					if (value == cmd)
					{
						logger.WriteLine($"verified: key created {key.Name}");
					}
					else
					{
						logger.WriteLine("could not get command value");
						verified = FAILURE;
					}
				}
				else
				{
					logger.WriteLine("key not created");
					verified = FAILURE;
				}
			}

			return verified;
		}


		/// <summary>
		/// Registers the handler class as an out-of-process COM server whose LocalServer32 is
		/// OneMoreProtocolHandler.exe, in both the 64-bit and 32-bit registry views so either
		/// bitness of OneNote can activate it. COM launches the exe with -Embedding. Any
		/// earlier registration is deleted first because an InprocServer32 would take
		/// precedence over LocalServer32. Returns false, leaving the exe command in place,
		/// if the exe can't be found or registration fails.
		/// </summary>
		private bool RegisterDelegate(string exe)
		{
			if (!File.Exists(exe))
			{
				logger.WriteLine($"warning: {exe} not found; cannot register COM protocol handler");
				return false;
			}

			try
			{
				UnregisterDelegate();

				foreach (var view in GetViews())
				{
					logger.WriteLine($"step {stepper.Step()}: registering {DelegateClsid} ({view})");

					using (var lm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
					using (var clsid = lm.CreateSubKey($@"SOFTWARE\Classes\CLSID\{DelegateClsid}", true))
					{
						clsid.SetValue(string.Empty, "OneMore Protocol Handler");

						using (var server = clsid.CreateSubKey("LocalServer32", true))
						{
							server.SetValue(string.Empty, $"\"{exe}\"");
						}
					}
				}

				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error registering COM protocol handler; using exe command");
				logger.WriteLine(exc);
				return false;
			}
		}


		/// <summary>
		/// Returns true if the COM handler should be registered. An explicit Com or Exe
		/// request wins; Auto chooses COM only when Defender's ASR rule that blocks Office
		/// child processes is in effect, and Exe when it isn't or can't be determined.
		/// </summary>
		private bool ResolveCom()
		{
			if (mode != ProtocolMode.Auto)
			{
				logger.WriteLine($"step {stepper.Step()}: protocol mode {mode} requested");
				return mode == ProtocolMode.Com;
			}

			var state = AsrDetector.Detect(logger);
			var com = state == AsrState.Blocking;

			logger.WriteLine(
				$"step {stepper.Step()}: protocol mode Auto, ASR is {state}; using {(com ? "COM" : "EXE")}");

			return com;
		}


		/// <summary>
		/// Records the requested mode so an upgrade, which is a fresh install once the old
		/// product is removed, can restore it; see PROTOCOLMODE in Product.wxs. The requested
		/// mode is saved rather than the applied one so a transient registration failure
		/// doesn't permanently downgrade the choice. Failure here is only a warning.
		/// </summary>
		private void SaveMode()
		{
			var saved = mode.ToString().ToUpperInvariant();
			logger.WriteLine($"step {stepper.Step()}: saving protocol mode {saved}");

			try
			{
				using (var lm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, GetViews()[0]))
				using (var key = lm.CreateSubKey(ModeKeyPath, true))
				{
					key.SetValue(ModeValueName, saved);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("warning saving protocol mode");
				logger.WriteLine(exc);
			}
		}


		private void ClearMode()
		{
			try
			{
				using (var lm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, GetViews()[0]))
				{
					using (var key = lm.OpenSubKey(ModeKeyPath, true))
					{
						key?.DeleteValue(ModeValueName, false);
					}

					// remove the vendor key too, but only if nothing else lives there
					using (var key = lm.OpenSubKey(ModeKeyPath, false))
					{
						if (key != null && key.ValueCount == 0 && key.SubKeyCount == 0)
						{
							lm.DeleteSubKey(ModeKeyPath, false);
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("warning clearing protocol mode");
				logger.WriteLine(exc);
			}
		}


		private void UnregisterDelegate()
		{
			foreach (var view in GetViews())
			{
				try
				{
					using (var lm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
					{
						lm.DeleteSubKeyTree($@"SOFTWARE\Classes\CLSID\{DelegateClsid}", false);
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine($"warning deleting {DelegateClsid} ({view})");
					logger.WriteLine(exc);
				}
			}
		}


		private static RegistryView[] GetViews()
		{
			return Environment.Is64BitOperatingSystem
				? new[] { RegistryView.Registry64, RegistryView.Registry32 }
				: new[] { RegistryView.Default };
		}


		/// <summary>
		/// Removes the onemore:// protocol handler registry tree from
		/// HKLM\Software\Classes\onemore.
		/// </summary>
		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("ProtocolHandlerAction.Uninstall ---");

			// on a major upgrade this also runs for the old product, but the installer has
			// already read the saved mode (AppSearch) and the new install saves it again
			UnregisterDelegate();
			ClearMode();

			// protocol handler...
			using var hive = RegistryKey.OpenBaseKey(
				RegistryHive.LocalMachine, RegistryView.Default);

			var path = @"Software\Classes\onemore";
			logger.WriteLine($@"step {stepper.Step()}: deleting HKLM:\{path}");

			try
			{
				hive.DeleteSubKeyTree(path, false);
			}
			catch (Exception exc)
			{
				logger.WriteLine("warning deleting protocol class");
				logger.WriteLine(exc);
				return FAILURE;
			}

			// confirm
			var verified = SUCCESS;
			using (var key = hive.OpenSubKey(path, false))
			{
				if (key == null)
				{
					logger.WriteLine("key deleted");
				}
				else
				{
					logger.WriteLine("key not deleted");
					verified = FAILURE;
				}
			}

			return verified;
		}
	}
}
