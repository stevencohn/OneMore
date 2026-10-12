//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Globalization;


	/// <summary>
	/// Identifies the part of a date/time that a format segment represents.
	/// </summary>
	internal enum DatePart
	{
		/// <summary>A literal piece of text, not editable.</summary>
		None,

		Year,
		Month,
		Day,
		DayOfWeek,
		Hour12,
		Hour24,
		Minute,
		Second,
		AmPm,

		/// <summary>A displayed but non-editable field such as era, fractions or time zone.</summary>
		Other
	}


	/// <summary>
	/// Represents one run of a parsed date format pattern; either a literal or a field.
	/// </summary>
	internal sealed class DateSegment
	{
		/// <summary>
		/// Gets the date part this segment represents or None for literals.
		/// </summary>
		public DatePart Part { get; }


		/// <summary>
		/// Gets the custom format letter for fields, e.g. 'd', 'M', 'y', 'h'.
		/// </summary>
		public char Letter { get; }


		/// <summary>
		/// Gets the number of repetitions of Letter in the original pattern.
		/// </summary>
		public int Count { get; }


		/// <summary>
		/// Gets the literal text, for Part == None.
		/// </summary>
		public string Literal { get; }


		private DateSegment(DatePart part, char letter, int count, string literal)
		{
			Part = part;
			Letter = letter;
			Count = count;
			Literal = literal;
		}


		public static DateSegment MakeLiteral(string text)
		{
			return new DateSegment(DatePart.None, '\0', 0, text);
		}


		public static DateSegment MakeField(DatePart part, char letter, int count)
		{
			return new DateSegment(part, letter, count, null);
		}


		public bool IsLiteral => Part == DatePart.None;


		/// <summary>
		/// Gets a value indicating whether the user can change this segment.
		/// Weekday names are derived from the date so they are display-only.
		/// </summary>
		public bool IsEditable =>
			Part != DatePart.None && Part != DatePart.Other && Part != DatePart.DayOfWeek;


		/// <summary>
		/// Gets the maximum number of digits that may be typed for numeric parts, or 0
		/// if the part is textual (month names, AM/PM).
		/// </summary>
		public int MaxDigits
		{
			get
			{
				switch (Part)
				{
					case DatePart.Year:
						return Count <= 2 ? 2 : 4;

					case DatePart.Month:
						return Count <= 2 ? 2 : 0;

					case DatePart.Day:
					case DatePart.Hour12:
					case DatePart.Hour24:
					case DatePart.Minute:
					case DatePart.Second:
						return 2;

					default:
						return 0;
				}
			}
		}


		/// <summary>
		/// Gets the text this segment displays for the given date.
		/// </summary>
		public string Format(DateTime value, CultureInfo culture)
		{
			if (IsLiteral)
			{
				return Literal;
			}

			// a single custom letter must be prefixed with % to not be taken as a
			// standard format specifier
			var pattern = Count == 1 ? $"%{Letter}" : new string(Letter, Count);
			return value.ToString(pattern, culture);
		}


		/// <summary>
		/// Gets the longest text this segment could display, used to reserve a stable width
		/// so the control does not shift around while editing.
		/// </summary>
		public string WidestText(CultureInfo culture)
		{
			if (IsLiteral)
			{
				return Literal;
			}

			var best = string.Empty;
			var info = culture.DateTimeFormat;

			void Consider(string text)
			{
				if (text is not null && text.Length > best.Length)
				{
					best = text;
				}
			}

			switch (Part)
			{
				case DatePart.Month when Count >= 4:
					foreach (var name in info.MonthGenitiveNames)
					{
						Consider(name);
					}
					foreach (var name in info.MonthNames)
					{
						Consider(name);
					}
					break;

				case DatePart.Month when Count == 3:
					foreach (var name in info.AbbreviatedMonthGenitiveNames)
					{
						Consider(name);
					}
					foreach (var name in info.AbbreviatedMonthNames)
					{
						Consider(name);
					}
					break;

				case DatePart.DayOfWeek when Count >= 4:
					foreach (var name in info.DayNames)
					{
						Consider(name);
					}
					break;

				case DatePart.DayOfWeek:
					foreach (var name in info.AbbreviatedDayNames)
					{
						Consider(name);
					}
					break;

				case DatePart.AmPm:
					Consider(info.AMDesignator);
					Consider(info.PMDesignator);
					if (Count == 1)
					{
						best = best.Length > 0 ? best.Substring(0, 1) : best;
					}
					break;

				case DatePart.Year:
					best = new string('8', Count <= 2 ? 2 : 4);
					break;

				case DatePart.Month:
				case DatePart.Day:
				case DatePart.Hour12:
				case DatePart.Hour24:
				case DatePart.Minute:
				case DatePart.Second:
					best = new string('8', Math.Max(Count, 2));
					break;

				default:
					best = Format(DateTime.Now, culture);
					break;
			}

			return best;
		}
	}
}
