//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.Settings;
	using System.Threading.Tasks;


	/// <summary>
	/// Toggles between names and full hierarchical paths in the Favorites ribbon menu,
	/// then immediately rebuilds the menu.
	/// </summary>
	internal class FavoritesPathsCommand : Command
	{
		public FavoritesPathsCommand()
		{
			// do not write to MRU
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(nameof(FavoritesSheet));

			// flip the stored value; the ribbon's own toggle state is not refreshed when
			// a dynamic menu is rebuilt so it cannot be trusted
			if (settings.Get(FavoritesMenu.FullPathsSetting, false))
			{
				settings.Remove(FavoritesMenu.FullPathsSetting);
			}
			else
			{
				settings.Add(FavoritesMenu.FullPathsSetting, true);
			}

			provider.SetCollection(settings);
			provider.Save();

			ribbon.SafeInvalidateControl(FavoritesMenu.MenuID);

			await Task.Yield();
		}
	}
}
