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
			using var guard = EnterOnce();
			if (guard is null) { return; }

			var provider = new TableThemeProvider();
			var systemThemes = provider.GetSystemThemes(unfiltered: true);
			var userThemes = provider.GetUserThemes();

			using var dialog = new EditTableThemesDialog(systemThemes, userThemes);
			if (dialog.ShowDialog(owner) == DialogResult.OK)
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
