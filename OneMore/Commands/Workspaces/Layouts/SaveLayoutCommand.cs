//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands.Layouts;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Saves all currently open OneNote windows as a named layout, creating it if the name
	/// is new or overwriting its windows if the name already exists.
	/// </summary>
	internal class SaveLayoutCommand : Command, ICliInteractiveCommand
	{
		public SaveLayoutCommand()
		{
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "SaveLayout";


		public string Description => "Save all open OneNote windows as a named layout";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("name", "Name of the layout to save", required: true);

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

			using var provider = new LayoutsProvider();
			var existing = provider.ReadLayouts();

			if (string.IsNullOrWhiteSpace(name))
			{
				// interactive only; the CLI host guarantees --name is already populated
				using var dialog = new RenameDialog(Enumerable.Empty<string>(), string.Empty,
					createTitle: Resx.SaveLayoutCommand_Text,
					label: Resx.word_Name);

				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				name = dialog.Value;
			}

			await using var one = new OneNote();
			var captured = await CaptureWindows(one);

			if (captured.Count == 0)
			{
				ShowError(Resx.SaveLayoutCommand_noWindows);
				return;
			}

			var layout = existing.Layouts.FirstOrDefault(l =>
				l.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

			var layoutID = layout?.LayoutID ?? provider.CreateLayout(name);
			if (layoutID == 0)
			{
				ShowError(Resx.SaveLayoutCommand_error);
				return;
			}

			// overwrite: replace this layout's existing windows with the current snapshot
			if (layout is not null)
			{
				foreach (var window in layout.Windows)
				{
					provider.DeleteWindow(window.ID);
				}
			}

			var saved = 0;
			foreach (var window in captured)
			{
				window.LayoutID = layoutID;
				if (provider.WriteWindow(window))
				{
					saved++;
				}
			}

			CliOutput = $"Saved layout '{name}' with {saved} window(s).";
		}


		/// <summary>
		/// Captures every currently open OneNote window as a LayoutWindow, ordered by their
		/// actual on-screen Z-order (ZOrder 0 is the topmost). LayoutID is left at 0 - the
		/// caller assigns it once the target layout (new or existing) is known. Windows that
		/// aren't showing a resolvable page are skipped.
		/// </summary>
		internal static async Task<List<LayoutWindow>> CaptureWindows(OneNote one)
		{
			var windows = await one.GetWindows();
			var ordered = OrderByZOrder(windows);
			var captured = new List<LayoutWindow>();

			for (var i = 0; i < ordered.Count; i++)
			{
				var current = ordered[i];

				var info = string.IsNullOrEmpty(current.CurrentPageId)
					? null
					: await one.GetPageInfo(current.CurrentPageId);

				if (info is null)
				{
					// window isn't showing a resolvable page; skip it
					continue;
				}

				captured.Add(new LayoutWindow
				{
					Name = info.Name,
					Location = info.Path,
					Uri = info.Link,
					NotebookID = current.CurrentNotebookId,
					SectionID = current.CurrentSectionId,
					PageID = current.CurrentPageId,
					ZOrder = i,
					Device = GetDeviceName(current.WindowHandle),
					WinLeft = current.Bounds.Left,
					WinTop = current.Bounds.Top,
					WinRight = current.Bounds.Right,
					WinBottom = current.Bounds.Bottom
				});
			}

			return captured;
		}


		/// <summary>
		/// Returns the device name (e.g. "\\.\DISPLAY1") of the monitor showing the window
		/// with the given handle, or null if it can't be determined.
		/// </summary>
		private static string GetDeviceName(string windowHandle)
		{
			// Screen.FromHandle must see real physical pixels regardless of this process's
			// ambient DPI awareness, which isn't always reliably per-monitor-aware
			using var dpiScope = new Native.ThreadDpiAwarenessScope(
				Native.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

			try
			{
				var handle = (IntPtr)Convert.ToInt64(windowHandle, 16);
				return Screen.FromHandle(handle).DeviceName;
			}
			catch
			{
				return null;
			}
		}


		/// <summary>
		/// Orders the given windows front-to-back by their actual on-screen Z-order (index 0
		/// is the topmost window). OneNote's COM Windows collection has no Z-order of its own,
		/// so this cross-references each window's handle against EnumWindows, which walks
		/// top-level windows in Z-order.
		/// </summary>
		private static List<WindowInfo> OrderByZOrder(List<WindowInfo> windows)
		{
			var ranks = new Dictionary<string, int>();
			var rank = 0;

			Native.EnumWindows((hwnd, _) =>
			{
				var handle = $"{hwnd.ToInt64():x}";
				if (!ranks.ContainsKey(handle))
				{
					ranks[handle] = rank++;
				}

				return true;
			},
			IntPtr.Zero);

			return windows
				.OrderBy(w => ranks.TryGetValue(w.WindowHandle, out var r) ? r : int.MaxValue)
				.ToList();
		}
	}
}
