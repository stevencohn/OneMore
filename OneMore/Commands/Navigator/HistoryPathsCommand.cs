//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System.Threading.Tasks;


	/// <summary>
	/// Toggles between page names and full hierarchical paths in the Navigator history
	/// dropdown, then immediately rebuilds the dropdown.
	/// </summary>
	internal class HistoryPathsCommand : Command
	{
		public HistoryPathsCommand()
		{
			// do not write to MRU
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(nameof(NavigatorSheet));

			// flip the stored value; the ribbon's own toggle state is not refreshed when
			// a dynamic menu is rebuilt so it cannot be trusted
			if (settings.Get(HistoryMenu.FullPathsSetting, false))
			{
				settings.Remove(HistoryMenu.FullPathsSetting);
			}
			else
			{
				settings.Add(HistoryMenu.FullPathsSetting, true);
			}

			provider.SetCollection(settings);
			provider.Save();

			ribbon.SafeInvalidateControl("ribNavigatorButton");

			await Task.Yield();
		}
	}
}
