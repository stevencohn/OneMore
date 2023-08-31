//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.IO;
	using System.Text;


	internal class ImageDetector
	{
		private readonly byte[] BmpHead;
		private readonly byte[] Gif87aHead;
		private readonly byte[] Gif89aHead;
		private readonly byte[] JpgHead;
		private readonly byte[] JpgTail;
		private readonly byte[] PngHead;
		private readonly byte[] TiffIHead;
		private readonly byte[] TiffMHead;

		public ImageDetector()
		{
			BmpHead = Encoding.ASCII.GetBytes("BM");                // BMP "BM { 0x42, 0x4D }
			Gif87aHead = Encoding.ASCII.GetBytes("GIF87a");         // GIF87a  { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }
			Gif89aHead = Encoding.ASCII.GetBytes("GIF89a");         // GIF89a  { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }
			JpgHead = new byte[] { 0xFF, 0xD8, 0xFF };				// JPEG JFIF (SOI "\xFF\xD8" and half next marker xFF)
			JpgTail = new byte[] { 0xFF, 0xD9 };                    // JPEG EOI "\xFF\xD9"
			PngHead = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG "\x89PNG\x0D\0xA\0x1A\0x0A"
			TiffIHead = new byte[] { 0x49, 0x49, 0x2A, 0x00 };      // TIFF II "II\x2A\x00"
			TiffMHead = new byte[] { 0x4D, 0x4D, 0x00, 0x2A };      // TIFF MM "MM\x00\x2A"
		}


		/// <summary>
		/// Reads the header of different image formats
		/// </summary>
		/// <param name="file">Image file</param>
		/// <returns>true if valid file signature (magic number/header marker) is found</returns>
		public ImageSignature GetSignature(MemoryStream stream)
		{
			try
			{
				// this is the internal buffer by reference
				var buffer = stream.GetBuffer();

				if (buffer.StartsWith(BmpHead))
					return ImageSignature.BMP;

				if (buffer.StartsWith(Gif87aHead) || buffer.StartsWith(Gif89aHead))
					return ImageSignature.GIF;

				if (buffer.StartsWith(PngHead))
					return ImageSignature.PNG;

				if (buffer.StartsWith(TiffIHead) || buffer.StartsWith(TiffMHead))
					return ImageSignature.TIFF;

				if (buffer.StartsWith(JpgHead))
				{
					// Offset 0 (Two Bytes): JPEG SOI marker (FFD8 hex)
					// Offest 1 (Two Bytes): Application segment (FF?? normally ??=E0)
					// Trailer (Last Two Bytes): EOI marker FFD9 hex
					if (buffer.EndsWith(JpgTail))
					{
						return ImageSignature.JPG;
					}
				}
			}
			catch
			{
				// no-op
			}

			return ImageSignature.Unknown;
		}
	}
}
