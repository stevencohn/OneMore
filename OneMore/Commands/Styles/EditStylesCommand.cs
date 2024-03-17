//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Edit style theme
	/// </summary>
	internal class EditStylesCommand : Command
	{
		public EditStylesCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out _, OneNote.PageDetail.Basic);
			var pageColor = page.GetPageColor(out var automatic, out var black);

			if (automatic)
			{
				pageColor = Color.Transparent;
			}
			else if (black)
			{
				// if Office Black theme, translate to slightly softer shade
				pageColor = BasicColors.BlackSmoke;
			}

			var theme = new ThemeProvider().Theme;

			var dialog = new StyleDialog(theme, pageColor, black);
			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				ThemeProvider.Save(dialog.Theme);
				ThemeProvider.RecordTheme(dialog.Theme.Key);

				ribbon.Invalidate();
			}

			ribbon.Invalidate();

			await Task.Yield();
		}
	}
}
