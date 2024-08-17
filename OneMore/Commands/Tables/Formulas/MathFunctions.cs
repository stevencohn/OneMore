//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S125 // Sections of code should not be commented out

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;
	using System.Collections.Generic;
	using System.Linq;


	internal delegate double MathFuncDelegate(FormulaValues args);


	internal class MathFunction
	{
		public string Name { get; private set; }

		public bool Spacial { get; private set; }

		public MathFuncDelegate Fn { get; private set; }

		public MathFunction(string name, MathFuncDelegate fn, bool spacial = false)
		{
			Name = name;
			Spacial = spacial;
			Fn = fn;
		}
	}


	internal static class MathFunctions
	{
		private static readonly List<MathFunction> functions = new();
		private static readonly FormulaValueType D = FormulaValueType.Double;

		static MathFunctions()
		{
			functions.Add(new MathFunction("abs", (p) => Math.Abs(p.Match(D)[0])));
			functions.Add(new MathFunction("acos", (p) => Math.Acos(p.Match(D)[0])));
			functions.Add(new MathFunction("asin", (p) => Math.Asin(p.Match(D)[0])));
			functions.Add(new MathFunction("atan", (p) => Math.Atan(p.Match(D)[0])));
			functions.Add(new MathFunction("atan2", (p) => Math.Atan2(p.Match(D, D)[0], p[1])));
			functions.Add(new MathFunction("average", (p) => Average(p.Match(D, D).ToDoubleArray())));
			functions.Add(new MathFunction("ceiling", (p) => Math.Ceiling(p.Match(D, D)[0])));
			functions.Add(new MathFunction("cell", (p) => Cell(p), true));
			functions.Add(new MathFunction("cos", (p) => Math.Cos(p.Match(D)[0])));
			functions.Add(new MathFunction("cosh", (p) => Math.Cosh(p.Match(D)[0])));
			functions.Add(new MathFunction("countif", (p) => CountIf(p)));
			functions.Add(new MathFunction("exp", (p) => Math.Exp(p.Match(D)[0])));
			functions.Add(new MathFunction("floor", (p) => Math.Floor(p.Match(D)[0])));
			functions.Add(new MathFunction("log", (p) => Math.Log(p.Match(D)[0])));
			functions.Add(new MathFunction("log10", (p) => Math.Log10(p.Match(D)[0])));
			functions.Add(new MathFunction("max", (p) => Max(p.Match(D).ToDoubleArray())));
			functions.Add(new MathFunction("median", (p) => Median(p.Match(D).ToDoubleArray())));
			functions.Add(new MathFunction("min", (p) => Min(p.Match(D).ToDoubleArray())));
			functions.Add(new MathFunction("mode", (p) => Mode(p.Match(D).ToDoubleArray())));
			functions.Add(new MathFunction("pow", (p) => Math.Pow(p.Match(D, D)[0], p[1])));
			functions.Add(new MathFunction("range", (p) => Range(p.Match(D).ToDoubleArray())));
			functions.Add(new MathFunction("round", (p) => Math.Round(p.Match(D)[0])));
			functions.Add(new MathFunction("sign", (p) => Math.Sign(p.Match(D)[0])));
			functions.Add(new MathFunction("sin", (p) => Math.Sin(p.Match(D)[0])));
			functions.Add(new MathFunction("sinh", (p) => Math.Sinh(p.Match(D)[0])));
			functions.Add(new MathFunction("sqrt", (p) => Math.Sqrt(p.Match(D)[0])));
			functions.Add(new MathFunction("stdev", (p) => StandardDeviation(p.Match(D).ToDoubleArray())));
			functions.Add(new MathFunction("sum", (p) => Sum(p.Match(D).ToDoubleArray())));
			functions.Add(new MathFunction("tablecols", (p) => TableCols(p), true));
			functions.Add(new MathFunction("tablerows", (p) => TableRows(p), true));
			functions.Add(new MathFunction("tan", (p) => Math.Tan(p.Match(D)[0])));
			functions.Add(new MathFunction("tanh", (p) => Math.Tanh(p.Match(D)[0])));
			functions.Add(new MathFunction("trunc", (p) => Math.Truncate(p.Match(D)[0])));
			functions.Add(new MathFunction("variance", (p) => Variance(p.Match(D).ToDoubleArray())));
		}


		public static MathFunction Find(string name)
		{
			return functions.Find(f => f.Name == name);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private static double Average(double[] p)
		{
			if (p.Length == 0)
				return 0.0;

			return p.AsEnumerable().Average();
		}


		private static double Cell(FormulaValues p)
		{
			// user may specify one or two parameters (colOffset) or (colOffset,Rowoffset)
			// but Calculator will transform it to (colOffset, rowOffset, table, col, row)
			if (p.Count < 4 || p.Count > 5)
				throw new FormulaException("cell function requires one or two parameters");

			var array = p.ToArray();

			// if we don't have a table then do not fail; instead presume this is being
			// called as a mock run from FormulaDialog
			if (array[2].Type != FormulaValueType.Table || array[2].Value is not Models.Table table)
			{
				return 0.0;
			}

			if (array[0].Type == FormulaValueType.Double && array[0].Value is double colOffset &&
				array[1].Type == FormulaValueType.Double && array[1].Value is double rowOffset &&
				array[3].Type == FormulaValueType.Double && array[3].Value is double col &&
				array[4].Type == FormulaValueType.Double && array[4].Value is double row)
			{
				// row and col are 1-based but we need 0-based indexes
				var r = (int)row - 1 + (int)rowOffset;
				var c = (int)col - 1 + (int)colOffset;

				if (c < 0 || c > table.ColumnCount - 1 ||
					r < 0 || r > table.RowCount - 1)
				{
					throw new FormulaException(
						"cell function col/row indexes are out of range\n" +
						$"cell(coff:{colOffset}, roff:{rowOffset}, col:{col}, row:{row}) -> " +
						$"c:{c} r:{r}");
				}

				var cell = table[r][c];
				if (cell is not null && double.TryParse(cell.GetText(), out var value))
				{
					Logger.Current.Verbose(
						$"fn cell(coff:{colOffset}, roff:{rowOffset}, Col:{col}, row:{row}) " +
						$"= {(double)cell.Value}");

					return value;
				}

				// assumption
				return 0.0;
			}

			throw new FormulaException("cell function has invalid arguments");
		}


		private static double CountIf(FormulaValues p)
		{
			if (p.Count < 2)
				throw new FormulaException($"countif function requires at least two parameters");

			var array = p.ToArray();

			// values are items 0..last-1, ignore empty cells
			var values = array.Take(p.Count - 1)
				.Where(p => p.Type != FormulaValueType.String || ((string)p.Value).Length > 0);

			// the countif testcase is always the last parameter
			var test = array[array.Length - 1];

			var oper = test.ToString()[0];
			var result = 0;
			string s;
			if (oper == '<' || oper == '>' || oper == '!')
			{
				s = test.ToString().Substring(1);
				if (oper == '>') result = 1;
				else if (oper == '<') result = -1;
			}
			else
			{
				s = test.ToString();
			}

			FormulaValue expected;
			if (double.TryParse(s, out var d)) // Culture-specific user input?!
			{
				expected = new FormulaValue(d);
			}
			else if (bool.TryParse(s, out bool b))
			{
				expected = new FormulaValue(b);
			}
			else
			{
				expected = new FormulaValue(s);
			}

			return oper == '!'
				? values.Count(v => v.CompareTo(expected) != 0)
				: values.Count(v => v.CompareTo(expected) == result);
		}


		private static double Max(double[] p)
		{
			if (p.Length == 0)
				return 0.0;

			return p.AsEnumerable().Max();
		}


		private static double Median(double[] p)
		{
			if (p.Length == 0)
				return 0.0;

			int count = p.Count();
			if (count % 2 == 0)
			{
				return p.OrderBy(n => n).Skip((count / 2) - 1).Take(2).Average();
			}

			return p.OrderBy(n => n).ElementAt(count / 2);
		}


		private static double Min(double[] p)
		{
			if (p.Length == 0)
				return 0.0;

			return p.AsEnumerable().Min();
		}


		private static double Mode(double[] values)
		{
			return values
				.GroupBy(n => n)
				.OrderByDescending(g => g.Count())
				.Select(g => g.Key).FirstOrDefault();
		}


		private static double Range(double[] p)
		{
			if (p.Length == 0)
				return 0.0;

			return p.AsEnumerable().Max() - p.AsEnumerable().Min();
		}


		private static double Sum(double[] p)
		{
			if (p.Length == 0)
				return 0.0;

			return p.AsEnumerable().Sum();
		}


		private static double StandardDeviation(double[] values)
		{
			var variance = Variance(values);

			if (variance.EstEquals(0.0, double.Epsilon))
				return 0.0;

			return Math.Sqrt(variance);
		}


		private static double TableCols(FormulaValues p)
		{
			// The user fn requires no params but Calculator will include (tablew)
			if (p.Count > 1)
				throw new FormulaException("tablecols function does not accept parameters");

			var array = p.ToArray();

			// if we don't have a table then do not fail; instead presume this is being
			// called as a mock run from FormulaDialog
			if (array[2].Type != FormulaValueType.Table || array[2].Value is not Models.Table table)
			{
				return 0.0;
			}

			Logger.Current.Verbose($"fn tablecols() = {table.ColumnCount}");
			return table.ColumnCount;
		}


		private static double TableRows(FormulaValues p)
		{
			// The user fn requires no params but Calculator will include (tablew)
			if (p.Count > 1)
				throw new FormulaException("tableRows function does not accept parameters");

			var array = p.ToArray();

			// if we don't have a table then do not fail; instead presume this is being
			// called as a mock run from FormulaDialog
			if (array[2].Type != FormulaValueType.Table || array[2].Value is not Models.Table table)
			{
				return 0.0;
			}

			Logger.Current.Verbose($"fn tablerows() = {table.ColumnCount}");
			return table.RowCount;
		}


		private static double Variance(double[] values)
		{
			var mean = 0.0;
			var sum = 0.0;
			var variance = 0.0;
			var n = 0;
			foreach (var value in values)
			{
				n++;
				var delta = value - mean;
				mean += delta / n;
				sum += delta * (value - mean);
			}

			if (n > 1)
			{
				// if (Population)
				variance = sum / (n - 1);

				// else if (Sample)
				//variance = sum / n;
			}

			return variance;
		}
	}
}
