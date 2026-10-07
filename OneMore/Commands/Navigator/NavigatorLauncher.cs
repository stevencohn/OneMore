//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using System;
	using System.Threading.Tasks;
	using HistoryRecord = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	/// <summary>
	/// The outcome of opening a history or pinned record.
	/// </summary>
	internal sealed class LaunchResult
	{
		public bool Success { get; set; }

		/// <summary>What to tell the user when it did not open, or null.</summary>
		public string Message { get; set; }

		/// <summary>
		/// The same page as OneNote has it now, if the stored record was out of date and the page
		/// was found; the caller saves it with <see cref="NavigationProvider.Replace"/>.
		/// </summary>
		public HistoryRecord Healed { get; set; }
	}


	/// <summary>
	/// Opens a page from the Navigator's history or reading list. OneNote says it succeeded even
	/// when a stored link names a page that has moved or no longer has that ID, so after navigating
	/// this checks where OneNote ended up and, if it is somewhere else, finds the page by its
	/// identity and opens that instead.
	/// </summary>
	internal static class NavigatorLauncher
	{
		// how long to wait for OneNote to finish navigating before deciding the link did not work;
		// a link that works is confirmed on the first or second look
		private const int LandingChecks = 8;
		private const int LandingDelay = 125;


		/// <summary>
		/// Gets what to tell the user when a page could not be found.
		/// </summary>
		internal static string MessageFor(ResolveOutcome outcome)
		{
			return outcome switch
			{
				ResolveOutcome.Pending => Resx.NavigatorLauncher_pending,
				ResolveOutcome.Offline => Resx.NavigatorLauncher_offline,
				ResolveOutcome.Ambiguous => Resx.NavigatorLauncher_ambiguous,
				_ => Resx.NavigatorLauncher_broken
			};
		}


		/// <summary>
		/// Determines whether the page OneNote is showing is the one the stored link names.
		/// </summary>
		/// <returns>False only if it is certain OneNote is somewhere else. True if it is there, or
		/// if there is no way to tell, so that a doubt never turns a good click into an error.</returns>
		internal static bool LandedOn(string storedLink, string currentPageLink)
		{
			var expected = LinkGuids.PageGuid(storedLink);
			var actual = LinkGuids.PageGuid(currentPageLink);

			return expected is null || actual is null ||
				string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Gets what is remembered about a record, as the resolver wants it. A history record has no
		/// page key yet, so it is found by its ID, then by the GUID inside its link.
		/// </summary>
		internal static TargetQuery QueryFor(HistoryRecord record)
		{
			return new TargetQuery
			{
				NotebookID = record.NotebookId,
				SectionID = record.SectionId,
				PageID = record.PageId,
				Uri = record.Link,
				Location = record.Path
			};
		}


		/// <summary>
		/// Opens a page for a click, saves the record if it had to be repaired, and tells the user
		/// if the page could not be found.
		/// </summary>
		/// <param name="owner">The window to show a message over, or null</param>
		/// <param name="record">A history or pinned record</param>
		/// <param name="newWindow">True to open in a new OneNote window</param>
		public static async Task<bool> OpenAndReport(
			System.Windows.Forms.IWin32Window owner, HistoryRecord record, bool newWindow = false)
		{
			var result = await Open(record, newWindow);

			if (result.Healed is not null)
			{
				try
				{
					using var provider = new NavigationProvider();
					await provider.Replace(record, result.Healed);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error saving healed page {record.Path}", exc);
				}
			}

			if (!result.Success && !string.IsNullOrEmpty(result.Message))
			{
				UI.MoreMessageBox.ShowError(owner, result.Message);
			}

			return result.Success;
		}


		/// <summary>
		/// Opens the page a record refers to, finding it by identity if the stored link is dead.
		/// </summary>
		/// <param name="record">A history or pinned record</param>
		/// <param name="newWindow">True to open in a new OneNote window</param>
		public static async Task<LaunchResult> Open(HistoryRecord record, bool newWindow = false)
		{
			var logger = Logger.Current;

			// the stored link opens the page in nearly every case and costs nothing extra
			var success = !string.IsNullOrEmpty(record.Link) && await Navigate(record.Link, newWindow);

			// a new window takes the focus, so what is current there says nothing about the link
			if (success && !newWindow)
			{
				success = await Landed(record);
			}

			if (success)
			{
				return new LaunchResult { Success = true };
			}

			// it did not, so find where the page is now
			TargetResolution resolution;
			try
			{
				resolution = await WorkspaceResolver.Resolve(QueryFor(record));
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error finding page {record.Path}", exc);
				return new LaunchResult { Message = Resx.NavigatorLauncher_failed };
			}

			if (!resolution.IsResolved)
			{
				logger.WriteLine($"page {record.Path} is {resolution.Outcome}: {resolution.Reason}");
				return new LaunchResult { Message = MessageFor(resolution.Outcome) };
			}

			HistoryRecord healed = null;
			string link;

			try
			{
				await using var one = new OneNote();

				link = one.GetHyperlink(resolution.PageID, string.Empty);
				if (string.IsNullOrEmpty(link))
				{
					return new LaunchResult { Message = Resx.NavigatorLauncher_failed };
				}

				success = await Navigate(link, newWindow);

				if (success && resolution.IsConfident)
				{
					healed = await Refresh(one, record, resolution, link);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error opening page {record.Path}", exc);
				return new LaunchResult { Message = Resx.NavigatorLauncher_failed };
			}

			if (success && !resolution.IsConfident)
			{
				// a match by name alone is good enough to open, not to remember
				logger.WriteLine($"page {record.Path} opened by name only");
			}

			return new LaunchResult
			{
				Success = success,
				Healed = healed,
				Message = success ? null : Resx.NavigatorLauncher_failed
			};
		}


		// reads the page as it is now, or falls back to the old record with the new IDs
		private static async Task<HistoryRecord> Refresh(
			OneNote one, HistoryRecord record, TargetResolution resolution, string link)
		{
			HistoryRecord healed = null;

			try
			{
				healed = await one.GetPageInfo(resolution.PageID);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error reading healed page {record.Path}", exc);
			}

			healed ??= new HistoryRecord
			{
				PageId = resolution.PageID,
				TitleId = record.TitleId,
				SectionId = resolution.SectionID ?? record.SectionId,
				NotebookId = resolution.NotebookID ?? record.NotebookId,
				Name = record.Name,
				Path = record.Path,
				Link = link,
				Color = record.Color,
				SectionGroups = record.SectionGroups
			};

			healed.Visited = record.Visited;

			// a paragraph reference cannot be trusted: its object ID changed with the page ID, so
			// the entry falls back to the page itself and keeps the paragraph's text as its name
			healed.ObjectId = null;

			Logger.Current.WriteLine($"page {record.Path} found by {resolution.Method}, now {healed.Path}");
			return healed;
		}


		// waits briefly for OneNote to settle, then compares where it is with where the link points
		private static async Task<bool> Landed(HistoryRecord record)
		{
			try
			{
				await using var one = new OneNote();

				for (var attempt = 0; attempt < LandingChecks; attempt++)
				{
					var page = one.GetHyperlink(one.CurrentPageId, string.Empty);
					if (LandedOn(record.Link, page))
					{
						return true;
					}

					await Task.Delay(LandingDelay);
				}
			}
			catch (Exception exc)
			{
				// never let the check turn a good click into an error
				Logger.Current.WriteLine($"error checking where {record.Path} opened", exc);
				return true;
			}

			Logger.Current.WriteLine($"{record.Path} did not open where its link points");
			return false;
		}


		private static async Task<bool> Navigate(string target, bool newWindow)
		{
			try
			{
				await using var one = new OneNote();
				return await one.NavigateTo(target, newWindow);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error navigating to {target}", exc);
				return false;
			}
		}
	}
}
