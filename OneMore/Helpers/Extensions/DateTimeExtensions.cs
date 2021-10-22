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
	}
}
