//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System.Drawing;
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

			// the page background determines whether OneNote renders colors in dark mode
			await using var one = new OneNote(out var page, out _, OneNote.PageDetail.Basic);
			var pageColor = Color.Transparent;
			if (page is not null)
			{
				pageColor = page.GetPageColor(out var automatic, out var black);
				if (automatic)
				{
					pageColor = black ? BasicColors.BlackSmoke : Color.White;
				}
			}

			using var dialog = new EditTableThemesDialog(systemThemes, userThemes, pageColor);
			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				if (dialog.Modified)
				{
					ribbon.SafeInvalidate();
				}
			}

			await Task.Yield();
		}
	}
}
