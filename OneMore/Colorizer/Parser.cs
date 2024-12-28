//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************                

//#define DEBUGLOG // uncomment to enable the DebugLog() conditional method

namespace River.OneMoreAddIn.Colorizer
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
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
	internal class Parser : Loggable
	{
		#region Note
		// Due to the way the regular expression are defined, we may end up with a MatchCollection
		// that groups together scoped captured, out of sequence from how they appear in the text.
		// For example, XML like <foo a1="v1" a2="v2/> will be captured as
		//
		//  Matches[0].Group[0].Capture[0] = "<"    ; index=1
		//  Matches[0].Group[1].Capture[0] = "foo"  ; index=2
		//  Matches[0].Group[2].Capture[0] = "a1"   ; index=6
		//  Matches[0].Group[2].Capture[1] = "a2"   ; index=14
		//  Matches[0].Group[3].Capture[0] = "="    ; index=8
		//  Matches[0].Group[3].Capture[1] = "="    ; index=16
		//  Matches[0].Group[4].Capture[0] = """    ; index=9
		//  Matches[0].Group[4].Capture[1] = """    ; index=17
		//  Matches[0].Group[5].Capture[0] = "v1"   ; index=10
		//  Matches[0].Group[5].Capture[1] = "v2"   ; index=18
		//  Matches[0].Group[6].Capture[0] = """    ; index=12
		//  Matches[0].Group[6].Capture[1] = """    ; index=20
		//  Matches[0].Group[7].Capture[0] = "/>"   ; index=21
		//
		// Notice that the index of each capture is not in sequence and that scoped values, like
		// "a1" and "a2" are grouped together. CollectCaptures.OrderBy will project these into a
		// list of OrderedCapture items that can be sorted by index offset
		#endregion
		private sealed class OrderedCapture
		{
			public int Scope;
			public int Index;
			public int Length;
			public string Value;
		}


		private readonly ICompiledLanguage language;
		private readonly Regex crPattern;
		private MatchCollection matches;
		private int captureIndex;
		private string scopeOverride;


		public Parser(ICompiledLanguage language)
		{
			this.language = language;
			crPattern = new Regex(@"(?:\r\n)|(?:\n\r)", RegexOptions.Compiled);
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
			source = crPattern.Replace(source, "\n");

			matches = language.Regex.Matches(source);

			if (matches.Count == 0)
			{
				logger.Debug($"report(\"{source}\", null); - no match");

				captureIndex = 0;
				report(source, null);
				return;
			}

			var index = 0;

			var captures = CollectCaptures(matches).OrderBy(c => c.Index).ToList();
			for (captureIndex = 0; captureIndex < captures.Count; captureIndex++)
			{
				var capture = captures[captureIndex];

				// report text prior to capture or in between captures...

				if (index < capture.Index)
				{
					DebugLog(
						$"report(\"{source.Substring(index, capture.Index - index)}\", " +
						$"{scopeOverride ?? "null"}); - space");

					report(source.Substring(index, capture.Index - index), scopeOverride ?? null);
				}

				// report capture...

				var scope = string.IsNullOrEmpty(scopeOverride)
					? language.Scopes[capture.Scope]
					: scopeOverride;

				logger.Debug(
					$"report(\"{capture.Value}\", {scope}); " +
					(scopeOverride is null ? string.Empty : $"- scopeOverride:{scopeOverride}"));

				report(capture.Value, scope);
				index = capture.Index + capture.Length;

				// check scope override...

				if (capture.Scope < 2)
				{
					// linebreak
					continue;
				}

				// capture.Scope will index the collated capture across all rules.
				// skip compiler-added scopes (0=*=entire string, 1=$=end of line)
				var over = 2;
				var ri = 0;
				while ((ri < language.Rules.Count) && (over < capture.Scope))
				{
					over += language.Rules[ri].Captures.Count;
					ri++;
				}

				if (ri < language.Rules.Count)
				{
					var rule = language.Rules[ri];
					var newOverride = rule.Scope;

					if (!string.IsNullOrEmpty(scopeOverride) && newOverride == scopeOverride)
					{
						DebugLog(
							$".. reverting {scopeOverride} scope override " +
							$"from rule {{ri}} /{{rule.Pattern}}/\"");

						scopeOverride = null;
					}
					else
					{
						DebugLog(
							$".. newOverride ({newOverride ?? "null"}) from rule {ri} /{rule.Pattern}/");

						// special case of multi-line comments, started by a rule with
						// the "comment" scope and ended by a rule with the "" scope
						// ignore other scopes until the ending "" scope is discovered

						if (newOverride == string.Empty)
							scopeOverride = null;
						else if (scopeOverride != "comment")
							scopeOverride = newOverride;

						DebugLog($".. scopeOverride ({scopeOverride ?? "null"})");
					}
				}
			}
			if (index < source.Length)
			{
				DebugLog($"report(\"{source.Substring(index)}\", null); - remaining");

				// remaining source after all captures
				report(source.Substring(index), null);
			}
		}


		private IEnumerable<OrderedCapture> CollectCaptures(MatchCollection matches)
		{
			for (var mi = 0; mi < matches.Count; mi++)
			{
				var match = matches[mi];
				var groups = match.Groups.Cast<Group>().Skip(1).Where(g => g.Success).ToList();
				for (var gi = 0; gi < groups.Count; gi++)
				{
					var group = groups[gi];
					for (var ci = 0; ci < group.Captures.Count; ci++)
					{
						var capture = group.Captures[ci];

						if (!int.TryParse(group.Name, out var scope))
						{
							scope = -1;
						}

						yield return new OrderedCapture
						{
							Scope = scope,
							Index = capture.Index,
							Length = capture.Length,
							Value = capture.Value
						};
					}
				}
			}
		}


		// #define DEBUGRIBBON to enable this method; otherwise compiler will remove it entirely
		[Conditional("DEBUGLOG")]
		private void DebugLog(string message)
		{
			logger.WriteLine(message);
		}
	}
}
