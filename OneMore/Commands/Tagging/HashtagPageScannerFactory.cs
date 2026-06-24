//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Models;
	using System.Collections.Concurrent;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Initialize a new page scanner. This centralizes and optimizes the instantiation
	/// of shared objects such as a compiled regular expression and style template.
	/// </summary>
	internal class HashtagPageSannerFactory
	{
		// RegexOptions.Compiled JITs via Reflection.Emit into a dynamic assembly that is
		// never unloaded on .NET Framework, so the compiled pattern is cached for the life
		// of the process instead of being recompiled on every scan (every 2 minutes)
		private static readonly ConcurrentDictionary<(bool Unfiltered, bool Doubled), Regex>
			patternCache = new();

		private readonly Regex hashPattern;
		private readonly XElement styleTemplate;


		/// <summary>
		/// Initialize a new factory and compiles the pattern match expression
		/// </summary>
		public HashtagPageSannerFactory(XElement styleTemplate, bool unfiltered, bool doubled)
		{
			hashPattern = patternCache.GetOrAdd((unfiltered, doubled), key => BuildPattern(key.Unfiltered, key.Doubled));
			this.styleTemplate = styleTemplate;
		}


		private static Regex BuildPattern(bool unfiltered, bool doubled)
		{
			// these force only ##hashtags and exclude all #hashtags
			var pounds = doubled ? "##" : "#";
			var count = doubled ? "2,2" : "1,2";

			// TODO: right-to-left languages?
			// Groups[1].Index, Length, Value
			// matches ##digits or ##word or #word, but not #digits
			return new Regex(unfiltered
				? @$"(?:^|[\s\[\({{,])(##\d[\w\-_]{{0,}}|{pounds}{{1,2}}[^\W\d][\w\-_]{{0,}})"
				: @$"(?:^|[\s\[\({{,])(##\d[\w\-_]{{0,}}|(?!#(?:[A-Fa-f0-9]{{6}}|define|else|endif|endregion|error|include|if|ifdef|ifndef|line|pragma|region|undef)(?:\s|$|\)|\]|}}|[^\w\d\-_]))#{{{count}}}[^\W\d][\w\-_]{{0,}})",
				RegexOptions.Compiled | RegexOptions.CultureInvariant
				);
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
