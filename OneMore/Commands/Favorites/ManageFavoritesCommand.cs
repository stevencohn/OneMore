//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Favorites;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	internal class ManageFavoritesCommand : Command
	{
		public ManageFavoritesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new ManageFavoritesDialog();
			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				ribbon?.InvalidateControl(FavoritesMenu.MenuID);
			}

			await Task.Yield();
		}
	}
}