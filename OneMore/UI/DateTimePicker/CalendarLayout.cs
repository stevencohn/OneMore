//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Globalization;


	/// <summary>
	/// The granularity currently shown by a MoreMonthCalendar.
	/// </summary>
	internal enum CalendarView
	{
		/// <summary>The days of a month.</summary>
		Days,

		/// <summary>The twelve months of a year.</summary>
		Months,

		/// <summary>Twelve years surrounding a decade.</summary>
		Years,

		/// <summary>Twelve decades surrounding a century.</summary>
		Decades
	}


	/// <summary>
	/// Pure grid arithmetic for MoreMonthCalendar; no painting and no theming so it can be
	/// unit tested.
	/// </summary>
	internal static class CalendarLayout
	{
		public const int Rows = 6;
		public const int Columns = 7;
		public const int PageSize = 12;

		private const long MaxDays = 3652058; // DateTime.MaxValue.Date in days


		/// <summary>
		/// Gets the date shown in the top-left cell of the 6x7 grid for the month that
		/// contains the given date. At least one trailing day of the previous month is
		/// always shown so the user can tell where the month starts.
		/// </summary>
		public static DateTime FirstVisibleDate(DateTime month, DayOfWeek firstDay)
		{
			var first = new DateTime(month.Year, month.Month, 1);
			var offset = (((int)first.DayOfWeek - (int)firstDay) + 7) % 7;
			if (offset == 0)
			{
				offset = 7;
			}

			var days = first.Ticks / TimeSpan.TicksPerDay;
			if (days < offset)
			{
				offset = (int)days;
			}

			return first.AddDays(-offset);
		}


		/// <summary>
		/// Gets the date of a cell in the 6x7 days grid, clamped to the last valid date.
		/// </summary>
		public static DateTime CellDate(DateTime firstVisible, int row, int column)
		{
			var n = (row * Columns) + column;
			var days = firstVisible.Ticks / TimeSpan.TicksPerDay;
			if (days + n > MaxDays)
			{
				return DateTime.MaxValue.Date;
			}

			return firstVisible.AddDays(n);
		}


		/// <summary>
		/// Finds the cell of a date in the 6x7 days grid.
		/// </summary>
		/// <returns>False if the date is not visible in the grid</returns>
		public static bool TryGetCell(
			DateTime firstVisible, DateTime date, out int row, out int column)
		{
			var n = (int)((date.Date.Ticks - firstVisible.Date.Ticks) / TimeSpan.TicksPerDay);
			if (n < 0 || n >= Rows * Columns)
			{
				row = column = -1;
				return false;
			}

			row = n / Columns;
			column = n % Columns;
			return true;
		}


		public static bool IsInRange(DateTime date, DateTime min, DateTime max)
		{
			return date.Date >= min.Date && date.Date <= max.Date;
		}


		public static bool IsMonthInRange(int year, int month, DateTime min, DateTime max)
		{
			if (year < 1 || year > 9999)
			{
				return false;
			}

			var first = new DateTime(year, month, 1);
			var last = new DateTime(year, month, DateTime.DaysInMonth(year, month));
			return last >= min.Date && first <= max.Date;
		}


		public static bool IsYearInRange(int year, DateTime min, DateTime max)
		{
			if (year < 1 || year > 9999)
			{
				return false;
			}

			return year >= min.Year && year <= max.Year;
		}


		/// <summary>
		/// Determines if any year of the decade starting at the given year is in range.
		/// </summary>
		public static bool IsDecadeInRange(int startYear, DateTime min, DateTime max)
		{
			return startYear + 9 >= Math.Max(1, min.Year) &&
				startYear <= Math.Min(9999, max.Year);
		}


		/// <summary>
		/// Gets the first year of a 12-year page; one year before the decade of the given
		/// year, then the ten years of the decade, then the first year of the next decade.
		/// </summary>
		public static int YearPageStart(int year)
		{
			return year - (year % 10) - 1;
		}


		/// <summary>
		/// Gets the first decade-start year of a 12-decade page; one decade before the
		/// century of the given year, then the ten decades of the century, then the first
		/// decade of the next century.
		/// </summary>
		public static int DecadePageStart(int year)
		{
			return year - (year % 100) - 10;
		}


		/// <summary>
		/// Gets the week-of-year number for the date, using the culture's week rule.
		/// </summary>
		public static int WeekNumber(DateTime date, CultureInfo culture, DayOfWeek firstDay)
		{
			return culture.Calendar.GetWeekOfYear(
				date, culture.DateTimeFormat.CalendarWeekRule, firstDay);
		}
	}
}
