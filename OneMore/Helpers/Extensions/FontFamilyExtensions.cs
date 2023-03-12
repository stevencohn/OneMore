//************************************************************************************************
// Copyright © 2022 Waters Corporation. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;


	internal static class FontFamilyExtensions
	{

		/// <summary>
		/// OneMore Extension >> Determines if the given font family represents a
		/// fixed-width/mono-spaced font.
		/// </summary>
		/// <param name="family">A FontFamily instance</param>
		/// <returns>True if the given font familly represents a fixed-width font</returns>
		public static bool IsFixedWidthFont(this FontFamily family)
		{
			using var image = new Bitmap(1, 1);
			using var g = Graphics.FromImage(image);
			var dc = g.GetHdc();

			var handle = IntPtr.Zero;
			var defaultHandle = IntPtr.Zero;
			var fixedWidth = false;

			using var font = new Font(family, 10);

			try
			{
				handle = font.ToHfont();
				defaultHandle = Native.SelectObject(dc, handle);

				if (Native.GetTextMetrics(dc, out var metrics))
				{
					fixedWidth = (metrics.tmPitchAndFamily & 1) == 0;
				}
			}
			finally
			{
				if (defaultHandle != IntPtr.Zero)
				{
					Native.SelectObject(dc, defaultHandle);
				}

				if (handle != IntPtr.Zero)
				{
					Native.DeleteObject(handle);
				}

				g.ReleaseHdc();
			}

			return fixedWidth;
		}
	}
}