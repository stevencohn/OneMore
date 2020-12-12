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
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using System.Runtime.InteropServices;


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
		private Process process;                    // current process, to kill if necessary
		private List<IDisposable> trash;            // track disposables


		// Lifecycle...

		/// <summary>
		/// Initializes the addin
		/// </summary>
		public AddIn()
		{
			//System.Diagnostics.Debugger.Launch();

			RedirectAssembly();

			logger = Logger.Current;
			trash = new List<IDisposable>();
			process = Process.GetCurrentProcess();

			UIHelper.PrepareUI();

			var thread = System.Threading.Thread.CurrentThread;
			Culture = thread.CurrentUICulture;

			//Culture = CultureInfo.GetCultureInfo("fr-FR");
			//thread.CurrentCulture = Culture;
			//thread.CurrentUICulture = Culture;

			logger.WriteLine();
			logger.Start(
				$"Starting {process.ProcessName} {process.Id}, " +
				$"{thread.CurrentCulture.Name}/{thread.CurrentUICulture.Name}, " +
				$"v{AssemblyInfo.Version}, OneNote {Office.GetOneNoteVersion()}, " +
				$"Office {Office.GetOfficeVersion()}");
		}


		/// <summary>
		/// Gets the thread culture for use in subsequent threads; used primarily for 
		/// debugging when explicitly setting the culture in the AddIn() constructor
		/// </summary>
		public static CultureInfo Culture { get; private set; }


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

			using (var one = new OneNote())
			{
				var folders = one.GetFolders();
				logger.WriteLine("OneNote backup folder:: " + folders.backupFolder);
				logger.WriteLine("OneNote default folder: " + folders.defaultFolder);
				logger.WriteLine("OneNote unfiled folder: " + folders.unfiledFolder);
				logger.End();

				factory = new CommandFactory(logger, ribbon, trash,
					// looks complicated but necessary for this to work
					new Win32WindowHandle(new IntPtr((long)one.WindowHandle)));
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
			logger.Start($"OnBeginShutdown({count})");

			try
			{
				logger.WriteLine("shutting down UI");

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
					logger.WriteLine($"disposing {trash.Count} streams");

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


		/// <summary>
		/// Since the addin runs in a wierd psudo-interop realm, it can't load the CompilerServices
		/// assembly even with a .config redirect entry so we programmatically redirect the loading
		/// of that assembly here. This assembly is a dependency of System.Text.Json
		/// </summary>
		private static void RedirectAssembly()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
			{
				var assembly = new AssemblyName(args.Name);
				if (assembly.Name != "System.Runtime.CompilerServices.Unsafe")
				{
					return null;
				}

				assembly.SetPublicKeyToken(
					new AssemblyName("x, PublicKeyToken=b03f5f7f11d50a3a").GetPublicKeyToken());

				assembly.Version = new Version("5.0.0.0");
				assembly.CultureInfo = CultureInfo.InvariantCulture;

				return Assembly.Load(assembly);
			};
		}
	}
}
