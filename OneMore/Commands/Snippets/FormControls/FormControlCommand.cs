//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Invoked by a form control hyperlink, onemore://FormControlCommand/{kind}/{id}[/{list}].
	/// Pops up a calendar or list near the mouse and replaces the hyperlink text with the
	/// chosen value, keeping the hyperlink so the choice can be changed.
	/// </summary>
	[CommandService]
	internal class FormControlCommand : Command
	{
		public const string DateKind = "date";
		public const string ListKind = "list";

		public FormControlCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (args.Length < 2 || args[0] is not string kind || args[1] is not string id)
			{
				return;
			}

			// the popup appears just below the click, which is where the mouse still is
			var origin = Cursor.Position;
			string text;

			if (kind == DateKind)
			{
				using var popup = new CalendarPopup(origin);
				if (ShowPopup(popup) != DialogResult.OK)
				{
					return;
				}

				using var provider = new FormControlsProvider();
				var format = provider.ReadSetting(
					FormControlsProvider.DateFormatKey, FormControlsProvider.DefaultDateFormat);

				text = popup.SelectedDate.ToString(format, AddIn.Locale);
			}
			else if (kind == ListKind)
			{
				var name = args.Length > 2 ? args[2] as string : null;
				List<string> items;
				using (var provider = new FormControlsProvider())
				{
					items = string.IsNullOrEmpty(name) ? new List<string>() : provider.ReadItems(name);
				}

				if (items.Count == 0)
				{
					ShowInfo($"The list \"{name}\" has no items. Add items in Settings > Form Controls.");
					return;
				}

				using var popup = new ListPopup(origin, items);
				if (ShowPopup(popup) != DialogResult.OK)
				{
					return;
				}

				text = popup.SelectedItem;
			}
			else
			{
				return;
			}

			await ReplaceLinkText(kind, id, text);
		}


		private DialogResult ShowPopup(FormControlPopup popup)
		{
			logger.WriteLine($"showing {popup.GetType().Name}");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			var result = popup.ShowDialog(owner);
			logger.WriteLine(
				$"{popup.GetType().Name} closed {result} after {watch.ElapsedMilliseconds}ms " +
				$"at {popup.Bounds}");

			return result;
		}


		private async Task ReplaceLinkText(string kind, string id, string text)
		{
			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic);

			// match on the unique id alone; OneNote may normalize the rest of the href
			var run = page.Root.Descendants(ns + "T")
				.FirstOrDefault(e => e.GetCData().Value.Contains(id));

			if (run is null)
			{
				logger.WriteLine($"form control link {id} not found on page {page.PageId}");
				foreach (var t in page.Root.Descendants(ns + "T")
					.Where(e => e.GetCData().Value.IndexOf("onemore", StringComparison.OrdinalIgnoreCase) >= 0)
					.Take(5))
				{
					logger.WriteLine($"candidate: {t.GetCData().Value}");
				}

				return;
			}

			var cdata = run.GetCData();
			var value = cdata.Value;

			// bound the anchor that contains the id: its opening tag, then the closing tag;
			// OneNote may break the tag after "<a" with a newline so do not expect a space
			var at = value.IndexOf(id, StringComparison.Ordinal);
			var start = value.LastIndexOf("<a", at, StringComparison.OrdinalIgnoreCase);
			var tag = start < 0 ? -1 : value.IndexOf('>', at);
			var end = tag < 0 ? -1 : value.IndexOf("</a>", tag, StringComparison.OrdinalIgnoreCase);
			if (end < 0)
			{
				logger.WriteLine($"form control anchor malformed: {value}");
				return;
			}

			// keep the opening tag and its href, replace only the link text
			cdata.Value = value.Substring(0, tag + 1) + WebUtility.HtmlEncode(text) +
				value.Substring(end);

			await one.Update(page);
		}


		public static string MakeHref(string kind, string id, string list = null)
		{
			var href = $"onemore://FormControlCommand/{kind}/{id}";
			if (!string.IsNullOrEmpty(list))
			{
				href = $"{href}/{Uri.EscapeDataString(list)}";
			}

			return href;
		}


		public static string MakeAnchor(string kind, string id, string text, string list = null)
		{
			return $"<a href=\"{MakeHref(kind, id, list)}\">{WebUtility.HtmlEncode(text)}</a>";
		}
	}
}
