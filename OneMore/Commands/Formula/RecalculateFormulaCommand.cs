//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands.Formula;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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

				var element = page.Root.Descendants(ns + "Cell")
					// first dive down to find the selected T
					.Elements(ns + "OEChildren")
					.Elements(ns + "OE")
					.Elements(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					// now move back up to the Table
					.Select(e => e.FirstAncestor(ns + "Table"))
					.FirstOrDefault();

				if (element != null)
				{
					var table = new Table(element);

					var cells =
						from r in table.Rows
						from c in r.Cells
						where c.Root.Elements(ns + "OEChildren").Elements(ns + "OE").Elements(ns + "Meta").Any()
						select c;

					if (cells?.Any() == true)
					{
						var processor = new Processor(table);
						processor.Execute(cells.ToList());

						manager.UpdatePageContent(page.Root);

						return;
					}
				}

				UIHelper.ShowMessage(Resx.RecalculateFormulaCommand_NoFormula);
			}
		}
	}
}
