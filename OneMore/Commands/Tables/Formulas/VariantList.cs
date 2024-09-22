//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#nullable enable

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;
	using System.Collections.Generic;
	using Resx = Properties.Resources;


	internal class VariantList
	{
		private readonly List<Variant> list;


		public VariantList()
		{
			list = new List<Variant>();
		}


		public VariantList(params double[] values)
			: this()
		{
			foreach (var value in values)
			{
				list.Add(new Variant(value));
			}
		}


		public VariantList(IEnumerable<double> values)
			: this()
		{
			foreach (var value in values)
			{
				list.Add(new Variant(value));
			}
		}


		public int Count
		{
			get => list.Count;
		}


		public double this[int i]
		{
			get => list[i].DoubleValue;
		}


		public void Add(Variant variant)
		{
			list.Add(variant);
		}


		public VariantList Assert(params VariantType[] types)
		{
			// list should contain at least the required types
			if (list.Count < types.Length)
			{
				throw new CalculatorException(
					string.Format(Resx.Calculator_ErrInvalidParamCount, types.Length, list.Count));
			}

			for (int i = 0; i < Math.Min(types.Length, list.Count); i++)
			{
				if (list[i].VariantType != types[i])
				{
					throw new CalculatorException(
						string.Format(Resx.Calculator_ErrInvalidFnParamType, i, types[i]));
				}
			}

			return this;
		}


		public Variant[] ToArray()
		{
			return list.ToArray();
		}


		public double[] ToDoubleArray()
		{
			var doubles = new List<double>();
			foreach (var value in list)
			{
				if (value.VariantType == VariantType.Double)
				{
					doubles.Add(value.DoubleValue);
				}
				else if (
					value.VariantType == VariantType.String &&
					!string.IsNullOrWhiteSpace(value.StringValue) &&
					double.TryParse(value.StringValue, out double d))
				{
					doubles.Add(d);
				}
			}

			return doubles.ToArray();
		}
	}
}
