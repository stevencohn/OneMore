//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands.Formula;
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

				var tables = page.Root.Descendants(ns + "Table")?
					.Select(o => new { table = o, selected = o.Attribute("selected")?.Value })
					.Where(o => o.selected == "all" || o.selected == "partial")
					.Select(o => o.table);

				if (tables?.Any() == true)
				{
					foreach (var element in tables)
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
				}
			}
		}
	}
}
