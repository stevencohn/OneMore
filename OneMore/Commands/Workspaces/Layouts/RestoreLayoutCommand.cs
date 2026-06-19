//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands.Layouts;
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

			if (string.IsNullOrWhiteSpace(name))
			{
				CliOutput = "The --name argument is required.";
				return;
			}

			using var provider = new LayoutsProvider();
			var collection = provider.ReadLayouts();

			var layout = collection.Layouts.FirstOrDefault(l =>
				l.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

			if (layout is null || layout.Windows.Count == 0)
			{
				ShowError(string.Format(Resx.RestoreLayoutCommand_notFound, name));
				return;
			}

			await using var one = new OneNote();

			// locate or open a window for every page in the layout, gathering handles in
			// back-to-front order (highest zOrder first) so the stacking pass below can
			// finish on the window that should end up on top
			var ordered = layout.Windows.OrderByDescending(w => w.ZOrder).ToList();
			var handles = new List<IntPtr>();

			foreach (var window in ordered)
			{
				var handle = await FindWindowHandle(one, window.PageID);

				if (handle == IntPtr.Zero)
				{
					// non-destructive: never close/reuse other windows, only add new ones
					await one.NavigateTo(window.Uri, newWindow: true);
					handle = await WaitForWindowHandle(one, window.PageID);
				}

				if (handle != IntPtr.Zero)
				{
					ApplyBounds(handle, window);
					handles.Add(handle);
				}
			}

			// re-stack: bring each window to the top in turn, finishing with the one that
			// has the lowest zOrder so it ends up topmost
			foreach (var handle in handles)
			{
				Native.BringWindowToTop(handle);
				await Task.Delay(50);
			}

			CliOutput = $"Restored layout '{name}'.";
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
