//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Threading;

	internal static class DateTimeExtensions
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime EndOfCalendarView(this DateTime date)
		{
			return date.StartOfCalendarMonthView().AddDays(41);
		}


		/// <summary>
		/// Gets the date of the last day of the month of the given DateTime
		/// </summary>
		/// <param name="date">A date specifying any day of a month</param>
		/// <returns>A DateTime specifying the last day of the month</returns>
		public static DateTime EndOfMonth(this DateTime date)
		{
			return new DateTime(
				date.Year,
				date.Month,
				DateTime.DaysInMonth(date.Year, date.Month))
				.Date;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="date"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool EqualsMonth(this DateTime date, DateTime other)
		{
			return date.Year == other.Year && date.Month == other.Month;
		}


		/// <summary>
		/// Determines if the current date is within the specified start and end dates
		/// </summary>
		/// <param name="date">This instance</param>
		/// <param name="startDate">The start date, inclusive</param>
		/// <param name="endDate">The end date, inclusive</param>
		/// <returns>True if the current instances is within range</returns>
		/// <remarks>The time components are ignored; only the date is compared</remarks>
		public static bool InRange(this DateTime date, DateTime startDate, DateTime endDate)
		{
			return
				date.Date.CompareTo(startDate.Date) >= 0 &&
				date.Date.CompareTo(endDate.Date) <= 0;
		}


		/// <summary>
		/// Gets the date of the first day of the month of the given DateTime
		/// </summary>
		/// <param name="date">A date specifying any day of a month</param>
		/// <returns>A DateTime specifying the first day of the month</returns>
		public static DateTime StartOfMonth(this DateTime date)
		{
			return new DateTime(date.Year, date.Month, 1).Date;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime StartOfCalendarMonthView(this DateTime date)
		{
			var start = date.Date;
			var firstDay = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.FirstDayOfWeek;

			var day = firstDay == DayOfWeek.Sunday
				? (int)start.DayOfWeek
				: start.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)start.DayOfWeek - 1;

			return day == 0 ? start : start.AddDays(-day);
		}
	}
}
