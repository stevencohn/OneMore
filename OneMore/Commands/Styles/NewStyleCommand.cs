//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class NewStyleCommand : Command
	{

		public NewStyleCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			Page page;
			Color pageColor;
			using (var one = new OneNote(out page, out _))
			{
				pageColor = page.GetPageColor(out _, out _);
			}

			var analyzer = new StyleAnalyzer(page.Root);

			var style = analyzer.CollectFromSelection();
			if (style == null)
			{
				return;
			}

			using (var dialog = new StyleDialog(style, pageColor))
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					if (dialog.Style != null)
					{
						ThemeProvider.Save(dialog.Style);
						ribbon.Invalidate();
					}
				}
			}

			await Task.Yield();
		}
	}
}
