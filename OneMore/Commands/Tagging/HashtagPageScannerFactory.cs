//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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
		public HashtagPageSannerFactory(XElement styleTemplate)
		{
			// TODO: right-to-left languages?
			// Groups[1].Index, Length, Value
			// matches ##digits or ##word or #word, but not #digits
			hashPattern = new Regex(@"(##\d[\w-_]+|#{1,2}[^\W\d][\w-_]{0,})");

			this.styleTemplate = styleTemplate;
		}


		/// <summary>
		/// Create a new page scanner
		/// </summary>
		/// <param name="root">The root element of the page</param>
		/// <returns>A HashtagPageScanner instance</returns>
		public HashtagPageScanner CreatePageScanner(XElement root)
		{
			return new HashtagPageScanner(root, hashPattern, styleTemplate);
		}
	}
}
