//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System;
	using System.Runtime.InteropServices;


	/// <summary>
	/// Some extension methods for the ribbon...
	/// </summary>

	internal static class RibbonExtensions
	{

		/// <summary>
		/// OneMore Extension >> Invalidate the ribbon, swallowing the COM interop
		/// failure that can occur if OneNote's COM surrogate has already begun
		/// tearing down. This surfaces as an InvalidCastException (QueryInterface
		/// failing with RPC_SERVER_UNAVAILABLE) or as a COMException on the cached
		/// IRibbonUI proxy. Ribbon refresh is cosmetic, so failures here are logged
		/// and ignored rather than propagated as command errors.
		/// </summary>
		/// <param name="ribbon">The ribbon to invalidate</param>

		public static void SafeInvalidate(this IRibbonUI ribbon)
		{
			try
			{
				ribbon?.Invalidate();
			}
			catch (InvalidCastException exc)
			{
				Logger.Current.WriteLine("ribbon invalidate failed, ignoring", exc);
			}
			catch (COMException exc)
			{
				Logger.Current.WriteLine("ribbon invalidate failed, ignoring", exc);
			}
		}


		/// <summary>
		/// OneMore Extension >> Invalidate a single ribbon control, swallowing the same
		/// COM interop failure described in <see cref="SafeInvalidate"/>.
		/// </summary>
		/// <param name="ribbon">The ribbon owning the control</param>
		/// <param name="controlId">The ID of the control to invalidate</param>

		public static void SafeInvalidateControl(this IRibbonUI ribbon, string controlId)
		{
			try
			{
				ribbon?.InvalidateControl(controlId);
			}
			catch (InvalidCastException exc)
			{
				Logger.Current.WriteLine("ribbon invalidate control failed, ignoring", exc);
			}
			catch (COMException exc)
			{
				Logger.Current.WriteLine("ribbon invalidate control failed, ignoring", exc);
			}
		}
	}
}
