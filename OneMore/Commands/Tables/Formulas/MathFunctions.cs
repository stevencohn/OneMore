//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S125 // Sections of code should not be commented out

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;
	using System.Collections.Generic;
	using System.Linq;


	internal delegate double MathFunc(params double[] args);


	internal class MathFunction
	{
		public string Name { get; private set; }

		public MathFunc Fn { get; private set; }

		public MathFunction(string name, MathFunc fn)
		{
			Name = name;
			Fn = fn;
		}
	}


	internal static class MathFunctions
	{
		private static readonly List<MathFunction> functions = new List<MathFunction>();

		static MathFunctions()
		{
			functions.Add(new MathFunction("abs", (double[] p) => Math.Abs(p[0])));
			functions.Add(new MathFunction("acos", (double[] p) => Math.Acos(p[0])));
			functions.Add(new MathFunction("asin", (double[] p) => Math.Asin(p[0])));
			functions.Add(new MathFunction("atan", (double[] p) => Math.Atan(p[0])));
			functions.Add(new MathFunction("atan2", (double[] p) => Math.Atan2(p[0], p[1])));
			functions.Add(new MathFunction("average", (double[] p) => Average(p)));
			functions.Add(new MathFunction("ceiling", (double[] p) => Math.Ceiling(p[0])));
			functions.Add(new MathFunction("cos", (double[] p) => Math.Cos(p[0])));
			functions.Add(new MathFunction("cosh", (double[] p) => Math.Cosh(p[0])));
			functions.Add(new MathFunction("exp", (double[] p) => Math.Exp(p[0])));
			functions.Add(new MathFunction("floor", (double[] p) => Math.Floor(p[0])));
			functions.Add(new MathFunction("log", (double[] p) => Math.Log(p[0])));
			functions.Add(new MathFunction("log10", (double[] p) => Math.Log10(p[0])));
			functions.Add(new MathFunction("max", (double[] p) => Max(p)));
			functions.Add(new MathFunction("median", (double[] p) => Median(p)));
			functions.Add(new MathFunction("min", (double[] p) => Min(p)));
			functions.Add(new MathFunction("mode", (double[] p) => Mode(p)));
			functions.Add(new MathFunction("pow", (double[] p) => Math.Pow(p[0], p[1])));
			functions.Add(new MathFunction("range", (double[] p) => Range(p)));
			functions.Add(new MathFunction("round", (double[] p) => Math.Round(p[0])));
			functions.Add(new MathFunction("sign", (double[] p) => Math.Sign(p[0])));
			functions.Add(new MathFunction("sin", (double[] p) => Math.Sin(p[0])));
			functions.Add(new MathFunction("sinh", (double[] p) => Math.Sinh(p[0])));
			functions.Add(new MathFunction("sqrt", (double[] p) => Math.Sqrt(p[0])));
			functions.Add(new MathFunction("stdev", (double[] p) => StandardDeviation(p)));
			functions.Add(new MathFunction("sum", (double[] p) => Sum(p)));
			functions.Add(new MathFunction("tan", (double[] p) => Math.Tan(p[0])));
			functions.Add(new MathFunction("tanh", (double[] p) => Math.Tanh(p[0])));
			functions.Add(new MathFunction("trunc", (double[] p) => Math.Truncate(p[0])));
			functions.Add(new MathFunction("variance", (double[] p) => Variance(p)));
		}


		public static MathFunc Find(string name)
		{
			return functions.FirstOrDefault(f => f.Name == name)?.Fn;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private static double Average(double[] p)
		{
			if (p.Length == 0)
				return 0.0;

			return p.AsEnumerable().Average();
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

			if (variance == 0.0)
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
