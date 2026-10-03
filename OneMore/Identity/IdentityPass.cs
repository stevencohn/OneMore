//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Identity
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// The part of OneNote the identity pass reads, so the pass can be tested with fixtures.
	/// </summary>
	internal interface IHierarchySource
	{
		/// <summary>Gets the list of open notebooks, or null if OneNote cannot be read.</summary>
		Task<XElement> GetNotebooks();

		/// <summary>Gets one notebook with its sections and pages, or null if it cannot be read.</summary>
		Task<XElement> GetNotebookPages(string notebookID);

		/// <summary>
		/// Gets the page GUID of a hyperlink to the page, or null if it cannot be read. This is a call
		/// to OneNote per page, but it loads no content and changes nothing.
		/// </summary>
		Task<string> GetPageGuid(string pageID);
	}


	/// <summary>
	/// Reads the hierarchy from OneNote.
	/// </summary>
	internal sealed class OneNoteHierarchySource : IHierarchySource, IAsyncDisposable
	{
		private readonly OneNote one = new OneNote();


		public Task<XElement> GetNotebooks()
		{
			return one.GetNotebooks();
		}


		public Task<XElement> GetNotebookPages(string notebookID)
		{
			return one.GetNotebook(notebookID, OneNote.Scope.Pages);
		}


		public Task<string> GetPageGuid(string pageID)
		{
			return Task.FromResult(LinkGuids.PageGuid(one.GetHyperlink(pageID, string.Empty)));
		}


		public async ValueTask DisposeAsync()
		{
			await one.DisposeAsync();
		}
	}


	/// <summary>
	/// Reads the hierarchy of every open notebook, without loading any page, and reconciles
	/// the pages found with the stored identities. This is the one place that keeps the identity
	/// catalog current; everything else asks it what a page is now.
	/// </summary>
	internal sealed class IdentityPass : Loggable
	{
		/// <summary>
		/// How long a page may be missing before it is considered deleted. A notebook that was
		/// closed or has not finished reopening is missing its pages for a while, and they must
		/// keep their identities when they return.
		/// </summary>
		public static readonly TimeSpan MissingGrace = TimeSpan.FromHours(6);

		/// <summary>
		/// How long one pass may spend reading the GUIDs of pages that have none yet. Reading one
		/// costs about 14 ms, so a catalog of two thousand pages fills in over a few dozen passes
		/// without anyone noticing.
		/// </summary>
		public static readonly TimeSpan BackfillBudget = TimeSpan.FromMilliseconds(1500);

		/// <summary>
		/// How long one pass may spend reading the GUIDs of pages that might be matched by them.
		/// Generous, because a page that is not matched is given a new identity, and then everything
		/// recorded against its old one is lost.
		/// </summary>
		public static readonly TimeSpan RescueBudget = TimeSpan.FromSeconds(10);

		private readonly PageIdentityProvider provider;
		private readonly IHierarchySource source;


		public IdentityPass(PageIdentityProvider provider, IHierarchySource source)
		{
			this.provider = provider;
			this.source = source;
		}


		/// <summary>
		/// Reads every open notebook and brings the identity catalog up to date.
		/// </summary>
		/// <param name="token">Optional token to cancel the pass between notebooks</param>
		/// <param name="fillGuids">True to also spend up to the backfill budget reading the hyperlink
		/// GUIDs of pages that have none. A caller that is waiting for the result, such as a search,
		/// passes false so the pass stays short; reading the GUIDs a page needs in order to be matched
		/// is never skipped.</param>
		/// <returns>What was found, or null if OneNote could not be read, in which case the
		/// catalog is left alone.</returns>
		public async Task<IdentitySnapshot> Run(CancellationToken token = default, bool fillGuids = true)
		{
			var clock = Stopwatch.StartNew();

			var root = await source.GetNotebooks();
			if (root is null)
			{
				logger.WriteLine("error identity pass could not list notebooks");
				return null;
			}

			var ns = root.Name.Namespace;
			var skipped = new List<string>();
			var notebooks = new List<IdentityNotebook>();

			foreach (var listing in root.Elements(ns + "Notebook"))
			{
				token.ThrowIfCancellationRequested();

				var hierarchy = await source.GetNotebookPages(listing.Attribute("ID")?.Value);
				if (hierarchy is null)
				{
					logger.WriteLine(
						$"error identity pass could not read notebook \"{listing.Attribute("name")?.Value}\"");
				}

				notebooks.Add(IdentityGatherer.Gather(listing, hierarchy, skipped));
			}

			// only notebooks that could be read are fully represented; for the others, not
			// being able to see pages is not evidence they are gone
			var listed = notebooks.Where(n => n.Listed).ToList();
			var pages = listed.SelectMany(n => n.Pages).ToList();

			var keys = listed.Select(n => n.Key).Distinct().ToList();
			var refs = pages.Select(p => p.Ref).ToList();

			// a page that no stored identity matches may still be one whose name, section and
			// creation time were all changed; its hyperlink GUID can tell. Read those before the
			// write lock is taken, so a slow call to OneNote never holds up the database
			var rescue = provider.FindPagesNeedingGuid(keys, refs, skipped);
			var read = rescue.Count > 0 ? await ReadGuids(rescue, token) : 0;

			var resolutions = provider.Reconcile(keys, refs, skipped);

			for (var i = 0; i < pages.Count; i++)
			{
				pages[i].Resolution = resolutions[i];
				pages[i].PageGuid = pages[i].Ref.PageGuid ?? resolutions[i].Row?.PageGuid;
			}

			if (fillGuids)
			{
				read += await BackfillGuids(pages, token);
			}

			var purged = provider.PurgeMissing(MissingGrace);

			clock.Stop();

			var pending = pages.Count(p => p.PageGuid is null);
			var snapshot = new IdentitySnapshot(
				notebooks, skipped, purged, clock.Elapsed, pending);

			var moved = pages.Count(p => p.Resolution.Kind != ResolutionKind.Known &&
				p.Resolution.Kind != ResolutionKind.SameLocation && p.Resolution.Kind != ResolutionKind.New);

			if (moved > 0 || purged.Count > 0 || read > 0 || logger.IsDebug)
			{
				logger.WriteLine(
					$"identity pass {pages.Count} pages in {listed.Count} of {notebooks.Count} notebooks, " +
					$"{moved} moved or renamed, {purged.Count} purged, {skipped.Count} sections skipped, " +
					$"{read} GUIDs read, {pending} pending, in {clock.ElapsedMilliseconds}ms");
			}

			return snapshot;
		}


		// reads the GUIDs of pages the matcher needs them for, within a time budget
		private async Task<int> ReadGuids(IReadOnlyList<PageRef> refs, CancellationToken token)
		{
			var clock = Stopwatch.StartNew();
			var count = 0;

			foreach (var page in refs)
			{
				token.ThrowIfCancellationRequested();

				if (clock.Elapsed > RescueBudget)
				{
					logger.WriteLine(
						$"identity pass ran out of time reading page GUIDs, {refs.Count - count} not read");

					break;
				}

				page.PageGuid = await source.GetPageGuid(page.PageID);
				count++;
			}

			return count;
		}


		// fills in the GUIDs of pages that have none, a few at a time, and records them
		private async Task<int> BackfillGuids(IReadOnlyList<IdentityPage> pages, CancellationToken token)
		{
			var clock = Stopwatch.StartNew();
			var found = new List<(long PageKey, string PageGuid)>();

			foreach (var page in pages)
			{
				if (page.PageGuid is not null || page.PageKey == 0)
				{
					continue;
				}

				token.ThrowIfCancellationRequested();

				if (clock.Elapsed > BackfillBudget)
				{
					break;
				}

				var guid = await source.GetPageGuid(page.Ref.PageID);
				if (guid is not null)
				{
					page.PageGuid = guid;
					found.Add((page.PageKey, guid));
				}
			}

			provider.WritePageGuids(found);
			return found.Count;
		}
	}
}