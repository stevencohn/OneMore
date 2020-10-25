//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Drawing;
	using System.Windows.Forms;


	internal class EditStylesCommand : Command
	{
		public EditStylesCommand()
		{
		}


		public override void Execute(params object[] args)
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
			DialogResult result;

			using (var dialog = new StyleDialog(styles, pageColor))
			{
				result = dialog.ShowDialog(owner);

				if (result == DialogResult.OK)
				{
					// save styles to remove delete items and preserve ordering
					styles = dialog.GetStyles();
					StyleProvider.Save(styles);
					ribbon.Invalidate();
				}
			}

			ribbon.Invalidate();
		}
	}
}
