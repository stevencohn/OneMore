//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Pipeline
{
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Walks the pages of an <see cref="IdentitySnapshot"/> and gives each page that any
	/// <see cref="IPageConsumer"/> needs to every consumer that needs it, reading the page from
	/// OneNote only once. It owns the throttle that keeps the background work from crowding the
	/// OneNote UI, so the throttle is spent once per page read and not once per consumer.
	/// </summary>
	internal sealed class PageContentPass : Loggable
	{
		/// <summary>
		/// The delay used between pages when a foreground command asks the background to back off.
		/// </summary>
		public const int PausedThrottle = 500;

		/// <summary>
		/// What a pass spent, for the consumer's own report.
		/// </summary>
		public sealed class Statistics
		{
			/// <summary>Milliseconds spent reading pages and handing them to consumers.</summary>
			public long FetchTime;

			/// <summary>Milliseconds spent waiting between pages.</summary>
			public long ThrottleTime;

			/// <summary>The number of pages read from OneNote.</summary>
			public int PagesRead;
		}


		private readonly IReadOnlyList<IPageConsumer> consumers;
		private readonly int throttle;
		private readonly Func<bool> isPaused;
		private readonly Func<OneNote, IdentityPage, Task<Page>> loader;
		private readonly bool opensOneNote;


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		/// <param name="consumers">The consumers to give pages to, in the order to call them</param>
		/// <param name="throttle">Milliseconds to wait after each page read, or zero for none</param>
		/// <param name="isPaused">Optional. Says whether a foreground command wants the background
		/// to back off, in which case <see cref="PausedThrottle"/> is used instead of the throttle</param>
		public PageContentPass(
			IReadOnlyList<IPageConsumer> consumers, int throttle, Func<bool> isPaused = null)
			: this(consumers, throttle, isPaused, LoadPage, opensOneNote: true)
		{
		}


		/// <summary>
		/// Initialize a new instance with its own page loader, so it can be tested without OneNote.
		/// </summary>
		/// <param name="opensOneNote">False to hand the loader and the consumers a null OneNote
		/// instead of opening one</param>
		internal PageContentPass(
			IReadOnlyList<IPageConsumer> consumers, int throttle, Func<bool> isPaused,
			Func<OneNote, IdentityPage, Task<Page>> loader, bool opensOneNote = false)
		{
			this.consumers = consumers;
			this.throttle = throttle;
			this.isPaused = isPaused ?? (() => false);
			this.loader = loader;
			this.opensOneNote = opensOneNote;
		}


		/// <summary>
		/// Gets what the last <see cref="Run"/> spent.
		/// </summary>
		public Statistics Stats { get; private set; } = new Statistics();


		/// <summary>
		/// Reads a page from OneNote with the detail that content consumers need.
		/// </summary>
		/// <returns>The page, or null if it could not be read, such as a locked section</returns>
		internal static async Task<Page> LoadPage(OneNote one, IdentityPage identity)
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
				Logger.Current.WriteLine("error reading page, possibly locked", exc);
				return null;
			}

			// avoids defect #1268: GetPage throws generic COM exception and returns null...
			if (page is null)
			{
				Logger.Current.WriteLine($"skipping null page {pageID} '{path}'");
				return null;
			}

			return page;
		}


		/// <summary>
		/// Runs the consumers over every page of every notebook of the snapshot.
		/// </summary>
		/// <param name="snapshot">The pages found by the identity pass</param>
		/// <param name="token">Optional token to cancel the pass between pages</param>
		public async Task Run(IdentitySnapshot snapshot, CancellationToken token = default)
		{
			Stats = new Statistics();

			if (!opensOneNote)
			{
				await Visit(null, snapshot, token);
				return;
			}

			await using var one = new OneNote();
			await Visit(one, snapshot, token);
		}


		private async Task Visit(OneNote one, IdentitySnapshot snapshot, CancellationToken token)
		{
			foreach (var consumer in consumers)
			{
				consumer.Begin(snapshot);
			}

			foreach (var notebook in snapshot.Notebooks)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				var active = new List<IPageConsumer>();
				foreach (var consumer in consumers)
				{
					if (consumer.BeginNotebook(notebook))
					{
						active.Add(consumer);
					}
				}

				if (active.Count == 0)
				{
					continue;
				}

				var dirty = new int[active.Count];

				// a notebook that could not be listed is not the same as an empty one; it has no
				// pages to visit but each consumer is still told it was accepted
				if (notebook.Listed)
				{
					await VisitPages(one, notebook, active, dirty, token);
				}

				for (var i = 0; i < active.Count; i++)
				{
					active[i].EndNotebook(notebook, dirty[i]);
				}
			}

			foreach (var consumer in consumers)
			{
				consumer.End(snapshot, token.IsCancellationRequested);
			}
		}


		private async Task VisitPages(
			OneNote one, IdentityNotebook notebook,
			List<IPageConsumer> active, int[] dirty, CancellationToken token)
		{
			var clock = new Stopwatch();
			var wanting = new List<int>(active.Count);

			foreach (var page in notebook.Pages)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				wanting.Clear();
				for (var i = 0; i < active.Count; i++)
				{
					if (active[i].NeedsPage(page))
					{
						wanting.Add(i);
					}
				}

				if (wanting.Count == 0)
				{
					continue;
				}

				// only pages that are actually fetched via COM incur any real cost, so only these
				// are timed and throttled; unmodified pages are skipped entirely and shouldn't pay
				// an idle delay

				clock.Restart();

				var loaded = await loader(one, page);
				if (loaded is not null)
				{
					foreach (var i in wanting)
					{
						if (await active[i].Consume(one, loaded, page))
						{
							dirty[i]++;
						}
					}
				}

				clock.Stop();
				Stats.FetchTime += clock.ElapsedMilliseconds;
				Stats.PagesRead++;

				// throttle the workload to give breathing room to OneNote UI; use an extended
				// delay when a foreground command is active. A consumer that wanted the page may
				// ask for more than the user's setting, which then acts as a floor
				var wait = throttle;
				foreach (var i in wanting)
				{
					wait = Math.Max(wait, active[i].MinimumThrottle);
				}

				if (wait > 0)
				{
					var delay = isPaused() ? PausedThrottle : wait;
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
		}
	}
}
