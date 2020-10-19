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

				var element = page.Root.Descendants(ns + "Table")
					.Where(e => e.Attributes("selected").Any())
					.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Ancestors(ns + "Table")
					.FirstOrDefault();

				if (element != null)
				{
					var table = new Table(element);

					var cells = table.Root.Descendants(ns + "Meta")
						.Where(e => e.Attribute("name").Value == "omfx")
						.Select(e => new TableCell(e.Parent.Parent.Parent))
						.ToList();

					var processor = new Processor(table);
					processor.Execute(cells);

					manager.UpdatePageContent(page.Root);
				}
				else
				{
					UIHelper.ShowMessage(Resx.RecalculateFormulaCommand_NoFormula);
				}
			}
		}
	}
}
