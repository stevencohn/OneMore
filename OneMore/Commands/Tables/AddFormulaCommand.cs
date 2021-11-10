//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class AddFormulaCommand : Command
	{
		public const string BoltSymbol = "140";

		private OneNote one;


		public AddFormulaCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote(out var page, out var ns))
			{
				if (!page.ConfirmBodyContext())
				{
					UIHelper.ShowInfo(one.Window, Resx.FormulaCommand_SelectOne);
					return;
				}

				// Find first selected cell as anchor point to locate table into which
				// the formula should be inserted; By filtering on selected=all, we avoid
				// including the parent table of a selected nested table.

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
					UIHelper.ShowInfo(one.Window, Resx.FormulaCommand_SelectOne);
					return;
				}

				var table = new Table(anchor.FirstAncestor(ns + "Table"));
				var cells = table.GetSelectedCells(out var range).ToList();

				if (range == TableSelectionRange.Rectangular)
				{
					UIHelper.ShowInfo(one.Window, Resx.FormulaCommand_Linear);
					return;
				}

				using (var dialog = new FormulaDialog())
				{
					// display selected cell names
					dialog.SetCellNames(
						string.Join(", ", cells.Select(c => c.Coordinates))); // + $" ({rangeType})");

					var cell = cells.First();

					// display formula of first cell if any
					var formula = new Formula(cell);
					if (formula.Valid)
					{
						dialog.Format = formula.Format;
						dialog.Formula = formula.Expression;
						dialog.DecimalPlaces = formula.DecimalPlaces;
					}

					var tagIndex = page.GetTagDefIndex(BoltSymbol);
					if (!string.IsNullOrEmpty(tagIndex))
					{
						if (cell.HasTag(tagIndex))
						{
							dialog.Tagged = true;
						}
					}

					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					if (dialog.Tagged)
					{
						tagIndex = page.AddTagDef(BoltSymbol, Resx.AddFormulaCommand_Calculated);
					}

					StoreFormula(cells,
						dialog.Formula, dialog.Format, dialog.DecimalPlaces,
						range, tagIndex);

					var processor = new Processor(table);
					processor.Execute(cells);

					await one.Update(page);
				}
			}
		}


		private void StoreFormula(
			IEnumerable<TableCell> cells,
			string expression, FormulaFormat format, int dplaces,
			TableSelectionRange rangeType, string tagIndex)
		{
			if (rangeType == TableSelectionRange.Single)
			{
				var cell = cells.First();

				cell.SetMeta("omfx", new Formula(format, dplaces, expression).ToString());
				if (!string.IsNullOrEmpty(tagIndex))
				{
					cell.SetTag(tagIndex);
				}

				return;
			}

			var regex = new Regex(@"([a-zA-Z]{1,3})(\d{1,3})");

			int offset = 0;
			foreach (var cell in cells)
			{
				var builder = new StringBuilder(expression);
				if (offset > 0)
				{
					var matches = regex.Matches(expression);
					foreach (Match match in matches)
					{
						string col;
						string row;

						if (rangeType == TableSelectionRange.Columns)
						{
							col = TableCell.IndexToLetters(
								TableCell.LettersToIndex(match.Groups[1].Value) + offset);

							row = match.Groups[2].Value;
						}
						else
						{
							col = match.Groups[1].Value;
							row = (int.Parse(match.Groups[2].Value) + offset).ToString();
						}

						builder.Replace(match.Value, $"{col}{row}", match.Index, match.Length);
					}
				}

				cell.SetMeta("omfx", new Formula(format, dplaces, builder.ToString()).ToString());

				if (!string.IsNullOrEmpty(tagIndex))
				{
					cell.SetTag(tagIndex);
				}

				offset++;
			}
		}
	}
}
