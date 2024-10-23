//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Extensions
{
	using System.Drawing;
	using System.Windows.Forms;


	internal static class ScreenExtensions
	{
		public const int ReasonableMargin = 20;


		/// <summary>
		/// Given a form on a screen (e.g. NavigatorWindow), calculate the maximum Left/Top 
		/// coordinate for that form on the screen as if it were corralled the screen's workarea
		/// with a reasonable margin.
		/// </summary>
		/// <param name="screen">The screen within which to corral the form</param>
		/// <param name="form">The form to be corralled</param>
		/// <returns></returns>
		public static Point GetBoundedLocation(this Screen screen, Form form)
		{
			// screen.Bounds is the entire canvas of the display including pinned taskbar.
			// screen.WorkArea is the working canvas of the display excluding the taskbar.
			// Both Bounds and WorkArea have the same Left/Top origin; but note that the work
			// area of a secondary extended display is relative to the primary display so its
			// left-most x coordinate may be greather than zero.

			// useable area of display, excluding taskbar, e.g. the "desktop"
			var area = screen.WorkingArea;

			return new Point(
				area.X + (area.Width - form.Width - ReasonableMargin),
				area.Height - form.Height - ReasonableMargin
				);
		}
	}
}
