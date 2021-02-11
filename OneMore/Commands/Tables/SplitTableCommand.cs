//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class SplitTableCommand : Command
	{

		public SplitTableCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				// Find first selected cell as anchor point to locate table; by filtering on
				// selected=all, we avoid including the parent table of a selected nested table.

				var anchor = page.Root.Descendants(ns + "Cell")
					// first dive down to find the selected T
					.Elements(ns + "OEChildren").Elements(ns + "OE")
					.Elements(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					// now move back up to the Cell
					.Select(e => e.Parent.Parent.Parent)
					.FirstOrDefault();

				if (anchor == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.InsertCellsCommand_NoSelection);
					return;
				}

				// abstract the table into a Table and find the achor row
				var table = new Table(anchor.FirstAncestor(ns + "Table"));
				var row = anchor.FirstAncestor(ns + "Row");

				// collect the anchor row and all subsequent rows
				var rows = new List<XElement>() { row };

				var following = row.ElementsAfterSelf();
				if (following?.Any() == true)
				{
					rows.AddRange(following);
				}

				// create split table to contain collection of rows
				var split = new Table(table);

				// remove rows from origin table and add to split table
				foreach (var r in rows)
				{
					r.Remove();
					split.AddRow(r);
				}

				table.Root.Parent.AddAfterSelf(
					new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))),
					new XElement(ns + "OE", split.Root)
					);

				await one.Update(page);
			}
		}
	}
}
