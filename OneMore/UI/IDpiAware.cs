//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	/// <summary>
	/// Implemented by controls that cache DPI-dependent pixel values, such as row heights,
	/// icon sizes or glyph sizes, so they can be recalculated when the DPI changes while the
	/// control is alive, e.g. after attaching to or detaching from a remote desktop session.
	/// MoreForm calls this on every descendant control after WinForms has finished rescaling.
	/// </summary>
	internal interface IDpiAware
	{
		/// <summary>
		/// Recalculate any cached, DPI-dependent values. The control's DeviceDpi already
		/// reflects the new DPI when this is called.
		/// </summary>
		/// <param name="oldDpi">The previous DPI, e.g. 96 or 144</param>
		/// <param name="newDpi">The new DPI</param>
		void OnDpiChanged(int oldDpi, int newDpi);
	}
}
