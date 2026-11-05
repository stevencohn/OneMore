//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System.Collections.Generic;


	internal enum ReferenceKind
	{
		Function,
		Operator,
		Variable,
		Cell
	}


	internal sealed class ReferenceEntry
	{
		public ReferenceEntry(
			ReferenceKind kind, string name, string signature, string description, string insert)
		{
			Kind = kind;
			Name = name;
			Signature = signature;
			Description = description;
			Insert = insert;
		}

		public ReferenceKind Kind { get; }
		public string Name { get; }
		public string Signature { get; }
		public string Description { get; }

		/// <summary>The text inserted at the cursor</summary>
		public string Insert { get; }

		/// <summary>True when selecting the entry opens a further detail view (countif)</summary>
		public bool Drill => Kind == ReferenceKind.Function
			&& Name.Equals("countif", System.StringComparison.Ordinal);

		/// <summary>True for user-defined variables from Settings</summary>
		public bool Custom { get; set; }

		public bool Matches(string filter)
		{
			return string.IsNullOrWhiteSpace(filter)
				|| Name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0
				|| Description.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}


	/// <summary>
	/// Static catalog of everything that can appear in a formula, shown in the dialog's
	/// reference panel. Descriptions are English only for now.
	/// </summary>
	internal static class FormulaReference
	{
		public static List<ReferenceEntry> Functions()
		{
			var f = ReferenceKind.Function;
			return new List<ReferenceEntry>
			{
				new(f, "abs", "abs(x)", "Absolute value", "abs("),
				new(f, "acos", "acos(x)", "Arc cosine, in radians", "acos("),
				new(f, "arccos", "arccos(x)", "Arc cosine, in radians", "arccos("),
				new(f, "arcsin", "arcsin(x)", "Arc sine, in radians", "arcsin("),
				new(f, "arctan", "arctan(x)", "Arc tangent, in radians", "arctan("),
				new(f, "asin", "asin(x)", "Arc sine, in radians", "asin("),
				new(f, "atan", "atan(x)", "Arc tangent, in radians", "atan("),
				new(f, "atan2", "atan2(y, x)", "Angle of the point (x, y)", "atan2("),
				new(f, "average", "average(range)", "Mean of the numbers in range", "average("),
				new(f, "ceil", "ceil(x)", "Round up to a whole number", "ceil("),
				new(f, "ceiling", "ceiling(x)", "Round up to a whole number", "ceiling("),
				new(f, "cos", "cos(x)", "Cosine", "cos("),
				new(f, "cosh", "cosh(x)", "Hyperbolic cosine", "cosh("),
				new(f, "countif", "countif(range, criteria)",
					"Counts cells in range that match the criteria", "countif("),
				new(f, "exp", "exp(x)", "e raised to the power x", "exp("),
				new(f, "floor", "floor(x)", "Round down to a whole number", "floor("),
				new(f, "log", "log(x)", "Natural logarithm", "log("),
				new(f, "log10", "log10(x)", "Base 10 logarithm", "log10("),
				new(f, "max", "max(range)", "Largest value", "max("),
				new(f, "median", "median(range)", "Middle value of the numbers", "median("),
				new(f, "min", "min(range)", "Smallest value", "min("),
				new(f, "mode", "mode(range)", "Most frequent value", "mode("),
				new(f, "pow", "pow(x, y)", "x raised to the power y", "pow("),
				new(f, "range", "range(range)", "Largest minus smallest value", "range("),
				new(f, "rem", "rem(x, y)", "Remainder of x divided by y", "rem("),
				new(f, "root", "root(x, n)", "The nth root of x", "root("),
				new(f, "round", "round(x)", "Round to the nearest whole number", "round("),
				new(f, "sign", "sign(x)", "-1, 0 or 1 for the sign of x", "sign("),
				new(f, "sin", "sin(x)", "Sine", "sin("),
				new(f, "sinh", "sinh(x)", "Hyperbolic sine", "sinh("),
				new(f, "sqrt", "sqrt(x)", "Square root", "sqrt("),
				new(f, "stdev", "stdev(range)", "Standard deviation", "stdev("),
				new(f, "sum", "sum(range)", "Total of the numbers", "sum("),
				new(f, "tan", "tan(x)", "Tangent", "tan("),
				new(f, "tanh", "tanh(x)", "Hyperbolic tangent", "tanh("),
				new(f, "trunc", "trunc(x)", "Remove the fractional part", "trunc("),
				new(f, "truncate", "truncate(x)", "Remove the fractional part", "truncate("),
				new(f, "variance", "variance(range)", "Variance of the numbers", "variance(")
			};
		}


		public static List<ReferenceEntry> Operators()
		{
			var o = ReferenceKind.Operator;
			return new List<ReferenceEntry>
			{
				new(o, "+", "a + b", "Add", " + "),
				new(o, "-", "a - b", "Subtract", " - "),
				new(o, "*", "a * b", "Multiply", " * "),
				new(o, "/", "a / b", "Divide", " / "),
				new(o, "^", "a ^ b", "Power", " ^ "),
				new(o, "%", "a % b", "Remainder", " % ")
			};
		}


		public static List<ReferenceEntry> Variables(IEnumerable<Variable> custom)
		{
			var v = ReferenceKind.Variable;
			var list = new List<ReferenceEntry>
			{
				new(v, "e", "e", "Euler's number, 2.71828...", "e"),
				new(v, "phi", "phi", "Golden ratio, 1.61803...", "phi"),
				new(v, "pi", "pi", "3.14159...", "pi"),
				new(v, "tao", "tao", "2 times pi", "tao"),
				new(v, "pitograd", "pitograd", "Multiplier converting radians to degrees", "pitograd"),
				new(v, "piofgrad", "piofgrad", "Multiplier converting degrees to radians", "piofgrad"),
				new(v, "tablecols", "tablecols", "Number of columns in the table", "tablecols"),
				new(v, "tablerows", "tablerows", "Number of rows in the table", "tablerows")
			};

			if (custom is not null)
			{
				foreach (var variable in custom)
				{
					list.Add(new(v, variable.Name, variable.Name,
						variable.Value.ToString(System.Globalization.CultureInfo.CurrentCulture),
						variable.Name) { Custom = true });
				}
			}

			return list;
		}


		/// <summary>
		/// Cell syntax examples resolved against the first target cell, for example A9.
		/// </summary>
		public static List<ReferenceEntry> Cells(string target)
		{
			var c = ReferenceKind.Cell;
			var above = Offset(target, -1);
			return new List<ReferenceEntry>
			{
				new(c, "A1", "A1 ... ZZZ999", "A single cell, column letters then row number", "A1"),
				new(c, "A1:A22", "A1:A22", "A range of cells in one row or column", "A1:A22"),
				new(c, "cell(col,row)", "cell(0,-1)",
					$"Relative to the target cell {target}; cell(0,-1) is {above}", "cell(0,-1)"),
				new(c, "A1:cell(0,-1)", "A1:cell(0,-1)",
					$"Range from A1 to the cell above {target}, which is A1:{above}", "A1:cell(0,-1)")
			};
		}


		private static string Offset(string target, int rows)
		{
			var i = 0;
			while (i < target.Length && char.IsLetter(target[i]))
			{
				i++;
			}

			if (i == 0 || i == target.Length || !int.TryParse(target.Substring(i), out var row))
			{
				return "the cell above";
			}

			return $"{target.Substring(0, i)}{System.Math.Max(1, row + rows)}";
		}
	}
}
