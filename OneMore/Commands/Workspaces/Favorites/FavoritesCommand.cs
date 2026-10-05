//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Workspaces;
	using System;
	using System.Globalization;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class FavoritesCommand : Command
	{
		// how long to wait for OneNote to finish navigating before deciding the link did not work;
		// a link that works is confirmed on the first or second look
		private const int LandingChecks = 8;
		private const int LandingDelay = 125;


		public FavoritesCommand()
		{
			// do not write to MRU
			IsCancelled = true;
		}


		/// <summary>
		/// Gets the favorite ID that a ribbon button's tag carries, or null if the tag is not one.
		/// Older tags are a link or an ID, never made only of digits.
		/// </summary>
		internal static int? ParseFavoriteID(string tag)
		{
			return !string.IsNullOrEmpty(tag) &&
				int.TryParse(tag, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
				? id
				: null;
		}


		/// <summary>
		/// Gets what to tell the user when a favorite could not be found.
		/// </summary>
		internal static string MessageFor(ResolveOutcome outcome)
		{
			return outcome switch
			{
				ResolveOutcome.Pending => Resx.FavoritesCommand_pending,
				ResolveOutcome.Offline => Resx.FavoritesCommand_offline,
				ResolveOutcome.Ambiguous => Resx.FavoritesCommand_ambiguous,
				_ => Resx.FavoritesCommand_broken
			};
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			var tag = args == null || args.Length == 0 ? null : (string)args[0];

			Favorite favorite = null;
			string uri;

			if (string.IsNullOrWhiteSpace(tag))
			{
				using var dialog = new FavoritesDialog(ribbon);
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return;
				}

				if (dialog.Manage)
				{
					await factory.Run<ManageWorkspaceCommand>(WorkspaceTab.Favorites);
					return;
				}

				favorite = dialog.Selected;
				uri = dialog.Uri;
			}
			else if (ParseFavoriteID(tag) is int id)
			{
				using var provider = new FavoritesProvider();
				favorite = provider.ReadFavorite(id);
				uri = favorite?.GetNavigationTarget();
			}
			else
			{
				// an older tag, which is a link or an ID
				uri = tag;
			}

			if (string.IsNullOrWhiteSpace(uri))
			{
				return;
			}

			// the stored link opens the target in nearly every case and costs nothing extra, but
			// OneNote says it succeeded even if the link names something that no longer exists,
			// so check where it actually ended up
			var success = await Navigate(uri);
			string failure = null;

			if (success && favorite is not null)
			{
				success = await Landed(favorite);
			}

			if (!success && favorite is not null)
			{
				// it did not, so find where the target is now
				(success, failure) = await NavigateByResolving(favorite);
			}

			// reset focus to OneNote window
			await using var onx = new OneNote();
			Native.SwitchToThisWindow(onx.WindowHandle, false);

			if (!success)
			{
				ShowError(failure ?? "Could not navigate at this time. Try again in a few seconds");
			}
		}


		// waits briefly for OneNote to settle, then compares where it is with where the link points
		private async Task<bool> Landed(Favorite favorite)
		{
			try
			{
				await using var one = new OneNote();

				for (var attempt = 0; attempt < LandingChecks; attempt++)
				{
					var page = one.GetHyperlink(one.CurrentPageId, string.Empty);
					var section = one.GetHyperlink(one.CurrentSectionId, string.Empty);

					if (WorkspaceResolver.LandedOn(favorite, page, section))
					{
						return true;
					}

					await Task.Delay(LandingDelay);
				}
			}
			catch (Exception exc)
			{
				// never let the check turn a good click into an error
				logger.WriteLine($"error checking where favorite {favorite.ID} opened", exc);
				return true;
			}

			logger.WriteLine(
				$"favorite {favorite.ID} {favorite.Location} did not open where its link points");

			return false;
		}


		private async Task<bool> Navigate(string target)
		{
			try
			{
				await using var one = new OneNote();
				return await one.NavigateTo(target);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error navigating to {target}", exc);
				return false;
			}
		}


		// finds where a favorite is now, opens it, and remembers what was found if it is certain
		private async Task<(bool, string)> NavigateByResolving(Favorite favorite)
		{
			TargetResolution resolution;

			try
			{
				resolution = await WorkspaceResolver.Resolve(TargetQuery.From(favorite));
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error finding favorite {favorite.ID}", exc);
				return (false, null);
			}

			if (!resolution.IsResolved)
			{
				logger.WriteLine(
					$"favorite {favorite.ID} {favorite.Location} is {resolution.Outcome}: {resolution.Reason}");

				return (false, MessageFor(resolution.Outcome));
			}

			await using var one = new OneNote();

			string Link(string id) => one.GetHyperlink(id, string.Empty);

			var target = WorkspaceResolver.NavigationTarget(resolution, favorite.Kind, Link);
			var success = !string.IsNullOrEmpty(target) && await Navigate(target);

			if (success && resolution.IsConfident &&
				WorkspaceResolver.Apply(favorite, resolution, Link))
			{
				// so the next click opens it directly
				using var provider = new FavoritesProvider();
				if (provider.UpdateTarget(favorite))
				{
					logger.WriteLine(
						$"favorite {favorite.ID} found by {resolution.Method}, now {favorite.Location}");

					ribbon.SafeInvalidateControl(FavoritesMenu.MenuID);
				}
			}

			if (success && !resolution.IsConfident)
			{
				// a match by name alone is good enough to open, not to remember
				logger.WriteLine(
					$"favorite {favorite.ID} {favorite.Location} opened by name only");
			}

			return (success, null);
		}
	}
}