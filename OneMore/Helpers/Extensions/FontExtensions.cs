//************************************************************************************************
// Copyright © 2022 Waters Corporation. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;

	internal static class FontExtensions
	{

		/// <summary>
		/// Determines if the given font is a fixed-width/mono-spaced font.
		/// </summary>
		/// <param name="font">A Font instance</param>
		/// <returns>True if the given font is fixed-width</returns>
		public static bool IsFixedWidthFont(this Font font)
		{
			using var image = new Bitmap(1, 1);
			using var g = Graphics.FromImage(image);
			var dc = g.GetHdc();

			var handle = IntPtr.Zero;
			var defaultHandle = IntPtr.Zero;
			var fixedWidth = false;

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


