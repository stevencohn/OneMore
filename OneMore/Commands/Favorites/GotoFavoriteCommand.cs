//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Favorites;
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class GotoFavoriteCommand : Command
	{
		public GotoFavoriteCommand()
		{
			// do not write to MRU
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			var uri = args == null || args.Length == 0 ? null : (string)args[0];

			if (string.IsNullOrWhiteSpace(uri))
			{
				using var dialog = new FavoritesDialog();
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return;
				}

				if (dialog.Manage)
				{
					await factory.Run<ManageFavoritesCommand>(ribbon);
					return;
				}

				uri = dialog.Uri;
			}

			if (string.IsNullOrWhiteSpace(uri))
			{
				return;
			}

			try
			{
				using var one = new OneNote();
				await one.NavigateTo(uri);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error navigating to {uri}", exc);
			}

			// reset focus to OneNote window
			using var onx = new OneNote();
			Native.SwitchToThisWindow(onx.WindowHandle, false);
		}
	}
}
