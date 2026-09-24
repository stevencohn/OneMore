//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

#pragma warning disable S2223 // Non-constant static fields should not be visible

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Windows.Forms;


	internal static class Program
	{
		private const string MutexName = @"Local\OneMoreCalendar.SingleInstance";
		private const int SW_RESTORE = 9;

		public static CalendarForm MainForm;


		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool IsIconic(IntPtr hWnd);


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// only one instance per user session; if another is running, activate it and exit
			using (var mutex = new Mutex(true, MutexName, out var isFirstInstance))
			{
				if (!isFirstInstance)
				{
					ActivateRunningInstance();
					return;
				}

				Run();
			}
		}


		/// <summary>
		/// Brings the main window of the already running instance to the foreground,
		/// restoring it first if it is minimized.
		/// </summary>
		private static void ActivateRunningInstance()
		{
			using (var current = Process.GetCurrentProcess())
			{
				var processes = Process.GetProcessesByName(current.ProcessName);
				foreach (var process in processes)
				{
					try
					{
						if (process.Id != current.Id &&
							process.SessionId == current.SessionId &&
							process.MainWindowHandle != IntPtr.Zero)
						{
							var handle = process.MainWindowHandle;
							if (IsIconic(handle))
							{
								ShowWindow(handle, SW_RESTORE);
							}

							SetForegroundWindow(handle);
							return;
						}
					}
					catch
					{
						// process may have exited or be inaccessible; try the next one
					}
					finally
					{
						process.Dispose();
					}
				}
			}
		}


		private static void Run()
		{
			Logger.SetApplication("OneMoreCalendar");
			Logger.Current.WriteLine();
			Logger.Current.WriteLine($"Starting OneMoreCalendar {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

			AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += CatchThreadException;

			// must run before any Form/Control is constructed; complements the
			// PerMonitorV2 DpiAwareness declared in App.config
			AppContext.SetSwitch(
				"Switch.System.Windows.Forms.EnableWindowsFormsHighDpiAutoResizing", true);

			// use the add-in's language for resources and date formatting; must precede the
			// command line date parsing and the construction of any Form
			SettingsProvider.ApplyAddInCulture();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// command line date specified? (e.g., "2024-01", "01-2025", "Jan 2024")
			var delta = 0;
			var args = Environment.GetCommandLineArgs();
			if (args.Length > 1 && DateTime.TryParse(
				args[1], CultureInfo.CurrentCulture, DateTimeStyles.None, out var udate))
			{
				var now = DateTime.Now;
				delta = ((udate.Year - now.Year) * 12) + udate.Month - now.Month;
			}

			// do not allow dates later than today
			MainForm = new CalendarForm(delta > 0 ? 0 : delta);
			Application.Run(MainForm);
		}


		/// <summary>
		/// Catch-all for exceptions raised on non-UI threads
		/// </summary>
		private static void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var msg = e.IsTerminating ? "Unhandled exception, terminating" : "Unhandled exception";

			if (e.ExceptionObject is Exception exc)
			{
				Logger.Current.WriteLine(msg, exc);
			}
			else
			{
				Logger.Current.WriteLine($"{msg}: {e.ExceptionObject?.GetType().FullName}");
			}
		}


		/// <summary>
		/// Catch-all for exceptions raised during the WinForms message loop, including
		/// those thrown by "async void" event handlers
		/// </summary>
		private static void CatchThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Logger.Current.WriteLine("Unhandled thread exception", e.Exception);
		}
	}
}
