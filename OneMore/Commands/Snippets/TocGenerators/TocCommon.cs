//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

namespace River.OneMoreAddIn.Commands.Snippets.TocGenerators
{
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
