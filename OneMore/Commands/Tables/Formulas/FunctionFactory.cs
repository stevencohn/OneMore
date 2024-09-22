//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Resx = Properties.Resources;


	internal class FunctionFactory
	{
		private const int CountIfNe = 3;

		private readonly Dictionary<string, Func<VariantList, double>> functions;

		public FunctionFactory()
		{
			functions = new Dictionary<string, Func<VariantList, double>>();
		}


		public Func<VariantList, double> Find(string name)
		{
			var lower = name.ToLower();
			if (functions.ContainsKey(lower))
			{
				return functions[lower];
			}

			// just a short alias
			var D = VariantType.Double;

			Func<VariantList, double> function = lower switch
			{
				"abs" => inputs => Math.Abs(inputs.Assert(D)[0]),
				"acos" => inputs => Math.Acos(inputs.Assert(D)[0]),
				"arccos" => inputs => Math.Acos(inputs.Assert(D)[0]),
				"arcsin" => inputs => Math.Asin(inputs.Assert(D)[0]),
				"arctan" => inputs => Math.Atan(inputs.Assert(D)[0]),
				"asin" => inputs => Math.Asin(inputs.Assert(D)[0]),
				"atan" => inputs => Math.Atan(inputs.Assert(D)[0]),
				"atan2" => inputs => Math.Atan2(inputs.Assert(D, D)[0], inputs[1]),
				"average" => inputs => Average(inputs.Assert(D, D).ToDoubleArray()),
				"ceil" => inputs => Math.Ceiling(inputs.Assert(D)[0]),
				"ceiling" => inputs => Math.Ceiling(inputs.Assert(D)[0]),
				"cos" => inputs => Math.Cos(inputs.Assert(D)[0]),
				"cosh" => inputs => Math.Cosh(inputs.Assert(D)[0]),
				"countif" => inputs => CountIf(inputs),
				"exp" => inputs => Math.Exp(inputs.Assert(D)[0]),
				"floor" => inputs => Math.Floor(inputs.Assert(D)[0]),
				"log" => inputs => Math.Log(inputs.Assert(D)[0]),
				"log10" => inputs => Math.Log10(inputs.Assert(D)[0]),
				"max" => inputs => Max(inputs.Assert(D).ToDoubleArray()),
				"median" => inputs => Median(inputs.Assert(D).ToDoubleArray()),
				"min" => inputs => Min(inputs.Assert(D).ToDoubleArray()),
				"mode" => inputs => Mode(inputs.Assert(D).ToDoubleArray()),
				"pow" => inputs => Math.Pow(inputs.Assert(D)[0], inputs[1]),
				"range" => inputs => Range(inputs.Assert(D).ToDoubleArray()),
				"rem" => inputs => Math.IEEERemainder(inputs.Assert(D)[0], inputs[1]),
				"root" => inputs => Math.Pow(inputs.Assert(D, D)[0], 1 / inputs[1]),
				"round" => inputs => Math.Round(inputs.Assert(D)[0], MidpointRounding.AwayFromZero),
				"sign" => inputs => Math.Sign(inputs.Assert(D)[0]),
				"sin" => inputs => Math.Sin(inputs.Assert(D)[0]),
				"sinh" => inputs => Math.Sinh(inputs.Assert(D)[0]),
				"sqrt" => inputs => Math.Sqrt(inputs.Assert(D)[0]),
				"stdev" => inputs => StandardDeviation(inputs.Assert(D).ToDoubleArray()),
				"sum" => inputs => Sum(inputs.Assert(D).ToDoubleArray()),
				"tan" => inputs => Math.Tan(inputs.Assert(D)[0]),
				"tanh" => inputs => Math.Tanh(inputs.Assert(D)[0]),
				"trunc" => inputs => Math.Truncate(inputs.Assert(D)[0]),
				"truncate" => inputs => inputs[0] < 0 ? -Math.Floor(-inputs.Assert(D)[0]) : Math.Floor(inputs.Assert(D)[0]),
				"variance" => inputs => Variance(inputs.Assert(D).ToDoubleArray()),
				_ => null
			};

			if (function is not null)
			{
				Debug.WriteLine($"fun=[{lower}]");
				functions.Add(lower, function);
				return function;
			}

			return null;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private static double Average(double[] p)
		{
			if (p.Length == 0)
				return 0.0;

			return p.AsEnumerable().Average();
		}


		private static double CountIf(VariantList p)
		{
			if (p.Count < 3)
			{
				// user specifies two parameters (range,opMatch) but preprocessing expands
				// that to three parameters (range,op,match) so test for three but report two!
				throw new CalculatorException(Resx.Calculator_ErrInvalidCountifParams);
			}

			var array = p.ToArray();

			// values are items 0..last-2, ignore empty cells
			var values = array.Take(p.Count - 2)
				.Where(p => p.VariantType != VariantType.String || p.StringValue.Length > 0);

			// result is a numerical reprepsentation of the op as second param (<, >, or !)
			var result = (int)array[array.Length - 2].DoubleValue;

			// the match pattern is always the last parameter
			var match = array[array.Length - 1];

			return result == CountIfNe
				? values.Count(v => v.CompareTo(match) != 0)
				: values.Count(v => v.CompareTo(match) == result);
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
