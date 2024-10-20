//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal static class XCDataExtensions
	{
		// https://www.fileformat.info/info/unicode/char/fffd/index.htm
		private const string UnicodeReplacementChar = "&#65533;";


		/// <summary>
		/// OneMore Extension >> Determines if the node contains multiple words, separated by any
		/// HTML space character
		/// </summary>
		/// <param name="cdata">The instance</param>
		/// <returns>True if the value contains multiple words, false otherwise</returns>
		public static bool ContainsMultipleWords(this XCData cdata)
		{
			// \s includes space, tab, CR, NL, form feed, vertical tab, and \u00A0
			return Regex.IsMatch(cdata.Value, @"\w+([\s]|&#160;|&nbsp;)\w+");
		}


		/// <summary>
		/// OneMore Extension >> Determines if the node is empty. This usually indicates the
		/// position of the text cursor but could also be randomly in the XML as well.
		/// </summary>
		/// <param name="cdata">The instance</param>
		/// <returns>True if the value is empty, false otherwise.</returns>
		public static bool IsEmpty(this XCData cdata)
		{
			return cdata.Value.Length == 0;
		}


		/// <summary>
		/// OneMore Extension >> Creates an XElement from the given cdata node.
		/// </summary>
		/// <param name="cdata">The instance</param>
		/// <returns>A new XElement with the name "cdata"</returns>
		/// <remarks>
		/// Since CData only has a Value, we need a wrapper to convert it to proper XML
		/// that we can then parse and modify.
		/// </remarks>
		public static XElement GetWrapper(this XCData cdata)
		{
			return cdata.Value.ToXmlWrapper("cdata");
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
