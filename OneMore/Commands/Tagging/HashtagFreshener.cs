//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Identity;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Makes the pages in a set of search results current before they are shown, so a result can
	/// always be opened.
	/// </summary>
	/// <remarks>
	/// OneNote regenerates a page's ID, and the IDs of its title and paragraphs, when its notebook
	/// is reopened or the page is moved. The background scan catches up with that on its next
	/// cycle, which can be minutes later, and until then the IDs a result carries do not exist and
	/// opening it fails. This runs at the moment a search is made instead.
	/// </remarks>
	internal static class HashtagFreshener
	{
		/// <summary>
		/// Finds the pages in the results whose recorded page ID no longer exists in OneNote.
		/// </summary>
		/// <param name="tags">The results of a search</param>
		/// <param name="pageExists">Tells whether OneNote still knows a page ID</param>
		/// <param name="isPresent">Tells whether the identity catalog has the page, given its key,
		/// and does not consider it missing. A page in a notebook that is closed has no ID in
		/// OneNote and is not worth looking for, so it is skipped.</param>
		/// <returns>One tag for each page that is stale</returns>
		internal static List<Hashtag> FindStale(
			IEnumerable<Hashtag> tags, Func<string, bool> pageExists, Func<long, bool> isPresent)
		{
			var stale = new List<Hashtag>();
			var seen = new HashSet<string>(StringComparer.Ordinal);

			foreach (var tag in tags)
			{
				if (string.IsNullOrEmpty(tag.MoreID) || !seen.Add(tag.MoreID))
				{
					continue;
				}

				// not a page key, so there is nothing to look the page up by
				if (!long.TryParse(tag.MoreID, NumberStyles.None, CultureInfo.InvariantCulture, out var key))
				{
					continue;
				}

				if (pageExists(tag.PageID) || !isPresent(key))
				{
					continue;
				}

				stale.Add(tag);
			}

			return stale;
		}


		/// <summary>
		/// Brings up to date the pages of the results that OneNote has changed since the last scan.
		/// It costs one check of each page's ID and, only if some are stale, one pass over the
		/// hierarchy and a scan of just those pages.
		/// </summary>
		/// <param name="tags">The results of a search</param>
		/// <param name="token">Optional token to cancel</param>
		/// <returns>True if any page was refreshed, in which case the search should be run again
		/// to read the new IDs</returns>
		public static async Task<bool> Freshen(IEnumerable<Hashtag> tags, CancellationToken token = default)
		{
			var logger = Logger.Current;

			await using var one = new OneNote();

			List<Hashtag> stale;
			using (var identity = new PageIdentityProvider())
			{
				stale = FindStale(tags, one.PageExists,
					key => identity.Read(key) is IdentityRow row && !row.IsMissing);
			}

			if (stale.Count == 0)
			{
				return false;
			}

			logger.WriteLine($"refreshing {stale.Count} pages in the search results");

			var snapshot = await HashtagScanner.ReadIdentities(token, fillGuids: false);
			if (snapshot is null)
			{
				return false;
			}

			var pages = snapshot.Pages
				.Where(p => p.PageKey > 0)
				.GroupBy(p => p.PageKey)
				.ToDictionary(g => g.Key, g => g.First());

			var refreshed = 0;
			using var scanner = new HashtagScanner();

			foreach (var tag in stale)
			{
				token.ThrowIfCancellationRequested();

				if (long.TryParse(tag.MoreID, NumberStyles.None, CultureInfo.InvariantCulture, out var key) &&
					pages.TryGetValue(key, out var page) &&
					await scanner.ScanPage(one, page, forceThru: true))
				{
					refreshed++;
				}
			}

			logger.WriteLine($"refreshed {refreshed} of {stale.Count} pages");
			return refreshed > 0;
		}
	}
}
