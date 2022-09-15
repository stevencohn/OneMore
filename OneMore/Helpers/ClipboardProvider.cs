//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************                

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Media.Imaging;
	using Win = System.Windows;


	internal class ClipboardProvider
	{
		private readonly object gate;
		private readonly Dictionary<Win.TextDataFormat, string> stash;
		private BitmapSource stashedImage;


		public ClipboardProvider()
		{
			gate = new object();
			stash = new Dictionary<Win.TextDataFormat, string>();
		}


		public async Task<string> GetHtml()
		{
			return await SingleThreaded.Invoke(() =>
			{
				return Win.Clipboard.ContainsText(Win.TextDataFormat.Html)
					? Win.Clipboard.GetText(Win.TextDataFormat.Html)
					: null;
			});
		}


		public async Task<string> GetText()
		{
			return await SingleThreaded.Invoke(() =>
			{
				return Win.Clipboard.ContainsText(Win.TextDataFormat.Text)
					? Win.Clipboard.GetText(Win.TextDataFormat.Text)
					: null;
			});
		}


		public async Task SetHtml(string text)
		{
			await SingleThreaded.Invoke(() =>
			{
				// avoids 0x800401D0/CLIPBRD_E_CANT_OPEN due to thread contentions
				lock (gate)
				{
					Win.Clipboard.SetText(text, Win.TextDataFormat.Html);
				}
			});
		}


		public async Task SetText(string text)
		{
			await SingleThreaded.Invoke(() =>
			{
				// avoids 0x800401D0/CLIPBRD_E_CANT_OPEN due to thread contentions
				lock (gate)
				{
					Win.Clipboard.SetText(text, Win.TextDataFormat.Text);
				}
			});
		}


		public async Task StashState()
		{
			stash.Clear();
			stashedImage = null;

			await SingleThreaded.Invoke(() =>
			{
				// prioritize images
				if (Win.Clipboard.ContainsImage())
				{
					stashedImage = Win.Clipboard.GetImage();
				}

				// collect each text format
				foreach (Win.TextDataFormat format in Enum.GetValues(typeof(Win.TextDataFormat)))
				{
					if (Win.Clipboard.ContainsText(format))
					{
						stash.Add(format, Win.Clipboard.GetText(format));
					}
				}

				// TODO: other formats, e.g. files?
			});
		}


		public async Task RestoreState()
		{
			await SingleThreaded.Invoke(() =>
			{
				// avoids 0x800401D0/CLIPBRD_E_CANT_OPEN due to thread contentions
				lock (gate)
				{
					// multiple formats must be collated into a single data object
					var data = new Win.DataObject();
					var something = false;

					if (stashedImage != null)
					{
						data.SetImage(stashedImage);
						something = true;
					}

					if (stash.Count > 0)
					{
						foreach (var key in stash.Keys)
						{
							data.SetData(ConvertToDataFormats(key), stash[key]);
							something = true;
						}
					}

					if (something)
					{
						Win.Clipboard.SetDataObject(data, true);
					}
				}
			});

			stash.Clear();
		}


		// duplicates the internal System.Windows.DataFormats.ConvertToDataFormats method
		private static string ConvertToDataFormats(Win.TextDataFormat textDataformat)
		{
			var result = Win.DataFormats.UnicodeText;
			switch (textDataformat)
			{
				case Win.TextDataFormat.Text:
					result = Win.DataFormats.Text;
					break;
				case Win.TextDataFormat.UnicodeText:
					result = Win.DataFormats.UnicodeText;
					break;
				case Win.TextDataFormat.Rtf:
					result = Win.DataFormats.Rtf;
					break;
				case Win.TextDataFormat.Html:
					result = Win.DataFormats.Html;
					break;
				case Win.TextDataFormat.CommaSeparatedValue:
					result = Win.DataFormats.CommaSeparatedValue;
					break;
				case Win.TextDataFormat.Xaml:
					result = Win.DataFormats.Xaml;
					break;
			}

			return result;
		}
	}
}
