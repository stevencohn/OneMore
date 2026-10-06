//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using River.OneMoreAddIn;
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Tray application used to perform specific tasks such as building the initial
	/// hashtag catalog at a scheduled time.
	/// </summary>
	/// <remarks>
	/// See the Developer section on the onemoreaddin.com wiki for an explanation of
	/// why this is a tray app instead of a Windows service!
	/// </remarks>
	internal static class Program
	{

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Logger.SetApplication(Resx.AppName);

			// only one tray per user session; the add-in stops an old tray before starting a
			// new one, but two add-in instances could race
			using var mutex = new Mutex(false, @"Local\OneMoreTray");
			var owned = false;
			try
			{
				owned = mutex.WaitOne(0);
			}
			catch (AbandonedMutexException)
			{
				// the previous owner was killed, which is how the add-in stops it
				owned = true;
			}

			if (!owned)
			{
				Logger.Current.WriteLine("another OneMoreTray is already running, exiting");
				return;
			}

			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new ScanningJob());
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}
	}
}
