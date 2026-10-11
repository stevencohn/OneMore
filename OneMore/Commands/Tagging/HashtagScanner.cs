//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S125

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Pipeline;
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
	/// Scans all notebooks for hashtags. As an <see cref="IPageConsumer"/> it can share each page
	/// it reads with other consumers in a <see cref="PageContentPass"/>; the
	/// <see cref="Scan(IdentitySnapshot, CancellationToken)"/> method runs a pass of its own for
	/// callers that have no other consumers.
	/// </summary>
	internal class HashtagScanner : Loggable, IPageConsumer, IDisposable
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

		// state of the pass in progress, set by Begin and BeginNotebook
		private HashtagNotebooks knownNotebooks;
		private Dictionary<string, HashtagPageInfo> tagged;
		private bool notebookForceThru;
		private HashSet<long> rehomed = new();


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
		/// Gets the delay, in milliseconds, between pages that a page content pass should wait
		/// for when it is not run by a scanner, so that every pass honors the same setting.
		/// </summary>
		public static int ReadThrottle()
		{
			return new SettingsProvider().GetCollection("HashtagSheet").Get("delay", DefaultThrottle);
		}


		/// <summary>
		/// Gets a short name used in log messages.
		/// </summary>
		public string Name => "hashtags";


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
		/// Scan the pages of an identity snapshot for all hashtags, with a page content pass of
		/// this scanner alone
		/// </summary>
		/// <param name="snapshot">The pages of every open notebook, each with its page key</param>
		/// <param name="token">Optional token to cancel the scan between pages</param>
		public Task Scan(IdentitySnapshot snapshot, CancellationToken token = default)
		{
			return Scan(snapshot, Array.Empty<IPageConsumer>(), token);
		}


		/// <summary>
		/// Scan the pages of an identity snapshot for all hashtags, in a page content pass that
		/// also gives each page it reads to other consumers
		/// </summary>
		/// <param name="snapshot">The pages of every open notebook, each with its page key</param>
		/// <param name="others">Other consumers that share the pages this pass reads. They are
		/// called before this scanner, which may write a hashtag style to a page</param>
		/// <param name="token">Optional token to cancel the scan between pages</param>
		public async Task Scan(
			IdentitySnapshot snapshot, IReadOnlyList<IPageConsumer> others, CancellationToken token = default)
		{
			var clock = new Stopwatch();
			clock.Start();

			var consumers = new List<IPageConsumer>(others);
			consumers.Add(this);

			var pass = new PageContentPass(consumers, throttle, () => HashtagServicePause.IsPaused);

			await pass.Run(snapshot, token);

			Stats.FetchTime += pass.Stats.FetchTime;
			Stats.ThrottleTime += pass.Stats.ThrottleTime;

			clock.Stop();
			Stats.Time = clock.ElapsedMilliseconds;
		}


		#region IPageConsumer
		// hashtags are only scanned for pages that changed, so the user's throttle is enough
		int IPageConsumer.MinimumThrottle => 0;


		void IPageConsumer.Begin(IdentitySnapshot snapshot)
		{
			var notebooks = snapshot.Notebooks;

			// OneNote regenerates notebook IDs when a notebook is reopened, so recognize a known
			// notebook by its name before anything is decided by its ID
			provider.ReconcileNotebooks(
				notebooks.Select(n => (n.ID, n.Name)).ToList());

			knownNotebooks = provider.ReadKnownNotebooks();

			Stats.Notebooks += notebooks.Count;
			Stats.KnownNotebooks += knownNotebooks.Count;

			tagged = provider.ReadTaggedPages();
		}


		bool IPageConsumer.BeginNotebook(IdentityNotebook notebook)
		{
			rehomed = new HashSet<long>();
			notebookForceThru = true;

			var known = knownNotebooks.Find(n => n.NotebookID == notebook.ID);

			if (known is not null && !known.Included)
			{
				logger.Verbose($"skipping excluded notebook {notebook.ID} \"{notebook.Name}\"");
				return false;
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

			if (accepted)
			{
				// force a full rescan of a known notebook if selected
				notebookForceThru =
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
						notebookForceThru = known.LastModified == string.Empty;
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
				return false;
			}

			logger.Debug(
				$"scanning notebook {notebook.ID} \"{notebook.Name}\"" +
				(notebookForceThru ? " (forceThru)" : ""));

			if (notebook.Listed)
			{
				PrepareNotebook(notebook);
			}

			return true;
		}


		bool IPageConsumer.NeedsPage(IdentityPage page)
		{
			// a page OneMore generated to index hashtags is not content
			if (page.IsTagIndex)
			{
				return false;
			}

			return NeedsScan(
				notebookForceThru, page.Ref.Modified, lastTime, page.Resolution.Kind,
				rehomed.Contains(page.PageKey));
		}


		async Task<bool> IPageConsumer.Consume(OneNote one, Page page, IdentityPage identity)
		{
			// rewrite a rehomed page's tags whether or not they look changed, so the new title and
			// paragraph IDs are recorded
			var forceThru = notebookForceThru || rehomed.Contains(identity.PageKey);

			return await ScanLoadedPage(one, page, identity, forceThru);
		}


		void IPageConsumer.EndNotebook(IdentityNotebook notebook, int dirtyPages)
		{
			Stats.DirtyPages += dirtyPages;

			// record the notebook regardless of whether we find tags; must be done
			// on initial discovery or user would have to explicitly pull it in
			provider.WriteNotebook(notebook.ID, notebook.Name, dirtyPages > 0);
		}


		void IPageConsumer.End(IdentitySnapshot snapshot, bool cancelled)
		{
			// pages the identity catalog gave up on because they were missing for too long
			var purged = provider.DeleteTags(snapshot.PurgedKeys.Select(Key));
			if (purged > 0)
			{
				logger.WriteLine($"deleted tags of {purged} pages that no longer exist");
			}

			// a scan that was cut short has not seen every page, so leave the time alone and
			// let the next scan pick up what it missed
			if (!cancelled)
			{
				provider.WriteScanTime(startTime);
			}
		}
		#endregion IPageConsumer


		private static string Key(long pageKey)
		{
			return pageKey.ToString(CultureInfo.InvariantCulture);
		}


		// counts what is about to be scanned and finds the tagged pages that moved or whose ID
		// changed, before the pages of a notebook are visited
		private void PrepareNotebook(IdentityNotebook notebook)
		{
			var pages = notebook.Pages.Where(p => !p.IsTagIndex).ToList();
			Stats.TotalPages += pages.Count;
			Stats.Sections += pages.Select(p => p.SectionID).Distinct().Count();

			// where tagged pages are recorded must follow them through reopens, moves and
			// renames; a page whose ID changed is also scanned again, because the IDs of its
			// title and paragraphs changed with it
			var moved = new List<HashtagPageInfo>();
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
			var page = await PageContentPass.LoadPage(one, identity);
			if (page is null)
			{
				return false;
			}

			return await ScanLoadedPage(one, page, identity, forceThru);
		}


		// scans a page that has already been read, which a pass shares with other consumers
		private async Task<bool> ScanLoadedPage(
			OneNote one, Page page, IdentityPage identity, bool forceThru)
		{
			var pageID = identity.Ref.PageID;
			var path = identity.SectionPath;

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
