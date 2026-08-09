//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Extensions
{
	using System;
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


		/// <summary>
		/// Calculate a Left/Top location for a popup of the given size, positioned beside
		/// the given anchor rectangle (e.g. a word or caret on the OneNote page) so the
		/// anchor remains visible. Prefers showing to the right of the anchor, flipping to
		/// the left if that would run off the screen; vertically aligned with the anchor and
		/// clamped to stay within the working area.
		/// </summary>
		/// <param name="screen">The screen within which to corral the popup</param>
		/// <param name="anchor">The screen-coordinate rectangle to show the popup beside</param>
		/// <param name="formSize">The size of the popup to be positioned</param>
		/// <returns></returns>
		public static Point GetBoundedLocationNear(this Screen screen, Rectangle anchor, Size formSize)
		{
			var area = screen.WorkingArea;

			// prefer showing to the right of the anchor; flip to the left if it would
			// otherwise run off the right edge of the working area
			var x = anchor.Right + ReasonableMargin;
			if (x + formSize.Width > area.Right)
			{
				x = anchor.Left - ReasonableMargin - formSize.Width;
			}

			x = Math.Max(area.X, Math.Min(x, area.Right - formSize.Width));

			var y = Math.Max(area.Y, Math.Min(anchor.Top, area.Bottom - formSize.Height));

			return new Point(x, y);
		}
	}
}
