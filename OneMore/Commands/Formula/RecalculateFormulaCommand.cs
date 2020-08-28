//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Linq;


	internal class RecalculateFormulaCommand : Command
	{

		public RecalculateFormulaCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());
				var ns = page.Namespace;

				// TODO: only target current/selected table(s)?

				var cells = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == "omfx")
					.Select(e => new { Element = e.Parent.Parent.Parent, formula = e.Attribute("content").Value });

				if (cells?.Count() > 0)
				{
					foreach (var cell in cells.ToList())
					{
						var calculator = new Calculator(ns, cell.formula);

						var values = calculator.CollectValues(cell.Element);
						var result = calculator.Calculate(values);
						calculator.ReportResult(cell.Element, result, values);
					}

					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
