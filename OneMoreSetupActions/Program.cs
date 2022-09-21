//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors

namespace OneMoreSetupActions
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Security.Principal;


	class Program
	{
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private static Logger logger;
		private static Stepper stepper;


		static void Main(string[] args)
		{
			// hide this console window
			// - This program needs to run as a console app so it blocks while the edge webview2
			// - installer runs and completes. But we want to hide the window so the user can't
			// - close it while the installer is running, leaving it in a bad state.
			// - Can comment this out for debugging
			ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 0);

			if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
			{
				// nothing to do
				return;
			}

			logger = new Logger("OneMoreSetup");
			stepper = new Stepper();

			logger.WriteLine();
			logger.WriteLine(new string('-', 70));

			if (args.Any(a => a == "--x64" || a == "--x86"))
			{
				CheckBitness(args.Any(a => a == "--x64"));
			}

			ReportContext();

			int status;

			switch (args[0])
			{
				case "--install":
					status = Install();
					break;

				case "--uninstall":
					status = Uninstall();
					break;

				// direct calls for testing...

				case "--install-activesetup":
					status = new ActiveSetupAction(logger, stepper).Install();
					break;

				case "--install-edge":
					status = new EdgeWebViewAction(logger, stepper).Install();
					break;

				case "--install-handler":
					status = new ProtocolHandlerAction(logger, stepper).Install();
					break;

				case "--install-registrywow":
					status = new RegistryWowAction(logger, stepper).Install();
					break;

				case "--install-shutdown":
					status = new ShutdownOneNoteAction(logger, stepper).Install();
					break;

				case "--install-trusted":
					status = new TrustedProtocolAction(logger, stepper).Install();
					break;

				case "--uninstall-edge":
					status = new EdgeWebViewAction(logger, stepper).Uninstall();
					break;

				case "--uninstall-registrywow":
					status = new RegistryWowAction(logger, stepper).Uninstall();
					break;

				case "--uninstall-shutdown":
					status = new ShutdownOneNoteAction(logger, stepper).Uninstall();
					break;

				default:
					logger.WriteLine($"unrecognized command: {args[0]}");
					status = CustomAction.FAILURE;
					break;
			}

			Environment.Exit(status);
		}


		static void CheckBitness(bool x64)
		{
			var oarc = Environment.Is64BitOperatingSystem ? "x64" : "x86";
			var iarc = Environment.Is64BitProcess ? "x64" : "x86";
			var rarc = x64 ? "x64" : "x86";
			logger.WriteLine($"Installer architecture ({iarc}), OS architecture ({oarc}), requesting ({rarc})");

			if (Environment.Is64BitOperatingSystem != Environment.Is64BitProcess ||
				Environment.Is64BitOperatingSystem != x64)
			{
				logger.WriteLine($"Installer architecture ({iarc}) does not match OS ({oarc}) or request ({rarc})");
				Environment.Exit(CustomAction.USEREXIT);
			}
		}


		static void ReportContext()
		{
			var sid = WindowsIdentity.GetCurrent().User.Value;
			var username = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();

			var elevated = new WindowsPrincipal(
				WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

			var elve = elevated ? "elevated" : string.Empty;
			logger.WriteLine($"OneMore installer running as user {username} ({sid}) {elve}");
		}


		static int Install()
		{
			// protocol handler...
			// Registers this program as the handler for the onemore:// protocol

			logger.WriteLine();
			logger.WriteLine($"Register... version {AssemblyInfo.Version}");

			try
			{
				if (new ShutdownOneNoteAction(logger, stepper).Install() == CustomAction.SUCCESS &&
					new ProtocolHandlerAction(logger, stepper).Install() == CustomAction.SUCCESS &&
					new TrustedProtocolAction(logger, stepper).Install() == CustomAction.SUCCESS &&
					new EdgeWebViewAction(logger, stepper).Install() == CustomAction.SUCCESS)
				{
					logger.WriteLine("install completed successfully");
					return CustomAction.SUCCESS;
				}

				logger.WriteLine("install completed suspiciously");
				return CustomAction.SUCCESS;
				//return FAILURE;
			}
			catch (Exception exc)
			{
				logger.WriteLine("install failed; error registering");
				logger.WriteLine(exc);
				return CustomAction.FAILURE;
			}
		}


		static int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine($"Unregister... version {AssemblyInfo.Version}");

			try
			{
				// unregister is more lenient than register... if any of these
				// actions don't succeed, we can still complete with SUCCESS

				var ok0 = new ShutdownOneNoteAction(logger, stepper).Uninstall() == CustomAction.SUCCESS;
				var ok1 = new ProtocolHandlerAction(logger, stepper).Uninstall() == CustomAction.SUCCESS;
				var ok2 = new TrustedProtocolAction(logger, stepper).Uninstall() == CustomAction.SUCCESS;
				var ok3 = new RegistryWowAction(logger, stepper).Uninstall() == CustomAction.SUCCESS;

				if (ok0 && ok1 && ok2 && ok3)
				{
					logger.WriteLine("uninstall completed successfully");
				}
				else
				{
					logger.WriteLine("uninstall completed with warnings");
				}

				return CustomAction.SUCCESS;
			}
			catch (Exception exc)
			{
				logger.WriteLine("uninstall failed; error unregistering");
				logger.WriteLine(exc);
				return CustomAction.FAILURE;
			}
		}
	}
}
