//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using River.OneMoreAddIn.Commands.Favorites;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	/// <summary>
	/// Shows ManageWorkspaceDialog, focused on the tab given as the first argument (a
	/// WorkspaceTab; defaults to Favorites if omitted).
	/// </summary>
	internal class ManageWorkspaceCommand : Command
	{
		public ManageWorkspaceCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var tab = args.Length > 0 && args[0] is WorkspaceTab requested
				? requested
				: WorkspaceTab.Favorites;

			string restoreLayoutName;

			using (var dialog = new ManageWorkspaceDialog { ActiveTab = tab })
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					ribbon?.InvalidateControl(FavoritesMenu.MenuID);
				}

				restoreLayoutName = dialog.RestoreLayoutName;
			}

			// only safe to do once the dialog (and its modal hold on OneNote's window) is gone
			if (restoreLayoutName is not null)
			{
				var command = new RestoreLayoutCommand();
				command.SetLogger(logger);
				command.SetOwner(owner);
				await command.Execute(restoreLayoutName);
			}
		}
	}
}
