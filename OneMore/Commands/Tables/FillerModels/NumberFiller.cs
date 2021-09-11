//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Globalization;


	internal class NumberFiller : Filler
	{
		private readonly bool isCurrency;
		private readonly bool isGrouped;
		private readonly bool isDecimal;
		private decimal value;


		public NumberFiller(TableCell cell)
			: base(cell)
		{
			var text = cell.GetText(true);

			var culture = CultureInfo.CurrentCulture;
			if (decimal.TryParse(text, NumberStyles.Currency, culture, out value))
			{
				isCurrency = text.StartsWith(culture.NumberFormat.CurrencySymbol);
				isGrouped = text.Contains(culture.NumberFormat.CurrencyGroupSeparator);
				isDecimal = text.Contains(culture.NumberFormat.CurrencyDecimalSeparator);
			}
		}


		public override FillType Type => FillType.Number;


		public decimal Value => value;


		public static bool CanParse(string text)
		{
			return decimal.TryParse(
				text, NumberStyles.Currency, CultureInfo.CurrentCulture, out _);
		}


		public override string Increment(int increment)
		{
			value += increment;
			if (isCurrency) return value.ToString("C");
			if (isGrouped && isDecimal) return string.Format("{0:#,##0.#####}", value);
			if (isGrouped) return string.Format("{0:n0}", value);
			if (isDecimal) return value.ToString();
			return string.Format("{0:0.#}", Math.Floor(value));
		}


		public override int Subtract(IFiller other)
		{
			if (other is NumberFiller o)
				return (int)(value - o.Value);

			return 0;
		}
	}
}
