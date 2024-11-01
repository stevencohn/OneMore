//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.IO;
	using System.Windows.Media.Imaging;


	/// <summary>
	/// Manages the metadata iTxt entry of an image data string.
	/// </summary>
	/// <remarks>
	/// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
	/// iTxt metadata does not survive roundtrips from byte[] > Image > byte[] but, luckily,
	/// we only need to manage the Base64(byte[]) in OneNote so can retain the metadata for
	/// storing and extracting.
	/// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
	/// https://learn.microsoft.com/en-us/windows/win32/wic/-wic-native-image-format-metadata-queries?redirectedfrom=MSDN#png-metadata
	/// </remarks>
	internal class ImageMetaTextEditor : Loggable
	{
		private const string PlantUmlEnd = "@enduml";

		private byte[] bytes;


		/// <summary>
		/// Initializes a new instance with the given byte array.
		/// </summary>
		/// <param name="bytes">The image unencoded byte array</param>
		public ImageMetaTextEditor(byte[] bytes)
		{
			this.bytes = bytes;
		}


		/// <summary>
		/// Initializes a new instance with the given Base64 image data string.
		/// </summary>
		/// <param name="data">The image Data as a base64 encoding</param>
		public ImageMetaTextEditor(string data)
		{
			bytes = Convert.FromBase64String(data);
		}


		/// <summary>
		/// Gest the unencoded byte array of the image
		/// </summary>
		public byte[] Bytes => bytes;


		/// <summary>
		/// Gets the image Data as a base64 encoding of the byte array
		/// </summary>
		public string Data => Convert.ToBase64String(bytes);


		/// <summary>
		/// Gets the keyword found in the metadata iTxt entry.
		/// </summary>
		public string Keyword { get; private set; }


		/// <summary>
		/// Read and return the metadata iTxt entry from the image data string.
		/// Sets the Keyword property with a known keyword, e.g. mermaid or plantuml.
		/// </summary>
		/// <returns>The TextEntry value</returns>
		public string ReadImageMetaTextEntry()
		{
			try
			{
				using var stream = new MemoryStream(bytes, 0, bytes.Length);

				var decoder = new PngBitmapDecoder(stream,
					BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

				var meta = (BitmapMetadata)decoder.Frames[0].Metadata;
				var result = meta.GetQuery("/iTXt");
				if (result is not null)
				{
					Keyword = meta.GetQuery("/iTXt/Keyword")?.ToString();
					//keyword.Dump("ExtractTxtFromImageData(byte[]) /iTxt/Keyword");
					if (Keyword == DiagramKeys.MermaidKey || Keyword == DiagramKeys.PlantUmlKey)
					{
						var text = meta.GetQuery("/iTXt/TextEntry")?.ToString();
						//text.Dump("ExtractTxtFromImageData(byte[]) /iTxt/TextEntry");
						if (!string.IsNullOrWhiteSpace(text))
						{
							if (Keyword == DiagramKeys.PlantUmlKey)
							{
								var end = text.IndexOf(PlantUmlEnd);
								if (end > 0)
								{
									return text.Substring(0, end + PlantUmlEnd.Length);
								}
							}

							return text;
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error reading image meta TextEntry", exc);
			}

			return null;
		}


		/// <summary>
		/// Writes the given text to the metadata TextEntry for the given keyword.
		/// Updates the Bytes property.
		/// </summary>
		/// <param name="text">The text to write</param>
		/// <param name="keyword">The iTxt keyword to apply</param>
		/// <returns>The base64 encoding of the byte array</returns>
		public string WriteImageMetaTextEntry(string text, string keyword)
		{
			try
			{
				using var stream = new MemoryStream(bytes);
				var frame = BitmapFrame.Create(stream);

				var meta = (BitmapMetadata)frame.Metadata.Clone();
				meta.SetQuery("/iTXt/Keyword", keyword.ToCharArray());
				meta.SetQuery("/iTXt/TextEntry", text);

				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(frame, frame.Thumbnail, meta, frame.ColorContexts));

				using var outstream = new MemoryStream();
				encoder.Save(outstream);

				bytes = outstream.GetBuffer();
				return Data;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error writing image meta TextEntry", exc);
				return null;
			}
		}
	}
}
