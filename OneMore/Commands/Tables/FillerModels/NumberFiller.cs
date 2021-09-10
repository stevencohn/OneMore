//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using System;
	using System.Globalization;


	internal class NumberFiller : IFiller
	{
		private readonly bool isCurrency;
		private readonly bool isGrouped;
		private readonly bool isDecimal;
		private decimal value;


		public NumberFiller(string text)
		{
			var culture = CultureInfo.CurrentCulture;
			if (decimal.TryParse(text, NumberStyles.Currency, culture, out value))
			{
				isCurrency = text.StartsWith(culture.NumberFormat.CurrencySymbol);
				isGrouped = text.Contains(culture.NumberFormat.CurrencyGroupSeparator);
				isDecimal = text.Contains(culture.NumberFormat.CurrencyDecimalSeparator);
			}
		}


		public FillType Type => FillType.Number;


		public decimal Value => value;


		public bool CanParse(string text)
		{
			return decimal.TryParse(
				text, NumberStyles.Currency, CultureInfo.CurrentCulture, out _);
		}


		public string Increment(int increment)
		{
			value += increment;
			if (isCurrency) return value.ToString("C");
			if (isGrouped && isDecimal) return string.Format("{0:#,##0.#####}", value);
			if (isGrouped) return string.Format("{0:n0}", value);
			if (isDecimal) return value.ToString();
			return string.Format("{0:0.#}", Math.Floor(value));
		}
	}
}
