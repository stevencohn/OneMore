//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.IO;
	using System.Reflection;


	/// <summary>
	/// Registers or unregisters a Windows shell handler for the onemore:// protocol that
	/// is used to send signals via names pipes to the OneMore addin.
	/// </summary>
	internal class ProtocolHandlerAction : CustomAction
	{
		private const string ProtocolHandler = "OneMoreProtocolHandler.exe";


		public ProtocolHandlerAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


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
			key.Dispose();

			parent.Dispose();

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
						logger.WriteLine("coult not get command value");
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


		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("ProtocolHandlerAction.Uninstall ---");

			// protocol handler...
			using (var hive = RegistryKey.OpenBaseKey(
				RegistryHive.LocalMachine, RegistryView.Default))
			{
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
}
