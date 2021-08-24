//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class EditStylesCommand : Command
	{
		public EditStylesCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			Color pageColor;
			using (var one = new OneNote(out var page, out _))
			{
				pageColor = page.GetPageColor(out _, out var black);
				if (black)
				{
					pageColor = ColorTranslator.FromHtml("#201F1E");
				}
			}

			var theme = new ThemeProvider().Theme;

			using (var dialog = new StyleDialog(theme, pageColor))
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					// save styles to remove deleted items and preserve ordering
					var styles = dialog.GetStyles();

					ThemeProvider.Save(styles, theme.Key);
					ThemeProvider.RecordTheme(theme.Key);

					ribbon.Invalidate();
				}
			}

			ribbon.Invalidate();

			await Task.Yield();
		}
	}
}
