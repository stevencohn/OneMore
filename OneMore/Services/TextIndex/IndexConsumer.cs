//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.TextIndex
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Pipeline;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Keeps the text index current as a consumer of the page content pass, sharing each page with
	/// the hashtag scanner so that a changed page is read from OneNote once. It reads the page text
	/// and never writes to a page.
	/// </summary>
	internal sealed class IndexConsumer : Loggable, IPageConsumer, IDisposable
	{
		/// <summary>
		/// The name of the settings collection of the indexing options.
		/// </summary>
		public const string SettingsName = "IndexSheet";

		/// <summary>
		/// How long, in milliseconds, one pass may spend reading pages for the index before it
		/// leaves the rest for the next pass. Reading a page costs OneNote roughly 35 to 90 ms, so
		/// the first index of a large set of notebooks would otherwise be one long burst that makes
		/// OneNote sluggish; this spreads it over many passes. At least one page is read per pass
		/// so that the backlog always shrinks. Measured with 3 s on 1,988 pages: about 62 pages and
		/// 5.5 s per pass, no lag noticed; raised to 5 s on that basis.
		/// </summary>
		public const int PassBudget = 5000;

		/// <summary>
		/// The least time, in milliseconds, to wait after reading a page for the index, even when
		/// the user has turned the hashtag throttle off.
		/// </summary>
		public const int ReadGap = 25;

		// where the installer records its choice; see OneMoreSetup\IndexDlg.wxs
		private const string InstallerKey = @"SOFTWARE\River\OneMoreAddIn";
		private const string InstallerValue = "TextIndex";

		private readonly Func<IndexProvider> providerFactory;
		private readonly TimeSpan budget;
		private readonly Stopwatch reading = new Stopwatch();
		private int requested;
		private int deferred;
		private int batches;
		private int totalIndexed;
		private readonly bool ownsProvider;
		private readonly Func<bool> indexAllSetting;
		private readonly Func<IReadOnlyCollection<long>> readKnownKeys;

		// the number of deliberate builds in progress; background indexing waits for them
		private static int suspended;

		private readonly bool isBuild;
		private readonly Action<int, int> progress;
		private IndexProvider provider;
		private bool indexAll;
		private int expected;
		private Dictionary<long, IndexStamp> stamps;
		private Dictionary<string, bool> choices;
		private bool swept;
		private string prunedFor;
		private bool disposed;


		/// <summary>
		/// Initialize a new instance that opens the standard index when it first runs, so that a
		/// user who never turns indexing on is never given an index database schema.
		/// </summary>
		public IndexConsumer()
			: this(() => new IndexProvider(), IsIndexAll, ReadIdentityKeys, ownsProvider: true)
		{
		}


		/// <summary>
		/// Initialize a new instance with its own index and sources, for testing.
		/// </summary>
		/// <param name="providerFactory">Gets the index to keep current</param>
		/// <param name="indexAllSetting">Says whether to index every notebook that has not been
		/// explicitly excluded, instead of only those explicitly included; read for each pass</param>
		/// <param name="readKnownKeys">Gets the keys of every page the identity catalog has</param>
		/// <param name="ownsProvider">True if this consumer disposes the index it was given</param>
		/// <param name="passBudget">Optional. How long one pass may spend reading pages, instead
		/// of <see cref="PassBudget"/></param>
		/// <param name="isBuild">True for a deliberate build, which is not held back while
		/// <see cref="SuspendBackground"/> is in effect, as the background work is</param>
		/// <param name="progress">Optional. Called after each page is indexed with the number of
		/// pages indexed so far and the number expected</param>
		internal IndexConsumer(
			Func<IndexProvider> providerFactory, Func<bool> indexAllSetting,
			Func<IReadOnlyCollection<long>> readKnownKeys, bool ownsProvider,
			TimeSpan? passBudget = null, bool isBuild = false, Action<int, int> progress = null)
		{
			this.providerFactory = providerFactory;
			this.indexAllSetting = indexAllSetting;
			this.readKnownKeys = readKnownKeys;
			this.ownsProvider = ownsProvider;
			this.isBuild = isBuild;
			this.progress = progress;
			budget = passBudget ?? TimeSpan.FromMilliseconds(PassBudget);
		}


		/// <summary>
		/// Creates a consumer for a deliberate build of the index. It has no time budget, only the
		/// small gap between reads, and reports its progress.
		/// </summary>
		/// <param name="progress">Called after each page is indexed with the number of pages
		/// indexed so far and the number expected</param>
		public static IndexConsumer ForBuild(Action<int, int> progress, bool indexAll)
		{
			return new IndexConsumer(
				() => new IndexProvider(), () => indexAll, ReadIdentityKeys, ownsProvider: true,
				passBudget: TimeSpan.MaxValue, isBuild: true, progress: progress);
		}


		/// <summary>
		/// Holds back the background indexing until the returned handle is disposed, so that a
		/// deliberate build does not compete with it.
		/// </summary>
		public static IDisposable SuspendBackground()
		{
			Interlocked.Increment(ref suspended);
			return new Resumer();
		}


		private sealed class Resumer : IDisposable
		{
			private int disposed;

			public void Dispose()
			{
				if (Interlocked.Exchange(ref disposed, 1) == 0)
				{
					Interlocked.Decrement(ref suspended);
				}
			}
		}


		public void Dispose()
		{
			if (!disposed)
			{
				if (ownsProvider)
				{
					provider?.Dispose();
				}

				provider = null;
				disposed = true;
			}
		}


		/// <summary>
		/// Gets whether the user has turned indexing on and the index can be used in this process.
		/// This reads the setting each time, so a change made while OneNote is running takes effect
		/// without a restart.
		/// </summary>
		public static bool IsEnabled()
		{
			// the setting is asked first so that nobody who has not turned indexing on pays for the
			// one-time probe that loads FTS5
			return IsEnabledSetting() && IndexProvider.IsAvailable;
		}


		/// <summary>
		/// Gets whether the user has turned indexing on, ignoring whether it can be used. A user
		/// who has not made a choice in Settings gets what the installer was asked for.
		/// </summary>
		public static bool IsEnabledSetting()
		{
			return new SettingsProvider().GetCollection(SettingsName)
				.Get("enabled", InstalledDefault());
		}


		/// <summary>
		/// Gets whether the installer was told to turn indexing on for this computer. Indexing is
		/// off unless it was. The installer cannot write the settings of the interactive user, so
		/// it leaves a machine-wide default that applies until the user chooses in Settings.
		/// </summary>
		public static bool InstalledDefault()
		{
			try
			{
				using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(InstallerKey, false);
				return key?.GetValue(InstallerValue) as string == "1";
			}
			catch (Exception)
			{
				return false;
			}
		}


		/// <summary>
		/// Gets whether every notebook is indexed unless the user excluded it.
		/// </summary>
		public static bool IsIndexAll()
		{
			// a user who has not chosen, on a computer where the installer turned indexing on,
			// gets every notebook; otherwise they would have to pick notebooks before anything
			// is indexed
			return new SettingsProvider().GetCollection(SettingsName)
				.Get("all", InstalledDefault());
		}


		public string Name => "index";


		/// <summary>
		/// Gets the least time to wait after reading a page that this consumer needs.
		/// </summary>
		public int MinimumThrottle => ReadGap;


		/// <summary>
		/// Gets how many pages were indexed by the last pass.
		/// </summary>
		public int Indexed { get; private set; }


		/// <summary>
		/// Determines whether a page must be read to bring the index up to date. Reading a page is
		/// the expensive step, so a page that has not changed is skipped.
		/// </summary>
		/// <param name="stamp">What was true of the page when it was indexed, or null if it never was</param>
		/// <param name="pageID">The current OneNote page ID of the page</param>
		/// <param name="modified">When the page was last modified, from the hierarchy</param>
		/// <param name="kind">How the identity catalog matched the page. A weak match means the page
		/// may not be the one that was indexed.</param>
		internal static bool NeedsIndexing(
			IndexStamp stamp, string pageID, string modified, ResolutionKind kind)
		{
			return stamp is null
				// OneNote gave the page a new ID, which regenerates the IDs of its paragraphs too,
				// and the index records those to navigate to
				|| !string.Equals(stamp.PageID, pageID, StringComparison.Ordinal)
				// compared for any difference, not only newer, so a page restored from an older
				// version is indexed again
				|| !string.Equals(stamp.Modified, modified, StringComparison.Ordinal)
				|| kind == ResolutionKind.SameTitle;
		}


		#region IPageConsumer
		public void Begin(IdentitySnapshot snapshot)
		{
			provider ??= providerFactory();
			indexAll = indexAllSetting();

			Indexed = 0;
			requested = 0;
			deferred = 0;
			reading.Reset();

			stamps = provider.ReadStamps();

			choices = provider.ReadNotebooks()
				.ToDictionary(n => n.NotebookKey, n => n.Included, StringComparer.Ordinal);

			// how much a build has to do, so that it can say how far along it is
			expected = 0;
			if (progress is not null)
			{
				foreach (var notebook in snapshot.Notebooks.Where(n => n.Listed && IsChosen(n)))
				{
					expected += notebook.Pages.Count(p => !p.IsTagIndex && NeedsIndexing(
						stamps.TryGetValue(p.PageKey, out var s) ? s : null,
						p.Ref.PageID, p.Ref.Modified, p.Resolution.Kind));
				}
			}
		}


		// Whether the notebook is indexed. When the user chose every notebook that is what it
		// means: a notebook that was left unchecked in the picker earlier is remembered, for when
		// the user goes back to choosing, but does not hold back "all".
		private bool IsChosen(IdentityNotebook notebook)
		{
			return IsChosenKey(notebook.Key);
		}


		private bool IsChosenKey(string notebookKey)
		{
			return indexAll
				|| (choices.TryGetValue(notebookKey, out var included) && included);
		}


		public bool BeginNotebook(IdentityNotebook notebook)
		{
			// a deliberate build is working through the same pages; let it finish
			if (!isBuild && Volatile.Read(ref suspended) > 0)
			{
				return false;
			}

			return IsChosenKey(notebook.Key);
		}


		public bool NeedsPage(IdentityPage page)
		{
			// a page OneMore generated to index hashtags is not content
			if (page.IsTagIndex)
			{
				return false;
			}

			stamps.TryGetValue(page.PageKey, out var stamp);

			if (!NeedsIndexing(stamp, page.Ref.PageID, page.Ref.Modified, page.Resolution.Kind))
			{
				return false;
			}

			// the page before this one was asked for but never given, because it could not be read
			if (reading.IsRunning)
			{
				reading.Stop();
			}

			// out of time for this pass; the page keeps its place in line, because it still has
			// no up to date stamp, and is asked for again next pass
			if (requested > 0 && reading.Elapsed >= budget)
			{
				deferred++;
				return false;
			}

			// the pass reads the page and hands it to Consume next, so the time until then is
			// the cost of reading it, and of whatever is done with it by the consumers before us
			requested++;
			reading.Start();
			return true;
		}


		public Task<bool> Consume(OneNote one, Page page, IdentityPage identity)
		{
			// Paragraph text only; a generated table of contents is not searched. The modified
			// time recorded is the one in the hierarchy when the pass began, which can only be
			// older than the page just read, so a page edited in between is indexed again.
			var paragraphs = PageTextExtractor.Extract(page, includeToc: false);

			provider.ReplacePage(
				identity.PageKey, identity.Ref.PageID, identity.Ref.Modified, paragraphs);

			stamps[identity.PageKey] = new IndexStamp
			{
				PageID = identity.Ref.PageID,
				Modified = identity.Ref.Modified
			};

			reading.Stop();
			Indexed++;

			progress?.Invoke(Indexed, expected);
			return Task.FromResult(true);
		}


		public void EndNotebook(IdentityNotebook notebook, int dirtyPages)
		{
			// the time is only recorded for a notebook the user chose, so that indexing every
			// notebook does not turn each into an explicit choice
			if (dirtyPages > 0 && choices.TryGetValue(notebook.Key, out var included) && included)
			{
				provider.WriteNotebook(notebook.Key, true, DateTime.Now.ToZuluString());
			}
		}


		public void End(IdentitySnapshot snapshot, bool cancelled)
		{
			// pages the identity catalog gave up on because they were missing for too long
			var purged = provider.DeletePages(snapshot.PurgedKeys.ToList());
			if (purged > 0)
			{
				logger.WriteLine($"removed {purged} pages that no longer exist from the text index");
			}

			// Notebooks that are no longer chosen keep nothing in the index: their text would only
			// go out of date, and take space. This is done when the choice changes, and once at the
			// start of a session, not every pass. A build does not prune; it only adds.
			// Not while a build is running either: its notebook choice may be one the user has
			// not saved yet, and pruning by the saved choice would remove what it is adding.
			if (!cancelled && !isBuild && Volatile.Read(ref suspended) == 0)
			{
				var signature = ChoiceSignature();
				if (signature != prunedFor)
				{
					var removed = provider.PruneNotebooks(IsChosenKey);
					prunedFor = signature;

					if (removed > 0)
					{
						logger.WriteLine(
							$"removed {removed} pages of notebooks that are not indexed from the text index");
					}
				}
			}

			// A purge is announced in a single pass, and this consumer may not have been running
			// then, so once per session remove what the catalog no longer knows. Pages that are
			// merely not visible now, such as those of a closed notebook, are still in the catalog.
			if (!swept && !cancelled)
			{
				swept = true;

				var known = readKnownKeys();
				if (known is not null)
				{
					var knownSet = new HashSet<long>(known);
					var orphans = stamps.Keys.Where(k => !knownSet.Contains(k)).ToList();
					if (orphans.Count > 0)
					{
						var removed = provider.DeletePages(orphans);
						logger.WriteLine($"removed {removed} orphaned pages from the text index");
					}
				}
			}

			// one line per batch, the pages read within a pass, to follow a long first index
			if (Indexed > 0)
			{
				batches++;
				totalIndexed += Indexed;

				var all = totalIndexed + deferred;
				var percent = all > 0 ? totalIndexed * 100 / all : 100;

				logger.Verbose(
					$"text index batch {batches}: {Indexed} pages in {reading.Elapsed.TotalSeconds:0.0}s, " +
					$"{totalIndexed} of {all} indexed ({percent}%), {deferred} remaining" +
					(cancelled ? ", cancelled" : string.Empty));
			}
		}
		#endregion IPageConsumer


		// what decides which notebooks are chosen, so that a change in it can be noticed
		private string ChoiceSignature()
		{
			var chosen = choices
				.Where(c => c.Value)
				.Select(c => c.Key)
				.OrderBy(k => k, StringComparer.Ordinal);

			return $"{indexAll}|{string.Join("\u001f", chosen)}";
		}


		// the keys of every page the identity catalog has; null if it cannot be read, in which
		// case nothing is swept
		private static IReadOnlyCollection<long> ReadIdentityKeys()
		{
			try
			{
				using var identity = new PageIdentityProvider();
				return identity.ReadAll().Select(r => r.PageKey).ToList();
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error reading page identities to sweep the text index", exc);
				return null;
			}
		}
	}
}
