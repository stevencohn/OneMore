//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Globalization;


	internal static class DateTimeExtensions
	{
		private static readonly string friendlyPattern =
			CultureInfo.CurrentCulture.DateTimeFormat
				.FullDateTimePattern.Replace(":ss", string.Empty);

		private const string ShortFriendlyPattern = "MMM d, yyyy h:mm tt";


		/// <summary>
		/// Gets the pattern to express a friendly full date time string.
		/// Used to set custom formatting in DateTimePicker
		/// </summary>
		public static string FriendlyPattern => friendlyPattern;


		/// <summary>
		/// Gets a friendly full date time string in local time
		/// similar to "Friday, October 22, 2021 11:20 AM"
		/// </summary>
		/// <param name="dttm">This DateTime to convert</param>
		/// <returns>A formatted string</returns>
		public static string ToFriendlyString(this DateTime dttm)
		{
			return dttm.ToLocalTime().ToString(friendlyPattern);
		}


		/// <summary>
		/// Gets a friendly date time string in local time similar to "October 22, 2021 11:20 AM"
		/// </summary>
		/// <param name="dttm">This DateTime to convert</param>
		/// <returns>A formatted string</returns>
		public static string ToShortFriendlyString(this DateTime dttm)
		{
			return dttm.ToLocalTime().ToString(ShortFriendlyPattern);
		}


		/// <summary>
		/// Gets a string formatted to universal Zulu time appropriate for XML
		/// </summary>
		/// <param name="dttm"></param>
		/// <returns></returns>
		public static string ToZuluString(this DateTime dttm)
		{
			return dttm.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffff'Z'");
		}
	}
}
