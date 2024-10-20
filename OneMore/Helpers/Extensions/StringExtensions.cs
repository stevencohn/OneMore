//************************************************************************************************
// Copyright © 2015 Waters Corporation. All rights reserved.
//************************************************************************************************

#pragma warning disable S1116 // Empty statements should be removed

namespace River.OneMoreAddIn
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml.Linq;


	/// <summary>
	/// Some extension methods for .NET types...
	/// </summary>

	internal static class Extensions
	{

		/// <summary>
		/// OneMore Extension >> Equivalent to matching with \w pattern.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="extras">Extra char to accept as word characters</param>
		/// <returns></returns>
		public static bool IsWordCharacter(this char c, params char[] extras)
		{
			// Get the Unicode category of the character
			var category = CharUnicodeInfo.GetUnicodeCategory(c);

			// Return true if the category is one of the word categories
			return category == UnicodeCategory.LowercaseLetter ||
				   category == UnicodeCategory.UppercaseLetter ||
				   category == UnicodeCategory.TitlecaseLetter ||
				   category == UnicodeCategory.OtherLetter ||
				   category == UnicodeCategory.ModifierLetter ||
				   category == UnicodeCategory.DecimalDigitNumber ||
				   category == UnicodeCategory.ConnectorPunctuation ||
				   (extras.Any() && extras.Contains(c));
		}


		/// <summary>
		/// OneMore Extension >> Determines if the given value is contained within the string,
		/// ignoring case
		/// </summary>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool ContainsICIC(this string s, string value)
		{
			return s.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0;
		}


		/// <summary>
		/// OneMore Extension >> Determines if the string ends with whitespace, either chars or
		/// HTML escapes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool EndsWithWhitespace(this string value)
		{
			// \s includes space, tab, CR, NL, FF, VT, and \u00A0
			return Regex.IsMatch(value, @"(\W|&#160;|&nbsp;)$");
		}


		/// <summary>
		/// OneMore Extension >> Determines if the string starts with whitespace, either chars
		/// or HTML escapes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool StartsWithWhitespace(this string value)
		{
			// \s includes space, tab, CR, NL, FF, VT, and \u00A0
			return Regex.IsMatch(value, @"^(\W|&#160;|&nbsp;)");
		}


		/// <summary>
		/// Escape all control chars in given string. Typically used to escape the search
		/// string when not using regular expressions
		/// </summary>
		/// <param name="plain">The string to treat as plain text</param>
		/// <returns>A string in which all regular expression control chars are escaped</returns>
		public static string EscapeForRegex(this string plain)
		{
			var codes = new char[] { '\\', '.', '*', '|', '?', '(', ')', '[', '$', '^', '+' };

			var builder = new StringBuilder();
			for (var i = 0; i < plain.Length; i++)
			{
				if (codes.Contains(plain[i]))
				{
					if (i == 0 || plain[i - 1] != '\\')
					{
						builder.Append('\\');
					}
				}

				builder.Append(plain[i]);
			}

			return builder.ToString();
		}


		/// <summary>
		/// OneMore Extension >> Compares this string with a given string ignoring case.
		/// </summary>
		/// <param name="s">The current string</param>
		/// <param name="value">The comparison string</param>
		/// <returns>True if both strings are equal</returns>

		public static bool EqualsICIC(this string s, string value)
		{
			return s.Equals(value, StringComparison.InvariantCultureIgnoreCase);
		}


		/// <summary>
		/// Determines if the current string is one of a given set of values
		/// </summary>
		/// <param name="item"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool In(this string item, params string[] items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			return items.Contains(item);
		}


		/// <summary>
		/// Strip all HTML from the given string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string PlainText(this string s)
		{
			// remove all HTML
			var text = Regex.Replace(s, @"\<\s*br\s*\>", "\n");
			text = Regex.Replace(text, @"\<[^>]+>", string.Empty);
			text = HttpUtility.HtmlDecode(text);

			// ligatures
			text = Regex.Replace(text, "…", "...");

			return text;
		}


		/// <summary>
		/// OneMore Extension >> Escapes only a select few special character in a URL. Needed
		/// for the Copy Link to Page command so the pasted link can be clicked and properly 
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
		/// OneMore Extension >> Determines if the given string starts with a given substring,
		/// ignoring case.
		/// </summary>
		/// <param name="s">The current string</param>
		/// <param name="value">The substring to use as a search</param>
		/// <returns>True if the current start starts with the given substring</returns>

		public static bool StartsWithICIC(this string s, string value)
		{
			return s.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
		}


		/// <summary>
		/// OneMore Extension >> 
		/// </summary>
		/// <param name="s"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static int DistanceFrom(this string s, string t)
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

			if (s.Equals(t))
			{
				return 0;
			}

			// vectors
			int[] v0 = new int[t.Length + 1];
			int[] v1 = new int[t.Length + 1];
			int[] vtemp;

			// initialize v0 (the previous row of distances)
			// this row is A[0][i]: edit distance for an empty s
			// the distance is just the number of characters to delete from t
			for (int i = 0; i < v0.Length; i++)
			{
				v0[i] = i;
			}

			for (int i = 0; i < s.Length; i++)
			{
				// calculate v1 (current row distances) from the previous row v0
				// first element of v1 is A[i+1][0]
				//   edit distance is delete (i+1) chars from s to match empty t
				v1[0] = i + 1;

				// use formula to fill in the rest of the row
				for (int j = 0; j < t.Length; j++)
				{
					int cost = 1;
					if (s[i] == t[j])
					{
						cost = 0;
					}
					v1[j + 1] = Math.Min(
							v1[j] + 1,              // Cost of insertion
							Math.Min(
									v0[j + 1] + 1,  // Cost of remove
									v0[j] + cost)); // Cost of substitution
				}

				// copy v1 (current row) to v0 (previous row) for next iteration
				//System.arraycopy(v1, 0, v0, 0, v0.length);

				// Flip references to current and previous row
				vtemp = v0;
				v0 = v1;
				v1 = vtemp;
			}

			return v0[t.Length];
		}


		/// <summary>
		/// OneMore Extension >> Splits a string into its first word and the remaining characters
		/// as delimited by the first non-word boundary.
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
		/// OneMore Extension >> Splits a string into its last word and the remaining characters
		/// as delimited by the last non-word boundary.
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
		/// OneMore Extension >> Build an XML wrapper with the specified content, ensuring the
		/// content is propertly formed XML
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static XElement ToXmlWrapper(this string s, string name = "wrapper")
		{
			// OneNote doesn't like &nbsp; inside CDATAs but &#160; is OK
			// and is the same as \u00A0 but 1-byte
			var value = s.Replace("&nbsp;", "&#160;");

			// XElement doesn't like <br> so replace with <br/>
			value = Regex.Replace(value, @"\<\s*br\s*\>", "<br/>");

			// quote unquote language attribute, e.g., lang=yo to lang="yo" (or two part en-US)
			value = Regex.Replace(value, @"(\s)lang=([\w\-]+)([\s/>])", "$1lang=\"$2\"$3");

			// escape &
			//value = System.Security.SecurityElement.Escape(value);

			try
			{
				return XElement.Parse($"<{name}>{value}</{name}>", LoadOptions.PreserveWhitespace);
			}
			catch
			{
				Logger.Current.WriteLine($"error wrapping /{value}/");
				throw;
			}
		}


		// StringBuilder...

		public static int IndexOf(this StringBuilder s, char c, int startAt = 0)
		{
			if (startAt >= 0 && startAt < s.Length)
			{
				int i = startAt;
				while (i < s.Length)
				{
					if (s[i] == c)
					{
						return i;
					}

					i++;
				}
			}

			return -1;
		}
	}
}
