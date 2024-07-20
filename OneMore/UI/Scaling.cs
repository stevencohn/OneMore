//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Some helper functions for dealing with Windows Forms on High DPI systems.
	/// </summary>

	internal class Scaling
	{
		private static bool unprepared = true;
		private static float xScalingFactor = 0;
		private static float yScalingFactor = 0;


		/// <summary>
		/// Initialize a new instance from the given resolutions which are typically
		/// from Image properties
		/// </summary>
		/// <param name="horizontalResolution">The Image.HorizontalResolution</param>
		/// <param name="verticalResolution">The Image.VerticalResolution</param>
		public Scaling(float horizontalResolution, float verticalResolution)
		{
			// set magic scaling factors
			var (dpiX, dpiY) = UI.Scaling.GetDpiValues();
			ScalingX = dpiX / horizontalResolution;
			ScalingY = dpiY / verticalResolution;

			(FactorX, FactorY) = UI.Scaling.GetScalingFactors();

			//Logger.Current.WriteLine(
			//	$"dpiX={dpiX} dpiY={dpiY} scalingX={scalingX} scalingY={scalingY} sx={sx} sy={sy}");
		}


		public float FactorX { get; private set; }


		public float FactorY { get; private set; }


		public float ScalingX { get; private set; }


		public float ScalingY { get; private set; }


		/// <summary>
		/// Gets the magic ratio, the larger of the horizontal or vertical ratio of the image
		/// </summary>
		/// <param name="image">The image to measure</param>
		/// <param name="boundingWidth">The bounding width in which to paint the image</param>
		/// <param name="boundingHeight">The bounding height in which to paint the image</param>
		/// <param name="margin">The margin to leave around the image</param>
		/// <returns></returns>
		public double GetRatio(Image image, int boundingWidth, int boundingHeight, int margin)
		{
			// return the larger ratio, horizontal or vertical of the image
			return Math.Max(
				// min of scaled image width or pictureBox width without margins
				image.Width / (Math.Min(Math.Round(image.Width * ScalingX), boundingWidth - margin * 2)),
				// min of scaled image height or pictureBox height without margins
				image.Height / (Math.Min(Math.Round(image.Height * ScalingY), boundingHeight - margin * 2))
				);
		}


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
			if (xScalingFactor.EstEquals(0) && yScalingFactor.EstEquals(0))
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
		public static void PrepareUI()
		{
			#region Explanation
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
			#endregion Explanation

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
