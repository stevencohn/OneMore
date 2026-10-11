//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.TextIndex
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;


	/// <summary>
	/// The answer of the text index to one search, for the pages whose index entries are current.
	/// A page that was changed since it was indexed, or was never indexed, is not answered here
	/// and must be read from OneNote.
	/// </summary>
	/// <remarks>
	/// The index only narrows what is tested; every paragraph it returns is tested with the same
	/// regular expression the live search uses, so what is found is what a scan would find.
	/// </remarks>
	internal sealed class IndexedHits
	{
		private static readonly IList<SearchHit> NoHits = Array.Empty<SearchHit>();

		// what was indexed, by the OneNote page ID the page had when it was indexed
		private readonly Dictionary<string, (long Key, string Modified)> pages =
			new Dictionary<string, (long, string)>(StringComparer.Ordinal);

		private readonly Dictionary<long, List<SearchHit>> hits = new Dictionary<long, List<SearchHit>>();


		private IndexedHits()
		{
		}


		/// <summary>
		/// Gets how many pages have an index entry, current or not.
		/// </summary>
		public int Pages => pages.Count;


		/// <summary>
		/// Gets how many paragraphs matched.
		/// </summary>
		public int Paragraphs { get; private set; }


		/// <summary>
		/// Asks the index for a search, if indexing is turned on and there is an index.
		/// </summary>
		/// <param name="builder">Translates the query for the index</param>
		/// <param name="query">The text the user entered</param>
		/// <param name="finder">The expression that tests each paragraph, from the same builder</param>
		/// <param name="token">Cancels the search</param>
		/// <returns>The answer, or null if the index cannot be used and every page must be read</returns>
		public static Task<IndexedHits> Load(
			TextMatchBuilder builder, string query, Regex finder, CancellationToken token)
		{
			return Task.Run(() =>
			{
				try
				{
					if (!IndexConsumer.IsEnabled() || !IndexProvider.CatalogExists())
					{
						return null;
					}

					using var provider = new IndexProvider();
					return Load(provider, builder.BuildIndexQuery(query), finder, token);
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception exc)
				{
					// not being able to use the index is never a reason for a search to fail
					Logger.Current.WriteLine("error using the text index, searching pages instead", exc);
					return null;
				}
			}, token);
		}


		/// <summary>
		/// Asks an index for a search.
		/// </summary>
		/// <param name="provider">The index</param>
		/// <param name="match">An FTS5 expression that finds a superset of the paragraphs that
		/// match, or null if every paragraph must be tested</param>
		/// <param name="finder">The expression that tests each paragraph</param>
		/// <param name="token">Cancels the search</param>
		internal static IndexedHits Load(
			IndexProvider provider, string match, Regex finder, CancellationToken token)
		{
			var result = new IndexedHits();

			var keys = new Dictionary<long, string>();
			foreach (var stamp in provider.ReadStamps())
			{
				result.pages[stamp.Value.PageID] = (stamp.Key, stamp.Value.Modified);
				keys[stamp.Key] = stamp.Value.PageID;
			}

			foreach (var paragraph in provider.Query(match))
			{
				token.ThrowIfCancellationRequested();

				if (!finder.IsMatch(paragraph.Text))
				{
					continue;
				}

				if (!result.hits.TryGetValue(paragraph.PageKey, out var list))
				{
					list = new List<SearchHit>();
					result.hits.Add(paragraph.PageKey, list);
				}

				list.Add(new SearchHit
				{
					// decoded for display, as the live search does; the match was on the raw text
					PlainText = HttpUtility.HtmlDecode(paragraph.Text),
					ObjectId = paragraph.ObjectID
				});

				result.Paragraphs++;
			}

			return result;
		}


		/// <summary>
		/// Gets what the index found on a page, if its entry is current.
		/// </summary>
		/// <param name="pageID">The OneNote page ID of the page now</param>
		/// <param name="modified">When OneNote says the page was last modified</param>
		/// <param name="found">The paragraphs found, none if nothing on the page matched</param>
		/// <returns>True if the index knows the page as it is now; false if it was changed or never
		/// indexed, in which case the page must be read</returns>
		public bool TryGet(string pageID, string modified, out IList<SearchHit> found)
		{
			found = NoHits;

			if (pageID is null || modified is null ||
				!pages.TryGetValue(pageID, out var entry) ||
				!string.Equals(entry.Modified, modified, StringComparison.Ordinal))
			{
				return false;
			}

			if (hits.TryGetValue(entry.Key, out var list))
			{
				found = list;
			}

			return true;
		}
	}
}
