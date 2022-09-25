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
	using WindowsInput;
	using WindowsInput.Native;
	using Win = System.Windows;


	/// <summary>
	/// Provides wrapped STA access to the Windows clipboard for use in OneNote's MTA environment
	/// </summary>
	internal class ClipboardProvider
	{
		private readonly object gate;
		private readonly Dictionary<Win.TextDataFormat, string> stash;
		private BitmapSource stashedImage;


		/// <summary>
		/// Initialize a new instance of the provider with an empty stash
		/// </summary>
		public ClipboardProvider()
		{
			gate = new object();
			stash = new Dictionary<Win.TextDataFormat, string>();
		}



		/// <summary>
		/// Initiates a copy operation by emitting a Ctrl+C keypress and delays the current
		/// thread so that Windows and the active application have time to complete the copy
		/// </summary>
		/// <returns></returns>
		public async Task Copy()
		{
			//SendKeys.SendWait("^(c)");
			new InputSimulator().Keyboard
				.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

			// wait for Windows to stabilize the clipboard
			await Task.Delay(200);
		}


		/// <summary>
		/// Returns any HTML content stored on the clipboard
		/// </summary>
		/// <returns>A string of HTML or null if the clipboard do not contain HTML</returns>
		public async Task<string> GetHtml()
		{
			return await SingleThreaded.Invoke(() =>
			{
				return Win.Clipboard.ContainsText(Win.TextDataFormat.Html)
					? Win.Clipboard.GetText(Win.TextDataFormat.Html)
					: null;
			});
		}


		/// <summary>
		/// Returns any plain text content stored on the clipboard
		/// </summary>
		/// <returns>A string plain text or null if the clipboard does not contain text</returns>
		public async Task<string> GetText()
		{
			return await SingleThreaded.Invoke(() =>
			{
				return Win.Clipboard.ContainsText(Win.TextDataFormat.Text)
					? Win.Clipboard.GetText(Win.TextDataFormat.Text)
					: null;
			});
		}


		/// <summary>
		/// Initiates a paste operation by emitting a Ctrl+V keypress and delays the current
		/// thread so that Windows and the active application have time to complete the paste
		/// </summary>
		/// <param name="delayBefore">
		/// Adds a delay prior to the paste for cases where we need to wait for preceding
		/// operations to stabilize
		/// </param>
		/// <returns></returns>
		public async Task Paste(bool delayBefore = false)
		{
			if (delayBefore)
			{
				await Task.Delay(200);
			}

			new InputSimulator().Keyboard
				.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

			// yeah this is dumb but have to wait for paste to complete
			await Task.Delay(200);
		}


		/// <summary>
		/// Restores the state of the clipboard to the content preserved using StashState()
		/// </summary>
		/// <returns></returns>
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


		/// <summary>
		/// Stores the given HTML on the clipboard; all other content is replaced
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
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


		/// <summary>
		/// Stores the given plain text on the clipboard; all other content is replaced
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
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


		/// <summary>
		/// Stashes the entire state of and clears out the clipboard so that it can be used
		/// temporarily. Remember to restore the state by calling RestoreState()
		/// </summary>
		/// <returns></returns>
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
	}
}
