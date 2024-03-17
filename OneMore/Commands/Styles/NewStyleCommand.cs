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
	/// Create a new style based on the style of the selected content
	/// </summary>
	internal class NewStyleCommand : Command
	{

		public NewStyleCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out _);
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

			var analyzer = new StyleAnalyzer(page.Root);

			var style = analyzer.CollectFromSelection();
			if (style == null)
			{
				return;
			}

			using var dialog = new StyleDialog(style, pageColor, black);
			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				if (dialog.Style != null)
				{
					ThemeProvider.Save(dialog.Style);
					ribbon.Invalidate();
				}
			}

			await Task.Yield();
		}
	}
}
