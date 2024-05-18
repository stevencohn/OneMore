﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Recalculates all formulas in the selected table(s)
	/// </summary>
	internal class RecalculateFormulaCommand : Command
	{

		public RecalculateFormulaCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			logger.StartClock();

			await using var one = new OneNote(out var page, out var ns);

			var element = page.Root.Descendants(ns + "Cell")
				// first dive down to find the selected T
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE")
				.Elements(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				// now move back up to the Table
				.Select(e => e.FirstAncestor(ns + "Table"))
				.FirstOrDefault();

			var updated = false;

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
					logger.WriteTime("calculation completed", true);

					await one.Update(page);
					updated = true;
				}
			}

			if (!updated)
			{
				ShowInfo(Resx.RecalculateFormulaCommand_NoFormula);
			}

			logger.WriteTime("recalcution");
		}
	}
}
