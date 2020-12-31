//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class EditStylesCommand : Command
	{
		public EditStylesCommand()
		{
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

			var provider = new StyleProvider();
			var styles = provider.GetStyles();

			using (var dialog = new StyleDialog(styles, pageColor))
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					// save styles to remove deleted items and preserve ordering
					styles = dialog.GetStyles();
					StyleProvider.Save(styles);
					ribbon.Invalidate();
				}
			}

			ribbon.Invalidate();

			await Task.Yield();
		}
	}
}
