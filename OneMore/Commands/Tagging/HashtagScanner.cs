//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S125

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Scans all notebooks for hashtags
	/// </summary>
	internal class HashtagScanner : Loggable, IDisposable
	{
		public class Statistics
		{
			public int Notebooks;
			public int KnownNotebooks;
			public int FilteredNotebooks;
			public int Sections;
			public int TotalPages;
			public int DirtyPages;
			public int Tags;
			public long Time;
			public long FetchTime;
			public long ThrottleTime;
		}

		public const int DefaultThrottle = 20;
		private const int MaxPagesThreshold = 100;

		private readonly string lastTime;
		private readonly string startTime;
		private readonly HashtagPageSannerFactory factory;
		private readonly SettingsCollection settings;
		private readonly int throttle;
		private readonly bool ownsProvider;
		private HashtagProvider provider;
		private string[] notebookFilters;
		private bool disposed;


		/// <summary>
		/// Initialize a new instance, creating and owning its own HashtagProvider
		/// </summary>
		public HashtagScanner() : this(new HashtagProvider())
		{
			ownsProvider = true;
		}


		/// <summary>
		/// Initialize a new instance using the given HashtagProvider, whose lifetime remains
		/// the responsibility of the caller (used by HashtagStage to reuse one connection
		/// across many scans instead of opening/closing one every cycle)
		/// </summary>
		/// <param name="provider">A provider owned and disposed by the caller</param>
		public HashtagScanner(HashtagProvider provider)
		{
			settings = new SettingsProvider().GetCollection("HashtagSheet");
			throttle = settings.Get("delay", DefaultThrottle);

			this.provider = provider;

			factory = new HashtagPageSannerFactory(
				GetStyleTemplate(),
				settings.Get<bool>("unfiltered"),
				settings.Get<bool>("doubled"));

			Stats = new Statistics();

			lastTime = provider.ReadScanTime();
			//logger.Verbose($"HashtagScanner lastTime {lastTime}");

			startTime = DateTime.Now.ToZuluString();
		}


		public Statistics Stats { get; private set; }


		/// <summary>
		/// A list of notebook IDs to target, used for rescans and rebuilds
		/// </summary>
		public void SetNotebookFilters(string[] filters)
		{
			notebookFilters = filters;
			Stats.FilteredNotebooks = filters.Length;
		}


		private XElement GetStyleTemplate()
		{
			var styleIndex = settings.Get("styleIndex", 0);

			if (styleIndex == 1)
			{
				return new XElement("span",
					new XAttribute("style", "color:red"),
					"$1");
			}
			else if (styleIndex == 2)
			{
				return new XElement("span",
					new XAttribute("style", "background:#FFFF99"),
					"$1");
			}
			else if (styleIndex > 2)
			{
				var styleName = settings.Get<string>("styleName");
				var theme = new ThemeProvider().Theme;
				var style = theme.GetStyles().Find(s => s.Name == styleName);
				if (style is not null)
				{
					style.ApplyColors = true;

					return new XElement("span",
						new XAttribute("style", style.ToCss()),
						"$1");
				}
			}

			return null;
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (ownsProvider)
					{
						provider.Dispose();
					}

					provider = null;
				}

				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		// extended delay used when a foreground command holds a HashtagServicePause token
		private const int PausedThrottle = 500;


		/// <summary>
		/// Reads the hierarchy of every open notebook and brings the identity catalog up to
		/// date, for callers that run outside the pipeline and so have no snapshot.
		/// </summary>
		/// <returns>The pages found, or null if OneNote could not be read</returns>
		internal static async Task<IdentitySnapshot> ReadIdentities(
			CancellationToken token = default, bool fillGuids = true)
		{
			using var identity = new PageIdentityProvider();
			await using var source = new OneNoteHierarchySource();
			return await new IdentityPass(identity, source).Run(token, fillGuids);
		}


		/// <summary>
		/// Determines whether a page must be read from OneNote to look for hashtags. Reading a
		/// page is the expensive step, so a page that has not changed and is known is skipped.
		/// </summary>
		/// <param name="forceThru">True to scan every page regardless</param>
		/// <param name="modified">When the page was last modified, from the hierarchy</param>
		/// <param name="lastTime">When the last complete scan started</param>
		/// <param name="kind">How the identity catalog matched the page. A page that is new to
		/// the catalog has never been scanned. A weak match means the page may not be the one
		/// the tags were recorded for.</param>
		/// <param name="rehomed">True if the page has tags recorded and OneNote has given it a
		/// new page ID, as happens when its notebook is reopened or it is moved. OneNote then
		/// regenerates the IDs of the page's title and paragraphs too, and the tags record
		/// those to navigate to, so the page must be read again to learn the new ones.</param>
		internal static bool NeedsScan(
			bool forceThru, string modified, string lastTime, ResolutionKind kind, bool rehomed)
		{
			return forceThru
				|| rehomed
				|| kind == ResolutionKind.New
				|| kind == ResolutionKind.SameTitle
				|| string.CompareOrdinal(modified, lastTime) > 0;
		}


		/// <summary>
		/// Scan all notebooks for all hashtags
		/// </summary>
		/// <param name="token">Optional token to cancel the scan between pages</param>
		public async Task Scan(CancellationToken token = default)
		{
			var snapshot = await ReadIdentities(token);
			if (snapshot is null)
			{
				logger.WriteLine("error HashtagScanner could not read the notebooks");
				return;
			}

			await Scan(snapshot, token);
		}


		/// <summary>
		/// Scan the pages of an identity snapshot for all hashtags
		/// </summary>
		/// <param name="snapshot">The pages of every open notebook, each with its page key</param>
		/// <param name="token">Optional token to cancel the scan between pages</param>
		public async Task Scan(IdentitySnapshot snapshot, CancellationToken token = default)
		{
			var clock = new Stopwatch();
			clock.Start();

			await using var one = new OneNote();

			var notebooks = snapshot.Notebooks;

			// OneNote regenerates notebook IDs when a notebook is reopened, so recognize a known
			// notebook by its name before anything is decided by its ID
			provider.ReconcileNotebooks(
				notebooks.Select(n => (n.ID, n.Name)).ToList());

			var knownNotebooks = provider.ReadKnownNotebooks();

			Stats.Notebooks += notebooks.Count;
			Stats.KnownNotebooks += knownNotebooks.Count;

			var tagged = provider.ReadTaggedPages();

			foreach (var notebook in notebooks)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				var known = knownNotebooks.Find(n => n.NotebookID == notebook.ID);

				if (known is not null && !known.Included)
				{
					logger.Verbose($"skipping excluded notebook {notebook.ID} \"{notebook.Name}\"");
					continue;
				}

				// Filter on three levels...
				//
				// knownNotebooks
				//   If knownNotebooks is empty, then we assume that this is the first scan
				//   and will allow all; otherwise only scan known notebooks, also include
				//   newly added notebooks that are below the size threshold to avoid causing
				//   OneNote to behave sluggishly - user must schedule a scan to pull in those
				//   new notebooks explicitly.
				//
				// notebookFilters
				//   If notebookFilters is empty then allow any notebook that has passed the
				//   knownNotebook test; otherwise, the user has explicitly requested a scan
				//   of notebooks specified in the notebookFilters list or the notebook is
				//   within the size threshold.
				//
				// size
				//   If there are known notebooks, yet this notebook is not one of them, and
				//   not filtered, then check the size of the new notebook; if below the set
				//   threshold then scan it, otherwise user must schedule explicitly.
				//

				// known is empty so accept any and all
				var accepted = knownNotebooks.Count == 0;

				var forceThru = true;

				if (accepted)
				{
					// force a full rescan of a known notebook if selected
					forceThru =
						notebookFilters is not null && notebookFilters.Contains(notebook.ID);
				}
				else
				{
					if (notebookFilters is null)
					{
						// known notebook so accept it
						if (known is not null)
						{
							accepted = true;
							forceThru = known.LastModified == string.Empty;
						}
						else
						{
							// notebook size is within threshold?
							accepted = notebook.Pages.Count < MaxPagesThreshold;
						}
					}
					else
					{
						accepted = notebookFilters.Contains(notebook.ID);
					}
				}

				if (!accepted)
				{
					logger.Verbose($"skipping notebook {notebook.ID} \"{notebook.Name}\"");
					continue;
				}

				logger.Debug(
					$"scanning notebook {notebook.ID} \"{notebook.Name}\"" +
					(forceThru ? " (forceThru)" : ""));

				var dp = 0;

				if (notebook.Listed)
				{
					dp = await ScanNotebook(one, notebook, tagged, forceThru, token);
					Stats.DirtyPages += dp;
				}

				// record the notebook regardless of whether we find tags; must be done
				// on initial discovery or user would have to explicitly pull it in
				provider.WriteNotebook(notebook.ID, notebook.Name, dp > 0);
			}

			// pages the identity catalog gave up on because they were missing for too long
			var purged = provider.DeleteTags(snapshot.PurgedKeys.Select(Key));
			if (purged > 0)
			{
				logger.WriteLine($"deleted tags of {purged} pages that no longer exist");
			}

			// a scan that was cut short has not seen every page, so leave the time alone and
			// let the next scan pick up what it missed
			if (!token.IsCancellationRequested)
			{
				provider.WriteScanTime(startTime);
			}

			clock.Stop();
			Stats.Time = clock.ElapsedMilliseconds;
		}


		private static string Key(long pageKey)
		{
			return pageKey.ToString(CultureInfo.InvariantCulture);
		}


		private async Task<int> ScanNotebook(
			OneNote one, IdentityNotebook notebook,
			Dictionary<string, HashtagPageInfo> tagged, bool forceThru,
			CancellationToken token)
		{
			var dirtyPages = 0;

			var pages = notebook.Pages.Where(p => !p.IsTagIndex).ToList();
			Stats.TotalPages += pages.Count;
			Stats.Sections += pages.Select(p => p.SectionID).Distinct().Count();

			// where tagged pages are recorded must follow them through reopens, moves and
			// renames; a page whose ID changed is also scanned again below, because the IDs of
			// its title and paragraphs changed with it
			var moved = new List<HashtagPageInfo>();
			var rehomed = new HashSet<long>();
			foreach (var page in pages)
			{
				if (tagged.TryGetValue(Key(page.PageKey), out var info))
				{
					var current = Describe(page, info.TitleID);
					if (!info.SameLocation(current))
					{
						moved.Add(current);
					}

					if (!string.Equals(info.PageID, page.Ref.PageID, StringComparison.Ordinal))
					{
						rehomed.Add(page.PageKey);
					}
				}
			}

			if (moved.Count > 0)
			{
				var count = provider.RefreshPageInfo(moved);
				logger.Verbose($"refreshed the location of {count} tagged pages in \"{notebook.Name}\"");
			}

			var clock = new Stopwatch();

			foreach (var page in pages)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				var isRehomed = rehomed.Contains(page.PageKey);

				if (!NeedsScan(forceThru, page.Ref.Modified, lastTime, page.Resolution.Kind, isRehomed))
				{
					continue;
				}

				// only pages that are actually fetched via COM incur any real cost, so only
				// these are timed and throttled; unmodified pages are skipped entirely and
				// shouldn't pay an idle delay

				clock.Restart();
				// rewrite a rehomed page's tags whether or not they look changed, so the new title and
				// paragraph IDs are recorded
				var dirty = await ScanPage(one, page, forceThru || isRehomed);

				clock.Stop();
				Stats.FetchTime += clock.ElapsedMilliseconds;

				if (dirty)
				{
					dirtyPages++;
				}

				// throttle the workload to give breathing room to OneNote UI;
				// use an extended delay when a foreground command is active
				if (throttle > 0)
				{
					var delay = HashtagServicePause.IsPaused ? PausedThrottle : throttle;
					try
					{
						clock.Restart();
						await Task.Delay(delay, token);
						clock.Stop();
						Stats.ThrottleTime += clock.ElapsedMilliseconds;
					}
					catch (OperationCanceledException)
					{
						break;
					}
				}
			}

			return dirtyPages;
		}


		private static HashtagPageInfo Describe(IdentityPage page, string titleID)
		{
			return new HashtagPageInfo
			{
				MoreID = Key(page.PageKey),
				PageID = page.Ref.PageID,
				TitleID = titleID,
				NotebookID = page.NotebookID,
				SectionID = page.SectionID,
				Path = page.SectionPath,
				Name = page.Ref.Title
			};
		}


		/// <summary>
		/// Scans one page for hashtags and records what it finds.
		/// </summary>
		/// <param name="one">An open OneNote</param>
		/// <param name="identity">The page, from an identity snapshot</param>
		/// <param name="forceThru">True to rewrite the tags even if they have not changed</param>
		/// <returns>True if tags were recorded or removed</returns>
		public async Task<bool> ScanPage(OneNote one, IdentityPage identity, bool forceThru)
		{
			var pageID = identity.Ref.PageID;
			var path = identity.SectionPath;
			Page page;

			try
			{
				page = await one.GetPage(pageID, OneNote.PageDetail.Basic);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error scanning page, possibly locked", exc);
				return false;
			}

			// avoids defect #1268: GetPage throws generic COM exception and returns null...
			if (page is null)
			{
				logger.WriteLine($"skipping null page {pageID} '{path}'");
				return false;
			}

			// the page key identifies the page in the catalog; nothing is written into the page
			var moreID = Key(identity.PageKey);
			var scanner = factory.CreatePageScanner(page, moreID);

			// scan and resolve...

			var candidates = scanner.Scan();

			// saved tags will be in document-order but not have DocumentOrder set,
			// we can rely on tag + objectID to continue resolving
			var saved = provider.ReadPageTags(moreID);

			var discovered = new Hashtags();
			var updated = new Hashtags();

			foreach (var candidate in candidates)
			{
				var found = saved.Find(s => s.Equals(candidate));
				if (found is null)
				{
					discovered.Add(candidate);
				}
				else
				{
					if (forceThru ||
						string.CompareOrdinal(candidate.LastModified, lastTime) > 0 ||
						candidate.DocumentOrder != found.DocumentOrder)
					{
						updated.Add(candidate);
					}

					saved.Remove(found);
				}
			}

			var dirtyPage = false;

			if (saved.Any() || updated.Any() || discovered.Any())
			{
				// much simpler to purge old and rewrite new, even if that means recreating a
				// few copied records. should scale without issue into the many tens-of-tags
				dirtyPage = provider.WriteTags(moreID, candidates);

				Stats.Tags += updated.Count + discovered.Count;
			}

			// the only reason to write the page is to apply the hashtag style the user chose
			if (scanner.UpdateStyle && dirtyPage)
			{
				await one.Update(page);
			}

			if (dirtyPage)
			{
				// will likely rewrite same data but needed in case old page is moved
				provider.WritePageInfo(
					moreID, pageID, page.TitleID,
					identity.NotebookID, identity.SectionID, path, identity.Ref.Title);

				logger.WriteLine($"updated tags found on page {path}/{identity.Ref.Title}");
				return true;
			}

			return false;
		}

		public void Report(string title = null)
		{
			if (!string.IsNullOrWhiteSpace(title))
			{
				logger.Write($"{title} ");
			}

			logger.WriteLine($"scanned {Stats.TotalPages} pages, " +
				$"{Stats.Notebooks} notebooks ({Stats.KnownNotebooks} known), " +
				$"{Stats.Sections} sections, updating {Stats.DirtyPages} pages, " +
				$"saving {Stats.Tags} tags, in {Stats.Time}ms " +
				$"(fetch {Stats.FetchTime}ms, throttle {Stats.ThrottleTime}ms)");
		}
	}
}
