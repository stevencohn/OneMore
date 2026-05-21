//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
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
		private static Architecture onArchitecture;


		/// <summary>
		/// Entry point. Parses the command-line verb and architecture flag, runs
		/// CheckBitnessAction first to gate everything else, then dispatches to Install,
		/// Uninstall, or a named direct action for testing individual steps.
		/// Runs as a hidden console app so it blocks (keeping the MSI transaction open)
		/// while child processes like the WebView2 bootstrapper complete.
		/// </summary>
		static void Main(string[] args)
		{
			// Must be a console app so execution blocks while child processes run (e.g. the
			// WebView2 bootstrapper). Window is hidden so the user cannot close it mid-install.
			// Comment out ShowWindow for debugging.
			ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 0);

			if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
			{
				// nothing to do
				return;
			}

			logger = new Logger("OneMoreSetup");
			stepper = new Stepper();

			var command = args[0];

			logger.WriteLine();
			if (command == "--install" || command == "--uninstall")
			{
				logger.WriteLine(new string('=', 70));
				logger.WriteLine($"starting action: {command} .. {DateTime.Now}");
			}
			else
			{
				logger.WriteLine(new string('-', 50));
				logger.WriteLine($"direct action: {command} .. {DateTime.Now}");
			}

			ReportContext(args.Any(a => a == "--install" || a == "--uninstall"));

			int status;

			var architecture = Architecture.X86;
			foreach (var arg in args)
			{
				if (arg.Equals("--x64", StringComparison.InvariantCultureIgnoreCase))
				{
					architecture = Architecture.X64;
				}
				else if (arg.Equals("--ARM64", StringComparison.InvariantCultureIgnoreCase))
				{
					architecture = Architecture.Arm64;
				}
			}

			var action = new CheckBitnessAction(logger, stepper, architecture);
			status = action.Install();
			if (status != CustomAction.SUCCESS)
			{
				Environment.Exit(status);
			}

			onArchitecture = action.OneNoteArchitecture;

			switch (command)
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

				case "--install-checkonenote":
					status = new CheckOneNoteAction(logger, stepper).Install();
					break;

				case "--install-edge":
					status = new EdgeWebViewAction(logger, stepper).Install();
					break;

				case "--install-handler":
					status = new ProtocolHandlerAction(logger, stepper).Install();
					break;

				case "--install-registry":
					status = new RegistryAction(logger, stepper, onArchitecture).Install();
					break;

				case "--install-registrywow":
					status = new RegistryWowAction(logger, stepper, onArchitecture).Install();
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

				case "--uninstall-registry":
					status = new RegistryAction(logger, stepper, onArchitecture).Uninstall();
					break;

				case "--uninstall-registrywow":
					status = new RegistryWowAction(logger, stepper, onArchitecture).Uninstall();
					break;

				case "--uninstall-shutdown":
					status = new ShutdownOneNoteAction(logger, stepper).Uninstall();
					break;

				default:
					logger.WriteLine($"unrecognized command: {command}");
					status = CustomAction.FAILURE;
					break;
			}

			Environment.Exit(status);
		}


		/// <summary>
		/// Logs the running process identity and the on-behalf-of user. Aborts with FAILURE
		/// if elevation is required but not present — the installer must run as administrator.
		/// </summary>
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


		/// <summary>
		/// Runs the full install action sequence: verify OneNote COM config, shut down OneNote,
		/// write registry entries, register the onemore:// protocol handler, trust the protocol,
		/// and install the Edge WebView2 runtime.
		/// </summary>
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

			var shutdown = new ShutdownOneNoteAction(logger, stepper);
			try
			{
				if (shutdown.Install() == CustomAction.SUCCESS &&
					new RegistryAction(logger, stepper, onArchitecture).Install() == CustomAction.SUCCESS &&
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
			finally
			{
				shutdown.StartClickToRun();
			}
		}


		/// <summary>
		/// Runs the full uninstall action sequence: shut down OneNote, remove the protocol
		/// handler, remove the trusted protocol, and delete registry entries. Individual
		/// action failures are treated as warnings — Uninstall always returns SUCCESS so
		/// the MSI can complete the removal even if cleanup is partial.
		/// </summary>
		static int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine($"Unregister... version {AssemblyInfo.Version}");

			var shutdown = new ShutdownOneNoteAction(logger, stepper);
			try
			{
				// unregister is more lenient than register... if any of these
				// actions don't succeed, we can still complete with SUCCESS

				var ok0 = shutdown.Uninstall() == CustomAction.SUCCESS;
				var ok1 = new ProtocolHandlerAction(logger, stepper).Uninstall() == CustomAction.SUCCESS;
				var ok2 = new TrustedProtocolAction(logger, stepper).Uninstall() == CustomAction.SUCCESS;
				var ok3 = new RegistryWowAction(logger, stepper, onArchitecture).Uninstall() == CustomAction.SUCCESS;
				var ok4 = new RegistryAction(logger, stepper, onArchitecture).Uninstall() == CustomAction.SUCCESS;

				if (ok0 && ok1 && ok2 && ok3 && ok4)
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
			finally
			{
				shutdown.StartClickToRun();
			}
		}
	}
}
