//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Recalculates all formulas in the selected table(s).
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

			if (element != null && RecalculateTable(page, element))
			{
				logger.WriteTime("calculation completed", true);
				await one.Update(page);
			}
			else
			{
				ShowInfo(Resx.RecalculateFormulaCommand_NoFormula);
			}

			logger.WriteTime("recalculated");
		}


		internal static bool RecalculateTable(Page page, XElement tableElement)
		{
			var ns = page.Namespace;
			var table = new Table(tableElement);

			var cells = (
				from r in table.Rows
				from c in r.Cells
				where c.Root.Elements(ns + "OEChildren").Elements(ns + "OE").Elements(ns + "Meta").Any()
				select c).ToList();

			if (cells.Count == 0) return false;

			new Processor(table).Execute(cells);
			return true;
		}
	}


	/// <summary>
	/// CLI command to recalculate all formula table cells across a notebook, section, or page.
	/// </summary>
	internal class RecalculateCommand : Command, ICliPageCommand
	{
		public RecalculateCommand()
		{
		}


		public string CommandName => "Recalculate";

		public string Description => "Recalculate formula table cells on one or more pages";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false);


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams == null) { return; }

			cliParams.TryGet("pageId", out string pageId);
			if (string.IsNullOrWhiteSpace(pageId)) { return; }

			await using var one = new OneNote();
			var page = await one.GetPage(pageId, OneNote.PageDetail.All);
			var ns = page.Namespace;

			var updated = page.Root.Descendants(ns + "Table").ToList()
				.Aggregate(false, (any, t) => RecalculateFormulaCommand.RecalculateTable(page, t) || any);

			if (updated)
			{
				logger.Verbose($"formulas recalculated on page: {page.Title}");
				await one.Update(page);
			}
		}
	}
}
