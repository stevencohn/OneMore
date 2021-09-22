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
	using System.Security.AccessControl;
	using System.Security.Principal;
	using System.Text;


	class Program
	{
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";
		private static Logger logger;

		private static RegistryRights rights =
			RegistryRights.CreateSubKey |
			RegistryRights.EnumerateSubKeys |
			RegistryRights.QueryValues |
			RegistryRights.ReadKey |
			RegistryRights.SetValue |
			RegistryRights.WriteKey;


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

			var sid = WindowsIdentity.GetCurrent().User.Value;
			var username = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();

			var elevated = new WindowsPrincipal(
				WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

			logger.WriteLine($"running as user {username} ({sid}) {(elevated ? "elevated" : string.Empty)}");

			RegisterProtocolHandler();
			RegisterTrustedProtocol();
		}


		static void RegisterProtocolHandler()
		{
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

			logger.WriteLine($@"opening HKLM:\{path}");
			var hive = Registry.LocalMachine;
			var parent = hive.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree, rights);
			if (parent == null)
			{
				logger.WriteLine($@"creating HKLM:\{path}");
				try
				{
					parent = hive.CreateSubKey(path, true);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error creating onemore parent key");
					logger.WriteLine(exc);
					return;
				}

				if (parent == null)
				{
					logger.WriteLine("could not create onemore parent key, unknown reason");
					return;
				}
			}

			logger.WriteLine($@"setting properties of HKLM:\{path}");
			parent.SetValue(string.Empty, "URL:OneMore Protocol Handler");
			parent.SetValue("URL Protocol", string.Empty);

			var cmdpath = @"shell\open\command";
			logger.WriteLine($@"opening HKLM:\{path}\{cmdpath}");
			var key = parent.OpenSubKey(cmdpath, RegistryKeyPermissionCheck.ReadWriteSubTree, rights);
			if (key == null)
			{
				logger.WriteLine($@"creating HKLM:\{path}\{cmdpath}");
				try
				{
					key = parent.CreateSubKey(cmdpath, true);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error creating onemore command");
					logger.WriteLine(exc);
					return;
				}

				if (key == null)
				{
					logger.WriteLine("could not create onemore command, unknown reason");
					return;
				}
			}

			logger.WriteLine($@"setting properties of HKLM:\{path}\{cmdpath}");
			var cmd = $"\"{Assembly.GetExecutingAssembly().Location}\" %1 %2 %3";
			key.SetValue(string.Empty, cmd);
			key.Dispose();

			parent.Dispose();

			// confirm
			using (key = hive.OpenSubKey($@"{path}\{cmdpath}", false))
			{
				if (key != null)
				{
					var value = key.GetValue(string.Empty, string.Empty) as string;
					if (value == cmd)
					{
						logger.WriteLine($"key created {key.Name}");
					}
					else
					{
						logger.WriteLine("coult not get command value");
					}
				}
				else
				{
					logger.WriteLine("key not created");
				}
			}
		}


		static void RegisterTrustedProtocol()
		{
			// Declares the onemore: protocol as trusted so OneNote doesn't show a security dialog

			/*
			[HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\
			Office\16.0\Common\Security\Trusted Protocols\All Applications\onemore:]
			*/

			var sid = GetUserSid("registering trusted protocol");

			var version = GetVersion("Excel", 16);
			var policiesPath = @"Software\Policies";
			var policyPath = $@"Microsoft\Office\{version}\Common\Security\Trusted Protocols\All Applications\onemore:";
			var path = $@"{policiesPath}\{policyPath}";

			logger.WriteLine($@"opening HKCU:\{path}");

			// running as a custom action from the installer, this will run under an elevated
			// context as the System account (S-1-5-18) so we need to impersonate the current
			// user by referencing their hive from HKEY_USERS\sid\...
			// HKEY_CURRENT_USER will point to the System account's hive and we don't want that!

			using (var hive = Registry.Users.OpenSubKey(sid))
			{
				var key = hive.OpenSubKey(path, false);
				if (key != null)
				{
					key.Dispose();
					logger.WriteLine("key already exists");
					return;
				}

				logger.WriteLine($@"creating HKCU:\{path}");
				try
				{
					using (var polKey = hive.OpenSubKey(policiesPath,
						RegistryKeyPermissionCheck.ReadWriteSubTree, rights))
					{
						key = polKey.CreateSubKey(policyPath, false);
						if (key == null)
						{
							logger.WriteLine("key not created, returned null");
							return;
						}

						key.Dispose();
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine("error registering trusted protocol");
					logger.WriteLine(exc);
					return;
				}

				// confirm
				using (key = hive.OpenSubKey(path, false))
				{
					if (key != null)
					{
						logger.WriteLine($"key created {key.Name}");
					}
					else
					{
						logger.WriteLine("key not created");
					}
				}
			}
		}


		static string GetUserSid(string note)
		{
			var username = Environment.GetEnvironmentVariable("USERNAME");
			var account = new NTAccount(username);
			var sid = ((SecurityIdentifier)account.Translate(typeof(SecurityIdentifier))).ToString();
			logger.WriteLine($"{note} for user {username} ({sid})");
			return sid;
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
				RegistryHive.LocalMachine, RegistryView.Default))
			{
				var path = @"Software\Classes\onemore";
				logger.WriteLine($@"deleting HKLM:\{path}");

				try
				{
					hive.DeleteSubKeyTree(path, true);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error deleting protocol class");
					logger.WriteLine(exc);
				}

				// confirm
				using (var key = hive.OpenSubKey(path, false))
				{
					if (key == null)
					{
						logger.WriteLine("key deleted");
					}
					else
					{
						logger.WriteLine("key not deleted");
					}
				}
			}

			// trusted protocol...

			var sid = GetUserSid("unregistering trusted protocol");
			using (var hive = Registry.Users.OpenSubKey(sid))
			{
				var version = GetVersion("Excel", 16);
				var path = $@"Software\Policies\Microsoft\Office\{version}\Common\Security\" +
					@"Trusted Protocols\All Applications\onemore:";

				logger.WriteLine($@"deleting HKCU:\{path}");

				try
				{
					hive.DeleteSubKey(path, true);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error deleting trusted protocol");
					logger.WriteLine(exc);
				}

				// confirm
				using (var key = hive.OpenSubKey(path, false))
				{
					if (key == null)
					{
						logger.WriteLine("key deleted");
					}
					else
					{
						logger.WriteLine("key not deleted");
					}
				}
			}
		}
		#endregion Registration
	}
}
