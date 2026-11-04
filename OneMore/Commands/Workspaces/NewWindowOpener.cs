//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Opens targets in new OneNote windows and finds and raises windows that already exist.
	/// Shared by restoring a layout and by opening favorites in new windows.
	/// </summary>
	internal static class NewWindowOpener
	{
		private const int MaxNewWindowWaitAttempts = 10;
		private const int NewWindowPollMilliseconds = 300;
		private const int OpenAttempts = 3;
		private const int RetryDelayMilliseconds = 1500;

		/// <summary>
		/// How long to let a newly opened window finish loading before asking for the next one. A
		/// second new-window link that arrives while the first is still loading is refused, and a
		/// refused link still leaves a stray window behind, showing the page of the first.
		/// </summary>
		public const int WindowSettleMilliseconds = 1500;


		/// <summary>
		/// Finds the top-level handle of the first open window showing a page.
		/// </summary>
		public static Task<IntPtr> FindWindowHandle(OneNote one, string pageID)
		{
			return FindWindowHandle(one, w => w.CurrentPageId == pageID);
		}


		/// <summary>
		/// Finds the top-level handle of the first open window that satisfies the predicate.
		/// </summary>
		/// <returns>The handle, or IntPtr.Zero if there is none</returns>
		public static async Task<IntPtr> FindWindowHandle(OneNote one, Func<WindowInfo, bool> match)
		{
			var open = await one.GetWindows(detailed: false);
			var found = open.FirstOrDefault(match);
			return found is null ? IntPtr.Zero : ParseHandle(found.WindowHandle);
		}


		/// <summary>
		/// Gets the handles of the windows that are open now.
		/// </summary>
		public static async Task<List<IntPtr>> GetHandles(OneNote one)
		{
			var open = await one.GetWindows(detailed: false);
			return open.Select(w => ParseHandle(w.WindowHandle)).ToList();
		}


		/// <summary>
		/// Opens a new window for the target, trying again if OneNote refuses the link. Right after
		/// a notebook has been reopened OneNote can refuse a link to a page that it opens a moment
		/// later, typically the second new window in quick succession.
		/// </summary>
		/// <param name="one">The OneNote connection</param>
		/// <param name="uri">The link or ID to open</param>
		/// <param name="match">Recognizes the new window once it shows the target</param>
		/// <param name="description">Names the target in the log</param>
		/// <param name="onRetry">Called after a failed attempt, or null</param>
		/// <param name="existing">The windows that were open before, to be ignored when looking for
		/// the new one; needed to open a second window for a target that is already showing</param>
		/// <returns>The handle of the new window, or IntPtr.Zero if it did not open</returns>
		public static async Task<IntPtr> OpenWindow(
			OneNote one, string uri, Func<WindowInfo, bool> match, string description,
			Func<Task> onRetry = null, ICollection<IntPtr> existing = null)
		{
			if (existing is not null)
			{
				var original = match;
				match = w => original(w) && !existing.Contains(ParseHandle(w.WindowHandle));
			}

			for (var attempt = 1; attempt <= OpenAttempts; attempt++)
			{
				// non-destructive: never close/reuse other windows, only add new ones. A refused
				// link opens nothing, so there is no window to wait for
				if (await one.NavigateTo(uri, newWindow: true))
				{
					var handle = await WaitForWindowHandle(one, match);
					if (handle != IntPtr.Zero)
					{
						return handle;
					}
				}

				if (attempt < OpenAttempts)
				{
					Logger.Current.WriteLine($"window {description} did not open, trying again");

					if (onRetry is not null)
					{
						await onRetry();
					}

					await Task.Delay(attempt * RetryDelayMilliseconds);
				}
			}

			return IntPtr.Zero;
		}


		private static async Task<IntPtr> WaitForWindowHandle(OneNote one, Func<WindowInfo, bool> match)
		{
			for (var attempt = 0; attempt < MaxNewWindowWaitAttempts; attempt++)
			{
				await Task.Delay(NewWindowPollMilliseconds);

				var handle = await FindWindowHandle(one, match);
				if (handle != IntPtr.Zero)
				{
					return handle;
				}
			}

			return IntPtr.Zero;
		}


		/// <summary>
		/// Shows a window, restoring it if minimized, and brings it to the front.
		/// </summary>
		public static void Raise(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}

			Native.ShowWindow(handle, Native.SW_RESTORE);
			Native.BringWindowToTop(handle);
			Native.SetForegroundWindow(handle);
		}


		/// <summary>
		/// Asks a window to close, as if the user clicked its close button.
		/// </summary>
		public static void Close(IntPtr handle)
		{
			if (handle != IntPtr.Zero)
			{
				Native.PostMessage(handle, Native.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			}
		}


		/// <summary>
		/// Closes every open OneNote window except the ones given.
		/// </summary>
		public static async Task CloseOthers(OneNote one, ICollection<IntPtr> keep)
		{
			// never close the last window, which would exit OneNote
			if (keep.Count == 0)
			{
				return;
			}

			var open = await one.GetWindows(detailed: false);
			foreach (var window in open)
			{
				var handle = ParseHandle(window.WindowHandle);
				if (!keep.Contains(handle))
				{
					Close(handle);
				}
			}
		}


		private static IntPtr ParseHandle(string hex)
		{
			return (IntPtr)Convert.ToInt64(hex, 16);
		}
	}
}
