//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors

namespace OneMoreSetupActions
{
	using System;
	using System.Diagnostics;
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

				case "--install-handler":
					status = new ProtocolHandlerDeployment(logger, stepper).Install();
					break;

				case "--install-edge":
					status = new EdgeWebViewDeployment(logger, stepper).Install();
					break;

				case "--install-registry":
					status = new RegistryDeployment(logger, stepper).Install();
					break;

				case "--install-shutdown":
					status = new ShutdownOneNoteDeployment(logger, stepper).Install();
					break;

				case "--install-trusted":
					status = new TrustedProtocolDeployment(logger, stepper).Install();
					break;

				case "--uninstall-edge":
					status = new EdgeWebViewDeployment(logger, stepper).Uninstall();
					break;

				case "--uninstall-registry":
					status = new RegistryDeployment(logger, stepper).Uninstall();
					break;

				case "--uninstall-shutdown":
					status = new ShutdownOneNoteDeployment(logger, stepper).Uninstall();
					break;

				default:
					logger.WriteLine($"unrecognized command: {args[0]}");
					status = Deployment.FAILURE;
					break;
			}

			Environment.Exit(status);
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
				if (new ShutdownOneNoteDeployment(logger, stepper).Install() == Deployment.SUCCESS &&
					new ProtocolHandlerDeployment(logger, stepper).Install() == Deployment.SUCCESS &&
					new TrustedProtocolDeployment(logger, stepper).Install() == Deployment.SUCCESS &&
					new EdgeWebViewDeployment(logger, stepper).Install() == Deployment.SUCCESS)
				{
					logger.WriteLine("completed successfully");
					return Deployment.SUCCESS;
				}

				logger.WriteLine("completed suspiciously");
				return Deployment.SUCCESS;
				//return FAILURE;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error registering");
				logger.WriteLine(exc);
				return Deployment.FAILURE;
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

				var ok0 = new ShutdownOneNoteDeployment(logger, stepper).Uninstall() == Deployment.SUCCESS;
				var ok1 = new ProtocolHandlerDeployment(logger, stepper).Uninstall() == Deployment.SUCCESS;
				var ok2 = new TrustedProtocolDeployment(logger, stepper).Uninstall() == Deployment.SUCCESS;
				var ok3 = new RegistryDeployment(logger, stepper).Uninstall() == Deployment.SUCCESS;

				if (ok0 && ok1 && ok2 && ok3)
				{
					logger.WriteLine("completed successfully");
				}
				else
				{
					logger.WriteLine("completed with warnings");
				}

				return Deployment.SUCCESS;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error unregistering");
				logger.WriteLine(exc);
				return Deployment.FAILURE;
			}
		}
	}
}
