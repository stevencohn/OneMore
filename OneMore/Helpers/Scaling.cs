﻿//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Some helper functions for dealing with Windows Forms on High DPI systems.
	/// </summary>

	internal static class Scaling
	{
		private static bool unprepared = true;
		private static float xScalingFactor = 0;
		private static float yScalingFactor = 0;


		/// <summary>
		/// Get the current horizontal and vertical dots per inch measurements of the screen
		/// </summary>
		/// <returns>A Tuple with DpiX and DpiY values</returns>
		public static (float, float) GetDpiValues()
		{
			using var graphics = Graphics.FromHwnd(IntPtr.Zero);
			return (graphics.DpiX, graphics.DpiY);
		}


		/// <summary>
		/// Get the HighDPI scaling factor for both width and height.
		/// Should be 1.0 for non-HighDPI monitors.
		/// </summary>
		/// <returns>A Tuple with xScalingFactor and yScalingFactor</returns>
		public static (float, float) GetScalingFactors()
		{
			if (xScalingFactor == 0 && yScalingFactor == 0)
			{
				using var g = Graphics.FromHwnd(IntPtr.Zero);
				IntPtr desktop = g.GetHdc();

				int physScreenWidth = Native.GetDeviceCaps(desktop, Native.DEVICECAPS_DESKTOPHORZRES);
				int physScreenHeight = Native.GetDeviceCaps(desktop, Native.DEVICECAPS_DESKTOPVERTRES);

				xScalingFactor = physScreenWidth / (float)System.Windows.SystemParameters.WorkArea.Width;
				yScalingFactor = physScreenHeight / (float)System.Windows.SystemParameters.WorkArea.Height;
			}

			return (xScalingFactor, yScalingFactor); // 1.25 = 125%
		}


		/// <summary>
		/// Initialize Windows Forms to scale appropriately on High DPI systems.
		/// </summary>

		public static void PrepareUI ()
		{
			/*

			High DPI support in Windows Forms
			https://docs.microsoft.com/en-us/dotnet/framework/winforms/high-dpi-support-in-windows-forms

			In addition, to configure high DPI support in your Windows Forms application,
			you must do the following:

			Declare compatibility with Windows 10. To do this, add the following to your
			manifest file:

			<compatibility xmlns="urn:schemas-microsoft.comn:compatibility.v1">
			  <application>
				<!-- Windows 10 compatibility -->
				<supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
			  </application>
			</compatibility>

			Enable per-monitor DPI awareness in the app.config file. Windows Forms introduces
			a new <System.Windows.Forms.ApplicationConfigurationSection> element to support
			new features and customizations added starting with the .NET Framework 4.7. To take
			advantage of the new features that support high DPI, add the following to your
			application configuration file.

			<System.Windows.Forms.ApplicationConfigurationSection>
			  <add key="DpiAwareness" value="PerMonitorV2" />
			</System.Windows.Forms.ApplicationConfigurationSection>      


			Call the static EnableVisualStyles method. This should be the first method call
			in your application entry point. For example:

			static void Main()
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new Form2());   
			}

			*/

			if (unprepared)
			{
				Native.SetProcessDPIAware();
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				unprepared = false;
			}
		}
	}
}
