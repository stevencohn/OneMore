//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Workspaces;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	internal class ManageFavoritesCommand : Command
	{
		public ManageFavoritesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new ManageWorkspaceDialog { ActiveTab = WorkspaceTab.Favorites };
			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				ribbon?.InvalidateControl(FavoritesMenu.MenuID);
			}

			await Task.Yield();
		}
	}
}