//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

#pragma warning disable S2223 // Non-constant static fields should not be visible

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Globalization;
	using System.Windows.Forms;


	internal static class Program
	{
		public static CalendarForm MainForm;


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Logger.SetApplication("OneMoreCalendar");
			Logger.Current.WriteLine();
			Logger.Current.WriteLine("Starting OneMoreCalendar");

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
	}
}
