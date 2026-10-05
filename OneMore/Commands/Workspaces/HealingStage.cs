//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Layouts;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Pipeline;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// The favorites and layouts stage of the background pipeline. Each cycle it finds where every
	/// favorite and every window of every layout points now, using the identity snapshot the
	/// identity stage just published, and saves what it finds, so that they keep working through
	/// reopens, moves and renames.
	/// </summary>
	/// <remarks>
	/// It does not depend on any user setting, so these are kept up to date even for someone who has
	/// turned hashtags off. It runs before the hashtag stage so that a long hashtag scan never
	/// delays it. Favorites and layouts are healed independently: a failure in one does not stop the
	/// other.
	/// </remarks>
	internal sealed class HealingStage : Loggable, IPipelineStage
	{
		private readonly Action changed;
		private string lastFavoritesSummary;
		private string lastLayoutsSummary;


		/// <param name="changed">Optional. Called after favorites were saved, so the ribbon menu
		/// can show what changed.</param>
		public HealingStage(Action changed = null)
		{
			this.changed = changed;
		}


		public string Name => "favorites";

		public int Interval => IdentityStage.PassInterval;

		public bool IsEnabled => true;


		public void Initialize()
		{
		}


		public Task<bool> IsReady(CancellationToken token)
		{
			return Task.FromResult(true);
		}


		public async Task Run(PipelineContext context, CancellationToken token)
		{
			// without a list of the pages there is nothing to compare with
			if (!context.TryGet<IdentitySnapshot>(out var snapshot))
			{
				return;
			}

			// OneNote is only needed to make a link for something that is being saved
			OneNote one = null;
			string Link(string id)
			{
				one ??= new OneNote();
				return one.GetHyperlink(id, string.Empty);
			}

			try
			{
				using var identity = new PageIdentityProvider();
				var resolver = new TargetResolver(snapshot, identity.Read);

				var savedFavorites = Heal("favorites", () => HealFavorites(resolver, Link, token));
				Heal("layouts", () => HealWindows(resolver, Link, token));

				if (savedFavorites > 0)
				{
					changed?.Invoke();
				}
			}
			finally
			{
				if (one is not null)
				{
					await one.DisposeAsync();
				}
			}
		}


		// runs one kind of healing so that a failure in it is logged and does not stop the other
		private int Heal(string what, Func<int> heal)
		{
			try
			{
				return heal();
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error healing {what}", exc);
				return 0;
			}
		}


		private int HealFavorites(TargetResolver resolver, Func<string, string> link, CancellationToken token)
		{
			using var provider = new FavoritesProvider();
			var collection = provider.ReadFavorites();

			var favorites = collection.Folders
				.SelectMany(f => f.Items)
				.Concat(collection.Items)
				.OrderBy(f => f.ID)
				.ToList();

			if (favorites.Count == 0)
			{
				return 0;
			}

			var plan = WorkspaceHealer.Plan(favorites, resolver, link);

			var saved = 0;
			foreach (var item in plan.ToSave)
			{
				token.ThrowIfCancellationRequested();

				if (provider.UpdateTarget(item.Favorite, out var duplicate))
				{
					saved++;
					LogSaved(item);
				}
				else if (duplicate)
				{
					LogDuplicate(item);
				}
			}

			LogSummary("favorites", plan, favorites.Count, ref lastFavoritesSummary);
			return saved;
		}


		private int HealWindows(TargetResolver resolver, Func<string, string> link, CancellationToken token)
		{
			using var provider = new LayoutsProvider();

			var windows = provider.ReadLayouts().Layouts
				.SelectMany(l => l.Windows)
				.OrderBy(w => w.ID)
				.ToList();

			if (windows.Count == 0)
			{
				return 0;
			}

			var plan = WorkspaceHealer.PlanWindows(windows, resolver, link);

			var saved = 0;
			foreach (var item in plan.ToSave)
			{
				token.ThrowIfCancellationRequested();

				if (provider.UpdateTarget(item.Window, out var duplicate))
				{
					saved++;
					LogSaved(item);
				}
				else if (duplicate)
				{
					LogDuplicate(item);
				}
			}

			LogSummary("layouts", plan, windows.Count, ref lastLayoutsSummary);
			return saved;
		}


		// one line per repaired reference is detail; the summary says how many, so this is verbose
		private void LogSaved(HealItem item)
		{
			logger.Verbose(
				$"{item.What} {item.ID} found by {item.Resolution.Method}, now {item.Label}");
		}


		private void LogDuplicate(HealItem item)
		{
			logger.WriteLine(
				$"{item.What} {item.ID} {item.Label} points at the same target as another");
		}


		// says so when the picture changes, not every cycle
		private void LogSummary(string what, HealPlan plan, int total, ref string last)
		{
			var summary = plan.Summary();
			if (summary != last && summary is not null)
			{
				logger.WriteLine($"{what}: {summary} of {total}");

				foreach (var item in plan.Items.Where(i => i.Kind == HealKind.Duplicate ||
					i.Kind == HealKind.Broken || i.Kind == HealKind.Ambiguous))
				{
					logger.WriteLine(
						$"..{item.What} {item.ID} {item.Label} is {item.Kind}" +
						(item.Resolution.Reason is null ? string.Empty : $": {item.Resolution.Reason}"));
				}
			}

			last = summary;
		}


		public void Dispose()
		{
		}
	}
}