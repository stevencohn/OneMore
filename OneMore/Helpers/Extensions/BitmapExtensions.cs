//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Runtime.InteropServices.ComTypes;


	internal static class BitmapExtensions
	{

		/// <summary>
		/// OneMore Extension >> Gets a readonly stream from the given bitmap.
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static IStream GetReadOnlyStream(this Bitmap bitmap)
		{
			ReadOnlyStream stream = null;

			try
			{
				var memory = new MemoryStream();
				bitmap.Save(memory, ImageFormat.Png);
				stream = new ReadOnlyStream(memory);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc);
			}

			return stream;
		}
	}
}
