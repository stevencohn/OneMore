//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Identity
{
	using System.Text.RegularExpressions;


	/// <summary>
	/// Reads the GUIDs inside a OneNote hyperlink, such as
	/// onenote:https://...Notes.one#Title&amp;section-id={GUID}&amp;page-id={GUID}&amp;end
	/// </summary>
	/// <remarks>
	/// These GUIDs are not the IDs of the hierarchy, such as {GUID}{1}{E1951...}, and there is
	/// no mapping from one to the other. The page GUID has survived every change measured so far:
	/// a notebook reopen, a restart and moving the page to another section, which changes only
	/// the section GUID. The only way to learn the GUID of a page is to ask OneNote for a
	/// hyperlink to it. It is not unique: copies of a page can share it.
	/// </remarks>
	internal static class LinkGuids
	{
		private static readonly Regex PagePattern =
			new(@"[#&]page-id=(\{[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\})");

		private static readonly Regex SectionPattern =
			new(@"[#&]section-id=(\{[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\})");


		/// <summary>
		/// Gets the page GUID of a hyperlink, in upper case with braces, or null if the link
		/// has none, such as a link to a section or a notebook.
		/// </summary>
		public static string PageGuid(string uri)
		{
			return Find(PagePattern, uri);
		}


		/// <summary>
		/// Gets the section GUID of a hyperlink, in upper case with braces, or null.
		/// </summary>
		public static string SectionGuid(string uri)
		{
			return Find(SectionPattern, uri);
		}


		private static string Find(Regex pattern, string uri)
		{
			if (string.IsNullOrEmpty(uri))
			{
				return null;
			}

			var match = pattern.Match(uri);
			return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
		}
	}
}
