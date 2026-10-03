//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands.Layouts;
	using River.OneMoreAddIn.Commands.Workspaces;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Restores a named layout: opens a new window for each layout window whose page isn't
	/// already open, and re-stacks every window belonging to the layout according to its
	/// saved zOrder (lowest zOrder ends up topmost).
	/// </summary>
	internal class RestoreLayoutCommand : Command, ICliInteractiveCommand
	{
		private const int MaxNewWindowWaitAttempts = 10;
		private const int NewWindowPollMilliseconds = 300;
		private const int OpenAttempts = 3;
		private const int WindowSettleMilliseconds = 1500;
		private const int RetryDelayMilliseconds = 1500;


		public RestoreLayoutCommand()
		{
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "RestoreLayout";


		public string Description => "Restore all windows of a named layout";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("name", "Name of the layout to restore", required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			string name = null;

			if (args.Length > 0 && args[0] is CliParameterSet cliParams)
			{
				cliParams.TryGet("name", out name);
			}
			else if (args.Length > 0 && args[0] is string s)
			{
				name = s;
			}

			if (string.IsNullOrWhiteSpace(name) && runningFromCli)
			{
				CliOutput = "The --name argument is required.";
				return;
			}

			using var provider = new LayoutsProvider();
			var collection = provider.ReadLayouts();

			var layout = string.IsNullOrWhiteSpace(name)
				? collection.Layouts.FirstOrDefault()
				: collection.Layouts.FirstOrDefault(l =>
					l.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

			if (layout is null || layout.Windows.Count == 0)
			{
				ShowError(string.Format(Resx.RestoreLayoutCommand_notFound, name));
				return;
			}

			await using var one = new OneNote();

			// OneNote regenerates every page ID when a notebook is reopened, so a window's remembered
			// ID may not exist any more. Find each page as it is now first; both the search for an
			// open window and the link to open then use what was found, not what was remembered
			TargetResolver resolver = null;
			try
			{
				resolver = await WorkspaceResolver.ReadResolver();
			}
			catch (Exception exc)
			{
				logger.WriteLine("could not read the notebooks to restore a layout", exc);
			}

			string Link(string id) => one.GetHyperlink(id, string.Empty);

			// back-to-front order (highest zOrder first) so the stacking pass below can finish on the
			// window that should end up on top
			var plan = LayoutRestorePlan.Plan(
				layout.Windows.OrderByDescending(w => w.ZOrder), resolver, Link);

			var handles = new List<IntPtr>();
			var failed = new List<string>();
			var reused = 0;
			var opened = 0;

			await LogWindows(one, "before restoring");

			foreach (var item in plan)
			{
				if (!item.CanOpen)
				{
					logger.WriteLine(
						$"layout window {item.Window.Location} is {item.Outcome}: {item.Reason}");

					failed.Add(Describe(item.Window, ReasonFor(item.Outcome)));
					continue;
				}

				var handle = await FindWindowHandle(one, item.PageID);
				var justOpened = false;

				if (handle != IntPtr.Zero)
				{
					reused++;
				}
				else
				{
					handle = await OpenWindow(one, item);

					if (handle != IntPtr.Zero)
					{
						opened++;
						justOpened = true;
					}
				}

				if (handle == IntPtr.Zero)
				{
					logger.WriteLine($"layout window {item.Window.Location} did not open");
					failed.Add(Describe(item.Window, Resx.RestoreLayoutCommand_failed));
					continue;
				}

				ApplyBounds(handle, item.Window);
				handles.Add(handle);

				await LogWindows(one, $"after {item.Window.Name}");

				// remember what was found, so the next restore finds the page the same way
				if (item.Resolution is not null && item.Resolution.IsConfident &&
					WorkspaceResolver.Apply(item.Window, item.Resolution, Link))
				{
					provider.UpdateTarget(item.Window, out _);
				}

				// let a window that was just opened finish loading before asking for the next one: a
				// second new-window link that arrives while the first is still loading is refused, and a
				// refused link still leaves a stray window behind, showing the page of the first
				if (justOpened)
				{
					await Task.Delay(WindowSettleMilliseconds);
				}
			}

			// re-stack: bring each window to the top in turn, finishing with the one that
			// has the lowest zOrder so it ends up topmost
			foreach (var handle in handles)
			{
				Native.BringWindowToTop(handle);
				await Task.Delay(50);
			}

			logger.WriteLine(
				$"layout '{layout.Name}': {reused} windows already open, {opened} opened, " +
				$"{failed.Count} not restored, of {plan.Count}");

			if (failed.Count == 0)
			{
				CliOutput = $"Restored layout '{layout.Name}'.";
				return;
			}

			// say which windows were not restored, instead of silently skipping them
			var list = string.Concat(failed.Select(f => $"{Environment.NewLine}- {f}"));
			var message = string.Format(
				Resx.RestoreLayoutCommand_incomplete, handles.Count, plan.Count, list);

			CliOutput = message;

			if (!runningFromCli)
			{
				ShowError(message);
			}
		}


		// Opens a new window for the page, trying again if OneNote refuses the link. Right after a
		// notebook has been reopened OneNote can refuse a link to a page that it opens a moment later,
		// typically the second new window in quick succession, so one refusal is not the end of it.
		private async Task<IntPtr> OpenWindow(OneNote one, RestoreItem item)
		{
			for (var attempt = 1; attempt <= OpenAttempts; attempt++)
			{
				// non-destructive: never close/reuse other windows, only add new ones. A refused
				// link opens nothing, so there is no window to wait for
				if (await one.NavigateTo(item.Uri, newWindow: true))
				{
					var handle = await WaitForWindowHandle(one, item.PageID);
					if (handle != IntPtr.Zero)
					{
						return handle;
					}
				}

				if (attempt < OpenAttempts)
				{
					logger.WriteLine(
						$"layout window {item.Window.Location} did not open, trying again");

					await LogWindows(one, "after a failed attempt");

					await Task.Delay(attempt * RetryDelayMilliseconds);
				}
			}

			return IntPtr.Zero;
		}

		// says which page each open OneNote window is showing, so that a surprising restore can be
		// understood afterwards: an ID that no longer exists shows as such
		private async Task LogWindows(OneNote one, string when)
		{
			try
			{
				var open = await one.GetWindows();
				var shown = new List<string>();

				foreach (var window in open)
				{
					var info = string.IsNullOrEmpty(window.CurrentPageId)
						? null
						: await one.GetPageInfo(window.CurrentPageId);

					shown.Add(info is null ? "(not a current page)" : $"'{info.Name}'");
				}

				logger.Verbose($"open windows {when}: {open.Count}: {string.Join(", ", shown)}");
			}
			catch (Exception exc)
			{
				logger.WriteLine("could not list the open windows", exc);
			}
		}


		private static string Describe(LayoutWindow window, string reason)
		{
			return $"{LayoutRestorePlan.NameOf(window)}: {reason}";
		}


		// the user-facing reason a window could not be opened
		internal static string ReasonFor(ResolveOutcome? outcome)
		{
			return outcome switch
			{
				ResolveOutcome.Pending => Resx.RestoreLayoutCommand_pending,
				ResolveOutcome.Offline => Resx.RestoreLayoutCommand_offline,
				ResolveOutcome.Ambiguous => Resx.RestoreLayoutCommand_ambiguous,
				_ => Resx.RestoreLayoutCommand_broken
			};
		}

		/// <summary>
		/// Moves/resizes the window to its saved bounds, leaving z-order untouched (the
		/// stacking pass handles that separately). If the saved monitor is no longer
		/// connected, clamps the bounds to the primary screen so the window doesn't end up
		/// off-screen.
		/// </summary>
		private static void ApplyBounds(IntPtr handle, LayoutWindow window)
		{
			if (window.WinLeft is null || window.WinTop is null ||
				window.WinRight is null || window.WinBottom is null)
			{
				return;
			}

			var bounds = new Rectangle(
				window.WinLeft.Value, window.WinTop.Value,
				window.WinRight.Value - window.WinLeft.Value,
				window.WinBottom.Value - window.WinTop.Value);

			// GetWindowRect/SetWindowPos/Screen.* must see real physical pixels regardless
			// of this process's ambient DPI awareness, which isn't always reliably
			// per-monitor-aware (e.g. inside the shared dllhost.exe COM surrogate)
			using (new Native.ThreadDpiAwarenessScope(
				Native.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
			{
				if (!string.IsNullOrEmpty(window.Device) &&
					!Screen.AllScreens.Any(s => s.DeviceName == window.Device))
				{
					bounds = ClampToScreen(bounds, Screen.PrimaryScreen.WorkingArea);
				}

				var before = new Native.Rectangle();
				Native.GetWindowRect(handle, ref before);

				// SetWindowPos silently ignores X/Y/cx/cy while the window is maximized or
				// minimized (its restored bounds are tracked separately) - force it to a
				// normal show-state first so the saved position/size actually takes effect.
				// A no-op if the window is already in a normal state.
				var wasVisible = Native.ShowWindow(handle, Native.SW_RESTORE);

				var moved = Native.SetWindowPos(handle, IntPtr.Zero, bounds.Left, bounds.Top,
					bounds.Width, bounds.Height, Native.SWP_NOZORDER | Native.SWP_NOACTIVATE);

				var error = moved ? 0 : Marshal.GetLastWin32Error();

				var after = new Native.Rectangle();
				Native.GetWindowRect(handle, ref after);

				Logger.Current.WriteLine(
					$"ApplyBounds handle={handle.ToInt64():x} " +
					$"target=({bounds.Left},{bounds.Top},{bounds.Width}x{bounds.Height}) " +
					$"before=({before.Left},{before.Top},{before.Right - before.Left}x{before.Bottom - before.Top}) " +
					$"after=({after.Left},{after.Top},{after.Right - after.Left}x{after.Bottom - after.Top}) " +
					$"ShowWindow={wasVisible} SetWindowPos={moved} win32Error={error}");
			}
		}


		/// <summary>
		/// Shrinks and shifts bounds, if needed, so they fit entirely within the given screen.
		/// </summary>
		private static Rectangle ClampToScreen(Rectangle bounds, Rectangle screen)
		{
			var width = Math.Min(bounds.Width, screen.Width);
			var height = Math.Min(bounds.Height, screen.Height);
			var left = Math.Max(screen.Left, Math.Min(bounds.Left, screen.Right - width));
			var top = Math.Max(screen.Top, Math.Min(bounds.Top, screen.Bottom - height));

			return new Rectangle(left, top, width, height);
		}


		private static async Task<IntPtr> FindWindowHandle(OneNote one, string pageID)
		{
			var open = await one.GetWindows();
			var match = open.FirstOrDefault(w => w.CurrentPageId == pageID);
			return match is null ? IntPtr.Zero : ParseHandle(match.WindowHandle);
		}


		private static async Task<IntPtr> WaitForWindowHandle(OneNote one, string pageID)
		{
			for (var attempt = 0; attempt < MaxNewWindowWaitAttempts; attempt++)
			{
				await Task.Delay(NewWindowPollMilliseconds);

				var handle = await FindWindowHandle(one, pageID);
				if (handle != IntPtr.Zero)
				{
					return handle;
				}
			}

			return IntPtr.Zero;
		}


		private static IntPtr ParseHandle(string hex)
		{
			return (IntPtr)Convert.ToInt64(hex, 16);
		}
	}
}
