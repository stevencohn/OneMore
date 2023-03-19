//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************                

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
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
		private const int PreambleSize = 105;
		private const char Space = '\u00a0'; // Unicode no-break space
		private const string StartFragment = "<!--StartFragment-->";
		private const string EndFragment = "<!--EndFragment-->";

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
		/// Stash the given value. Can be used to build up a multi-entry stash that can then
		/// be rehydrated onto the clipbard using RestoreState().
		/// Presumes StashState has not been called.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="text"></param>
		public void Stash(Win.TextDataFormat format, string text)
		{
			if (stash.ContainsKey(format))
			{
				stash[format] = text;
			}
			else
			{
				stash.Add(format, text);
			}
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
					try
					{
						stashedImage = Win.Clipboard.GetImage();
					}
					catch
					{
						stashedImage = null;
					}
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


		/// <summary>
		/// Wraps an HTML fragment in a container including a preamble that describes its
		/// size. This is a prescribed Clipboard form described by
		/// https://docs.microsoft.com/en-us/windows/win32/dataxchg/html-clipboard-format
		/// </summary>
		/// <param name="html">
		/// The HTML fragment to wrap. Can include HTML/BODY/StartFragment-EndFragment but if not
		/// then it will be wrapped
		/// </param>
		/// <returns>A new string</returns>
		public static string WrapWithHtmlPreamble(string html)
		{
			/* Version:0.9
			 * StartHTML:0000000071
			 * EndHTML:0000000170
			 * StartFragment:0000000140  (this starts just after the StartFragment comment)
			 * EndFragment:0000000160	 (this starts just before the EndFragment comment)
			 * <html>
			 * <body>
			 * <!--StartFragment--> ... <!--EndFragment-->
			 * </body>
			 * </html>
			 */

			System.Diagnostics.Debugger.Launch();

			var builder = new StringBuilder(html.Length + PreambleSize);
			builder.AppendLine("Version:0.9");
			builder.AppendLine("StartHTML:0000000000");
			builder.AppendLine("EndHTML:1111111111");
			builder.AppendLine("StartFragment:2222222222");
			builder.AppendLine("EndFragment:3333333333");

			// calculate offsets, accounting for Unicode no-break space chars

			int startHtml = builder.Length;
			builder.Replace("0000000000", startHtml.ToString("D10"));

			var spaces = 0;

			var start = html.IndexOf(StartFragment);
			var naked = start < 0;
			if (naked)
			{
				var len = builder.Length;
				builder.AppendLine("<html>");
				builder.AppendLine("<body>");

				if (start < 0)
				{
					// start is less than 0 when naked
					start = builder.Length - len;
				}

				builder.AppendLine(StartFragment);
			}
			else
			{
				// incoming HTML may have no-break spaces
				spaces = CountNoBreakSpaces(html, 0, start);
			}

			int startFragment = startHtml + start + StartFragment.Length + spaces;
			builder.Replace("2222222222", startFragment.ToString("D10"));

			builder.AppendLine(html);

			// fragment content may have no-break spaces
			var end = html.IndexOf(EndFragment);
			spaces += CountNoBreakSpaces(html, startFragment, end) - 1;

			int endFragment;
			if (naked)
			{
				endFragment = builder.Length + spaces;
				builder.AppendLine(EndFragment);
				builder.AppendLine("</body>");
				builder.AppendLine("</html>");
			}
			else
			{
				endFragment = startHtml + end + spaces;
			}

			builder.Replace("3333333333", endFragment.ToString("D10"));

			int endHtml = builder.Length + spaces;
			builder.Replace("1111111111", endHtml.ToString("D10"));

			return builder.ToString();
		}


		private static int CountNoBreakSpaces(string html, int start, int end)
		{
			var spaces = 0;
			for (var i = start; i < end; i++)
			{
				if (html[i] == Space)
				{
					spaces++;
				}
			}

			return spaces;
		}
	}
}
