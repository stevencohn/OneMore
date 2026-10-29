//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;


	/// <summary>
	/// Detects column data types and computes a stable multi-key row ordering.
	/// Blank cells always sort last, regardless of direction.
	/// </summary>
	/// <remarks>
	/// Values are parsed using the supplied culture, which should be AddIn.Locale so that
	/// they are read back the same way formulas format them: locale-specific decimal and
	/// group separators, currency symbols, percent signs, and TimeSpan durations.
	/// </remarks>
	internal static class TableSorter
	{
		/// <summary>
		/// Determines the data type that best describes the non-blank values of a column,
		/// using the same 80% sampling as the Format Cells command so that a few stray
		/// values do not force the column to Text.
		/// </summary>
		public static SortDataType Detect(IEnumerable<string> values, CultureInfo culture)
		{
			switch (CellFormatter.DetectCategory(values, culture))
			{
				case CellFormatCategory.Number:
				case CellFormatCategory.Percent:
				case CellFormatCategory.Currency:
					return SortDataType.Number;

				case CellFormatCategory.Date:
				case CellFormatCategory.Time:
				case CellFormatCategory.DateTime:
					return SortDataType.DateTime;

				default:
					return SortDataType.Text;
			}
		}


		/// <summary>
		/// Returns the row indexes of <paramref name="rows"/> in sorted order. Rows are
		/// arrays of cell text; missing cells are treated as blank.
		/// </summary>
		public static int[] Sort(
			IReadOnlyList<string[]> rows, IReadOnlyList<SortRule> rules, CultureInfo culture)
		{
			var count = rows.Count;

			// parse each key once
			var keys = new IComparable[rules.Count][];
			for (var r = 0; r < rules.Count; r++)
			{
				var rule = rules[r];
				var type = rule.DataType;
				if (type == SortDataType.Auto)
				{
					type = Detect(GetColumn(rows, rule.ColumnIndex), culture);
				}

				keys[r] = new IComparable[count];
				for (var i = 0; i < count; i++)
				{
					keys[r][i] = ParseKey(GetCell(rows[i], rule.ColumnIndex), type, culture);
				}
			}

			var order = new List<int>(count);
			for (var i = 0; i < count; i++)
			{
				order.Add(i);
			}

			// List.Sort is unstable so fall back to the original index to keep ties stable
			order.Sort((a, b) =>
			{
				for (var r = 0; r < rules.Count; r++)
				{
					var ka = keys[r][a];
					var kb = keys[r][b];

					if (ka is null && kb is null)
					{
						continue;
					}

					if (ka is null)
					{
						return 1;
					}

					if (kb is null)
					{
						return -1;
					}

					var c = ka is string sa
						? string.Compare(sa, (string)kb, StringComparison.CurrentCultureIgnoreCase)
						: ka.CompareTo(kb);

					if (c != 0)
					{
						return rules[r].Descending ? -c : c;
					}
				}

				return a.CompareTo(b);
			});

			return order.ToArray();
		}


		private static IEnumerable<string> GetColumn(IReadOnlyList<string[]> rows, int column)
		{
			foreach (var row in rows)
			{
				yield return GetCell(row, column);
			}
		}


		private static string GetCell(string[] row, int column)
		{
			return column < row.Length ? row[column] : null;
		}


		private static IComparable ParseKey(string text, SortDataType type, CultureInfo culture)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}

			switch (type)
			{
				case SortDataType.Number:
					return TryParseNumber(text, culture, out var number) ? number : null;

				case SortDataType.DateTime:
					return TryParseDate(text, culture, out var date) ? date : null;

				default:
					return text.Trim();
			}
		}


		/// <summary>
		/// Parses plain numbers, currency in the culture's symbol, currency in any other
		/// culture's symbol, and percentages (sorted by their numeric value).
		/// </summary>
		internal static bool TryParseNumber(string text, CultureInfo culture, out decimal value)
		{
			var s = text.Trim();

			// percent, e.g. "12.5%" or "%12,5"
			var percent = culture.NumberFormat.PercentSymbol;
			if (s.IndexOf(percent, StringComparison.Ordinal) >= 0 || s.IndexOf('%') >= 0)
			{
				s = s.Replace(percent, string.Empty).Replace("%", string.Empty).Trim();
			}

			// handles the culture's own separators, sign, parentheses, and currency symbol
			if (decimal.TryParse(s, NumberStyles.Currency, culture, out value))
			{
				return true;
			}

			// ISO code or the symbol of some other culture, e.g. "€" on a workstation using "$"
			var stripped = StripOtherCurrency(s, culture);
			if (stripped.Length != s.Length &&
				decimal.TryParse(stripped, NumberStyles.Currency, culture, out value))
			{
				return true;
			}

			value = 0;
			return false;
		}


		private static string StripOtherCurrency(string text, CultureInfo culture)
		{
			var iso = GetIsoSymbol(culture);
			if (!string.IsNullOrEmpty(iso))
			{
				if (text.StartsWith(iso, StringComparison.OrdinalIgnoreCase))
				{
					text = text.Substring(iso.Length);
				}
				else if (text.EndsWith(iso, StringComparison.OrdinalIgnoreCase))
				{
					text = text.Substring(0, text.Length - iso.Length);
				}
			}

			var builder = new StringBuilder(text.Length);
			foreach (var c in text)
			{
				if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.CurrencySymbol)
				{
					builder.Append(c);
				}
			}

			return builder.ToString().Trim();
		}


		private static string GetIsoSymbol(CultureInfo culture)
		{
			try
			{
				return new RegionInfo(culture.Name).ISOCurrencySymbol;
			}
			catch (ArgumentException)
			{
				// invariant and neutral cultures have no region
				return null;
			}
		}


		/// <summary>
		/// Parses dates and times of day, and also durations such as "1.02:03:04" which is
		/// how Time formulas write their results. Durations are anchored to today so they
		/// compare consistently with times of day.
		/// </summary>
		internal static bool TryParseDate(string text, CultureInfo culture, out DateTime value)
		{
			var s = text.Trim();
			if (DateTime.TryParse(s, culture, DateTimeStyles.None, out value))
			{
				return true;
			}

			// require a colon so that a bare number is never read as a count of days
			if (s.IndexOf(':') >= 0 &&
				TimeSpan.TryParse(s, culture, out var span) &&
				span.Ticks >= 0 &&
				span.Ticks < DateTime.MaxValue.Ticks - DateTime.Today.Ticks)
			{
				value = DateTime.Today.AddTicks(span.Ticks);
				return true;
			}

			value = DateTime.MinValue;
			return false;
		}
	}
}
