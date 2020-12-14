//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Parses given source using the specified language and reports tokens and text runs
	/// through an Action.
	/// </summary>
	/// <remarks>
	/// Output from Parser is a stream of generic string/scope pairs. These can be formatted
	/// for different visualizations such as HTML, RTF, or OneNote. See the Colorizer class
	/// for a OneNote visualizer.
	/// </remarks>
	internal class Parser
	{
		private readonly ICompiledLanguage language;
		private MatchCollection matches;
		private int captureIndex;
		private string scopeOverride;


		public Parser(ICompiledLanguage language)
		{
			this.language = language;
		}


		/// <summary>
		/// Indicates whether there are more captures to come, not including the last line break;
		/// can be used from within reporters, specifically for ColorizeOne
		/// </summary>
		/// <remarks>
		/// Filters out the end of the line token, implicitly matched by ($) but allows explicit
		/// newline chars such as \n and \r
		/// </remarks>
		public bool HasMoreCaptures
			=> captureIndex < matches.Count - 1
			|| (captureIndex == matches.Count - 1 && matches[captureIndex].Value.Length > 0);


		/// <summary>
		/// Parse the given source code, invoking the specified reporter for each matched rule
		/// as a string/scope pair through the provided Action
		/// </summary>
		/// <param name="source">The source code to parse</param>
		/// <param name="report">
		/// An Action to invoke with the piece of source code and its scope name
		/// </param>
		public void Parse(string source, Action<string, string> report)
		{
			#region Note
			// Implementation note: Originally used Regex.Match and then iterated with NextMatch
			// but there was no way to support the HasMoreCaptures property so switched to using
			// Matches instead; slightly more complicated logic but the effect is the same.
			#endregion Note

			// collapse \r\n sequence to just \n to make parsing easier;
			// this sequence appears when using C# @"verbatim" multiline strings
			source = Regex.Replace(source, @"(?:\r\n)|(?:\n\r)", "\n");

			matches = language.Regex.Matches(source);

			if (matches.Count == 0)
			{
				//Logger.Current.WriteLine($"reporting nomatch \"{source}\"");

				captureIndex = 0;
				report(source, null);
				return;
			}

			var index = 0;

			for (captureIndex = 0; captureIndex < matches.Count; captureIndex++)
			{
				var match = matches[captureIndex];

				if (match.Length > 0)
				{
					// Groups will contain a list of all possible captures in the regex, for both
					// successful and unsuccessful captures. The 0th entry is the capture but
					// doesn't indicate the group name. The next Successful entry is this capture
					// and indicates the group name which should be an index offset of the capture
					// in the entire regex; we can use that to index the appropriate scope.

					var groups = match.Groups.Cast<Group>().Skip(1).Where(g => g.Success);
					foreach (var group in groups)
					{
						if (group.Index > index)
						{
							//Logger.Current.WriteLine(
							//	$"reporting prior1 \"{source.Substring(index, group.Index - index)}\" " +
							//	$"scopeOverride:({scopeOverride ?? "null"})");

							// default text prior to match or in between matches
							report(source.Substring(index, group.Index - index), scopeOverride ?? null);
						}

						if (int.TryParse(group.Name, out var scope))
						{
							//Logger.Current.WriteLine(
							//	$"reporting \"{group.Value}\" " +
							//	$"scope:({scope}) scopeOverride:({scopeOverride ?? "null"})");

							report(group.Value,
								string.IsNullOrEmpty(scopeOverride)
								? language.Scopes[scope]
								: scopeOverride);

							// check scope override
							var over = 2; // start after our preset scopes added by Compiler
							var r = 0;
							while ((r < language.Rules.Count) && (over < scope))
							{
								over += language.Rules[r].Captures.Count;
								r++;
							}
							if (r < language.Rules.Count)
							{
								var newOverride = language.Rules[r].Scope;

								if (newOverride == string.Empty)
									scopeOverride = null;
								else
									scopeOverride = newOverride == scopeOverride ? null : newOverride;

								//Logger.Current.WriteLine($"scopeOverride = ({scopeOverride ?? "null"}) from rule {r}");
							}
						}
						else
						{
							//Logger.Current.WriteLine(
							//	$"reporting alternate \"{group.Value}\" " +
							//	$"scopeOverride:({scopeOverride ?? "null"})");

							// shouldn't happen but report as default text anyway
							report(group.Value, scopeOverride ?? null);
						}

						index = group.Index + group.Length;
					}
				}
				else
				{
					if (match.Index > index)
					{
						//Logger.Current.WriteLine(
						//	$"reporting prior2 \"{source.Substring(index, match.Index - index)}\" " +
						//	$"scopeOverride:({scopeOverride ?? "null"})");

						// default text prior to match or in between matches
						report(source.Substring(index, match.Index - index), scopeOverride ?? null);
						index = match.Index;
					}

					// captured end-of-line? or line break?
					var group = match.Groups.Cast<Group>().Skip(1).FirstOrDefault(g => g.Success);

					if ((group != null) && int.TryParse(group.Name, out var scope))
					{
						//Logger.Current.WriteLine(
						//	$"reporting empty scope:({scope}) scopeOverride:({scopeOverride ?? "null"})");

						report(string.Empty, language.Scopes[scope]);
						index++;
					}
				}
			}

			if (index < source.Length)
			{
				//Logger.Current.WriteLine($"reporting post \"{source.Substring(index)}\"");

				// remaining source after all captures
				report(source.Substring(index), null);
			}
		}
	}
}
