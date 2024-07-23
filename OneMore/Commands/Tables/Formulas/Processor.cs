﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	internal class Processor : Loggable
	{
		/// <summary>
		/// Regex pattern for matching cell addresses of the form [col-letters][row-number] where
		/// row-number is a positive, non-zero integer. Capture groups are named c)ell and r)row.
		/// </summary>
		public const string AddressPattern = @"^(?<c>[a-zA-Z]{1,3})(?<r>\d{1,3})$";

		/// <summary>
		/// Regex pattern for matching cell addresses of the form [col-letters][row-number]
		/// where row-num can be a negative integer specifying offset from last row in table.
		/// Capture groups are named c)ell, o)ffset, and r)row.
		/// </summary>
		public const string OffsetPattern = @"^(?<c>[a-zA-Z]{1,3})(?<o>-)?(?<r>\d{1,3})$";

		private readonly Table table;
		private int maxdec;
		private List<TagDef> tags;


		public Processor(Table table)
		{
			this.table = table;
			maxdec = 0;
		}


		public void Execute(IEnumerable<TableCell> cells)
		{
			var calculator = new Calculator();
			calculator.ProcessSymbol += ResolveCellReference;

			foreach (var cell in cells)
			{
				var formula = new Formula(cell);
				if (!formula.Valid)
				{
					logger.WriteLine($"cell {cell.Coordinates} is missing its formula");
					continue;
				}

				try
				{
					var result = calculator.Execute(formula.Expression, cell.RowNum);

					Report(cell, formula, result);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error calculating {cell.Coordinates} formula '{formula}'", exc);
					UI.MoreMessageBox.ShowError(null, exc.Message);
				}
			}
		}


		private void ResolveCellReference(object sender, SymbolEventArgs e)
		{
			var name = e.Name.ToUpper();
			if (e.Name.IndexOf('-') >= 1)
			{
				// convert relative cell ref (B-1) to absolute (B4) using formula index offset
				var match = Regex.Match(name, Processor.OffsetPattern);
				if (!match.Success)
					throw new FormulaException($"Invalid cell ref {name}", 0);

				if (int.Parse(match.Groups["r"].Value) == 0)
				{
					// do not include result cell (or off-table) in calculations
					throw new FormulaException("row offset cannot be zero");
				}

				var col = match.Groups["c"].Value;
				var row = match.Groups["o"].Success && e.IndexOffset > 0
					? $"{e.IndexOffset - int.Parse(match.Groups["r"].Value)}"
					: match.Groups["r"].Value;

				name = $"{col}{row}";
			}

			//logger.WriteLine($"resolve {e.Name} indexOffset={e.IndexOffset} -> {name}");

			var cell = table.GetCell(name);
			if (cell == null)
			{
				e.Status = SymbolStatus.UndefinedSymbol;
				return;
			}

			var text = cell.GetText().Trim()
				.Replace(AddIn.Culture.NumberFormat.CurrencySymbol, string.Empty)
				.Replace(AddIn.Culture.NumberFormat.PercentSymbol, string.Empty);

			// common case is double
			if (double.TryParse(text, out var dvalue)) // Culture-specific user input?!
			{
				maxdec = Math.Max(dvalue.ToString().Length - ((int)dvalue).ToString().Length - 1, maxdec);

				e.SetResult(dvalue);
				return;
			}

			// has a todo checkbox? If so then the comparison is limited to the checkbox
			// and WILL NOT fall thru to a string comparison!
			var tagx = cell.Root.Descendants().FirstOrDefault(d => d.Name.LocalName == "Tag");
			if (tagx != null)
			{
				var index = tagx.Attribute("index").Value;
				if (index != null)
				{
					tags ??= DiscoverTags();

					var tag = tags.Find(t => t.Index == index);
					if (tag != null)
					{
						if (tag.IsToDo())
						{
							e.SetResult(tagx.Attribute("completed").Value == "true");
							return;
						}
					}
				}
			}

			// can text be interpereted as a boolean?
			if (bool.TryParse(text, out var bvalue))
			{
				e.SetResult(bvalue);
				return;
			}

			// treat it as a string
			e.SetResult(text);
		}


		private List<TagDef> DiscoverTags()
		{
			var pageElement = table.Root.Ancestors().FirstOrDefault(e => e.Name.LocalName == "Page");
			if (pageElement == null)
			{
				return new List<TagDef>();
			}

			var page = new Page(pageElement);
			var map = page.GetTagDefMap();
			var list = map.Where(m => m.TagDef.IsToDo()).Select(m => m.TagDef).ToList();
			return list;
		}


		private void Report(TableCell cell, Formula formula, double result)
		{
			int dplaces = formula.Version >= 2 ? formula.DecimalPlaces : maxdec;

			string text = string.Empty;
			switch (formula.Format)
			{
				case FormulaFormat.Currency:
					text = result.ToString($"C{dplaces}", AddIn.Culture);
					break;

				case FormulaFormat.Number:
					text = result.ToString($"N{dplaces}", AddIn.Culture);
					break;

				case FormulaFormat.Percentage:
					text = (result / 100).ToString($"P{dplaces}", AddIn.Culture);
					break;
			}

			cell.SetContent(text);

			//logger.WriteLine(
			//	$"Cell {cell.Coordinates} calculated {result}, " +
			//	$"formatted \"{text}\" for culture {AddIn.Culture.Name}");
		}
	}
}
