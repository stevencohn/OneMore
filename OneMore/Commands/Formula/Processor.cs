//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Formula
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;


	internal class Processor
	{
		private readonly ILogger logger;
		private readonly Table table;
		private int maxdec;


		public Processor(Table table)
		{
			this.table = table;
			maxdec = 0;

			logger = Logger.Current;
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
					logger.WriteLine($"Cell {cell.Coordinates} is missing its formula");
					continue;
				}

				try
				{
					var result = calculator.Execute(formula.Expression);

					Report(cell, formula, result);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"Error calculating {cell.Coordinates} formula '{formula}'", exc);
					UIHelper.ShowError(exc.Message);
				}
			}
		}


		private void ResolveCellReference(object sender, SymbolEventArgs e)
		{
			var cell = table.GetCell(e.Name.ToUpper());
			if (cell != null)
			{
				var text = cell.GetContent()
					.Replace("$", string.Empty)
					.Replace("%", string.Empty);

				if (double.TryParse(text, out var value))
				{
					maxdec = Math.Max(value.ToString().Length - ((int)value).ToString().Length - 1, maxdec);

					e.Result = value;
					e.Status = SymbolStatus.OK;
				}
				else
					e.Status = SymbolStatus.None;
			}
			else
			{
				e.Status = SymbolStatus.UndefinedSymbol;
			}
		}


		private void Report(TableCell cell, Formula formula, double result)
		{
			int dplaces = formula.Version >= 2 ? formula.DecimalPlaces : maxdec;

			string text = string.Empty;
			switch (formula.Format)
			{
				case FormulaFormat.Currency:
					text = result.ToString($"C{dplaces}");
					break;

				case FormulaFormat.Number:
					text = result.ToString($"N{dplaces}");
					break;

				case FormulaFormat.Percentage:
					text = $"{(result / 100):P}";
					break;
			}

			cell.SetContent(text);

			//logger.WriteLine($"Cell {cell.Coordinates} calculated result = ({text})");
		}
	}
}
