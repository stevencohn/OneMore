//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
	using System.Xml.Linq;


	internal class RecalculateFormulaCommand : Command
	{
		private ApplicationManager manager;
		private Page page;
		private XNamespace ns;

		private FormulaDirection direction;
		private FormulaFormat format;
		private FormulaFunction function;


		public RecalculateFormulaCommand() : base()
		{
		}


		public void Execute()
		{
			using (manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage());
				ns = page.Namespace;

				// TODO: only target current/selected table(s)?

				var cells = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == "omfx")
					.Select(e => new { Element = e.Parent.Parent.Parent, formula = e.Attribute("content").Value });

				if (cells?.Count() > 0)
				{
					foreach (var cell in cells.ToList())
					{
						ParseFormula(cell.formula);

						var calculator = new Calculator(ns, direction, function, format);

						var values = calculator.CollectValues(cell.Element);
						var result = calculator.Calculate(values);
						calculator.ReportResult(cell.Element, result);
					}

					manager.UpdatePageContent(page.Root);
				}
			}
		}


		private void ParseFormula(string formula)
		{
			var parts = formula.Split(';');

			// version
			if (parts[0] == "0")
			{
				//content="0;Horizontal;Number;Sum"
				direction = (FormulaDirection)Enum.Parse(typeof(FormulaDirection), parts[1]);
				format = (FormulaFormat)Enum.Parse(typeof(FormulaFormat), parts[2]);
				function = (FormulaFunction)Enum.Parse(typeof(FormulaFunction), parts[3]);
			}
		}
	}
}
