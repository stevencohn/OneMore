//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001  // Type is not CLS-compliant
#pragma warning disable S1215   // "GC.Collect" should not be called
#pragma warning disable IDE0042 // Deconstruct variable declaration

namespace River.OneMoreAddIn
{
	using Extensibility;
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Management;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;


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

			var thread = System.Threading.Thread.CurrentThread;

			var settings = new SettingsProvider().GetCollection("GeneralSheet");
			var lang = settings.Get("language", thread.CurrentUICulture.Name);
			Culture = CultureInfo.GetCultureInfo(lang);
			thread.CurrentCulture = Culture;
			thread.CurrentUICulture = Culture;

			GetCurrentClockSpeed();

			logger.WriteLine();
			logger.Start(
				$"Starting {process.ProcessName} {process.Id}, CPU={clockSpeed}Mhz, " +
				$"{thread.CurrentCulture.Name}/{thread.CurrentUICulture.Name}, " +
				$"v{AssemblyInfo.Version}, OneNote {Office.GetOneNoteVersion()}, " +
				$"Office {Office.GetOfficeVersion()}");
		}


		/// <summary>
		/// Gets the thread culture for use in subsequent threads; used primarily for 
		/// debugging when explicitly setting the culture in the AddIn() constructor
		/// </summary>
		public static CultureInfo Culture { get; private set; }



		private void GetCurrentClockSpeed()
		{
			// using this as a means of short-circuiting the Ensure methods for slower machines
			// to speed up the display of the menus. CurrentClockSpeed will vary depending on
			// battery capacity and other factors, whereas MaxClockSpeed is a constant

			clockSpeed = ReasonableClockSpeed;
			using (var searcher = 
				new ManagementObjectSearcher("select CurrentClockSpeed from Win32_Processor"))
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

			try
			{
				using (var one = new OneNote())
				{
					factory = new CommandFactory(logger, ribbon, trash,
						// looks complicated but necessary for this to work
						new Win32WindowHandle(new IntPtr((long)one.WindowHandle)));
				}

				var mainproc = Process.GetProcessesByName("ONENOTE");
				if (mainproc.Length > 0)
				{
					var module = mainproc[0].MainModule;
					logger.WriteLine($"{module.FileName} ({module.FileVersionInfo.ProductVersion})");
				}

				// command listener for Refresh links
				new CommandService(factory).Startup();

				// reminder task scanner
				new Commands.ReminderService().Startup();

				// hotkeys
				RegisterHotkeys();

				// activate enablers and update check
				Task.Run(async () => { await SetGeneralOptions(); });

				logger.WriteLine($"ready");
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error starting add-on", exc);
				UIHelper.ShowError(Properties.Resources.StartupFailureMessage);
			}

			logger.End();
		}


		private async Task SetGeneralOptions()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("GeneralSheet");
			EnablersEnabled = settings.Get("enablers", true);

			if (settings.Get("checkUpdates", false))
			{
				try
				{
					await factory.Run<Commands.UpdateCommand>();
				}
				catch (Exception exc)
				{
					logger.WriteLine("error checking for updates", exc);
				}
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		// Shutdown functions are called in this order:
		//
		// 1. OnBeginShutdown
		// 2. OnDisconnection

		public void OnBeginShutdown(ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.Start($"OnBeginShutdown({count})");

			try
			{
				logger.WriteLine("shutting down UI");

				HotkeyManager.Unregister();

				UIHelper.Shutdown();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error shutting down UI", exc);
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
					logger.WriteLine($"disposing {trash.Count} streams");

					foreach (var item in trash)
					{
						item?.Dispose();
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error disconnecting", exc);
			}

			logger.WriteLine("closing log");
			logger.Dispose();
			logger = null;

			ribbon = null;
			trash = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			// this is a hack, modeless dialogs seem to keep OneNote open :-(
			Environment.Exit(0);
		}
	}
}
