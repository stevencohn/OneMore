//************************************************************************************************
// Copyright © 2015 Waters Corporation. All rights reserved.
//************************************************************************************************

#pragma warning disable S1116 // Empty statements should be removed

namespace River.OneMoreAddIn
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Some extension methods for .NET types...
	/// </summary>

	internal static class Extensions
	{

		/// <summary>
		/// Determines if the string ends with whitespace, either chars or HTML escapes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool EndsWithWhitespace(this string value)
		{
			// \s includes space, tab, CR, NL, FF, VT, and \u00A0
			return Regex.IsMatch(value, @"(\W|&#160;|&nbsp;)$");
		}


		/// <summary>
		/// Determines if the string starts with whitespace, either chars or HTML escapes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool StartsWithWhitespace(this string value)
		{
			// \s includes space, tab, CR, NL, FF, VT, and \u00A0
			return Regex.IsMatch(value, @"^(\W|&#160;|&nbsp;)");
		}


		/// <summary>
		/// Compares this string with a given string ignoring case.
		/// </summary>
		/// <param name="s">The current string</param>
		/// <param name="value">The comparison string</param>
		/// <returns>True if both strings are equal</returns>

		public static bool EqualsICIC(this string s, string value)
		{
			return s.Equals(value, StringComparison.InvariantCultureIgnoreCase);
		}


		/// <summary>
		/// Escapes only a select few special character in a URL. Needed for the
		/// Copy Link to Page command so the pasted link can be clicked and properly 
		/// navigate back to the source page.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string SafeUrlEncode(this string s)
		{
			// all normally escaped chars --> " $&`:<>[]{}\"+#%@/;=?\\^|~',"

			var builder = new StringBuilder();
			for (var i = 0; i < s.Length; i++)
			{
				if (s[i] == ' ' || s[i] == '"' || s[i] == '\'')
				{
					builder.Append($"%{((int)s[i]):X2}");
				}
				else
				{
					builder.Append(s[i]);
				}
			}

			return builder.ToString();
		}


		/// <summary>
		/// Determines if the given string starts with a given substring, ignoring case.
		/// </summary>
		/// <param name="s">The current string</param>
		/// <param name="value">The substring to use as a search</param>
		/// <returns>True if the current start starts with the given substring</returns>

		public static bool StartsWithICIC(this string s, string value)
		{
			return s.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static int SimilarityTo(this string s, string t)
		{
			if (String.IsNullOrEmpty(s))
			{
				if (String.IsNullOrEmpty(t))
				{
					return 0;
				}

				return t.Length;
			}

			if (string.IsNullOrEmpty(t))
			{
				return s.Length;
			}

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...

			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 1; j <= m; d[0, j] = j++) ;

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					int min1 = d[i - 1, j] + 1;
					int min2 = d[i, j - 1] + 1;
					int min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}


		/// <summary>
		/// Splits a string into its first word and the remaining characters as delimited by
		/// the first non-word boundary.
		/// </summary>
		/// <param name="s">A string with one or more words</param>
		/// <returns>
		/// A two-part ValueTuple with the first word and the remaining string. If the given
		/// string does not contain a word boundary then this returns (null,s)
		/// </returns>
		public static (string, string) SplitAtFirstWord(this string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				// capture first word preceding whitespace char or esc sequence
				var match = Regex.Match(s, @"^(?<word>\w+)\b?"); // (?:[\W\s\n]|&#160;|&nbsp;)?", RegexOptions.Singleline);
				if (match.Success)
				{
					var capture = match.Groups["word"];
					if (capture.Length > 0)
					{
						return (capture.Value, s.Remove(capture.Index, capture.Length));
					}
				}
			}

			return (null, s);
		}


		/// <summary>
		/// Splits a string into its last word and the remaining characters as delimited by
		/// the last non-word boundary.
		/// </summary>
		/// <param name="s">A string with one or more words</param>
		/// <returns>
		/// A two-part ValueTuple with the last word and the remaining string. If the given
		/// string does not contain a word boundary then this returns (null,s)
		/// </returns>
		public static (string, string) SplitAtLastWord(this string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				// use greedy match, skip text and last whitespace to capture last word
				var match = Regex.Match(s, @"\b?(?<word>\w+)$"); // @"(?:[\W\s\n]|&#160;|&nbsp;)?(?<word>\w+)$", RegexOptions.Singleline);
				if (match.Success)
				{
					var capture = match.Groups["word"];
					if (capture.Length > 0)
					{
						return (capture.Value, s.Remove(capture.Index, capture.Length));
					}
				}
			}

			return (null, s);
		}


		/// <summary>
		/// Build an XML wrapper with the specified content, ensuring the content
		/// is propertly formed XML
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static XElement ToXmlWrapper(this string s)
		{
			// ensure proper XML

			// OneNote doesn't like &nbsp; inside CDATAs but &#160; is OK
			// and is the same as \u00A0 but 1-byte
			var value = s.Replace("&nbsp;", "&#160;");

			// XElement doesn't like <br> so replace with <br/>
			value = Regex.Replace(value, @"\<\s*br\s*\>", "<br/>");

			// quote unquote language attribute, e.g., lang=yo to lang="yo" (or two part en-US)
			value = Regex.Replace(value, @"(\s)lang=([\w\-]+)([\s/>])", "$1lang=\"$2\"$3");

			return XElement.Parse($"<wrapper>{value}</wrapper>");
		}


		// StringBuilder...

		public static int IndexOf(this StringBuilder s, char c)
		{
			int i = 0;
			while (i < s.Length)
			{
				if (s[i] == c)
				{
					return i;
				}

				i++;
			}

			return -1;
		}
	}
}
