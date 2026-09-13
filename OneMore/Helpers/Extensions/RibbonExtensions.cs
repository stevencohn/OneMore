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
	}
}
