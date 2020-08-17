//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;

	internal class HighlightFormulaCommand : Command
	{

		public HighlightFormulaCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());
				var ns = page.Namespace;

				// only highlight formula cells from current table

				var tables = page.Root.Descendants(ns + "Table")?
					.Select(o => new { table = o, selected = o.Attribute("selected")?.Value })
					.Where(o => o.selected == "all" || o.selected == "partial")
					.Select(o => o.table);

				if (tables?.Count() > 0)
				{
					foreach (var table in tables)
					{
						var cells = table.Descendants(ns + "Cell")
							.Where(e => e.Attribute("selected") != null);

						foreach (var cell in cells)
						{
							cell.DescendantNodes().OfType<XAttribute>()
								.Where(a => a.Name == "selected")
								.Remove();

							var attr = cell.Attribute("selected");
							if (attr != null)
							{
								attr.Remove();
							}
						}

						var hicells = table.Descendants(ns + "Meta")
							.Where(e => e.Attribute("name").Value == "omfx")
							.Select(e => e.Parent.Parent.Parent);

						foreach (var cell in hicells)
						{
							cell.SetAttributeValue("selected", "all"); // Cell
						}
					}

					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
