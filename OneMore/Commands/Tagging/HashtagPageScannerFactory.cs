//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Models;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Initialize a new page scanner. This centralizes and optimizes the instantiation
	/// of shared objects such as a compiled regular expression and style template.
	/// </summary>
	internal class HashtagPageSannerFactory
	{
		private readonly Regex hashPattern;
		private readonly XElement styleTemplate;


		/// <summary>
		/// Initialize a new factory and compiles the pattern match expression
		/// </summary>
		public HashtagPageSannerFactory(XElement styleTemplate, bool unfiltered)
		{
			// TODO: right-to-left languages?
			// Groups[1].Index, Length, Value
			// matches ##digits or ##word or #word, but not #digits
			hashPattern = new Regex(unfiltered
				? @"(?:^|[\s\[\({,])(##\d[\w\-_]{0,}|#{1,2}[^\W\d][\w\-_]{0,})"
				: @"(?:^|[\s\[\({,])(##\d[\w\-_]{0,}|(?!#(?:[A-Fa-f0-9]{6}|define|else|endif|endregion|error|include|if|ifdef|ifndef|line|pragma|region|undef)(?:\s|$|\)|\]|}|[^\w\d\-_]))#{1,2}[^\W\d][\w\-_]{0,})"
				);

			this.styleTemplate = styleTemplate;
		}


		/// <summary>
		/// Create a new page scanner
		/// </summary>
		/// <param name="root">The root element of the page</param>
		/// <returns>A HashtagPageScanner instance</returns>
		public HashtagPageScanner CreatePageScanner(Page page)
		{
			return new HashtagPageScanner(page, hashPattern, styleTemplate);
		}
	}
}
