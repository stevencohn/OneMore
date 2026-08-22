//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
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

			XElement element;
			Page page;

			await using (var one = new OneNote(out page, out var ns))
			{
				element = page.Root.Descendants(ns + "Cell")
					// first dive down to find the selected T
					.Elements(ns + "OEChildren")
					.Elements(ns + "OE")
					.Elements(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					// now move back up to the Table
					.Select(e => e.FirstAncestor(ns + "Table"))
					.FirstOrDefault();
			}

			var updated = false;
			var faulted = false;

			if (element != null)
			{
				// one/page are no longer needed here; the page is a POCO so it is safe
				// to carry across the boundary, but a fresh OneNote COM instance is
				// created inside the worker callback since it will run on its own STA
				// thread.

				using var progress = new UI.ProgressDialog(10);
				var result = progress.ShowTimedDialog(async (dialog, token) =>
				{
					try
					{
						updated = RecalculateTable(page, element, dialog);

						if (updated)
						{
							dialog.SetMessage(Resx.FormulaCommand_Saving);

							await using var one = new OneNote();
							await one.Update(page);
						}

						return true;
					}
					catch (Exception exc)
					{
						logger.WriteLine("error recalculating formula", exc);
						return false;
					}
				}, cancelable: false);

				faulted = result != DialogResult.OK;
			}

			if (updated && !faulted)
			{
				logger.WriteTime("calculation completed", true);
			}
			else if (faulted)
			{
				ShowInfo("Error recalculating formula. Please check the OneMore.log file");
			}
			else
			{
				ShowInfo(Resx.RecalculateFormulaCommand_NoFormula);
			}

			logger.WriteTime("recalculated");
		}


		internal static bool RecalculateTable(
			Page page, XElement tableElement, UI.ProgressDialog progress = null)
		{
			var ns = page.Namespace;
			var table = new Table(tableElement);

			var cells = (
				from r in table.Rows
				from c in r.Cells
				where c.Root.Elements(ns + "OEChildren").Elements(ns + "OE").Elements(ns + "Meta").Any()
				select c).ToList();

			if (cells.Count == 0) return false;

			new Processor(table).Execute(cells, progress);
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
