//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Text.RegularExpressions;


	/// <summary>
	/// The parsed components of a Search Page Titles query string.
	/// </summary>
	internal sealed class TitleQuery
	{
		/// <summary>
		/// True if the query included a ">" token, requesting a most-recently-modified-first
		/// sort rather than the default alphabetical-by-name sort.
		/// </summary>
		public bool SortByModified { get; set; }


		/// <summary>
		/// The notebook name (or partial name) extracted from an "nb:\&lt;name&gt;" token, "*"
		/// if "nb:\*" was specified (search all notebooks), or null if no nb:\ token was present
		/// (search the current notebook).
		/// </summary>
		public string NotebookFilter { get; set; }


		/// <summary>
		/// Hashtag tokens (including their leading '#') extracted from the query. Pages must
		/// match every hashtag in this list (implicit AND) in addition to the TitleText match.
		/// </summary>
		public List<string> Hashtags { get; } = new List<string>();


		/// <summary>
		/// Whatever remains of the query after stripping the sort token, nb:\ token, and
		/// hashtag tokens. Passed to TextMatchBuilder to match against page names.
		/// </summary>
		public string TitleText { get; set; }
	}


	/// <summary>
	/// Parses the extended query syntax accepted by Search Page Titles: a ">" token, anywhere
	/// in the query, to sort by most-recently-modified, an "nb:\&lt;name&gt;" (or "nb:\*") token
	/// to scope the search to one or more notebooks, and "#hashtag" tokens to additionally
	/// filter by the hashtag catalog. Whatever remains is matched against page titles via
	/// TextMatchBuilder.
	/// </summary>
	internal static class TitleQueryParser
	{
		private static readonly Regex NotebookPattern = new Regex(
			@"nb:\\(?:""(?<quoted>[^""]*)""|(?<bare>\S*))",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex HashtagPattern = new Regex(
			@"(?<!\S)#\S+", RegexOptions.Compiled);

		// matches a ">" only at a word boundary (start of string or preceded by whitespace) so
		// it can appear anywhere in the query - ">foo nb:\*" or "nb:\* >foo" - without
		// mistaking a literal '>' embedded in a word (e.g. "a>b") for the sort flag
		private static readonly Regex SortPattern = new Regex(
			@"(?<!\S)>", RegexOptions.Compiled);

		private static readonly Regex NotebookMarker = new Regex(
			@"nb:\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex Whitespace = new Regex(@"\s+", RegexOptions.Compiled);


		/// <summary>
		/// Parses the given raw query string into its component parts.
		/// </summary>
		public static TitleQuery Parse(string query)
		{
			var result = new TitleQuery();

			if (string.IsNullOrWhiteSpace(query))
			{
				result.TitleText = string.Empty;
				return result;
			}

			var text = query.Trim();

			var sortMatch = SortPattern.Match(text);
			if (sortMatch.Success)
			{
				result.SortByModified = true;
				text = text.Remove(sortMatch.Index, sortMatch.Length);
			}

			var nbMatch = NotebookPattern.Match(text);
			if (nbMatch.Success)
			{
				result.NotebookFilter = nbMatch.Groups["quoted"].Success
					? nbMatch.Groups["quoted"].Value
					: nbMatch.Groups["bare"].Value;

				text = text.Remove(nbMatch.Index, nbMatch.Length);
			}

			foreach (Match m in HashtagPattern.Matches(text))
			{
				result.Hashtags.Add(m.Value);
			}

			if (result.Hashtags.Count > 0)
			{
				text = HashtagPattern.Replace(text, string.Empty);
			}

			result.TitleText = Whitespace.Replace(text, " ").Trim();

			return result;
		}


		/// <summary>
		/// Counts the "significant" characters in a raw query string for type-ahead purposes.
		/// The '#' and '>' characters and the literal "nb:\" marker are considered insignificant;
		/// every other non-whitespace character counts. Callers should run a live search once
		/// this count exceeds 3.
		/// </summary>
		public static int CountSignificantChars(string query)
		{
			if (string.IsNullOrEmpty(query))
			{
				return 0;
			}

			var stripped = NotebookMarker.Replace(query, string.Empty)
				.Replace(">", string.Empty)
				.Replace("#", string.Empty);

			var count = 0;
			foreach (var c in stripped)
			{
				if (!char.IsWhiteSpace(c))
				{
					count++;
				}
			}

			return count;
		}
	}
}
