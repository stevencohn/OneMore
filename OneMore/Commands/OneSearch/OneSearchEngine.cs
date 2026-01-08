//************************************************************************************************
// Search engine over cached markdown content
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.OneSearch
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.RegularExpressions;


	internal sealed class SearchQuery
	{
		public string Text { get; set; }
		public bool UseRegex { get; set; }
		public bool CaseSensitive { get; set; }
		public SearchScope Scope { get; set; }
		public string CacheRoot { get; set; }
		public string CurrentPageId { get; set; }
		public string CurrentSectionId { get; set; }
		public string CurrentNotebookId { get; set; }
	}


	internal enum SearchScope
	{
		CurrentPage,
		CurrentSection,
		CurrentNotebook,
		AllNotebooks
	}


	internal sealed class SearchResult
	{
		public string PageId { get; set; }
		public string PageTitle { get; set; }
		public string NotebookName { get; set; }
		public string SectionName { get; set; }
		public string Snippet { get; set; }
		public string FilePath { get; set; }
	}


	internal sealed class SearchEngine
	{
		public List<SearchResult> Search(SearchQuery query)
		{
			var results = new List<SearchResult>();
			if (query == null || string.IsNullOrWhiteSpace(query.Text))
			{
				return results;
			}

			if (string.IsNullOrWhiteSpace(query.CacheRoot) || !Directory.Exists(query.CacheRoot))
			{
				return results;
			}

			Regex regex = null;
			if (query.UseRegex)
			{
				var options = RegexOptions.Compiled;
				if (!query.CaseSensitive)
				{
					options |= RegexOptions.IgnoreCase;
				}

				regex = new Regex(query.Text, options);
			}

			foreach (var file in Directory.GetFiles(query.CacheRoot, "*.md", SearchOption.AllDirectories))
			{
				var content = File.ReadAllText(file);
				var metadata = MarkdownMetadata.Parse(content, out var body);
				if (!MatchesScope(metadata, query))
				{
					continue;
				}

				if (TryMatch(body, query, regex, out var matchIndex, out var matchLength))
				{
					var title = metadata.PageTitle;
					if (string.IsNullOrWhiteSpace(title))
					{
						title = Path.GetFileNameWithoutExtension(file);
					}

					results.Add(new SearchResult
					{
						PageId = metadata.PageId,
						PageTitle = title,
						SectionName = metadata.SectionName,
						NotebookName = metadata.NotebookName,
						Snippet = BuildSnippet(body, matchIndex, matchLength),
						FilePath = file
					});
				}
			}

			return results;
		}


		private static bool TryMatch(string body, SearchQuery query, Regex regex, out int matchIndex, out int matchLength)
		{
			matchIndex = -1;
			matchLength = 0;

			if (string.IsNullOrEmpty(body))
			{
				return false;
			}

			if (regex != null)
			{
				var match = regex.Match(body);
				if (!match.Success)
				{
					return false;
				}

				matchIndex = match.Index;
				matchLength = match.Length;
				return true;
			}

			var comparison = query.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
			matchIndex = body.IndexOf(query.Text, comparison);
			if (matchIndex < 0)
			{
				return false;
			}

			matchLength = query.Text.Length;
			return true;
		}


		private static string BuildSnippet(string body, int matchIndex, int matchLength)
		{
			if (string.IsNullOrEmpty(body) || matchIndex < 0)
			{
				return string.Empty;
			}

			var start = Math.Max(0, matchIndex - 40);
			var end = Math.Min(body.Length, matchIndex + matchLength + 40);
			var snippet = body.Substring(start, end - start)
				.Replace("\r\n", " ")
				.Replace("\n", " ")
				.Trim();

			if (start > 0)
			{
				snippet = "..." + snippet;
			}

			if (end < body.Length)
			{
				snippet += "...";
			}

			return snippet;
		}


		private static bool MatchesScope(MarkdownMetadata metadata, SearchQuery query)
		{
			if (metadata == null)
			{
				return false;
			}

			return query.Scope switch
			{
				SearchScope.CurrentPage => string.Equals(metadata.PageId, query.CurrentPageId, StringComparison.OrdinalIgnoreCase),
				SearchScope.CurrentSection => string.Equals(metadata.SectionId, query.CurrentSectionId, StringComparison.OrdinalIgnoreCase),
				SearchScope.CurrentNotebook => string.Equals(metadata.NotebookId, query.CurrentNotebookId, StringComparison.OrdinalIgnoreCase),
				_ => true
			};
		}
	}
}
