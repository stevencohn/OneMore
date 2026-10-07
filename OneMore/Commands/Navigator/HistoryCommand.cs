//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using HierarchyInfo = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	/// <summary>
	/// Presents a filterable, keyboard-navigable picker of recently visited pages, or
	/// navigates directly to a page when invoked from the ribbon history dropdown.
	/// </summary>
	internal class HistoryCommand : Command
	{
		public HistoryCommand()
		{
			// do not write to MRU
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			var settings = new SettingsProvider().GetCollection(nameof(NavigatorSheet));
			if (settings.Get("disabled", false))
			{
				ShowInfo(Resx.NavigatorWindow_disabled);
				return;
			}

			var uri = args == null || args.Length == 0 ? null : (string)args[0];

			if (string.IsNullOrWhiteSpace(uri))
			{
				using var dialog = new HistoryDialog();
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return;
				}

				uri = dialog.Uri;
			}

			if (string.IsNullOrWhiteSpace(uri))
			{
				return;
			}

			// the ribbon dropdown only carries the link, so find the record it came from
			HierarchyInfo record = null;
			try
			{
				using var provider = new NavigationProvider();
				var log = await provider.ReadHistoryLog();
				record = log.History.Find(r => r.Link == uri);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading history for {uri}", exc);
			}

			// an unknown link can still be opened, and found by the GUID inside it if it is dead
			record ??= new HierarchyInfo { Link = uri, Path = uri };

			_ = await NavigatorLauncher.OpenAndReport(owner, record);

			// reset focus to OneNote window
			await using var onx = new OneNote();
			Native.SwitchToThisWindow(onx.WindowHandle, false);
		}
	}
}
