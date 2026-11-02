//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tables
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Globalization;


	[TestClass]
	public class CellFormatterTests
	{
		private static readonly CultureInfo US = new CultureInfo("en-US");
		private static readonly CultureInfo DE = new CultureInfo("de-DE");
		private static readonly CultureInfo FR = new CultureInfo("fr-FR");


		private static string Format(string text, CellFormat format, CultureInfo culture = null)
		{
			return CellFormatter.Format(text, format, culture ?? US);
		}


		#region Number

		[TestMethod]
		public void Number_GroupsAndPadsDecimals()
		{
			var format = new CellFormat { Category = CellFormatCategory.Number, Decimals = 2 };
			Assert.AreEqual("1,234.50", Format("1234.5", format));
		}


		[TestMethod]
		public void Number_WithoutSeparator()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Number,
				Decimals = 2,
				UseSeparator = false
			};

			Assert.AreEqual("1234.50", Format("1234.5", format));
		}


		[TestMethod]
		public void Number_ZeroDecimalsRoundsAwayFromZero()
		{
			var format = new CellFormat { Category = CellFormatCategory.Number, Decimals = 0 };
			Assert.AreEqual("1,235", Format("1234.5", format));
		}


		[TestMethod]
		public void Number_NegativeInParentheses()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Number,
				Decimals = 2,
				Negative = NegativeStyle.Parentheses
			};

			Assert.AreEqual("(1,234.50)", Format("-1234.5", format));
			Assert.AreEqual("1,234.50", Format("1234.5", format));
		}


		[TestMethod]
		public void Number_ReadsCurrencyText()
		{
			var format = new CellFormat { Category = CellFormatCategory.Number, Decimals = 1 };
			Assert.AreEqual("1,234.5", Format("$1,234.50", format));
		}


		[TestMethod]
		public void Number_UsesCultureSeparators()
		{
			var format = new CellFormat { Category = CellFormatCategory.Number, Decimals = 2 };

			var expected = "1" + DE.NumberFormat.NumberGroupSeparator + "234" +
				DE.NumberFormat.NumberDecimalSeparator + "50";

			Assert.AreEqual(expected, Format("1234,5", format, DE));
		}


		[TestMethod]
		public void Number_FrenchUsesCommaDecimal()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Number,
				Decimals = 1,
				UseSeparator = false
			};

			Assert.AreEqual("12,5", Format("12,5", format, FR));
		}

		#endregion Number

		#region Percent

		[TestMethod]
		public void Percent_FractionInput()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Percent,
				Decimals = 0,
				PercentIsFraction = true
			};

			Assert.AreEqual("25%", Format("0.25", format));
		}


		[TestMethod]
		public void Percent_AlreadyPercentInput()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Percent,
				Decimals = 0,
				PercentIsFraction = false
			};

			Assert.AreEqual("25%", Format("25", format));
		}


		[TestMethod]
		public void Percent_ExplicitSignOverridesInputMode()
		{
			var fraction = new CellFormat
			{
				Category = CellFormatCategory.Percent,
				Decimals = 1,
				PercentIsFraction = true
			};

			var percent = new CellFormat
			{
				Category = CellFormatCategory.Percent,
				Decimals = 1,
				PercentIsFraction = false
			};

			Assert.AreEqual("12.5%", Format("12.5%", fraction));
			Assert.AreEqual("12.5%", Format("12.5%", percent));
		}

		#endregion Percent

		#region Currency

		[TestMethod]
		public void Currency_LocaleSymbol()
		{
			var format = new CellFormat { Category = CellFormatCategory.Currency, Decimals = 2 };
			Assert.AreEqual("$1,234.50", Format("1234.5", format));
		}


		[TestMethod]
		public void Currency_CustomSymbol()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Currency,
				Decimals = 2,
				CurrencySymbol = "€"
			};

			Assert.AreEqual("€1,234.50", Format("1234.5", format));
		}


		[TestMethod]
		public void Currency_NegativeInParentheses()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Currency,
				Decimals = 2,
				Negative = NegativeStyle.Parentheses
			};

			Assert.AreEqual("($1,234.50)", Format("-1234.5", format));
		}


		[TestMethod]
		public void Currency_NegativeWithMinus()
		{
			// the US culture's own negative currency pattern is parentheses, so a minus
			// sign must be requested explicitly
			var format = new CellFormat
			{
				Category = CellFormatCategory.Currency,
				Decimals = 2,
				Negative = NegativeStyle.Minus
			};

			Assert.AreEqual("-$1,234.50", Format("-1234.5", format));
		}


		[TestMethod]
		public void Currency_NegativeStylesKeepSymbolPlacement()
		{
			// de-DE places the symbol after the number
			var minus = new CellFormat
			{
				Category = CellFormatCategory.Currency,
				Decimals = 0,
				UseSeparator = false,
				Negative = NegativeStyle.Minus
			};

			var parens = new CellFormat
			{
				Category = CellFormatCategory.Currency,
				Decimals = 0,
				UseSeparator = false,
				Negative = NegativeStyle.Parentheses
			};

			var symbol = DE.NumberFormat.CurrencySymbol;
			var minusText = Format("-1234", minus, DE);
			var parenText = Format("-1234", parens, DE);

			Assert.IsTrue(minusText.StartsWith("-1234"), minusText);
			Assert.IsTrue(minusText.EndsWith(symbol), minusText);
			Assert.IsTrue(parenText.StartsWith("(1234"), parenText);
			Assert.IsTrue(parenText.EndsWith(symbol + ")"), parenText);
		}


		[TestMethod]
		public void Currency_WithoutSeparator()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Currency,
				Decimals = 0,
				UseSeparator = false
			};

			Assert.AreEqual("$1235", Format("1234.5", format));
		}

		#endregion Currency

		#region General

		[TestMethod]
		public void General_StripsFormatting()
		{
			var format = new CellFormat { Category = CellFormatCategory.General };
			Assert.AreEqual("1234.5", Format("$1,234.50", format));
			Assert.AreEqual("0.25", Format("25%", format));
		}

		#endregion General

		#region Date and time

		[TestMethod]
		public void Date_IsoPattern()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Date,
				Pattern = "yyyy-MM-dd"
			};

			Assert.AreEqual("2026-10-28", Format("10/28/2026", format));
		}


		[TestMethod]
		public void Date_LongMonthPattern()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Date,
				Pattern = "MMM d, yyyy"
			};

			Assert.AreEqual("Oct 28, 2026", Format("10/28/2026", format));
		}


		[TestMethod]
		public void Date_ReadsUsingTheCulture()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Date,
				Pattern = "yyyy-MM-dd"
			};

			Assert.AreEqual("2026-10-28", Format("28.10.2026", format, DE));
		}


		[TestMethod]
		public void Date_DefaultsToFirstPattern()
		{
			var format = new CellFormat { Category = CellFormatCategory.Date };
			Assert.AreEqual("10/28/2026", Format("2026-10-28", format));
		}


		[TestMethod]
		public void Time_TwentyFourHour()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.Time,
				Pattern = "HH:mm"
			};

			Assert.AreEqual("15:45", Format("3:45 PM", format));
		}


		[TestMethod]
		public void DateTime_IsoPattern()
		{
			var format = new CellFormat
			{
				Category = CellFormatCategory.DateTime,
				Pattern = "yyyy-MM-dd HH:mm"
			};

			Assert.AreEqual("2026-10-28 15:45", Format("10/28/2026 3:45 PM", format));
		}

		#endregion Date and time

		#region Unparseable

		[TestMethod]
		public void BlankText_IsSkipped()
		{
			var format = new CellFormat { Category = CellFormatCategory.Number };
			Assert.IsNull(Format(null, format));
			Assert.IsNull(Format(string.Empty, format));
			Assert.IsNull(Format("   ", format));
		}


		[TestMethod]
		public void Text_IsSkipped()
		{
			Assert.IsNull(Format("abc", new CellFormat { Category = CellFormatCategory.Number }));
			Assert.IsNull(Format("abc", new CellFormat { Category = CellFormatCategory.Percent }));
			Assert.IsNull(Format("abc", new CellFormat { Category = CellFormatCategory.Currency }));
			Assert.IsNull(Format("abc", new CellFormat { Category = CellFormatCategory.Date }));
		}


		[TestMethod]
		public void Number_IsNotReadAsDate()
		{
			Assert.IsNull(Format("1234", new CellFormat { Category = CellFormatCategory.Date }));
		}

		#endregion Unparseable

		#region DetectCategory

		private static CellFormatCategory Detect(CultureInfo culture, params string[] texts)
		{
			return CellFormatter.DetectCategory(texts, culture);
		}


		[TestMethod]
		public void Detect_Numbers()
		{
			Assert.AreEqual(CellFormatCategory.Number, Detect(US, "1", "2.5", "1,000", "-3"));
		}


		[TestMethod]
		public void Detect_Percents()
		{
			Assert.AreEqual(CellFormatCategory.Percent, Detect(US, "12%", "0.5%", "25%"));
		}


		[TestMethod]
		public void Detect_Currency()
		{
			Assert.AreEqual(CellFormatCategory.Currency, Detect(US, "$1.50", "$20", "$3"));
		}


		[TestMethod]
		public void Detect_Dates()
		{
			Assert.AreEqual(
				CellFormatCategory.Date, Detect(US, "10/28/2026", "2026-10-28", "Oct 28, 2026"));
		}


		[TestMethod]
		public void Detect_Times()
		{
			Assert.AreEqual(CellFormatCategory.Time, Detect(US, "3:45 PM", "15:45", "9:05 AM"));
		}


		[TestMethod]
		public void Detect_DateTimes()
		{
			Assert.AreEqual(
				CellFormatCategory.DateTime,
				Detect(US, "10/28/2026 3:45 PM", "2026-10-28 15:45", "2026-10-28T15:45:30"));
		}


		[TestMethod]
		public void Detect_UsesCulture()
		{
			Assert.AreEqual(CellFormatCategory.Number, Detect(DE, "1.234,5", "2,5", "10"));
			Assert.AreEqual(CellFormatCategory.Date, Detect(DE, "28.10.2026", "01.05.2026"));
		}


		[TestMethod]
		public void Detect_ToleratesFewStrays()
		{
			// exactly 80% matches
			Assert.AreEqual(
				CellFormatCategory.Date,
				Detect(US, "10/28/2026", "10/29/2026", "10/30/2026", "10/31/2026", "n/a"));
		}


		[TestMethod]
		public void Detect_TooManyStraysIsGeneral()
		{
			Assert.AreEqual(
				CellFormatCategory.General,
				Detect(US, "10/28/2026", "10/29/2026", "10/30/2026", "n/a", "tbd"));
		}


		[TestMethod]
		public void Detect_IgnoresBlanks()
		{
			Assert.AreEqual(CellFormatCategory.Number, Detect(US, string.Empty, null, "  ", "5"));
		}


		[TestMethod]
		public void Detect_NothingToExamineIsGeneral()
		{
			Assert.AreEqual(CellFormatCategory.General, Detect(US));
			Assert.AreEqual(CellFormatCategory.General, Detect(US, string.Empty, null));
		}


		[TestMethod]
		public void Detect_TextIsGeneral()
		{
			Assert.AreEqual(CellFormatCategory.General, Detect(US, "apple", "banana", "cherry"));
		}

		#endregion DetectCategory

		#region CellFormat

		[TestMethod]
		public void CellFormat_ClampsDecimals()
		{
			Assert.AreEqual(CellFormat.MaxDecimals, new CellFormat { Decimals = 99 }.Decimals);
			Assert.AreEqual(0, new CellFormat { Decimals = -3 }.Decimals);
		}

		#endregion CellFormat
	}
}
