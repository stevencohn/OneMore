//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Opens the table theme editor dialog, providing a way to create and modify
	/// custom table themes
	/// </summary>
	internal class EditTableThemesCommand : Command
	{

		public EditTableThemesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var provider = new TableThemeProvider();
			var themes = provider.GetUserThemes();

			using var dialog = new EditTableThemesDialog(themes);
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				if (dialog.Modified)
				{
					ribbon.Invalidate();
				}
			}

			await Task.Yield();
		}
	}
}
