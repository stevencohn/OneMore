//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant

namespace River.OneMoreAddIn
{
	using Extensibility;
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Management;
	using System.Runtime.InteropServices;
	using System.Timers;


	/// <summary>
	/// This is the OneNote addin component for OneMore functionality
	/// </summary>

	[ComVisible(true)]
	[Guid("88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61")]
	[ProgId("River.OneMoreAddin")]
	public partial class AddIn : IDTExtensibility2, IRibbonExtensibility
	{
		private const uint ReasonableClockSpeed = 1800;

		private IRibbonUI ribbon;                   // the ribbon control
		private ILogger logger;                     // our diagnostic logger
		private CommandFactory factory;
		private readonly Process process;           // current process, to kill if necessary
		private List<IDisposable> trash;            // track disposables
		private uint clockSpeed;                    // Mhz of CPU


		// Lifecycle...

		/// <summary>
		/// Initializes the addin
		/// </summary>
		public AddIn()
		{
			//System.Diagnostics.Debugger.Launch();

			logger = Logger.Current;
			trash = new List<IDisposable>();
			process = Process.GetCurrentProcess();

			UIHelper.PrepareUI();

			GetCurrentClockSpeed();

			var thread = System.Threading.Thread.CurrentThread;

			logger.WriteLine();
			logger.WriteLine(
				$"Starting {process.ProcessName}, process PID={process.Id}, CPU={clockSpeed}Mhz " +
				$"Language={thread.CurrentCulture.Name}/{thread.CurrentUICulture.Name}");
		}


		private void GetCurrentClockSpeed()
		{
			// using this as a means of short-circuiting the Ensure methods for slower machines
			// to speed up the display of the menus. CurrentClockSpeed will vary depending on
			// battery capacity and other factors, whereas MaxClockSpeed is a constant

			clockSpeed = ReasonableClockSpeed;
			using (var searcher = new ManagementObjectSearcher(
				"select CurrentClockSpeed from Win32_Processor"))
			{
				foreach (var item in searcher.Get())
				{
					clockSpeed = (uint)item["CurrentClockSpeed"];
					item.Dispose();
				}
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		// IDTExtensibility2
		//
		// Startup functions are called in this order:
		//
		// 1. OnConnection
		// 2. OnAddInsUpdate
		// 3. GetCustomUI
		// 4. OnStartupComplete

		/// <summary>
		/// Called upon startup; required to keep a reference to the OneNote application object.
		/// </summary>
		/// <param name="Application"></param>
		/// <param name="ConnectMode"></param>
		/// <param name="AddInInst"></param>
		/// <param name="custom"></param>

		public void OnConnection(
			object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
		{
			// do not grab a reference to Application here as it tends to prevent OneNote
			// from shutting down. Instead, use our ApplicationManager only as needed.

			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnConnection(ConnectionMode:{ConnectMode},{count})");
		}


		public void OnAddInsUpdate(ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OneAddInsUpdate(custom[]:{count})");
		}


		public void OnStartupComplete(ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnStartupComplete(custom[]:{count})");

			using (var manager = new ApplicationManager())
			{
				var (backupFolder, defaultFolder, unfiledFolder) = manager.GetLocations();
				logger.WriteLine("OneNote backup folder:: " + backupFolder);
				logger.WriteLine("OneNote default folder: " + defaultFolder);
				logger.WriteLine("OneNote unfiled folder: " + unfiledFolder);

				factory = new CommandFactory(logger, ribbon, trash,
					// looks complicated but necessary for this to work
					new Win32WindowHandle(new IntPtr((long)manager.WindowHandle)));
			}

			RegisterHotkeys();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		// Shutdown functions are called in this order:
		//
		// 1. OnBeginShutdown
		// 2. OnDisconnection

		public void OnBeginShutdown(ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnBeginShutdown({count})");

			try
			{
				logger.WriteLine("Shutting down UI");

				HotkeyManager.Unregister();

				UIHelper.Shutdown();
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
		}


		public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnDisconnection(RemoveMode:{RemoveMode},{count})");

			try
			{
				if (trash.Count > 0)
				{
					logger.WriteLine($"Disposing {trash.Count} streams");

					foreach (var item in trash)
					{
						item?.Dispose();
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			logger.WriteLine("Closing log");
			logger.Dispose();
			logger = null;

			ribbon = null;
			trash = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var stopTimer = new Timer();
			stopTimer.Elapsed += StopTimer_Elapsed;
			stopTimer.Interval = 2000;
			stopTimer.Start();
		}


		private void StopTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
#if FORCEDKILL
			var procs = Process.GetProcessesByName("ONENOTE");
			if (procs.Length > 0)
			{
				foreach (var proc in procs)
				{
					// TODO: there must be a friendlier way to do this?!
					proc.Kill();
				}
			}
#endif
			try
			{
				if (process != null)
				{
					process.Kill();
					process.Dispose();
				}
			}
			catch
			{
				// we're leaving anyway, so fuhgettaboutit
			}
		}
	}
}
