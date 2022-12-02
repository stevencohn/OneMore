//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Splits the current table starting at the row containing the input cursor.
	/// </summary>
	/// <remarks>
	/// Optionally, the header can be duplicated in the new table and columns in both can be
	/// fixed to their current widths so the two tables remain aligned.
	/// </remarks>
	internal class SplitTableCommand : Command
	{

		public SplitTableCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);

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
				UIHelper.ShowInfo(one.Window, Resx.SplitTableCommand_NoSelection);
				return;
			}

			var row = anchor.FirstAncestor(ns + "Row");
			if (!row.ElementsBeforeSelf(ns + "Row").Any())
			{
				UIHelper.ShowInfo(one.Window, Resx.SplitTableCommand_FirstRow);
				return;
			}

			var copyHeader = false;
			var fixedCols = false;
			using (var dialog = new SplitTableDialog())
			{
				if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				copyHeader = dialog.CopyHeader;
				fixedCols = dialog.FixedColumns;
			}

			// abstract the table into a Table and find the achor row
			var table = new Table(anchor.FirstAncestor(ns + "Table"));

			if (fixedCols)
			{
				table.FixColumns(true);
			}

			// collect the anchor row and all subsequent rows
			var rows = new List<XElement>() { row };

			var following = row.ElementsAfterSelf();
			if (following?.Any() == true)
			{
				rows.AddRange(following);
			}

			// create split table to contain collection of rows
			var split = new Table(table);

			if (copyHeader)
			{
				var header = table.Rows.First();
				split.AddRow(header.Root);
			}

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
