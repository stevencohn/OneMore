//************************************************************************************************
// Copyright © 2015 Waters Corporation. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
    using System.Text.RegularExpressions;


    /// <summary>
    /// Some extension methods for .NET types...
    /// </summary>

    internal static class Extensions
	{

		/// <summary>
		/// Compares this string with a given string ignoring case.
		/// </summary>
		/// <param name="s">The current string</param>
		/// <param name="value">The comparison string</param>
		/// <returns>True if both strings are equal</returns>

		public static bool EqualsICIC (this string s, string value)
		{
			return s.Equals(value, StringComparison.InvariantCultureIgnoreCase);
		}


		/// <summary>
		/// Determines if the given string starts with a given substring, ignoring case.
		/// </summary>
		/// <param name="s">The current string</param>
		/// <param name="value">The substring to use as a search</param>
		/// <returns>True if the current start starts with the given substring</returns>

		public static bool StartsWithICIC (this string s, string value)
		{
			return s.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
		}


		/// <summary>
		/// Extract the first word delimeted by a non-word boundary from the given
		/// string and returns the word and updated string.
		/// </summary>
		/// <param name="s">A string with one or more words</param>
		/// <returns>
		/// A two-part ValueTuple with the word and the updated string.
		/// </returns>
		public static (string, string) ExtractFirstWord(this string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				var match = Regex.Match(s, @"^(\w+)\b*");
				if (match.Success)
				{
					var capture = match.Captures[0];
					return (capture.Value, s.Remove(capture.Index, capture.Length));
				}
			}

			return (null, s);
		}


		/// <summary>
		/// Extract the last word delimeted by a non-word boundary from the given
		/// string and returns the word and updated string.
		/// </summary>
		/// <param name="s">A string with one or more words</param>
		/// <returns>
		/// A two-part ValueTuple with the word and the updated string.
		/// </returns>
		public static (string, string) ExtractLastWord (this string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				var match = Regex.Match(s, @"\b*(\w+)$");
				if (match.Success)
				{
					var capture = match.Captures[0];
					return (capture.Value, s.Remove(capture.Index, capture.Length));
				}
			}

			return (null, s);
		}
	}
}
