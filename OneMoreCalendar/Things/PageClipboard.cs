//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using OneMoreCalendar.Properties;
	using River.OneMoreAddIn;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Windows.Forms;


	/// <summary>
	/// Places page hyperlinks on the clipboard in formats suitable for OneNote, rich text
	/// editors, and plain text editors.
	/// </summary>
	internal static class PageClipboard
	{
		private const string OneNoteLinkFormat = "OneNote Link";

		private static int onFormat;


		[DllImport("user32.dll")]
		private static extern int RegisterClipboardFormat(string Format);


		/// <summary>
		/// Copies the links of the given pages, both the hyperlink and the web hyperlink, as
		/// used by the day header's copy button; pages without a hyperlink are skipped.
		/// </summary>
		/// <param name="pages">Pages with Hyperlink and WebHyperlink already filled in</param>
		/// <returns>The number of pages copied</returns>
		public static int CopyLinks(IEnumerable<CalendarPage> pages)
		{
			var linked = pages.Where(p => p.Hyperlink is not null).ToList();
			if (linked.Count == 0)
			{
				return 0;
			}

			// HTML Format (for pasting in Word and other non-OneNote rich text apps)
			var html = new StringBuilder();
			// Text (for pasting into Notepad
			var text = new StringBuilder();
			// OneNote Link (special case for pasting into OneNote)
			var onlink = new StringBuilder();

			var many = linked.Count > 1;
			foreach (var page in linked)
			{
				var web = $"<a href=\"{page.Hyperlink}\">{page.Title}</a>";
				onlink.AppendLine(many ? $"<p lang=en-US>{web}</p>{Environment.NewLine}" : web);

				var both = $"{web} (<a href=\"{page.WebHyperlink}\">{Resources.MonthView_WebView}</a>)";
				html.AppendLine(many ? $"<p lang=en-US>{both}</p>{Environment.NewLine}" : both);

				text.AppendLine(page.WebHyperlink);
				text.AppendLine(page.Hyperlink);
			}

			// do not use the System.Windows.Clipboard classes here because they
			// screw up the DPI of the app window!!!
			var data = new DataObject();

			var wrap = ClipboardProvider.WrapWithHtmlPreamble(
				Resources.HtmlClipboardPreamble + Environment.NewLine +
				html.ToString()
				);

			data.SetText(wrap, TextDataFormat.Html);

			var t = text.ToString().Trim();
			data.SetText(t, TextDataFormat.Text);
			data.SetText(t, TextDataFormat.UnicodeText);

			SetOneNoteLink(data, onlink.ToString());

			Clipboard.SetDataObject(data, true, 3, 100);

			return linked.Count;
		}


		/// <summary>
		/// Copies only the internal onenote: hyperlink of the given page, with no web link
		/// in any of the clipboard formats.
		/// </summary>
		/// <returns>True if copied; false if the page has no onenote: hyperlink</returns>
		public static bool CopyOneNoteLink(CalendarPage page)
		{
			if (string.IsNullOrWhiteSpace(page.OneNoteHyperlink))
			{
				return false;
			}

			var data = new DataObject();

			var anchor = $"<a href=\"{page.OneNoteHyperlink}\">{page.Title}</a>";

			// HTML Format (for pasting in Word and other non-OneNote rich text apps)
			data.SetText(
				ClipboardProvider.WrapWithHtmlPreamble(
					Resources.HtmlClipboardPreamble + Environment.NewLine + anchor),
				TextDataFormat.Html);

			// Text (for pasting into Notepad)
			data.SetText(page.OneNoteHyperlink, TextDataFormat.Text);
			data.SetText(page.OneNoteHyperlink, TextDataFormat.UnicodeText);

			// OneNote Link (special case for pasting into OneNote)
			SetOneNoteLink(data, anchor);

			Clipboard.SetDataObject(data, true, 3, 100);

			return true;
		}


		private static void SetOneNoteLink(DataObject data, string html)
		{
			if (onFormat == 0)
			{
				onFormat = RegisterClipboardFormat(OneNoteLinkFormat);
			}

			var wrap = ClipboardProvider.WrapWithHtmlPreamble(
				Resources.HtmlClipboardPreamble + Environment.NewLine + html);

			var stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(wrap));
			data.SetData(OneNoteLinkFormat, stream);
		}


		/// <summary>
		/// Copies the online (OneNote Web) hyperlink of the given page as an HTML anchor
		/// and as plain text.
		/// </summary>
		/// <returns>True if copied; false if the page has no web hyperlink</returns>
		public static bool CopyWebLink(CalendarPage page)
		{
			if (string.IsNullOrWhiteSpace(page.WebHyperlink))
			{
				return false;
			}

			var data = new DataObject();

			var html = $"<a href=\"{page.WebHyperlink}\">{page.Title}</a>";
			data.SetText(
				ClipboardProvider.WrapWithHtmlPreamble(
					Resources.HtmlClipboardPreamble + Environment.NewLine + html),
				TextDataFormat.Html);

			data.SetText(page.WebHyperlink, TextDataFormat.Text);
			data.SetText(page.WebHyperlink, TextDataFormat.UnicodeText);

			Clipboard.SetDataObject(data, true, 3, 100);

			return true;
		}
	}
}
