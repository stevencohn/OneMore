//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors
#pragma warning disable S6605 // "Exists" method should be used instead of the "Any" extension

namespace OneMoreSetupActions
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Security.Principal;
	using System.Windows.Forms;


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
			if (args[0] == "--install" || args[0] == "--uninstall")
			{
				logger.WriteLine(new string('=', 70));
				logger.WriteLine($"starting action: {args[0]} .. {DateTime.Now}");
			}
			else
			{
				logger.WriteLine(new string('-', 50));
				logger.WriteLine($"direct action: {args[0]} .. {DateTime.Now}");
			}

			ReportContext(args.Any(a => a == "--install" || a == "--uninstall"));

			int status;

			if (args.Any(a => a == "--x64" || a == "--x86"))
			{
				var x64 = args.Any(a => a == "--x64");
				status = new CheckBitnessAction(logger, stepper, x64).Install();
				if (status != CustomAction.SUCCESS)
				{
					Environment.Exit(status);
				}
			}

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

				case "--install-checkbitness":
					status = new CheckBitnessAction(logger, stepper, true).Install();
					break;

				case "--install-checkonenote":
					status = new CheckOneNoteAction(logger, stepper).Install();
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


		static void ReportContext(bool requireElevated)
		{
			// current user...

			var sid = WindowsIdentity.GetCurrent().User.Value;
			var username = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();

			var elevated = new WindowsPrincipal(
				WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

			var elve = elevated ? "elevated" : string.Empty;
			logger.WriteLine($"OneMore installer running as user {username} ({sid}) {elve}");

			// invoking user...

			var domain = Environment.UserDomainName;
			username = Environment.UserName;

			var userdom = domain != null
				? $@"{domain.ToUpper()}\{username.ToLower()}"
				: username.ToLower();

			logger.WriteLine($"on behalf of {userdom}");

			if (!elevated && requireElevated)
			{
				logger.WriteLine($"aborting without elevated privileges");

				MessageBox.Show(
					"This installer must be run as an administrator using elevated privileges",
					"Not Elevated",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				Environment.Exit(CustomAction.FAILURE);
			}
		}


		static int Install()
		{
			// protocol handler...
			// Registers this program as the handler for the onemore:// protocol

			logger.WriteLine();
			logger.WriteLine($"Register... version {AssemblyInfo.Version}");

			var status = new CheckOneNoteAction(logger, stepper).Install();
			if (status != CustomAction.SUCCESS)
			{
				MessageBox.Show(
					"The OneNote installation looks to be invalid. OneMore may not appear in the" +
					"OneNote ribbon until OneNote is repaired. For more information, check the logs at\n" +
					logger.LogPath,
					"OneNote Configuration Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

				// treat as warning for now...
				//return CustomAction.FAILURE;
			}

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

				MessageBox.Show($"Error installing. Check the logs {logger.LogPath}",
					"Action Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

				MessageBox.Show($"Error uninstalling. Check the logs {logger.LogPath}",
					"Action Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				return CustomAction.FAILURE;
			}
		}
	}
}
