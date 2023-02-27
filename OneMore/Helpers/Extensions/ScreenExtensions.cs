//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Extensions
{
	using System.Drawing;
	using System.Windows.Forms;


	internal static class ScreenExtensions
	{
		private const int ReasonableMargin = 20;


		public static Point GetBoundedLocation(this Screen screen, Form form)
		{
			// While Bounds is the entire screen with zero-based coordinates from upper left,
			// WorkingArea is the subset of Bound minus space for the taskbar; so it shares the
			// same origin of upper-left of screen but Top,Left,Right,Bottom may be adjusted
			// to accomodate the position of the taskbar...

			var area = screen.WorkingArea;

			// Note also that the working area of an secondary extended display is relative to
			// the primary display so its left-most X coordinate may be greater than zero...

			return new Point(
				area.X + (area.Width - form.Width - ReasonableMargin),
				area.Height - form.Height - ReasonableMargin
				);
		}
	}
}
