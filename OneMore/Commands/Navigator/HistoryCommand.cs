//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;
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

			var success = true;
			try
			{
				await using var one = new OneNote();
				success = await one.NavigateTo(uri);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error navigating to {uri}", exc);
				success = false;
			}

			// reset focus to OneNote window
			await using var onx = new OneNote();
			Native.SwitchToThisWindow(onx.WindowHandle, false);

			if (!success)
			{
				ShowError("Could not navigate at this time. Try again in a few seconds");
			}
		}
	}
}
