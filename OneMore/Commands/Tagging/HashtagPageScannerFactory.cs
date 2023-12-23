﻿//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Fabricates a new page scanner. This optimizes the construction of a shared
	/// compiled regular expression that is used for pattern matching across all pages.
	/// </summary>
	internal class HashtagPageSannerFactory
	{
		private readonly Regex hashPattern;


		/// <summary>
		/// Initialize a new factory and compiles the pattern match expression
		/// </summary>
		public HashtagPageSannerFactory()
		{
			// TODO: right-to-left languages?

			hashPattern = new Regex(@"##[\w\d-_]+\b");
		}


		/// <summary>
		/// Create a new page scanner
		/// </summary>
		/// <param name="root">The root element of the page</param>
		/// <returns>A HashtagPageScanner instance</returns>
		public HashtagPageScanner CreatePageScanner(XElement root)
		{
			return new HashtagPageScanner(root, hashPattern);
		}
	}

}
