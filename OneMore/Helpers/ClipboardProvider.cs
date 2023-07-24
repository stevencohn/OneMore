//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************                

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Media.Imaging;
	using WindowsInput;
	using WindowsInput.Native;
	using Win = System.Windows;


	/// <summary>
	/// Provides wrapped STA access to the Windows clipboard for use in OneNote's MTA environment
	/// </summary>
	internal class ClipboardProvider : Loggable
	{
		private const string StartFragmentLine = "<!--StartFragment-->";
		private const string EndFragmentLine = "<!--EndFragment-->";

		//Message: OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))
		//HResult: 0x800401D0 (-2147221040)
		private const int CLIPBRD_E_CANT_OPEN = -2147221040;



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
		/// Removes all content from the system clipboard.
		/// </summary>
		/// <returns></returns>
		public async Task Clear()
		{
			await SingleThreaded.Invoke(() =>
			{
				Win.Clipboard.Clear();
			});
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
						try
						{
							Win.Clipboard.SetDataObject(data, true);
						}
						catch (COMException ex)
							when (ex.ErrorCode == CLIPBRD_E_CANT_OPEN)
						{
							logger.WriteLine("clipboard possibly locked by another application", ex);
						}
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
		/// Wraps the given HTML into the Windows Clipboard HTML Format CF_HTML, including a
		/// preamble that describes the size and important offsets needed to transfer HTML.
		/// Th CF_HTML format is described here
		/// https://docs.microsoft.com/en-us/windows/win32/dataxchg/html-clipboard-format
		/// </summary>
		/// <param name="html">
		/// The HTML fragment to wrap. Can include HTML/BODY/StartFragment..EndFragment but,
		/// if not, then those tags will be added accordingly.
		/// </param>
		/// <returns>A new string in CF_HTML format</returns>
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

			var NLCount = Environment.NewLine.Length;
			string head;

			// The HTML might come from a Linux (LF) file but by the time it gets to the clipboard
			// Windows wants it to be in Windows (CRLF) format, so convert Unix file to Windows
			// file now to properly calculate offsets
			var body = Regex.Replace(html, @"(?<!\r)\n", "\r\n");

			var index = html.IndexOf(StartFragmentLine);
			if (index > 0)
			{
				head = html.Substring(0, index).Trim();
				body = body.Substring(index + StartFragmentLine.Length).Trim();
			}
			else
			{
				head = $"<html>{Environment.NewLine}<body>";
			}

			index = body.IndexOf(EndFragmentLine);
			if (index > 0)
			{
				body = body.Substring(0, index);
			}

			var builder = new StringBuilder();
			builder.AppendLine("Version:0.9");
			builder.AppendLine("StartHTML:0000000000");
			builder.AppendLine("EndHTML:1111111111");
			builder.AppendLine("StartFragment:2222222222");
			builder.AppendLine("EndFragment:3333333333");
			var startHtml = builder.Length;

			builder.AppendLine(head);

			// Unicode chars in UTF8 will look like one char in a string but may be mutiple
			// bytes, so we need to count bytes instead of string lengths. That also include
			// the Unicode no-break space \u00a0
			var headLen = Encoding.UTF8.GetByteCount(head) + NLCount;

			builder.AppendLine(StartFragmentLine);
			var startFragment = startHtml + headLen + StartFragmentLine.Length;

			builder.AppendLine(body);
			var bodyLen = Encoding.UTF8.GetByteCount(body) + NLCount;
			var endFragment = startFragment + bodyLen;

			builder.AppendLine(EndFragmentLine);
			builder.AppendLine("</body>");
			builder.AppendLine("</html>");

			// 18 is the length of </body>+NL and </html>+NL
			var endHtml = endFragment + EndFragmentLine.Length + 18 + NLCount;

			builder.Replace("0000000000", startHtml.ToString("D10"));
			builder.Replace("1111111111", endHtml.ToString("D10"));
			builder.Replace("2222222222", startFragment.ToString("D10"));
			builder.Replace("3333333333", endFragment.ToString("D10"));

			return builder.ToString();
		}
	}
}
