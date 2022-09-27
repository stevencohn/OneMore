//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System.Collections.Generic;


	/// <summary>
	/// Accepted types of values in a formula
	/// </summary>
	internal enum FormulaValueType
	{
		Boolean,
		Double,
		String,
		Unknown
	}


	/// <summary>
	/// Allowed operators in first character of a countif comparison
	/// </summary>
	internal enum CountIfOperator
	{
		GreaterThan,
		LessThan,
		NotEqual
	}


	/// <summary>
	/// Boxy formula value representing a double, string, or boolean value.
	/// </summary>
	internal class FormulaValue
	{
		public FormulaValueType Type { get; private set; }
		public CountIfOperator Operator { get; private set; }
		public object Value { get; private set; }
		public double DoubleValue { get => (double)Value; }

		public FormulaValue(bool value)
		{
			Value = value;
			Type = FormulaValueType.Boolean;
		}
		public FormulaValue(double value)
		{
			Value = value;
			Type = FormulaValueType.Double;
		}
		public FormulaValue(int value)
		{
			Value = (double)value;
			Type = FormulaValueType.Double;
		}
		public FormulaValue(string value)
		{
			Value = value;
			Type = FormulaValueType.String;
		}

		public int CompareTo(FormulaValue template)
		{
			if (template.Type == Type)
			{
				switch (template.Type)
				{
					case FormulaValueType.Double:
						return ((double)Value).CompareTo(template.DoubleValue);

					case FormulaValueType.Boolean:
						return ((bool)Value).CompareTo((bool)template.Value);

					default:
						var v1 = Value.ToString().ToLowerInvariant();
						var v2 = template.Value.ToString().ToLowerInvariant();
						return v1.CompareTo(v2);
				}
			}

			return -1;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}


	/// <summary>
	/// Collection of formula values
	/// </summary>
	internal class FormulaValues
	{
		protected readonly List<FormulaValue> values;

		public FormulaValues()
		{
			values = new List<FormulaValue>();
		}

		public void Add(bool value)
		{
			values.Add(new FormulaValue(value));
		}

		public void Add(double value)
		{
			values.Add(new FormulaValue(value));
		}

		public void Add(double[] valueList)
		{
			foreach (var v in valueList)
			{
				Add(v);
			}
		}

		public void Add(FormulaValue other)
		{
			values.Add(other);
		}

		public void Add(string value)
		{
			values.Add(new FormulaValue(value));
		}

		public int Count
		{
			get => values.Count;
		}

		public double this[int i]
		{
			get => (double)values[i].Value;
		}

		public bool GetBool(int i)
		{
			return (bool)values[i].Value;
		}

		public string GetString(int i)
		{
			return (string)values[i].Value;
		}

		public FormulaValue ItemAt(int index)
		{
			if (index >= 0 && index < values.Count)
			{
				return values[index];
			}

			throw new FormulaException("ItemAt index is out of range");
		}

		public FormulaValues Match(params FormulaValueType[] types)
		{
			// values should contain at least the required types
			if (types.Length <= values.Count)
			{
				for (int i = 0; i < types.Length; i++)
				{
					// does each value match the required type in sequence
					if (types[i] != values[i].Type)
					{
						throw new FormulaException($"parameter {i} is not of type {types[i]}");
					}
				}
			}

			return this;
		}

		public FormulaValue[] ToArray()
		{
			return values.ToArray();
		}

		public double[] ToDoubleArray()
		{
			var list = new List<double>();
			values.ForEach(v =>
			{
				if (v.Value is double d)
				{
					list.Add(d);
				}
				// empty cells are not included in the array
				else if (v.Value is string s && !string.IsNullOrWhiteSpace(s))
				{
					if (double.TryParse(s, out var ds))
					{
						list.Add(ds);
					}
				}
			});

			return list.ToArray();
		}
	}
}