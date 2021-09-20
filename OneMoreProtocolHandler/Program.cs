//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors

namespace OneMoreProtocolHandler
{
	using Microsoft.Win32;
	using System;
	using System.IO.Pipes;
	using System.Reflection;
	using System.Text;


	class Program
	{
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";
		private static Logger logger;


		static void Main(string[] args)
		{
			if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
			{
				// nothing to do
				return;
			}

			logger = new Logger();

			if (args[0] == "--register")
			{
				Register();
				return;
			}

			if (args[0] == "--unregister")
			{
				Unregister();
				return;
			}

			SendCommand(args[0]);
		}


		static void SendCommand(string arg)
		{
			try
			{
				string pipeName = null;
				var key = Registry.ClassesRoot.OpenSubKey(KeyPath, false);
				if (key != null)
				{
					// get default value string
					pipeName = (string)key.GetValue(string.Empty);
				}

				if (string.IsNullOrEmpty(pipeName))
				{
					// problem!
					return;
				}

				using (var client = new NamedPipeClientStream(".",
					pipeName, PipeDirection.Out, PipeOptions.Asynchronous))
				{
					// being an event-driven program to bounce messages back to OneMore
					// set timeout because we shouldn't need to wait for server ever!
					client.Connect(500);
					logger.WriteLine("pipe client connected");
					logger.WriteLine($"writing '{arg}'");

					var buffer = Encoding.UTF8.GetBytes(arg);
					client.Write(buffer, 0, buffer.Length);
					client.Flush();
					client.Close();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error writing '{arg}'");
				logger.WriteLine(exc);
			}
		}


		#region Registration
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		static void Register()
		{
			// protocol handler...
			// Registers this program as the handler for the onemore:// protocol

			logger.WriteLine(string.Empty);
			logger.WriteLine("Register...");

			/*
			[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\onemore]
			@="URL:OneMore Protocol Handler"
			"URL Protocol"=""

			[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\onemore\shell\open\command]
			@="C:\\Github\\OneMore\\bin\\Debug\\OneMoreProtocolHandler.exe"
			 */
			using (var hive = RegistryKey.OpenBaseKey(
				RegistryHive.LocalMachine, RegistryView.Registry64))
			{
				var path = @"Software\Classes\onemore";

				logger.WriteLine($@"opening HKLM:\{path}");
				var key = hive.OpenSubKey(path, true);
				if (key == null)
				{
					logger.WriteLine($@"creating HKLM:\{path}");
					key = hive.CreateSubKey(path, true);
				}

				logger.WriteLine($@"setting properties of HKLM:\{path}");
				key.SetValue(string.Empty, "URL:OneMore Protocol Handler");
				key.SetValue("URL Protocol", string.Empty);
				key.Flush();
				key.Dispose();

				path = $@"{path}\shell\open\command";
				logger.WriteLine($@"opening HKLM:\{path}");
				key = hive.OpenSubKey(path, true);
				if (key == null)
				{
					logger.WriteLine($@"creating HKLM:\{path}");
					key = hive.CreateSubKey(path, true);
				}

				logger.WriteLine($@"setting properties of HKLM:\{path}");
				var cmd = $"\"{Assembly.GetExecutingAssembly().Location}\" %1 %2 %3";
				key.SetValue(string.Empty, cmd);
				key.Flush();
				key.Dispose();
			}

			// trusted protocol...
			// Declares the onemore: protocol as trusted so OneNote doesn't show a security dialog

			/*
			[HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Office\16.0\Common\Security\
				Trusted Protocols\All Applications\onemore:]
			*/
			using (var hive = RegistryKey.OpenBaseKey(
				RegistryHive.CurrentUser, RegistryView.Registry64))
			{
				var version = GetVersion("Excel", 16);
				var path = $@"Software\Policies\Microsoft\Office\{version}\Common\Security\" +
					@"Trusted Protocols\All Applications\onemore:";

				logger.WriteLine($@"opening HKCU:\{path}");
				var key = hive.OpenSubKey(path, true);
				if (key == null)
				{
					logger.WriteLine($@"creating HKCU:\{path}");
					try
					{
						key = hive.CreateSubKey(path, true);
						key.Flush();
					}
					catch (Exception exc)
					{
						logger.WriteLine("error registering trusted protocol");
						logger.WriteLine(exc);
					}
				}

				key.Dispose();
			}
		}


		static Version GetVersion(string name, int latest)
		{
			using (var key = Registry.ClassesRoot.OpenSubKey($@"\{name}.Application\CurVer", false))
			{
				if (key != null)
				{
					// get default value string
					var value = (string)key.GetValue(string.Empty);
					// extract version number
					var version = new Version(value.Substring(value.LastIndexOf('.') + 1) + ".0");
					logger.WriteLine($"found Office version {latest}");
					return version;
				}
			}

			// presume latest
			logger.WriteLine($"defaulting Office version to {latest}");
			return new Version(latest, 0);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		static void Unregister()
		{
			logger.WriteLine(string.Empty);
			logger.WriteLine("Unregister...");

			// protocol handler...
			using (var hive = RegistryKey.OpenBaseKey(
				RegistryHive.LocalMachine, RegistryView.Registry64))
			{
				var path = @"Software\Classes\onemore";
				logger.WriteLine($@"deleting HKLM:\{path}");

				try
				{
					hive.DeleteSubKeyTree(path, false);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error deleting protocol class");
					logger.WriteLine(exc);
				}
			}


			// trusted protocol...
			using (var hive = RegistryKey.OpenBaseKey(
				RegistryHive.CurrentUser, RegistryView.Registry64))
			{
				var version = GetVersion("Excel", 16);
				var path = $@"Software\Policies\Microsoft\Office\{version}\Common\Security\" +
					@"Trusted Protocols\All Applications\onemore:";

				logger.WriteLine($@"deleting HKCU:\{path}");

				try
				{
					hive.DeleteSubKey(path, false);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error deleting trusted protocol");
					logger.WriteLine(exc);
				}
			}
		}
		#endregion Registration
	}
}
