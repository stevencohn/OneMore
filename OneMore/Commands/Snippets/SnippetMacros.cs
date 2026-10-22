//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Expands the {=NAME} and {=NAME(argument)} macros found in snippets. Macros are not case
	/// sensitive and unrecognized macros are left as-is. Substituted values are HTML encoded
	/// because snippet text runs hold HTML fragments.
	/// </summary>
	internal class SnippetMacros
	{
		public const string BodyTag = "{SNIPPET_BODY}";

		private const string CountersFilename = "counters.json";

		// {=DATETIME(fmt)} is matched separately because a format may contain parentheses
		private static readonly Regex DateTimeRegex =
			new(@"\{=DATETIME\(([^""]+?)\)\}", RegexOptions.IgnoreCase);

		private static readonly Regex MacroRegex =
			new(@"\{=(\w+)(?:\(([^)}]*)\))?\}", RegexOptions.IgnoreCase);

		private readonly DateTime now;
		private bool countersUsed;


		public SnippetMacros(DateTime now)
		{
			this.now = now;
		}


		public string PageTitle { get; set; }
		public string Section { get; set; }
		public string Notebook { get; set; }
		public string PageLink { get; set; }
		public DateTime? LastModified { get; set; }
		public string ClipboardText { get; set; }

		/// <summary>
		/// Gets the answers to PROMPT macros, keyed by label
		/// </summary>
		public Dictionary<string, string> Prompts { get; } =
			new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the last value issued by each COUNTER macro, keyed by name
		/// </summary>
		public Dictionary<string, int> Counters { get; } =
			new(StringComparer.OrdinalIgnoreCase);


		/// <summary>
		/// Gathers only the information needed by the macros found in the given snippet text,
		/// prompting the user for any PROMPT labels.
		/// </summary>
		/// <returns>The macro expander or null if the user cancelled a prompt</returns>
		public static async Task<SnippetMacros> Create(
			OneNote one, Page page, string text, IWin32Window owner)
		{
			var macros = new SnippetMacros(DateTime.Now);

			var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var labels = new List<string>();
			var counters = new List<string>();

			foreach (Match m in MacroRegex.Matches(text))
			{
				var name = m.Groups[1].Value;
				var arg = m.Groups[2].Success ? m.Groups[2].Value.Trim() : null;
				names.Add(name);

				if (name.Equals("PROMPT", StringComparison.OrdinalIgnoreCase) &&
					!string.IsNullOrEmpty(arg) &&
					!labels.Exists(l => l.Equals(arg, StringComparison.OrdinalIgnoreCase)))
				{
					labels.Add(arg);
				}
				else if (name.Equals("COUNTER", StringComparison.OrdinalIgnoreCase) &&
					!string.IsNullOrEmpty(arg))
				{
					counters.Add(arg);
				}
			}

			if (names.Count == 0)
			{
				return macros;
			}

			if (names.Contains("PAGETITLE") || names.Contains("PAGELINK"))
			{
				macros.PageTitle = page.Root.Attribute("name")?.Value;
			}

			if (names.Contains("PAGELINK"))
			{
				macros.PageLink = one.GetHyperlink(page.PageId, string.Empty);
			}

			if (names.Contains("LASTMODIFIED") &&
				DateTime.TryParse(page.Root.Attribute("lastModifiedTime")?.Value,
					CultureInfo.InvariantCulture,
					DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
					out var modified))
			{
				macros.LastModified = modified.ToLocalTime();
			}

			if (names.Contains("SECTION") || names.Contains("NOTEBOOK"))
			{
				var info = one.GetPageHierarchyInfo(page.PageId);
				macros.Section = string.IsNullOrEmpty(info.SectionId)
					? null : one.GetHierarchyNode(info.SectionId)?.Name;

				macros.Notebook = string.IsNullOrEmpty(info.NotebookId)
					? null : one.GetHierarchyNode(info.NotebookId)?.Name;
			}

			if (names.Contains("CLIPBOARD"))
			{
				macros.ClipboardText = await ClipboardProvider.GetText();
			}

			foreach (var label in labels)
			{
				// one prompt per distinct label; the dialog requires a non-empty value
				using var dialog = new UI.RenameDialog(
					Array.Empty<string>(), string.Empty,
					Resx.SnippetPromptDialog_Text, Resx.SnippetPromptDialog_Text,
					label + ":");

				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return null;
				}

				macros.Prompts[label] = dialog.Value;
			}

			if (counters.Count > 0)
			{
				// seed with the last value issued; each occurrence then increments it
				var stored = LoadCounters();
				foreach (var name in counters)
				{
					stored.TryGetValue(name, out var last);
					macros.Counters[name] = last;
				}

				macros.countersUsed = true;
			}

			return macros;
		}


		/// <summary>
		/// Replaces all recognized macros in the given text
		/// </summary>
		public string Expand(string text)
		{
			if (string.IsNullOrEmpty(text) || text.IndexOf("{=", StringComparison.Ordinal) < 0)
			{
				return text;
			}

			text = DateTimeRegex.Replace(text, m =>
			{
				try
				{
					return now.ToString(m.Groups[1].Value);
				}
				catch
				{
					return now.ToString();
				}
			});

			return MacroRegex.Replace(text, Evaluate);
		}


		private string Evaluate(Match match)
		{
			var arg = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;

			switch (match.Groups[1].Value.ToUpperInvariant())
			{
				case "QUARTER":
					return arg is null ? $"Q{((now.Month - 1) / 3) + 1}" : match.Value;

				case "WEEKNUMBER":
					return arg is null
						? IsoWeekNumber(now).ToString(CultureInfo.InvariantCulture)
						: match.Value;

				case "DAYSUNTIL":
					return DateTime.TryParse(arg, CultureInfo.InvariantCulture,
							DateTimeStyles.None, out var date)
						? (date.Date - now.Date).Days.ToString(CultureInfo.InvariantCulture)
						: match.Value;

				case "PAGETITLE":
					return arg is null ? Encode(PageTitle) : match.Value;

				case "SECTION":
					return arg is null ? Encode(Section) : match.Value;

				case "NOTEBOOK":
					return arg is null ? Encode(Notebook) : match.Value;

				case "PAGELINK":
					if (arg is not null)
					{
						return match.Value;
					}

					return string.IsNullOrEmpty(PageLink)
						? Encode(PageTitle)
						: $"<a href=\"{Encode(PageLink)}\">{Encode(PageTitle)}</a>";

				case "LASTMODIFIED":
					return arg is null
						? Encode(LastModified?.ToString("g", CultureInfo.CurrentCulture))
						: match.Value;

				case "CLIPBOARD":
					return arg is null ? Encode(ClipboardText) : match.Value;

				case "PROMPT":
					return arg is not null && Prompts.TryGetValue(arg, out var answer)
						? Encode(answer)
						: match.Value;

				case "COUNTER":
					if (arg is null || !Counters.TryGetValue(arg, out var counter))
					{
						return match.Value;
					}

					// each occurrence advances the named counter
					Counters[arg] = ++counter;
					return counter.ToString(CultureInfo.InvariantCulture);

				case "SELECTION":
				case "BODY":
				case "SNIPPET_BODY":
					// aliases of the canonical body token
					return arg is null ? BodyTag : match.Value;

				default:
					return match.Value;
			}
		}


		private static string Encode(string value)
		{
			return string.IsNullOrEmpty(value) ? string.Empty : WebUtility.HtmlEncode(value);
		}


		/// <summary>
		/// ISO 8601 week number; System.Globalization.ISOWeek is not available on .NET Framework
		/// </summary>
		public static int IsoWeekNumber(DateTime date)
		{
			var calendar = CultureInfo.InvariantCulture.Calendar;

			// Monday-Wednesday belong to the same ISO week as the following Thursday
			var day = calendar.GetDayOfWeek(date);
			if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
			{
				date = date.AddDays(3);
			}

			return calendar.GetWeekOfYear(date,
				CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
		}


		/// <summary>
		/// Persists the counter values used by this expansion; call after the snippet
		/// has been successfully inserted.
		/// </summary>
		public void CommitCounters()
		{
			if (!countersUsed)
			{
				return;
			}

			try
			{
				var stored = LoadCounters();
				foreach (var pair in Counters)
				{
					stored[pair.Key] = pair.Value;
				}

				File.WriteAllText(GetCountersPath(), JsonConvert.SerializeObject(stored));
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error saving snippet counters", exc);
			}
		}


		private static Dictionary<string, int> LoadCounters()
		{
			var stored = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			try
			{
				var path = GetCountersPath();
				if (File.Exists(path))
				{
					var loaded = JsonConvert.DeserializeObject<Dictionary<string, int>>(
						File.ReadAllText(path));

					if (loaded is not null)
					{
						foreach (var pair in loaded)
						{
							stored[pair.Key] = pair.Value;
						}
					}
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error reading snippet counters", exc);
			}

			return stored;
		}


		private static string GetCountersPath()
		{
			return Path.Combine(PathHelper.GetAppDataPath(), CountersFilename);
		}
	}
}
