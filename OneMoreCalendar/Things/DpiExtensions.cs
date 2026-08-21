//************************************************************************************************
// Copyright © 2026 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
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
	}
}
