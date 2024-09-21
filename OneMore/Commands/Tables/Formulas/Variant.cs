//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#nullable enable

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	internal enum VariantType
	{
		Boolean,
		Double,
		String,
		Empty
	}


	internal class Variant
	{
		private readonly VariantType variantType;
		private readonly bool? boolValue;
		private readonly double? doubleValue;
		private readonly string? stringValue;


		public Variant(bool value)
		{
			boolValue = value;
			variantType = VariantType.Boolean;
		}


		public Variant(double value)
		{
			doubleValue = value;
			variantType = VariantType.Double;
		}


		public Variant(int value)
		{
			doubleValue = value;
			variantType = VariantType.Double;
		}


		public Variant(string value)
		{
			stringValue = value;
			variantType = VariantType.String;
		}


		public VariantType VariantType => variantType;


		public bool BooleanValue => boolValue ?? false;


		public double DoubleValue => doubleValue ?? 0.0;


		public string StringValue => stringValue ?? string.Empty;


		public int CompareTo(Variant other)
		{
			if (other.VariantType == VariantType)
			{
				switch (other.VariantType)
				{
					case VariantType.Double:
						return doubleValue is null || other.doubleValue is null ? -1
							: ((double)doubleValue).CompareTo(other.DoubleValue);

					case VariantType.Boolean:
						return boolValue is null || other.boolValue is null ? -1
							: ((bool)boolValue).CompareTo(other.BooleanValue);

					default:
						if (stringValue is null || other.stringValue is null)
						{
							return -1;
						}
						var v1 = stringValue.ToLowerInvariant();
						var v2 = other.StringValue.ToLowerInvariant();
						return v1.CompareTo(v2);
				}
			}

			return -1;
		}


		public override string ToString()
		{
			return variantType switch
			{
				VariantType.Boolean => boolValue == true ? "True" : "False",
				VariantType.Double => doubleValue?.ToString() ?? "0",
				VariantType.String => stringValue ?? string.Empty,
				_ => string.Empty
			};
		}
	}
}
