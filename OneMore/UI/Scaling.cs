//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Some helper functions for dealing with Windows Forms on High DPI systems.
	/// </summary>

	internal class Scaling
	{
		private static bool unprepared = true;


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

			//Logger.Current.Verbose(
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
			var dpi = GetPrimaryMonitorDpi();
			if (dpi > 0)
			{
				return (dpi, dpi);
			}

			using var graphics = Graphics.FromHwnd(IntPtr.Zero);
			return (graphics.DpiX, graphics.DpiY);
		}


		/// <summary>
		/// Get the HighDPI scaling factor for both width and height of the primary monitor.
		/// Should be 1.0 for non-HighDPI monitors. This is read live on every call, never
		/// cached, because the DPI can change while OneNote is running, e.g. when attaching
		/// to or detaching from a remote desktop session or changing the display scale.
		/// Prefer <see cref="GetScalingFactors(Control)"/> when a control is available.
		/// </summary>
		/// <returns>A Tuple with xScalingFactor and yScalingFactor</returns>
		public static (float, float) GetScalingFactors()
		{
			var (dpiX, dpiY) = GetDpiValues();
			return (dpiX / 96f, dpiY / 96f); // 1.25 = 125%
		}


		/// <summary>
		/// Get the HighDPI scaling factor for the monitor currently hosting the given control.
		/// </summary>
		/// <param name="control">A control whose actual DPI is used, see GetDpi</param>
		/// <returns>A Tuple with xScalingFactor and yScalingFactor</returns>
		public static (float, float) GetScalingFactors(Control control)
		{
			var factor = GetDpi(control) / 96f;
			return (factor, factor); // 1.25 = 125%
		}


		/// <summary>
		/// Gets the actual DPI of the monitor hosting the given control.
		/// </summary>
		/// <remarks>
		/// Do not use Control.DeviceDpi. WinForms initializes it from the system DPI, which is
		/// fixed for the entire logon session. If the session started or was attached at a
		/// different DPI, e.g. over RDP at 96, DeviceDpi stays at that value even though the
		/// process is per-monitor aware and the monitor is, say, 144. The window's own DPI
		/// is always correct once it has a handle; before that, use the primary monitor.
		/// </remarks>
		/// <param name="control">The control, or any control within the same window</param>
		public static int GetDpi(Control control)
		{
			try
			{
				for (var c = control; c is not null; c = c.Parent)
				{
					if (c.IsHandleCreated)
					{
						var dpi = Native.GetDpiForWindow(c.Handle);
						if (dpi > 0)
						{
							return (int)dpi;
						}

						break;
					}
				}
			}
			catch (EntryPointNotFoundException)
			{
				// Windows older than 10 1607; fall through
			}

			var primary = GetPrimaryMonitorDpi();
			return primary > 0 ? primary : control.DeviceDpi;
		}


		/// <summary>
		/// Gets a system metric, such as <see cref="Native.SM_CXVSCROLL"/>, at the given DPI.
		/// Unlike SystemInformation.*, which reflects the system DPI captured when the process
		/// started, this honors the per-monitor DPI.
		/// </summary>
		/// <param name="index">One of the Native.SM_* indexes</param>
		/// <param name="dpi">Typically <see cref="GetDpi(Control)"/></param>
		public static int GetSystemMetric(int index, int dpi)
		{
			try
			{
				var value = Native.GetSystemMetricsForDpi(index, (uint)dpi);
				if (value > 0)
				{
					return value;
				}
			}
			catch (EntryPointNotFoundException)
			{
				// Windows older than 10 1607; fall through
			}

			// scale the metric at the process's own DPI
			var current = (int)GetDpiValues().Item1;
			var metric = Native.GetSystemMetrics(index);
			return current > 0 ? (int)Math.Round(metric * (dpi / (float)current)) : metric;
		}


		/// <summary>
		/// Scales the font of descendants that do not inherit the pinned ambient font, e.g.
		/// a designer-assigned font, or a ToolStrip which uses the system font. The ambient
		/// font itself is sized by MoreForm and MoreUserControl, see GetAmbientFontScale.
		/// </summary>
		/// <param name="root">The form or user control whose descendants are scaled</param>
		/// <param name="factor">The multiplier to apply to each font size</param>
		/// <param name="scaled">
		/// The controls and tool strip items whose fonts were scaled before, owned by the caller
		/// </param>
		/// <param name="changeOnly">
		/// False to scale every explicit font not yet in <paramref name="scaled"/> and add it;
		/// true to rescale only those already in it, after a DPI change
		/// </param>
		public static void ScaleExplicitFonts(
			Control root, float factor, HashSet<object> scaled, bool changeOnly)
		{
			if (factor <= 0 || (changeOnly && factor.EstEquals(1f)))
			{
				return;
			}

			void Apply(object target, Func<Font> get, Action<Font> set)
			{
				// controls are recorded even when the factor is 1, so that a later DPI change
				// knows they have a non-ambient font that needs rescaling
				if (changeOnly ? !scaled.Contains(target) : !scaled.Add(target))
				{
					return;
				}

				if (factor.EstEquals(1f))
				{
					return;
				}

				var font = get();
				set(new Font(
					font.FontFamily, font.SizeInPoints * factor, font.Style,
					GraphicsUnit.Point, font.GdiCharSet));
			}

			void Walk(Control parent)
			{
				foreach (Control child in parent.Controls)
				{
					// a nested user control pins and scales its own ambient font and
					// tracks its own descendants
					if (child is MoreUserControl)
					{
						continue;
					}

					var inherits = ReferenceEquals(child.Font, parent.Font);
					var current = child;
					if (!inherits || (changeOnly && scaled.Contains(child)))
					{
						Apply(child, () => current.Font, f => current.Font = f);
					}

					if (child is ToolStrip strip)
					{
						foreach (ToolStripItem item in strip.Items)
						{
							if (!ReferenceEquals(item.Font, strip.Font) ||
								(changeOnly && scaled.Contains(item)))
							{
								var currentItem = item;
								Apply(item, () => currentItem.Font, f => currentItem.Font = f);
							}
						}
					}

					Walk(child);
				}
			}

			Walk(root);
		}


		/// <summary>
		/// Gets the system DPI, which is fixed when the process starts and is what WinForms
		/// uses to measure fonts for AutoScaleMode.Font, or 0 if it can't be determined.
		/// </summary>
		public static int GetSystemDpi()
		{
			try
			{
				return (int)Native.GetDpiForSystem();
			}
			catch (EntryPointNotFoundException)
			{
				return 0;
			}
		}


		/// <summary>
		/// Gets the multiplier to apply to the pinned ambient font size so that layouts
		/// authored at a given DPI come out right at <paramref name="actualDpi"/>.
		/// </summary>
		/// <remarks>
		/// Font sizes are converted to pixels with the system DPI, which can differ from the
		/// DPI of the monitor actually displaying the window, e.g. after attaching to or
		/// detaching from a remote session or changing the display scale while OneNote runs.
		/// Scaling the point size by actual/system makes AutoScaleMode.Font measure and lay
		/// out as if the system DPI matched the monitor. Returns 1.0 when they already match.
		/// </remarks>
		/// <param name="actualDpi">The DPI of the monitor, e.g. from GetDpi</param>
		public static float GetAmbientFontScale(int actualDpi)
		{
			var system = GetSystemDpi();
			return system > 0 && actualDpi > 0 ? actualDpi / (float)system : 1f;
		}


		/// <summary>
		/// Reads the effective DPI of the primary monitor, or 0 if it can't be determined.
		/// </summary>
		private static int GetPrimaryMonitorDpi()
		{
			try
			{
				var monitor = Native.MonitorFromWindow(
					IntPtr.Zero, Native.MONITOR_DEFAULTTOPRIMARY);

				if (monitor != IntPtr.Zero &&
					Native.GetDpiForMonitor(
						monitor, Native.MDT_EFFECTIVE_DPI, out uint dpiX, out _) == 0 &&
					dpiX > 0)
				{
					return (int)dpiX;
				}
			}
			catch (EntryPointNotFoundException)
			{
				// Windows older than 8.1; fall through
			}
			catch (DllNotFoundException)
			{
				// shcore.dll is missing; fall through
			}

			return 0;
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
				EnablePerMonitorDpiAwareness();

				// Documented escape hatch for code that cannot rely on the app.config
				// EnableWindowsFormsHighDpiAutoResizing appSetting; must run before any
				// Form/Control is constructed, which is guaranteed since this one of the
				// first statements executed by AddIn()'s constructor.
				AppContext.SetSwitch(
					"Switch.System.Windows.Forms.EnableWindowsFormsHighDpiAutoResizing", true);

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				unprepared = false;
			}
		}


		/// <summary>
		/// OneMore is COM-activated into the generic dllhost.exe surrogate rather than
		/// run as its own process, so the declarative DpiAwareness setting in App.config
		/// and the dpiAware entry in app.manifest are never read by Windows; only a live
		/// API call made inside the actual host process takes effect. Try the modern
		/// per-monitor APIs first, falling back through older Windows versions down to
		/// the legacy System DPI Aware call.
		/// </summary>
		private static void EnablePerMonitorDpiAwareness()
		{
			try
			{
				if (Native.SetProcessDpiAwarenessContext(
					Native.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
				{
					Logger.Current.Verbose(
						"Startup: DPI awareness - PerMonitorV2 (SetProcessDpiAwarenessContext)");
					return;
				}
			}
			catch (EntryPointNotFoundException)
			{
				// Windows older than 10 1703; fall through
			}

			try
			{
				if (Native.SetProcessDpiAwareness(Native.PROCESS_PER_MONITOR_DPI_AWARE) == 0)
				{
					Logger.Current.Verbose(
						"Startup: DPI awareness - PerMonitor (SetProcessDpiAwareness)");
					return;
				}
			}
			catch (EntryPointNotFoundException)
			{
				// Windows older than 8.1; fall through
			}

			Native.SetProcessDPIAware();
			Logger.Current.Verbose(
				"Startup: DPI awareness - System (legacy SetProcessDPIAware fallback)");
		}
	}
}
