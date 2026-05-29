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
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
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

		private IRibbonUI ribbon;                   // the ribbon control
		private ILogger logger;                     // our diagnostic logger
		private CommandFactory factory;
		//private readonly Process process;         // current process, to kill if necessary
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
			//process = Process.GetCurrentProcess();

			UI.Scaling.PrepareUI();

			Helpers.SessionLogger.WriteSessionHeader();

			Self = this;

			AppDomain.CurrentDomain.AssemblyResolve += CustomAssemblyResolve;
			AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;
		}


		/// <summary>
		/// Special handler to load third-party DLL references from nugets like GTranslate
		/// which for some reason aren't found using the default path traversal.
		/// Also handles satellite resource assemblies by searching culture subdirectories,
		/// including same-language siblings so zh-CHS/zh-Hans fallbacks find zh-CN resources.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private System.Reflection.Assembly CustomAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var asmName = new System.Reflection.AssemblyName(args.Name);

			if (asmName.Name.EndsWith(".resources"))
			{
				// Satellite resource assemblies live in culture subdirectories, not the root bin dir.
				// Walk the culture parent chain first (zh-CN → zh-Hans → zh), then scan any
				// sibling directory sharing the same two-letter language code (handles zh-CHS → zh-CN).
				var binDir = Path.GetDirectoryName(
					new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

				var culture = asmName.CultureInfo;
				while (culture != null && culture != CultureInfo.InvariantCulture)
				{
					var p = Path.Combine(binDir, culture.Name, asmName.Name + ".dll");
					if (File.Exists(p))
					{
						try { return System.Reflection.Assembly.LoadFrom(p); } catch { }
					}

					culture = culture.Parent;
				}

				var twoLetter = asmName.CultureInfo?.TwoLetterISOLanguageName;
				if (twoLetter != null)
				{
					foreach (var dir in Directory.GetDirectories(binDir))
					{
						try
						{
							var folderCulture = CultureInfo.GetCultureInfo(
								Path.GetFileName(dir));

							if (folderCulture.TwoLetterISOLanguageName != twoLetter) continue;

							var p = Path.Combine(dir, asmName.Name + ".dll");
							if (File.Exists(p))
							{
								try { return System.Reflection.Assembly.LoadFrom(p); } catch { }
							}
						}
						catch { /* unrecognized culture dir name, skip */ }
					}
				}

				return null;
			}

			logger.Debug($"AssemblyResolve of '{args.Name}'");

			var path = new Uri(Path.Combine(
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase),
				asmName.Name + ".dll"
				)).LocalPath;

			try
			{
				logger.Debug($"resolving {path}");
				var asm = System.Reflection.Assembly.LoadFrom(path);
				return asm;
			}
			catch (Exception exc)
			{
				logger.Debug($"AssemblyResolve exception {exc.Message} for {path}");
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
			//Debugger.Launch();

			var entry = new EventLog("Application") { Source = "OneMore" };

			var msg = "OneMore UnhandledException";
			if (e.IsTerminating)
			{
				msg += ", Terminating";
			}

			msg += ": ";

			if (e.ExceptionObject is not null)
			{
				msg += Environment.NewLine + (e.ExceptionObject is Exception exc
					? exc.FormatDetails()
					: e.ExceptionObject.GetType().FullName);
			}
			else
			{
				msg += "null ExceptionObject in CatchUnhandledException";
			}

			entry.WriteEntry(msg, EventLogEntryType.Error, 881);
			logger.WriteLine($"Unhandled appdomain exception: {msg}");

			Array custom = null;
			OnBeginShutdown(ref custom);
		}


		private static CultureInfo GetCultureSetting()
		{
			var thread = System.Threading.Thread.CurrentThread;

			var settings = new SettingsProvider().GetCollection(nameof(GeneralSheet));
			var lang = settings.Get("language", thread.CurrentUICulture.Name);
			var culture = CultureInfo.GetCultureInfo(lang);
			thread.CurrentCulture = culture;
			thread.CurrentUICulture = culture;
			return culture;
		}


		/// <summary>
		/// Gets the thread culture for use in subsequent threads; used primarily for 
		/// debugging when explicitly setting the culture in the AddIn() constructor
		/// </summary>
		public static CultureInfo Culture { get; private set; } = GetCultureSetting();


		/// <summary>
		/// Gets the AddIn instance for use in reflection like CommandPaletteCommand
		/// </summary>
		public static AddIn Self { get; private set; }


		/// <summary>
		/// Gets a value indicating whether telemetry collection is enabled for the session.
		/// </summary>
		public static bool Telemetry { get; set; } = false;


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
				Task.Run(async () =>
				{
					// hotkeys
					await RegisterHotkeys();

					factory = new CommandFactory(logger, ribbon, trash);

					// command listener for Refresh links
					new CommandService(factory).Startup();

					// reminder task scanner
					new Commands.ReminderService().Startup();

					// navigation listener
					new Commands.NavigationService().Startup();

					// hashtags scanner
					new Commands.HashtagService().Startup();

					// settings and update check
					await SetGeneralOptions();

					if (Telemetry)
					{
						await TelemetryClient.Warmup();
					}

					logger.WriteLine($"ready");
				});
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error starting add-on", exc);
				UI.MoreMessageBox.ShowError(null, Properties.Resources.StartupFailureMessage);
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

			if (!settings.Contains("telemetry"))
			{
				try
				{
					await factory.Run<Commands.TelemetryCommand>();
					provider = new SettingsProvider();
					settings = provider.GetCollection(nameof(GeneralSheet));
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error checking telemetry", exc);
				}
			}

			Telemetry = settings.Get("telemetry", false);
			logger.WriteLine("telemetry is " + (Telemetry ? "enabled" : "disabled"));
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

				System.Windows.Forms.Application.Exit();
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

			AppDomain.CurrentDomain.AssemblyResolve -= CustomAssemblyResolve;
			AppDomain.CurrentDomain.UnhandledException -= CatchUnhandledException;

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
			//GC.WaitForPendingFinalizers();

			// this is a hack, modeless dialogs seem to keep OneNote open :-(
			Environment.Exit(0);
		}


		private static string DescribeCustom(Array custom)
		{
			var description = string.Empty;
			if (custom is not null)
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
