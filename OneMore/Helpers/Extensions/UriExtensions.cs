//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;


	internal static class UriExtensions
	{

		/// <summary>
		/// Determines if this Uri is the same logical page as the given Uri
		/// </summary>
		/// <param name="uri">The current Uri instance</param>
		/// <param name="other">The Uri to compare</param>
		/// <returns>True if both are logically equivalent</returns>
		public static bool SamePage(this Uri uri, Uri other)
		{
			return Uri.Compare(
				NormalizeUriPath(uri),
				NormalizeUriPath(other),
				// losely compare, ignoring scheme
				UriComponents.HostAndPort | UriComponents.Path,
				UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0;
		}


		private static Uri NormalizeUriPath(Uri uri)
		{
			var last = uri.Segments[uri.Segments.Length - 1];
			if (last.Contains("index.htm") || last.Contains("home.htm"))
			{
				var builder = new UriBuilder(uri);
				builder.Path = string.Concat(uri.Segments.Take(uri.Segments.Length - 1));
				return builder.Uri;
			}

			return uri;
		}
	}
}
