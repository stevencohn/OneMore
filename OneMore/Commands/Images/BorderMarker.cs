//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;


	/// <summary>
	/// Describes the geometry AddBorderCommand produced, so RemoveBorderCommand can undo it
	/// exactly instead of guessing from pixels.
	/// </summary>
	internal readonly struct BorderMarkerInfo
	{
		public BorderMarkerInfo(bool shadow, int border, Rectangle content)
		{
			Shadow = shadow;
			Border = border;
			Content = content;
		}

		public bool Shadow { get; }
		public int Border { get; }
		public Rectangle Content { get; }
	}


	/// <summary>
	/// Reads and writes a tiny invisible marker in the bottom-right corner of an image.
	/// The marker is four pixels, each with alpha 1 (imperceptible) and 3 payload bytes in
	/// the R, G, B channels, for 12 bytes total: magic, version|flags, then border, content
	/// left, top, width and height as 16-bit big-endian values.
	/// </summary>
	internal static class BorderMarker
	{
		private const byte Magic = 0xB7;
		private const byte Version = 1;
		private const byte FlagShadow = 0x01;
		private const int Pixels = 4;
		private const int Bytes = Pixels * 3;


		/// <summary>
		/// Minimum number of pixels the canvas needs in each dimension to hold the marker.
		/// </summary>
		public const int Span = Pixels;


		/// <summary>
		/// Writes the marker into the last row, rightmost pixels of the bitmap. Call after
		/// all drawing is complete so nothing blends over it.
		/// </summary>
		public static void Write(Bitmap bitmap, BorderMarkerInfo info)
		{
			var payload = Pack(info);

			var rect = new Rectangle(0, bitmap.Height - 1, bitmap.Width, 1);
			var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			try
			{
				var row = new byte[bitmap.Width * 4];
				Marshal.Copy(data.Scan0, row, 0, row.Length);

				for (var p = 0; p < Pixels; p++)
				{
					var offset = (bitmap.Width - Pixels + p) * 4;
					row[offset + 0] = payload[(p * 3) + 2]; // B
					row[offset + 1] = payload[(p * 3) + 1]; // G
					row[offset + 2] = payload[(p * 3) + 0]; // R
					row[offset + 3] = 1;                    // A
				}

				Marshal.Copy(row, 0, data.Scan0, row.Length);
			}
			finally
			{
				bitmap.UnlockBits(data);
			}
		}


		/// <summary>
		/// Reads and validates the marker; returns false if absent or inconsistent.
		/// </summary>
		public static bool TryRead(Bitmap bitmap, out BorderMarkerInfo info)
		{
			info = default;

			if (bitmap.Width < Pixels || bitmap.Height < 1)
			{
				return false;
			}

			var rect = new Rectangle(0, bitmap.Height - 1, bitmap.Width, 1);
			var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			var payload = new byte[Bytes];
			try
			{
				var row = new byte[bitmap.Width * 4];
				Marshal.Copy(data.Scan0, row, 0, row.Length);

				for (var p = 0; p < Pixels; p++)
				{
					var offset = (bitmap.Width - Pixels + p) * 4;
					if (row[offset + 3] != 1)
					{
						return false;
					}

					payload[(p * 3) + 0] = row[offset + 2]; // R
					payload[(p * 3) + 1] = row[offset + 1]; // G
					payload[(p * 3) + 2] = row[offset + 0]; // B
				}
			}
			finally
			{
				bitmap.UnlockBits(data);
			}

			return Unpack(payload, bitmap.Width, bitmap.Height, out info);
		}


		// layout: 0 magic, 1 version|flags, 2-3 border, 4-5 left, 6-7 top, 8-9 width,
		// 10-11 height; validated by magic, version, alpha 1 and bounds on read
		private static byte[] Pack(BorderMarkerInfo info)
		{
			var b = new byte[Bytes];
			b[0] = Magic;
			b[1] = (byte)((Version << 4) | (info.Shadow ? FlagShadow : 0));
			b[2] = (byte)(info.Border >> 8);
			b[3] = (byte)info.Border;
			b[4] = (byte)(info.Content.X >> 8);
			b[5] = (byte)info.Content.X;
			b[6] = (byte)(info.Content.Y >> 8);
			b[7] = (byte)info.Content.Y;
			b[8] = (byte)(info.Content.Width >> 8);
			b[9] = (byte)info.Content.Width;
			b[10] = (byte)(info.Content.Height >> 8);
			b[11] = (byte)info.Content.Height;
			return b;
		}


		private static bool Unpack(byte[] b, int width, int height, out BorderMarkerInfo info)
		{
			info = default;

			if (b[0] != Magic || (b[1] >> 4) != Version)
			{
				return false;
			}

			var border = (b[2] << 8) | b[3];
			var left = (b[4] << 8) | b[5];
			var top = (b[6] << 8) | b[7];
			var w = (b[8] << 8) | b[9];
			var h = (b[10] << 8) | b[11];

			if (w <= 0 || h <= 0 || left + w > width || top + h > height)
			{
				return false;
			}

			info = new BorderMarkerInfo(
				(b[1] & FlagShadow) != 0, border, new Rectangle(left, top, w, h));

			return true;
		}
	}
}
