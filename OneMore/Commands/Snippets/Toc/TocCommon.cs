//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

namespace River.OneMoreAddIn.Commands.Snippets.Toc
{
	using System.Linq;
	using System.Xml.Linq;


	internal static class Toc
	{

		/// <summary>
		/// Name of one:Meta element marking the one:Table describinging the TOC
		/// </summary>
		public const string MetaName = "omToc";


		/// <summary>
		/// Styles for TOC refresh link
		/// </summary>
		public const string RefreshStyle =
			"font-weigth:normal;font-style:italic;font-size:9.0pt;color:#808080";


		/// <summary>
		/// The root URI of TOC refresh URLs embedded on the page
		/// </summary>
		public const string RefreshUri = "onemore://InsertTocCommand/";


		/// <summary>
		/// Removes the embedded "[Refresh]" link, if present, from a TOC title/heading's
		/// HTML text, leaving the rest of the markup intact
		/// </summary>
		/// <param name="html">The raw title text, possibly containing the refresh span</param>
		/// <returns>The title text with the refresh span removed</returns>
		public static string StripRefreshLink(string html)
		{
			var wrapper = new XCData(html).GetWrapper();

			var link = wrapper.Descendants("span")
				.FirstOrDefault(s => s.Descendants("a")
					.Any(a => a.Attribute("href")?.Value.StartsWith(RefreshUri) == true));

			link?.Remove();

			return string.Concat(wrapper.Nodes().Select(n => n.ToString(SaveOptions.DisableFormatting)));
		}
	}


	/// <summary>
	/// User selected option when there is a hierarchy page to refresh
	/// </summary>
	internal enum RefreshOption
	{
		Cancel,
		Build,
		Refresh
	}
}
