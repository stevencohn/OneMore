//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;


	internal enum CellFormatCategory
	{
		General,
		Number,
		Percent,
		Currency,
		Date,
		Time,
		DateTime
	}


	internal enum NegativeStyle
	{
		Minus,
		Parentheses
	}


	internal enum FormatScope
	{
		SelectedCells,
		EntireColumns
	}


	/// <summary>
	/// Describes how the values of table cells are to be formatted by the Format Cells command.
	/// </summary>
	internal class CellFormat
	{
		public const int MaxDecimals = 10;

		private int decimals = 2;


		public CellFormatCategory Category { get; set; }

		/// <summary>
		/// Gets or sets the number of decimal places, 0..MaxDecimals; used by the Number,
		/// Percent, and Currency categories.
		/// </summary>
		public int Decimals
		{
			get => decimals;
			set => decimals = Math.Max(0, Math.Min(MaxDecimals, value));
		}

		public bool UseSeparator { get; set; } = true;

		public NegativeStyle Negative { get; set; }

		/// <summary>
		/// Gets or sets the currency symbol; null or empty to use the locale's symbol.
		/// </summary>
		public string CurrencySymbol { get; set; }

		/// <summary>
		/// Gets or sets whether plain values like 0.25 are fractions displayed as 25%; if false,
		/// values like 25 are already percents. A value with an explicit percent sign is always
		/// treated as a percent.
		/// </summary>
		public bool PercentIsFraction { get; set; } = true;

		/// <summary>
		/// Gets or sets the date and/or time pattern, used by the Date, Time, and DateTime
		/// categories; null or empty to use the first pattern of the category.
		/// </summary>
		public string Pattern { get; set; }

		public FormatScope Scope { get; set; }
	}
}
