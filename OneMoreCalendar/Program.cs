//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

#pragma warning disable S2223 // Non-constant static fields should not be visible

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
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
			Logger.Current.WriteLine("Starting OneMoreCalendar");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new CalendarForm();
			Application.Run(MainForm);
		}
	}
}
