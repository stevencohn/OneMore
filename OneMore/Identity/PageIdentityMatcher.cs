//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Identity
{
	using System;
	using System.Collections.Generic;
	using System.Linq;


	/// <summary>
	/// Matches the pages currently in the OneNote hierarchy to stored page identities, so a
	/// page keeps its identity when OneNote changes its IDs, when it is moved, or when its
	/// creation time is edited. Pure logic with no database or OneNote access.
	/// </summary>
	/// <remarks>
	/// Stages run strongest evidence first and each stage only considers pages and stored
	/// identities that earlier stages left unclaimed:
	/// <list type="number">
	/// <item>Same page ID.</item>
	/// <item>Same notebook, section, title and creation time (a notebook reopen).</item>
	/// <item>Same notebook, title and creation time (moved to another section).</item>
	/// <item>Same title and creation time (moved to another notebook).</item>
	/// <item>Same notebook, section and title (creation time was edited).</item>
	/// </list>
	/// A match is made only when it is unambiguous, one page to one stored identity. When
	/// several pages are indistinguishable the match is declined, and the pages are treated
	/// as new, which costs a re-examination but is never wrong. The one exception is
	/// identical pages in the same location with the same modified time: their text is
	/// the same, so which one gets which identity does not matter.
	/// </remarks>
	internal static class PageIdentityMatcher
	{
		private const string Sep = "\u001f";


		/// <param name="rows">Stored identities that may be claimed</param>
		/// <param name="pages">Every page currently present in the scope being reconciled.
		/// It must be the whole scope at once, so a page moved between notebooks in the same
		/// scan finds its old identity.</param>
		public static PageMatchResult Match(
			IReadOnlyList<IdentityRow> rows, IReadOnlyList<PageRef> pages)
		{
			var resolutions = new PageResolution[pages.Count];
			var claimed = new HashSet<long>();

			void Claim(int index, IdentityRow row, ResolutionKind kind)
			{
				resolutions[index] = new PageResolution(pages[index], row, kind);
				claimed.Add(row.PageKey);
			}

			MatchKnown(rows, pages, claimed, Claim);

			MatchByKey(ResolutionKind.SameLocation, pairIdentical: true,
				p => string.Join(Sep, p.NotebookKey, p.SectionKey, p.Title, p.Created),
				r => string.Join(Sep, r.NotebookKey, r.SectionKey, r.Title, r.Created));

			MatchByKey(ResolutionKind.MovedSection, pairIdentical: false,
				p => string.Join(Sep, p.NotebookKey, p.Title, p.Created),
				r => string.Join(Sep, r.NotebookKey, r.Title, r.Created));

			MatchByKey(ResolutionKind.MovedNotebook, pairIdentical: false,
				p => string.Join(Sep, p.Title, p.Created),
				r => string.Join(Sep, r.Title, r.Created));

			MatchByKey(ResolutionKind.SameTitle, pairIdentical: false,
				p => string.Join(Sep, p.NotebookKey, p.SectionKey, p.Title),
				r => string.Join(Sep, r.NotebookKey, r.SectionKey, r.Title));

			var final = new PageResolution[pages.Count];
			for (var i = 0; i < pages.Count; i++)
			{
				final[i] = resolutions[i] ??
					new PageResolution(pages[i], null, ResolutionKind.New);
			}

			var orphans = rows.Where(r => !claimed.Contains(r.PageKey)).ToList();
			return new PageMatchResult(final, orphans);


			// matches pages to unclaimed stored identities that share a key, one to one
			void MatchByKey(ResolutionKind kind, bool pairIdentical,
				Func<PageRef, string> pageKey, Func<IdentityRow, string> rowKey)
			{
				var pending = new Dictionary<string, List<int>>(StringComparer.Ordinal);
				for (var i = 0; i < pages.Count; i++)
				{
					if (resolutions[i] is null)
					{
						Add(pending, pageKey(pages[i]), i);
					}
				}

				if (pending.Count == 0)
				{
					return;
				}

				var candidates = new Dictionary<string, List<IdentityRow>>(StringComparer.Ordinal);
				foreach (var row in rows)
				{
					if (!claimed.Contains(row.PageKey))
					{
						Add(candidates, rowKey(row), row);
					}
				}

				foreach (var pair in pending)
				{
					if (!candidates.TryGetValue(pair.Key, out var matches))
					{
						continue;
					}

					var indexes = pair.Value;
					if (indexes.Count == 1 && matches.Count == 1)
					{
						Claim(indexes[0], matches[0], kind);
					}
					else if (pairIdentical && indexes.Count == matches.Count
						&& AllSameModified(indexes.Select(i => pages[i].Modified)
							.Concat(matches.Select(m => m.Modified))))
					{
						for (var n = 0; n < indexes.Count; n++)
						{
							Claim(indexes[n], matches[n], kind);
						}
					}
				}
			}
		}


		private static void MatchKnown(
			IReadOnlyList<IdentityRow> rows, IReadOnlyList<PageRef> pages,
			HashSet<long> claimed, Action<int, IdentityRow, ResolutionKind> claim)
		{
			var byId = new Dictionary<string, IdentityRow>(StringComparer.Ordinal);
			foreach (var row in rows)
			{
				// a present identity wins over a missing one that has the same page ID
				if (row.PageID is not null &&
					(!byId.TryGetValue(row.PageID, out var existing) || (existing.IsMissing && !row.IsMissing)))
				{
					byId[row.PageID] = row;
				}
			}

			for (var i = 0; i < pages.Count; i++)
			{
				if (pages[i].PageID is not null
					&& byId.TryGetValue(pages[i].PageID, out var row) && !claimed.Contains(row.PageKey))
				{
					claim(i, row, ResolutionKind.Known);
				}
			}
		}


		private static void Add<T>(Dictionary<string, List<T>> map, string key, T value)
		{
			if (!map.TryGetValue(key, out var list))
			{
				list = new List<T>();
				map.Add(key, list);
			}

			list.Add(value);
		}


		private static bool AllSameModified(IEnumerable<string> times)
		{
			return times.Distinct(StringComparer.Ordinal).Count() == 1;
		}
	}
}
