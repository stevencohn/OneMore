//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Globalization;


	/// <summary>
	/// Pure date arithmetic used by MoreDateTimePicker to step and set individual parts of a
	/// DateTime. All results preserve the DateTimeKind and sub-second ticks of the source.
	/// </summary>
	internal static class DatePartMath
	{
		/// <summary>
		/// Builds a new DateTime from the given parts, clamping the day to the length of the
		/// month, and carrying over Kind and fractional seconds from source.
		/// </summary>
		public static DateTime With(
			DateTime source, int year, int month, int day, int hour, int minute, int second)
		{
			year = Math.Max(1, Math.Min(9999, year));
			month = Math.Max(1, Math.Min(12, month));
			day = Math.Max(1, Math.Min(DateTime.DaysInMonth(year, month), day));

			var fraction = source.Ticks % TimeSpan.TicksPerSecond;

			return new DateTime(year, month, day, hour, minute, second, source.Kind)
				.AddTicks(fraction);
		}


		/// <summary>
		/// Wraps value + delta into the inclusive range lo..hi.
		/// </summary>
		public static int Wrap(int value, int delta, int lo, int hi)
		{
			var range = hi - lo + 1;
			return (((value - lo + delta) % range) + range) % range + lo;
		}


		/// <summary>
		/// Gets the date clamped to the inclusive range min..max.
		/// </summary>
		public static DateTime Clamp(DateTime value, DateTime min, DateTime max)
		{
			if (value < min)
			{
				return min;
			}

			return value > max ? max : value;
		}


		/// <summary>
		/// Steps one part of the date up or down. Parts wrap within their own range, e.g.
		/// month 12 + 1 becomes month 1 of the same year, and do not carry into the next
		/// larger part, matching the behavior of the stock DateTimePicker.
		/// <para>
		/// A step that would land outside min..max is not clamped to the boundary, which
		/// would silently rewrite the other parts of the date. Instead, stepping continues
		/// in the same direction, wrapping, until an in-range value is found within one cycle
		/// of the part. If there is none, the value is returned unchanged.
		/// </para>
		/// </summary>
		public static DateTime Adjust(
			DateTime value, DatePart part, int delta, DateTime min, DateTime max)
		{
			var candidate = Step(value, part, delta);
			if (candidate is null)
			{
				return value;
			}

			if (candidate >= min && candidate <= max)
			{
				return candidate.Value;
			}

			int skips;
			switch (part)
			{
				case DatePart.Month:
					skips = 12;
					break;

				case DatePart.Day:
					skips = 31;
					break;

				case DatePart.Hour12:
				case DatePart.Hour24:
					skips = 24;
					break;

				case DatePart.Minute:
				case DatePart.Second:
					skips = 60;
					break;

				case DatePart.AmPm:
					skips = 2;
					break;

				default:
					// Year and DayOfWeek do not wrap; out of range is simply rejected
					return value;
			}

			var direction = delta < 0 ? -1 : 1;
			for (var i = 0; i < skips; i++)
			{
				candidate = Step(candidate.Value, part, direction);
				if (candidate is null)
				{
					break;
				}

				if (candidate >= min && candidate <= max)
				{
					return candidate.Value;
				}
			}

			return value;
		}


		/// <summary>
		/// Performs a single unclamped step of one part, or null if it cannot be applied.
		/// </summary>
		private static DateTime? Step(DateTime value, DatePart part, int delta)
		{
			var y = value.Year;
			var mo = value.Month;
			var d = value.Day;
			var h = value.Hour;
			var mi = value.Minute;
			var s = value.Second;

			switch (part)
			{
				case DatePart.Year:
					y = Math.Max(1, Math.Min(9999, y + delta));
					break;

				case DatePart.Month:
					mo = Wrap(mo, delta, 1, 12);
					break;

				case DatePart.Day:
					d = Wrap(d, delta, 1, DateTime.DaysInMonth(y, mo));
					break;

				case DatePart.DayOfWeek:
					{
						var days = value.Ticks / TimeSpan.TicksPerDay;
						if (days + delta < 0 || days + delta > 3652058)
						{
							return null;
						}

						return value.AddDays(delta);
					}

				case DatePart.Hour12:
					{
						var half = h >= 12 ? 12 : 0;
						h = Wrap(h % 12, delta, 0, 11) + half;
					}
					break;

				case DatePart.Hour24:
					h = Wrap(h, delta, 0, 23);
					break;

				case DatePart.Minute:
					mi = Wrap(mi, delta, 0, 59);
					break;

				case DatePart.Second:
					s = Wrap(s, delta, 0, 59);
					break;

				case DatePart.AmPm:
					h = (h + 12) % 24;
					break;

				default:
					return null;
			}

			return With(value, y, mo, d, h, mi, s);
		}


		/// <summary>
		/// Sets one part of the date to an absolute number. Returns null if the number is not
		/// valid for the part. For Hour12 the number is 1..12 and the existing AM/PM is kept;
		/// for AmPm the number is 0 (AM) or 1 (PM).
		/// </summary>
		public static DateTime? SetPart(DateTime value, DatePart part, int number)
		{
			var y = value.Year;
			var mo = value.Month;
			var d = value.Day;
			var h = value.Hour;
			var mi = value.Minute;
			var s = value.Second;

			switch (part)
			{
				case DatePart.Year:
					if (number < 1 || number > 9999)
					{
						return null;
					}

					y = number;
					break;

				case DatePart.Month:
					if (number < 1 || number > 12)
					{
						return null;
					}

					mo = number;
					break;

				case DatePart.Day:
					if (number < 1 || number > DateTime.DaysInMonth(y, mo))
					{
						return null;
					}

					d = number;
					break;

				case DatePart.Hour12:
					if (number < 1 || number > 12)
					{
						return null;
					}

					h = (number % 12) + (h >= 12 ? 12 : 0);
					break;

				case DatePart.Hour24:
					if (number < 0 || number > 23)
					{
						return null;
					}

					h = number;
					break;

				case DatePart.Minute:
					if (number < 0 || number > 59)
					{
						return null;
					}

					mi = number;
					break;

				case DatePart.Second:
					if (number < 0 || number > 59)
					{
						return null;
					}

					s = number;
					break;

				case DatePart.AmPm:
					if (number != 0 && number != 1)
					{
						return null;
					}

					h = (h % 12) + (number == 1 ? 12 : 0);
					break;

				default:
					return null;
			}

			return With(value, y, mo, d, h, mi, s);
		}


		/// <summary>
		/// Expands a two-digit year into four digits using the calendar's TwoDigitYearMax.
		/// Years with more than two typed digits are returned as is.
		/// </summary>
		public static int ExpandYear(int year, int typedDigits, Calendar calendar)
		{
			if (typedDigits > 2 || calendar is null || year > 99)
			{
				return year;
			}

			try
			{
				return calendar.ToFourDigitYear(year);
			}
			catch (ArgumentOutOfRangeException)
			{
				return year;
			}
		}


		/// <summary>
		/// Gets the smallest number valid for the part when typing digits.
		/// </summary>
		public static int MinEntry(DatePart part, int maxDigits)
		{
			switch (part)
			{
				case DatePart.Year:
					return maxDigits <= 2 ? 0 : 1;

				case DatePart.Month:
				case DatePart.Day:
				case DatePart.Hour12:
					return 1;

				default:
					return 0;
			}
		}


		/// <summary>
		/// Gets the largest number valid for the part when typing digits.
		/// </summary>
		public static int MaxEntry(DatePart part, DateTime value, int maxDigits)
		{
			switch (part)
			{
				case DatePart.Year:
					return maxDigits <= 2 ? 99 : 9999;

				case DatePart.Month:
					return 12;

				case DatePart.Day:
					return DateTime.DaysInMonth(value.Year, value.Month);

				case DatePart.Hour12:
					return 12;

				case DatePart.Hour24:
					return 23;

				case DatePart.Minute:
				case DatePart.Second:
					return 59;

				default:
					return 0;
			}
		}


		/// <summary>
		/// Appends a typed digit to the entry buffer for a numeric part. If the combined
		/// number would exceed the part's maximum the buffer restarts with just the new digit.
		/// </summary>
		/// <param name="buffer">The digits typed so far, possibly null or empty</param>
		/// <param name="digit">The key typed</param>
		/// <param name="part">The part being edited</param>
		/// <param name="value">The current date, needed for the length of the month</param>
		/// <param name="maxDigits">The maximum digits of the part</param>
		/// <param name="result">The new buffer</param>
		/// <param name="complete">
		/// True if no further digit can be accepted so editing should advance to the next part
		/// </param>
		/// <returns>True if the digit was accepted, false if it is not a digit</returns>
		public static bool TryEnter(
			string buffer, char digit, DatePart part, DateTime value, int maxDigits,
			out string result, out bool complete)
		{
			result = buffer ?? string.Empty;
			complete = false;

			if (digit < '0' || digit > '9' || maxDigits <= 0)
			{
				return false;
			}

			var max = MaxEntry(part, value, maxDigits);
			var min = MinEntry(part, maxDigits);

			var candidate = result + digit;
			if (candidate.Length > maxDigits || int.Parse(candidate) > max)
			{
				candidate = digit.ToString();
			}

			var number = int.Parse(candidate);
			complete = candidate.Length >= maxDigits || number * 10 > max;

			if (complete && number < min)
			{
				// e.g. "00" for a month; wait for a valid digit
				candidate = "0";
				complete = false;
			}

			result = candidate;
			return true;
		}
	}
}
