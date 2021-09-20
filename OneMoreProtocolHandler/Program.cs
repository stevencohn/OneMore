//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors

namespace OneMoreProtocolHandler
{
	using Microsoft.Win32;
	using System;
	using System.Diagnostics;
	using System.IO.Pipes;
	using System.Reflection;
	using System.Text;


	class Program
	{
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";


		static void Main(string[] args)
		{
			// project is configured as a Windows Application so add Console as an output
			var listener = new TextWriterTraceListener(Console.Out);
			Debug.Listeners.Add(listener);

			if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
			{
				// nothing to do
				return;
			}

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
					Debug.WriteLine("pipe client connected");

					var buffer = Encoding.UTF8.GetBytes(arg);
					client.Write(buffer, 0, buffer.Length);
					client.Flush();
					client.Close();
				}
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
			}
		}


		#region Registration
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		static void Register()
		{
			// protocol handler...
			// Registers this program as the handler for the onemore:// protocol

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
				var key = hive.OpenSubKey(path, true);
				if (key == null)
				{
					key = hive.CreateSubKey(path, true);
				}

				key.SetValue(string.Empty, "URL:OneMore Protocol Handler");
				key.SetValue("URL Protocol", string.Empty);
				key.Dispose();

				path = $@"{path}\shell\open\command";
				key = hive.OpenSubKey(path, true);
				if (key == null)
				{
					Debug.WriteLine($@"creating HKLM:\{path}");
					key = hive.CreateSubKey(path, true);
				}

				var cmd = $"\"{Assembly.GetExecutingAssembly().Location}\" %1 %2 %3";
				key.SetValue(string.Empty, cmd);
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

				var key = hive.OpenSubKey(path, true);
				if (key == null)
				{
					Debug.WriteLine($@"creating HKCU:\{path}");
					try
					{
						key = hive.CreateSubKey(path, true);
						key.Flush();
					}
					catch (Exception exc)
					{
						WriteEvent($"error adding trusted procol '{exc.Message}'");
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
					return version;
				}
			}

			// presume latest
			return new Version(latest, 0);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		static void Unregister()
		{
			// protocol handler...
			using (var hive = RegistryKey.OpenBaseKey(
				RegistryHive.LocalMachine, RegistryView.Registry64))
			{
				var path = @"Software\Classes\onemore";
				Debug.WriteLine($@"deleting HKLM:\{path}");

				try
				{
					hive.DeleteSubKeyTree(path, false);
				}
				catch (Exception exc)
				{
					WriteEvent($"error deleting protocol class: {exc.Message}");
				}
			}


			// trusted protocol...
			using (var hive = RegistryKey.OpenBaseKey(
				RegistryHive.CurrentUser, RegistryView.Registry64))
			{
				var version = GetVersion("Excel", 16);
				var path = $@"Software\Policies\Microsoft\Office\{version}\Common\Security\" +
					@"Trusted Protocols\All Applications\onemore:";

				Debug.WriteLine($@"deleting HKCU:\{path}");

				try
				{
					hive.DeleteSubKey(path, false);
				}
				catch (Exception exc)
				{
					WriteEvent($"error deleting trusted protocol: {exc.Message}");
				}
			}
		}


		private static void WriteEvent(string message)
		{
			if (!EventLog.SourceExists("OneMore"))
			{
				EventLog.CreateEventSource("OneMore", "Application");
			}
			var log = new EventLog("Application")
			{
				Source = "OneMore"
			};
			log.WriteEntry(message);
			Debug.WriteLine($"event:{message}");
		}
		#endregion Registration
	}
}
