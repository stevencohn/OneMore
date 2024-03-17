//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using River.OneMoreAddIn;
	using System;
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

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ScanningJob());
		}
	}
}
