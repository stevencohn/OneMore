//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Globalization;


	internal static class DateTimeHelper
	{
		public const int TicksInOneSecond = 10000000;


		/// <summary>
		/// Converts tick-seconds to a new DateTime.
		/// </summary>
		/// <param name="ticks">Number of seconds since 1 Jan 0001</param>
		/// <returns></returns>
		public static DateTime FromTicksSeconds(long ticks)
		{
			return new DateTime(ticks * TicksInOneSecond, DateTimeKind.Unspecified);
		}
	}


	internal static class DateTimeExtensions
	{
		private static readonly string friendlyPattern =
			CultureInfo.CurrentCulture.DateTimeFormat
				.FullDateTimePattern.Replace(":ss", string.Empty);

		private const string ShortFriendlyPattern = "MMM d, yyyy h:mm tt";


		/// <summary>
		/// OneMore Extension >> Gets the pattern to express a friendly full date time string.
		/// Used to set custom formatting in DateTimePicker
		/// </summary>
		public static string FriendlyPattern => friendlyPattern;


		/// <summary>
		/// OneMore Extension >> Gets the number of seconds since 1-Jan-0001
		/// </summary>
		/// <param name="dttm"></param>
		/// <returns></returns>
		public static long GetTickSeconds(this DateTime dttm)
		{
			return dttm.Ticks / DateTimeHelper.TicksInOneSecond;
		}


		/// <summary>
		/// OneMore Extension >> Gets a friendly full date time string in local time
		/// similar to "Friday, October 22, 2021 11:20 AM"
		/// </summary>
		/// <param name="dttm">This DateTime to convert</param>
		/// <returns>A formatted string</returns>
		public static string ToFriendlyString(this DateTime dttm)
		{
			return dttm.Kind == DateTimeKind.Utc
				? dttm.ToLocalTime().ToString(friendlyPattern)
				: dttm.ToString(FriendlyPattern);
		}


		/// <summary>
		/// OneMore Extension >>  Gets a friendly date time string in local time similar
		/// to "October 22, 2021 11:20 AM"
		/// </summary>
		/// <param name="dttm">This DateTime to convert</param>
		/// <returns>A formatted string</returns>
		public static string ToShortFriendlyString(this DateTime dttm)
		{
			return dttm.Kind == DateTimeKind.Utc
				? dttm.ToLocalTime().ToString(ShortFriendlyPattern)
				: dttm.ToString(ShortFriendlyPattern);
		}


		/// <summary>
		/// OneMore Extension >> Gets a string formatted to universal Zulu time appropriate for XML
		/// </summary>
		/// <param name="dttm"></param>
		/// <returns></returns>
		public static string ToZuluString(this DateTime dttm)
		{
			return dttm.Kind == DateTimeKind.Utc
				? dttm.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffff'Z'")
				: dttm.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffff'Z'");
		}
	}
}
