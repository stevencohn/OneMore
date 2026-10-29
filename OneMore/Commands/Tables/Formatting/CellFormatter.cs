//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Reformats the text of a table cell according to a CellFormat. This class is pure, it
	/// has no dependency on OneNote, so it can be tested in isolation.
	/// </summary>
	internal static class CellFormatter
	{
		private static readonly Regex TimeOfDay = new(
			@"\d{1,2}:\d{2}(:\d{2}([.,]\d+)?)?", RegexOptions.CultureInvariant);


		public static readonly string[] DatePatterns = new[]
		{
			"M/d/yyyy",
			"MM/dd/yyyy",
			"yyyy-MM-dd",
			"d-MMM-yyyy",
			"MMM d, yyyy",
			"MMMM d, yyyy",
			"dddd, MMMM d, yyyy",
			"d MMM yy",
			"MMM yyyy",
			"M/d"
		};


		public static readonly string[] TimePatterns = new[]
		{
			"h:mm tt",
			"hh:mm tt",
			"H:mm",
			"HH:mm",
			"h:mm:ss tt",
			"HH:mm:ss"
		};


		public static readonly string[] DateTimePatterns = new[]
		{
			"M/d/yyyy h:mm tt",
			"yyyy-MM-dd HH:mm",
			"yyyy-MM-dd HH:mm:ss",
			"MMM d, yyyy h:mm tt",
			"MMMM d, yyyy h:mm tt",
			"dddd, MMMM d, yyyy h:mm tt",
			"yyyy-MM-ddTHH:mm:ss"
		};


		/// <summary>
		/// Gets the patterns offered for the given category, or an empty array.
		/// </summary>
		public static string[] GetPatterns(CellFormatCategory category)
		{
			switch (category)
			{
				case CellFormatCategory.Date: return DatePatterns;
				case CellFormatCategory.Time: return TimePatterns;
				case CellFormatCategory.DateTime: return DateTimePatterns;
				default: return Array.Empty<string>();
			}
		}


		/// <summary>
		/// Guesses the category that best describes a set of cell texts, so the Format Cells
		/// dialog can preselect it. Blank texts are ignored. At least 80% of the remaining
		/// texts must be of the same kind, numbers or dates, to avoid being thrown off by a
		/// few stray values; otherwise the result is General.
		/// </summary>
		/// <param name="texts">The cell texts to examine</param>
		/// <param name="culture">The culture used to parse values</param>
		public static CellFormatCategory DetectCategory(
			IEnumerable<string> texts, CultureInfo culture)
		{
			var counts = new Dictionary<CellFormatCategory, int>();
			var total = 0;

			foreach (var text in texts)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}

				total++;
				var category = Classify(text, culture);
				if (category.HasValue)
				{
					counts.TryGetValue(category.Value, out var n);
					counts[category.Value] = n + 1;
				}
			}

			if (total == 0)
			{
				return CellFormatCategory.General;
			}

			int Count(params CellFormatCategory[] categories)
			{
				return categories.Sum(c => counts.TryGetValue(c, out var n) ? n : 0);
			}

			// integer math for the 80% threshold
			var numbers = Count(
				CellFormatCategory.Number, CellFormatCategory.Percent, CellFormatCategory.Currency);

			if (numbers * 10 >= total * 8)
			{
				if (Count(CellFormatCategory.Percent) * 2 > numbers)
				{
					return CellFormatCategory.Percent;
				}

				if (Count(CellFormatCategory.Currency) * 2 > numbers)
				{
					return CellFormatCategory.Currency;
				}

				return CellFormatCategory.Number;
			}

			var date = Count(CellFormatCategory.Date);
			var time = Count(CellFormatCategory.Time);
			var both = Count(CellFormatCategory.DateTime);

			if ((date + time + both) * 10 >= total * 8)
			{
				if (both >= date && both >= time)
				{
					return CellFormatCategory.DateTime;
				}

				return date >= time ? CellFormatCategory.Date : CellFormatCategory.Time;
			}

			return CellFormatCategory.General;
		}


		private static CellFormatCategory? Classify(string text, CultureInfo culture)
		{
			// decimal parsing tolerates misplaced group separators, so a date such as
			// "28.10.2026" in a culture that groups with "." also looks like a number
			if (TableSorter.TryParseNumber(text, culture, out _) &&
				!(HasIrregularGrouping(text, culture) && TableSorter.TryParseDate(text, culture, out _)))
			{
				if (HasPercentSign(text, culture))
				{
					return CellFormatCategory.Percent;
				}

				return HasCurrencySymbol(text, culture)
					? CellFormatCategory.Currency
					: CellFormatCategory.Number;
			}

			if (!TableSorter.TryParseDate(text, culture, out _))
			{
				return null;
			}

			if (text.IndexOf(':') < 0)
			{
				return CellFormatCategory.Date;
			}

			// remove the time of day and what remains, if anything, is a date
			var rest = TimeOfDay.Replace(text, " ");
			var dtf = culture.DateTimeFormat;
			foreach (var designator in new[] { dtf.AMDesignator, dtf.PMDesignator })
			{
				if (!string.IsNullOrEmpty(designator))
				{
					rest = Regex.Replace(
						rest, Regex.Escape(designator), " ", RegexOptions.IgnoreCase);
				}
			}

			return rest.Any(char.IsLetterOrDigit)
				? CellFormatCategory.DateTime
				: CellFormatCategory.Time;
		}


		private static bool HasIrregularGrouping(string text, CultureInfo culture)
		{
			var group = culture.NumberFormat.NumberGroupSeparator;
			if (string.IsNullOrEmpty(group))
			{
				return false;
			}

			// only the integer part is grouped
			var integer = text;
			var point = text.IndexOf(culture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal);
			if (point >= 0)
			{
				integer = text.Substring(0, point);
			}

			var escaped = Regex.Escape(group);
			foreach (Match run in Regex.Matches(integer, $@"\d+(?:{escaped}\d+)+"))
			{
				var parts = run.Value.Split(new[] { group }, StringSplitOptions.None);
				if (parts[0].Length > 3 || parts.Skip(1).Any(p => p.Length != 3))
				{
					return true;
				}
			}

			return false;
		}


		private static bool HasCurrencySymbol(string text, CultureInfo culture)
		{
			if (text.Any(c => CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.CurrencySymbol))
			{
				return true;
			}

			var symbol = culture.NumberFormat.CurrencySymbol;
			return !string.IsNullOrEmpty(symbol) &&
				text.IndexOf(symbol, StringComparison.Ordinal) >= 0;
		}


		/// <summary>
		/// Formats the given cell text.
		/// </summary>
		/// <param name="text">The current text of the cell</param>
		/// <param name="format">The format to apply</param>
		/// <param name="culture">The culture used both to parse and to display values</param>
		/// <returns>
		/// The formatted text, or null if the text is empty or cannot be parsed as the kind
		/// of value required by the format, in which case the cell should be left alone.
		/// </returns>
		public static string Format(string text, CellFormat format, CultureInfo culture)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}

			switch (format.Category)
			{
				case CellFormatCategory.Date:
				case CellFormatCategory.Time:
				case CellFormatCategory.DateTime:
					return FormatDate(text, format, culture);

				default:
					return FormatNumber(text, format, culture);
			}
		}


		private static string FormatDate(string text, CellFormat format, CultureInfo culture)
		{
			if (!TableSorter.TryParseDate(text, culture, out var value))
			{
				return null;
			}

			var pattern = format.Pattern;
			if (string.IsNullOrEmpty(pattern))
			{
				pattern = GetPatterns(format.Category)[0];
			}

			return value.ToString(pattern, culture);
		}


		private static string FormatNumber(string text, CellFormat format, CultureInfo culture)
		{
			if (!TableSorter.TryParseNumber(text, culture, out var value))
			{
				return null;
			}

			// reduce to the true value; "25%" is 0.25 no matter what the target format is
			if (HasPercentSign(text, culture) ||
				(format.Category == CellFormatCategory.Percent && !format.PercentIsFraction))
			{
				value /= 100m;
			}

			return FormatValue(value, format, culture);
		}


		/// <summary>
		/// Formats a number, already reduced to its true value, as a Number, Percent, Currency,
		/// or General value according to the format and culture.
		/// </summary>
		public static string FormatValue(decimal value, CellFormat format, CultureInfo culture)
		{
			var nfi = (NumberFormatInfo)culture.NumberFormat.Clone();
			var places = format.Decimals;
			var parentheses = format.Negative == NegativeStyle.Parentheses;

			switch (format.Category)
			{
				case CellFormatCategory.Number:
					if (!format.UseSeparator)
					{
						nfi.NumberGroupSizes = new[] { 0 };
					}

					// 0 is (n), 1 is -n
					nfi.NumberNegativePattern = parentheses ? 0 : 1;
					return value.ToString("N" + places, nfi);

				case CellFormatCategory.Currency:
					if (!string.IsNullOrEmpty(format.CurrencySymbol))
					{
						nfi.CurrencySymbol = format.CurrencySymbol;
					}

					if (!format.UseSeparator)
					{
						nfi.CurrencyGroupSizes = new[] { 0 };
					}

					// keep the culture's symbol placement; positive patterns are 0 $n, 1 n$,
					// 2 $ n, 3 n $; negative patterns are 1 -$n, 5 -n$, 9 -$ n, 8 -n $ and
					// 0 ($n), 4 (n$), 14 ($ n), 15 (n $)
					var positive = Math.Max(0, Math.Min(3, nfi.CurrencyPositivePattern));
					nfi.CurrencyNegativePattern = parentheses
						? new[] { 0, 4, 14, 15 }[positive]
						: new[] { 1, 5, 9, 8 }[positive];

					return value.ToString("C" + places, nfi);

				case CellFormatCategory.Percent:
					return value.ToString("P" + places, nfi);

				default:
					// General: the plain value with no grouping and no trailing zeros
					return value.ToString("0.############################", nfi);
			}
		}


		private static bool HasPercentSign(string text, CultureInfo culture)
		{
			return text.IndexOf('%') >= 0 ||
				text.IndexOf(culture.NumberFormat.PercentSymbol, StringComparison.Ordinal) >= 0;
		}
	}
}
