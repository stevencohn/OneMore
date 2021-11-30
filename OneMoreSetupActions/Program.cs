//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors

namespace OneMoreSetupActions
{
	using System;
	using System.Security.Principal;


	class Program
	{
		private const int SUCCESS = 0;
		private const int FAILURE = 1;

		private static Logger logger;
		private static Stepper stepper;


		static void Main(string[] args)
		{
			if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
			{
				// nothing to do
				return;
			}

			logger = new Logger("OneMoreSetup");
			stepper = new Stepper();

			ReportContext();

			if (args[0] == "--install")
			{
				Environment.Exit(Install());
			}
			else if (args[0] == "--uninstall")
			{
				Environment.Exit(Uninstall());
			}

			// direct calls for testing...

			else if (args[0] == "--install-handler")
			{
				var ok = new ProtocolHandlerDeployment(logger, stepper).Install();
				Environment.Exit(ok ? SUCCESS : FAILURE);
			}
			else if (args[0] == "--install-trusted")
			{
				var ok = new TrustedProtocolDeployment(logger, stepper).Install();
				Environment.Exit(ok ? SUCCESS : FAILURE);
			}
			else if (args[0] == "--install-edge")
			{
				var ok = new EdgeWebViewDeployment(logger, stepper).Install();
				Environment.Exit(ok ? SUCCESS : FAILURE);
			}
			else if (args[0] == "--uninstall-edge")
			{
				var ok = new EdgeWebViewDeployment(logger, stepper).Uninstall();
				Environment.Exit(ok ? SUCCESS : FAILURE);
			}
			else
			{
				logger.WriteLine($"unrecognized command: {args[0]}");
				Environment.Exit(FAILURE);
			}
		}


		static void ReportContext()
		{
			var sid = WindowsIdentity.GetCurrent().User.Value;
			var username = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();

			var elevated = new WindowsPrincipal(
				WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

			logger.WriteLine($"OneMore installer running as user {username} ({sid}) {(elevated ? "elevated" : string.Empty)}");

		}


		static int Install()
		{
			// protocol handler...
			// Registers this program as the handler for the onemore:// protocol

			logger.WriteLine();
			logger.WriteLine($"Register... version {AssemblyInfo.Version}");

			try
			{
				if (new ProtocolHandlerDeployment(logger, stepper).Install() &&
					new TrustedProtocolDeployment(logger, stepper).Install() &&
					new EdgeWebViewDeployment(logger, stepper).Install())
				{
					logger.WriteLine("completed successfully");
					return SUCCESS;
				}

				logger.WriteLine("completed suspiciously");
				return SUCCESS;
				//return FAILURE;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error registering");
				logger.WriteLine(exc);
				return FAILURE;
			}
		}


		static int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine($"Unregister... version {AssemblyInfo.Version}");

			try
			{
				// unregister is more lenient than register...
				// if it doesn't succeed, it still completely with SUCCESS

				var ok1 = new ProtocolHandlerDeployment(logger, stepper).Uninstall();
				var ok2 = new TrustedProtocolDeployment(logger, stepper).Uninstall();

				if (ok1 && ok2)
				{
					logger.WriteLine("completed successfully");
				}
				else
				{
					logger.WriteLine("completed with warnings");
				}

				return SUCCESS;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error unregistering");
				logger.WriteLine(exc);
				return FAILURE;
			}
		}
	}
}
