//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001  // Type is not CLS-compliant
#pragma warning disable S1215   // "GC.Collect" should not be called
#pragma warning disable IDE0042 // Deconstruct variable declaration
#pragma warning disable S3885   // Use Load instead of LoadFrom

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
	using System.IO;
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

			var settings = new SettingsProvider().GetCollection(nameof(GeneralSheet));
			var lang = settings.Get("language", thread.CurrentUICulture.Name);
			Culture = CultureInfo.GetCultureInfo(lang);
			thread.CurrentCulture = Culture;
			thread.CurrentUICulture = Culture;

			var (cpu, ram) = GetMachineProps();

			logger.WriteLine();
			logger.Start(
				$"Starting {process.ProcessName} {process.Id}, {cpu} Mhz, {ram.ToBytes()}, " +
				$"{thread.CurrentCulture.Name}/{thread.CurrentUICulture.Name}, " +
				$"v{AssemblyInfo.Version}, OneNote {Office.GetOneNoteVersion()}, " +
				$"Office {Office.GetOfficeVersion()}, " +
				DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

			logger.WriteLine(Commands.DiagnosticsCommand.GetWindowsProductName());

			var hostproc = Process.GetProcessesByName("ONENOTE");
			if (hostproc.Length > 0)
			{
				var module = hostproc[0].MainModule;
				logger.WriteLine($"{module.FileName} ({module.FileVersionInfo.ProductVersion})");
			}

			Self = this;

			AppDomain.CurrentDomain.AssemblyResolve += CustomAssemblyResolve;
			AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;
		}


		/// <summary>
		/// Special handler to load third-party DLL references from nugets like GTranslate
		/// which for some reason aren't found using the default path traversal.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private System.Reflection.Assembly CustomAssemblyResolve(object sender, ResolveEventArgs args)
		{
			//logger.WriteLine($"AssemblyResolve of '{args.Name}'");

			var path = new Uri(Path.Combine(
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase),
				args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll"
				)).LocalPath;

			try
			{
				//logger.WriteLine($"resolving {path}");
				var asm = System.Reflection.Assembly.LoadFrom(path);
				return asm;
			}
			catch (Exception)
			{
				//logger.WriteLine($"AssemblyResolve exception {exc.Message} for {path}");
				return null;
			}
		}


		/// <summary>
		/// Catch-all
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			logger.WriteLine("Unhandled appdomain exception", (Exception)e.ExceptionObject);
		}


		/// <summary>
		/// Gets the thread culture for use in subsequent threads; used primarily for 
		/// debugging when explicitly setting the culture in the AddIn() constructor
		/// </summary>
		public static CultureInfo Culture { get; private set; } = CultureInfo.GetCultureInfo("en");


		/// <summary>
		/// Gets the AddIn instance for use in reflection like CommandPaletteCommand
		/// </summary>
		public static AddIn Self { get; private set; }



		private (uint, ulong) GetMachineProps()
		{
			static T Query<T>(string field, string table)
			{
				T value = default(T);
				using var searcher = new ManagementObjectSearcher($"select {field} from {table}");
				foreach (var item in searcher.Get())
				{
					value = (T)item[field];
					item.Dispose();
				}
				return value;
			}

			// using this as a means of short-circuiting the Ensure methods for slower machines
			// to speed up the display of the menus. CurrentClockSpeed will vary depending on
			// battery capacity and other factors, whereas MaxClockSpeed is a constant
			var speed = Query<uint>("CurrentClockSpeed", "Win32_Processor");
			if (speed == 0) speed = ReasonableClockSpeed;

			// returns total RAM across all physical slots; as KB so convert to bytes
			var memory = Query<ulong>("MaxCapacityEx", "Win32_PhysicalMemoryArray") * 1024;

			return (speed, memory);
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

			var cude = DescribeCustom(custom);
			logger.WriteLine($"OnConnection(ConnectionMode:{ConnectMode},custom[{cude}])");
		}


		public void OnAddInsUpdate(ref Array custom)
		{
			var cude = DescribeCustom(custom);
			logger.WriteLine($"OneAddInsUpdate(custom[{cude}])");
		}


		public void OnStartupComplete(ref Array custom)
		{
			var cude = DescribeCustom(custom);
			logger.WriteLine($"OnStartupComplete(custom[{cude}])");

			try
			{
				// hotkeys
				Task.Run(async () => { await RegisterHotkeys(); });

				factory = new CommandFactory(logger, ribbon, trash);

				// command listener for Refresh links
				new CommandService(factory).Startup();

				// reminder task scanner
				new Commands.ReminderService().Startup();

				// navigation listener
				new Commands.NavigationService().Startup();

				// update check
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
			var settings = provider.GetCollection(nameof(GeneralSheet));

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
			var cude = DescribeCustom(custom);
			logger.Start($"OnBeginShutdown(custom[{cude}])");

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
			var cude = DescribeCustom(custom);
			logger.WriteLine($"OnDisconnection(RemoveMode:{RemoveMode},custom:[{cude}])");

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


		private string DescribeCustom(Array custom)
		{
			var description = string.Empty;
			if (custom != null)
			{
				// custom is a base-1 array
				for (var i = custom.GetLowerBound(0); i <= custom.GetUpperBound(0); i++)
				{
					if (description.Length > 0) description = $"{description},";
					var value = custom.GetValue(i);
					description = $"{description}{value}:{value.GetType().Name}";
				}
			}

			return description;
		}
	}
}
