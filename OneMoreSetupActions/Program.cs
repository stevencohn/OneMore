//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
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

			if (args[0] == "--install")
			{
				Environment.Exit(Register());
			}
			else if (args[0] == "--uninstall")
			{
				Environment.Exit(Unregister());
			}
			else
			{
				logger.WriteLine($"unrecognized command: {args[0]}");
				Environment.Exit(FAILURE);
			}
		}


		static int Register()
		{
			// protocol handler...
			// Registers this program as the handler for the onemore:// protocol

			logger.WriteLine(string.Empty);
			logger.WriteLine($"Register... version {AssemblyInfo.Version}");

			try
			{
				var sid = WindowsIdentity.GetCurrent().User.Value;
				var username = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();

				var elevated = new WindowsPrincipal(
					WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

				logger.WriteLine($"running as user {username} ({sid}) {(elevated ? "elevated" : string.Empty)}");

				if (new ProtocolHandlerActions(logger, stepper).Register() &&
					new TrustedProtocolActions(logger, stepper).Register())
				{
					logger.WriteLine("completed successfully");
					return SUCCESS;
				}

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


		static int Unregister()
		{
			logger.WriteLine(string.Empty);
			logger.WriteLine($"Unregister... version {AssemblyInfo.Version}");

			try
			{
				// unregister is more lenient than register...
				// if it doesn't succeed, it still completely with SUCCESS

				var ok1 = new ProtocolHandlerActions(logger, stepper).Unregister();
				var ok2 = new TrustedProtocolActions(logger, stepper).Unregister();

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
