//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Threading.Tasks;


	/// <summary>
	/// Clear the background color or (table cell) shading of the selected text and reset the
	/// text color to add contrast with page background.
	/// </summary>
	internal class ClearBackgroundCommand : Command
	{

		public ClearBackgroundCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var darkPage = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
				var updated = false;

				var analyzer = new StyleAnalyzer(page.Root, true);

				var runs = page.GetSelectedElements(all: true);

				foreach (var run in runs)
				{
					analyzer.Clear();
					var style = new Style(analyzer.CollectStyleProperties(run));

					if (!string.IsNullOrEmpty(style.Highlight))
					{
						style.Highlight = Style.Automatic;
					}

					var darkText = !style.Color.Equals(Style.Automatic) 
						&& ColorTranslator.FromHtml(style.Color).GetBrightness() < 0.5;

					if (darkText && darkPage)
					{
						style.Color = "#FFFFFF";
					}
					else if (!darkText && !darkPage)
					{
						style.Color = "#000000";
					}

					var stylizer = new Stylizer(style);
					stylizer.ApplyStyle(run);
					updated = true;
				}

				if (updated)
				{
					await one.Update(page);
				}
			}
		}
	}
}