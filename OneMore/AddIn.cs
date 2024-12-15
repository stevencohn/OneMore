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

		public static CultureInfo getCulture()
		{
			var thread = System.Threading.Thread.CurrentThread;

			var settings = new SettingsProvider().GetCollection(nameof(GeneralSheet));
			var lang = settings.Get("language", thread.CurrentUICulture.Name);
			return CultureInfo.GetCultureInfo(lang);
		}

		// Lifecycle...

		/// <summary>
		/// Initializes the addin
		/// </summary>
		public AddIn()
		{
			logger = Logger.Current;
			trash = new List<IDisposable>();
			//process = Process.GetCurrentProcess();

			UI.Scaling.PrepareUI();

			var thread = System.Threading.Thread.CurrentThread;
			thread.CurrentCulture = Culture;
			thread.CurrentUICulture = Culture;

			Helpers.SessionLogger.WriteSessionHeader();

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
			logger.Debug($"AssemblyResolve of '{args.Name}'");

			var path = new Uri(Path.Combine(
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase),
				args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll"
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


		/// <summary>
		/// Gets the thread culture for use in subsequent threads; used primarily for 
		/// debugging when explicitly setting the culture in the AddIn() constructor
		/// </summary>
		public static CultureInfo Culture { get; private set; } = getCulture();


		/// <summary>
		/// Gets the AddIn instance for use in reflection like CommandPaletteCommand
		/// </summary>
		public static AddIn Self { get; private set; }


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

				// hashtags scanner
				new Commands.HashtagService().Startup();

				// update check
				Task.Run(async () => { await SetGeneralOptions(); });

				logger.WriteLine($"ready");
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
