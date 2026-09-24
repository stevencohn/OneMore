//************************************************************************************************
// Copyright © 2026 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Helpers to scale literal pixel constants used throughout custom-drawn controls so they
	/// render at the same physical size on high-DPI monitors as they do at 96 DPI, now that the
	/// process is genuinely per-monitor DPI aware and Windows no longer bitmap-stretches the UI
	/// to compensate.
	/// </summary>
	internal static class DpiExtensions
	{
		public static float ScaleFactor(this Control control)
		{
			return control.DeviceDpi / 96f;
		}

		public static int Scaled(this Control control, int value)
		{
			return (int)Math.Round(value * control.ScaleFactor());
		}

		public static float Scaled(this Control control, float value)
		{
			return value * control.ScaleFactor();
		}

		/// <summary>
		/// Scales a layout that was authored in the designer at the given DPI to this control's
		/// actual DPI. Sizes and locations of the control and its children are scaled; fonts
		/// are not touched.
		/// </summary>
		/// <param name="control">The control, typically a Form, to scale</param>
		/// <param name="designDpi">The DPI at which the layout was authored</param>
		public static void ScaleLayout(this Control control, float designDpi)
		{
			var factor = control.DeviceDpi / designDpi;
			if (Math.Abs(factor - 1f) > 0.01f)
			{
				control.Scale(new SizeF(factor, factor));
			}
		}
	}
}
