//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal static class XCDataExtensions
	{

		/// <summary>
		/// Determines if the node contains multiple words, separated by any HTML space character
		/// </summary>
		/// <param name="cdata">The instance</param>
		/// <returns>True if the value contains multiple words, false otherwise</returns>
		public static bool ContainsMultipleWords(this XCData cdata)
		{
			// \s includes space, tab, CR, NL, form feed, vertical tab, and \u00A0
			return Regex.IsMatch(cdata.Value, @"\w+([\s]|&#160;|&nbsp;)\w+");
		}


		/// <summary>
		/// Determines if the node is empty. This usually indicates the position of the
		/// text cursor but could also be randomly in the XML as well.
		/// </summary>
		/// <param name="cdata">The instance</param>
		/// <returns>True if the value is empty, false otherwise.</returns>
		public static bool IsEmpty(this XCData cdata)
		{
			return cdata.Value.Length == 0;
		}


		/// <summary>
		/// Creates an XElement from the given cdata node.
		/// </summary>
		/// <param name="cdata">The instance</param>
		/// <returns>A new XElement with the name "cdata"</returns>
		/// <remarks>
		/// Since CData only has a Value, we need a wrapper to convert it to proper XML
		/// that we can then parse and modify.
		/// </remarks>
		public static XElement GetWrapper(this XCData cdata)
		{
			// ensure proper XML

			// OneNote doesn't like &nbsp; but &#160; is ok and is the same as \u00A0 but 1-byte
			var value = cdata.Value.Replace("&nbsp;", "&#160;");

			// XElement doesn't like <br> so replace with <br/>
			value = Regex.Replace(value, @"\<\s*br\s*\>", "<br/>");

			// quote unquoted language attribute, e.g., lang=yo to lang="yo" (or two part en-US)
			value = Regex.Replace(value, @"(\s)lang=([\w\-]+)([\s/>])", "$1lang=\"$2\"$3");

			try
			{
				return XElement.Parse("<cdata>" + value + "</cdata>", LoadOptions.PreserveWhitespace);
			}
			catch
			{
				Logger.Current.WriteLine($"error wrapping /{value}/");
				throw;
			}
		}


		public static bool EndsWithWhitespace(this XCData cdata)
		{
			// \s includes space, tab, CR, NL, FF, VT, and \u00A0
			return (cdata == null) || Regex.IsMatch(cdata.Value, @"([\s]|&#160;|&nbsp;)(</span>)?$");
		}


		public static bool StartsWithWhitespace(this XCData cdata)
		{
			// \s includes space, tab, CR, NL, FF, VT, and \u00A0
			return (cdata == null) || Regex.IsMatch(cdata.Value, @"^(<span>)?([\s]|&#160;|&nbsp;)");
		}
	}
}
