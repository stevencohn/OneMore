//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreService
{
	using River.OneMoreAddIn;
	using System;
	using System.Diagnostics;
	using System.Management;
	using System.ServiceProcess;
	using Resx = Properties.Resources;

	/*
	 * Articles:
	 * 
	 * https://micahvandeusen.com/the-power-of-seimpersonation/#:~:text=We%20can%20add%20or%20remove%20SeImpersonatePrivilege%20for%20a,privilege%2C%20and%20then%20import%20the%20modified%20security%20policy.
	 * https://www.codeproject.com/Articles/125810/A-Complete-Impersonation-Demo-in-Csharp-NET
	 * https://learn.microsoft.com/en-us/windows/win32/api/userenv/nf-userenv-loaduserprofilea?redirectedfrom=MSDN
	 * https://learn.microsoft.com/en-us/windows/win32/api/lmaccess/ns-lmaccess-user_info_11
	 * 
	 */

	internal class OneMoreService : ServiceBase
	{
		public OneMoreService()
		{
		}


		static void Main(string[] args)
		{
			Logger.SetApplication(Resx.ServiceName);
			Logger.SetServiceMode();

			var pid = GetServicePid();
			if (pid == 0)
			{
				Logger.StdIO = true;

				// this process running in a console
				StartFromConsole();
			}
			else if (pid > 0)
			{
				// this process running as a service
				StartFromService();
			}
			else
			{
				// other service process already running
				Logger.Current.WriteLine("Service process is already running");
			}
		}


		private static int GetServicePid()
		{
			var parent = ServiceNative.ParentProcessUtilities.GetParentProcess();
			if (parent != null && parent.ProcessName == "services")
			{
				return Process.GetCurrentProcess().Id;
			}
			
			var pid = 0;
			try
			{
				var controller = new ServiceController(Resx.ServiceName);
				if (controller.Status == ServiceControllerStatus.Running ||
					controller.Status == ServiceControllerStatus.StartPending)
				{
					var searcher = new ManagementObjectSearcher(
						$"SELECT ProcessId FROM Win32_Service WHERE Name='{Resx.ServiceName}'");

					foreach (ManagementBaseObject result in searcher.Get())
					{
						pid = (int)(uint)result["ProcessId"];
					}
				}
			}
			catch (InvalidOperationException)
			{
				Logger.Current.WriteLine($"{Resx.ServiceName} service not found");
			}

			// 0=console, pid=service, -1=duplicate
			return pid == 0 ? 0 : (pid == Process.GetCurrentProcess().Id ? pid : -1);
		}


		private static void StartFromConsole()
		{
			Logger.Current.WriteLine("Starting from console");

			var host = new OneMoreService
			{
				ServiceName = Resx.ServiceName
			};

			host.OnStart(null);

			Console.Out.WriteLine("\nPress Enter to abort:\n");
			Console.ReadLine();

			host.OnStop();
		}


		private static void StartFromService()
		{
			Logger.Current.WriteLine("Starting service");

			var host = new OneMoreService
			{
				ServiceName = Resx.ServiceName,
				CanShutdown = true
			};

			Run(host);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		protected override void OnContinue()
		{
			Logger.Current.WriteLine("OnContinue()");
			base.OnContinue();
		}


		protected override void OnPause()
		{
			Logger.Current.WriteLine("OnPause()");
			base.OnPause();
		}


		protected override void OnShutdown()
		{
			Logger.Current.WriteLine("OnShutdown()");

			// 10 seconds is arbitrary but should provide sufficient time for services to
			// wind down gracefully
			RequestAdditionalTime(10 * 1000);

			//StopServices(StopReason.Shutdown);

			base.OnShutdown();
		}


		protected override void OnStart(string[] args)
		{
			base.OnStart(args);

			// catch and log unhandled exceptions
			AppDomain.CurrentDomain.UnhandledException +=
				(object s, UnhandledExceptionEventArgs e) =>
				{
					Logger.Current.WriteLine("*** UNHANDLED EXCEPTION ***");
					Logger.Current.WriteLine((Exception)e.ExceptionObject);
				};

			var listener = new ServiceListener();
			listener.Startup();
		}
	}
}
