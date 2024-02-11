//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreService
{
	using System;
	using System.Diagnostics;
	using System.Management;
	using System.ServiceProcess;
	using Resx = Properties.Resources;


	internal class OneMoreService : ServiceBase
	{
		public OneMoreService()
		{
		}


		static void Main(string[] args)
		{
			var pid = GetServicePid();
			if (pid == 0)
			{
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
				Logger.Instance.WriteLine("Service process is already running");
			}
		}


		private static int GetServicePid()
		{
			var pid = 0;
			var controller = new ServiceController(Resx.ServiceName);
			if (controller.Status == ServiceControllerStatus.Running)
			{
				var searcher = new ManagementObjectSearcher(
					$"SELECT * FROM Win32_Service WHERE Name = '{Resx.ServiceName}'");

				foreach (ManagementBaseObject result in searcher.Get())
				{
					pid = (int)result["ProcessId"];
				}
			}

			// 0=console, pid=service, -1=duplicate
			return pid == 0 ? 0 : (pid == Process.GetCurrentProcess().Id ? pid : -1);
		}


		private static void StartFromConsole()
		{
			Logger.Instance.WriteLine("Starting from console");

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
			Logger.Instance.WriteLine("Starting service");

			var host = new OneMoreService
			{
				ServiceName = Resx.ServiceName,
				CanShutdown = true
			};

			Run(host);
		}


		protected override void OnContinue()
		{
			Logger.Instance.WriteLine("OnContinue()");
			base.OnContinue();
		}


		protected override void OnPause()
		{
			Logger.Instance.WriteLine("OnPause()");
			base.OnPause();
		}


		protected override void OnShutdown()
		{
			Logger.Instance.WriteLine("OnShutdown()");

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
					Logger.Instance.WriteLine("*** UNHANDLED EXCEPTION ***");
					Logger.Instance.WriteLine((Exception)e.ExceptionObject);
				};

			//StartServices();
		}
	}
}
