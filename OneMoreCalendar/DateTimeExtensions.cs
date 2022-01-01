//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;


	internal static class DateTimeExtensions
	{

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
	}
}
