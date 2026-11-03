//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Corrects the cell references of formulas in the selected table after rows or columns
	/// have been inserted or deleted, then recalculates the table.
	/// </summary>
	internal class FixFormulasCommand : Command
	{
		public FixFormulasCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			try
			{
				await using var one = new OneNote(out var page, out var ns);

				var element = page.Root.Descendants(ns + "Cell")
					.Elements(ns + "OEChildren").Elements(ns + "OE").Elements(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.FirstAncestor(ns + "Table"))
					.FirstOrDefault();

				if (element is null)
				{
					ShowInfo(Resx.FormulaCommand_SelectOne);
					return;
				}

				var table = new Table(element);
				var fixes = FormulaFixer.Fix(table, out var review);

				// recalculate, with fixing already done above
				var updated = RecalculateFormulaCommand.RecalculateTable(page, element);
				if (!updated)
				{
					ShowInfo(Resx.RecalculateFormulaCommand_NoFormula);
					return;
				}

				await one.Update(page);

				ShowInfo(string.Format(Resx.FixFormulasCommand_Result, fixes, review));
			}
			catch (Exception exc)
			{
				logger.WriteLine("error fixing formulas", exc);
				ShowInfo("Error fixing formulas. Please check the OneMore.log file");
			}
		}
	}
}
