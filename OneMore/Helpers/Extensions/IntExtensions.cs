//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

using System;

namespace River.OneMoreAddIn
{

	// copied verbatim unashamedly from
	// http://csharphelper.com/blog/2016/04/convert-to-and-from-roman-numerals-in-c/

	internal static class IntExtensions
	{
		// Map digits to letters.
		private static readonly string[] ThouLetters = { "", "M", "MM", "MMM" };

		private static readonly string[] HundLetters =
			{ "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };

		private static readonly string[] TensLetters =
			{ "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };

		private static readonly string[] OnesLetters =
			{ "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

		// byte size names (only handles up to TB)
		private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB" };


		// convert int to alphabet string
		public static string ToAlphabetic(this int value)
		{
			string result = string.Empty;
			while (--value >= 0)
			{
				result = (char)('A' + value % 26) + result;
				value /= 26;
			}

			return result;
		}


		/// <summary>
		/// OneMore Extension >> Format the given value as a string describing the amount of
		/// bytes, adjusted for bytes, Kilobytes, Megabytes, Gigabytes, etc.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="decimalPlaces"></param>
		/// <returns></returns>
		public static string ToBytes(this int value, int decimalPlaces = 0)
		{
			return ((long)value).ToBytes(decimalPlaces);
		}


		/// <summary>
		/// OneMore Extension >> Format the given value as a string describing the amount of
		/// bytes, adjusted for bytes, Kilobytes, Megabytes, Gigabytes, etc.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="decimalPlaces"></param>
		/// <returns></returns>
		public static string ToBytes(this long value, int decimalPlaces = 0)
		{
			var neg = value < 0;
			if (neg) value = -value;

			var mag = (int)Math.Max(0, Math.Log(value, 1024));
			var adjusted = Math.Round(value / Math.Pow(1024, mag), decimalPlaces);

			if (neg) adjusted = -adjusted;

			return string.Format("{0} {1}", adjusted, SizeSuffixes[mag]);
		}


		public static string ToBytes(this ulong value, int decimalPlaces = 0)
		{
			var mag = (int)Math.Max(0, Math.Log(value, 1024));
			var adjusted = Math.Round(value / Math.Pow(1024, mag), decimalPlaces);

			return string.Format("{0} {1}", adjusted, SizeSuffixes[mag]);
		}


		// OneMore Extension >> convert int to roman numerals
		public static string ToRoman(this int value)
		{
			if (value >= 4000)
			{
				// use parentheses
				int thou = value / 1000;
				value %= 1000;
				return "(" + ToRoman(thou) + ")" + ToRoman(value);
			}

			// otherwise process the letters
			string result = "";

			// pull out thousands
			int num;
			num = value / 1000;
			result += ThouLetters[num];
			value %= 1000;

			// handle hundreds
			num = value / 100;
			result += HundLetters[num];
			value %= 100;

			// handle tens
			num = value / 10;
			result += TensLetters[num];
			value %= 10;

			// handle ones
			result += OnesLetters[value];

			return result.ToLower();
		}
	}
}
