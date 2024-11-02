//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Searches a page for a specified pattern and replaces matches with a given string,
	/// possibly using capture group substitution.
	/// </summary>
	internal class SearchAndReplaceEditor
	{
		private readonly Regex regex;
		private readonly string replacementString;
		private readonly XElement replacementElement;


		/// <summary>
		/// Initialize a new editor, invoked primarily by the SearchAndReplace command
		/// </summary>
		/// <param name="pattern">A string or regular expression</param>
		/// <param name="replacement">A string which might contain capture references, e.g. $1</param>
		/// <param name="enableRegex">Determines whether to treat pattern as a regular expression</param>
		/// <param name="caseSensitive">Determines whether to be case sensitive</param>
		public SearchAndReplaceEditor(
			string pattern, string replacement, bool enableRegex, bool caseSensitive)
		{
			regex = new Regex(
				enableRegex ? pattern : pattern.EscapeForRegex(),
				caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase
				);

			replacementString = replacement;

			CaseSensitive = caseSensitive;
			EnableRegex = enableRegex;
			SearchPattern = pattern;
		}


		/// <summary>
		/// Initializes a new editor, invoked primarily by the LinkReferences command
		/// </summary>
		/// <param name="pattern">A string or regular expression</param>
		/// <param name="element">A element to insert in place of each match</param>
		/// <param name="enableRegex">Determines whether to treat pattern as a regular expression</param>
		/// <param name="caseSensitive">Determines whether to be case sensitive</param>
		public SearchAndReplaceEditor(
			string pattern, XElement element, bool enableRegex, bool caseSensitive)
		{
			regex = new Regex(
				enableRegex ? pattern : pattern.EscapeForRegex(),
				caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase
				);

			replacementElement = element;

			CaseSensitive = caseSensitive;
			EnableRegex = enableRegex;
			SearchPattern = pattern;
		}


		public bool CaseSensitive { get; private set; }

		public bool EnableRegex { get; private set; }

		public string ReplacementString => replacementString;
		// ?? replacementElement.ToString(SaveOptions.DisableFormatting);

		public string SearchPattern { get; private set; }


		/// <summary>
		/// Search selected content on the given page and replace matches with the
		/// specified replacement value.
		/// </summary>
		/// <param name="page">The page to modify</param>
		/// <returns>The number of successful replacements</returns>
		public int SearchAndReplace(Page page)
		{
			int count = 0;

			var range = new SelectionRange(page);
			var runs = range.GetSelections(defaulToAnytIfNoRange: true);
			if (runs.Any())
			{
				foreach (var run in runs)
				{
					count += ScanElement(run);
				}

				if (count > 0)
				{
					PatchEndingBreaks(page);
				}
			}

			return count;
		}


		public int SearchAndReplace(XElement paragraph)
		{
			var ns = paragraph.GetNamespaceOfPrefix(OneNote.Prefix);
			var elements = paragraph.Elements(ns + "T");

			int count = 0;
			if (elements.Any())
			{
				foreach (var element in elements)
				{
					count += ScanElement(element);
				}
			}

			var children = paragraph
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE");

			if (children.Any())
			{
				foreach (var child in children)
				{
					count += SearchAndReplace(child);
				}
			}

			var tables = paragraph.Elements(ns + "Table");
			if (tables.Any())
			{
				foreach (var table in tables)
				{
					var toes = table
						.Elements(ns + "Row")
						.Elements(ns + "Cell")
						.Elements(ns + "OEChildren")
						.Elements(ns + "OE");

					if (toes.Any())
					{
						foreach (var toe in toes)
						{
							count += SearchAndReplace(toe);
						}
					}
				}
			}

			return count;
		}


		// Replace all matches in the given T run
		private int ScanElement(XElement element)
		{
			// get a cleaned-up wrapper of the CDATA that we can parse
			var cdata = element.GetCData();
			if (cdata.Value.Length == 0)
			{
				return 0;
			}

			var wrapper = cdata.GetWrapper();

			// replace unicode no-break space with normal space
			var rawtext = wrapper.Value.Replace('\u00A0', ' ');

			// find all distinct occurrences of search string across all text of the run
			// regardless of internal SPANs; we'll compensate for those in Replace() below...

			var matches = regex.Matches(rawtext);
			if (matches.Count > 0)
			{
				// iterate backwards to avoid cumulative offets if Match length differs
				// from length of replacement text...

				for (var i = matches.Count - 1; i >= 0; i--)
				{
					var match = matches[i];

					// if there is exactly one group then the regex has neither capturing nor
					// non-capturing groups; otherwise there must be at least one group in the
					// regex so iterate the captures

					if (match.Groups.Count == 1)
					{
						// regex does not contain any capture or non-capture groups
						// for example "abc"
						Replace(wrapper, match.Index, match.Length,
							stringValue: replacementString,
							elementValue: replacementElement);
					}
					else if (match.Groups.Count > 1)
					{
						if (replacementElement != null)
						{
							var xml = ExpandSubstitutions(match,
								replacementElement.ToString(SaveOptions.DisableFormatting));

							Replace(wrapper, match.Groups[1].Index, match.Groups[1].Length,
								elementValue: XElement.Parse(xml));

						}
						else
						{
							Replace(wrapper, match.Groups[1].Index, match.Groups[1].Length,
								stringValue: ExpandSubstitutions(match, replacementString));
						}
					}
				}

				cdata.Value = wrapper.GetInnerXml();

				return matches.Count;
			}

			return 0;
		}


		// Substitute $1..$n parameters in replacementString with capture groups
		private string ExpandSubstitutions(Match match, string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return text;
			}

			var matches = Regex.Matches(text, @"\$\d+");
			if (matches.Count > 0)
			{
				for (int i = matches.Count - 1; i >= 0; i--)
				{
					var m = matches[i];

					// 1-based indexing can be used here because match.Groups[0] is always skipped
					var index = int.Parse(m.Value.Substring(1));

					if (index >= 0 && index <= match.Groups.Count)
					{
						text = text
							.Remove(m.Index, m.Length)
							.Insert(m.Index, match.Groups[index].Value);
					}
				}
			}

			return text;
		}


		// Replace a single match in the given text.
		// A wrapper consists of both XText nodes and XElement nodes where the elements are
		// most likely SPAN or BR tags. This routine ignores the SPAN XElements themsevles
		// and focuses only on the inner text of the SPAN.
		private void Replace(
			XElement wrapper,               // the wrapped CDATA of the run
			int searchIndex,                // the starting index of the match in the text
			int searchLength,               // the length of the match
			string stringValue = null,      // the replacement string value
			XElement elementValue = null)   // the replacement XML value
		{
			// XText and XElement nodes in wrapper
			var nodes = wrapper.Nodes().ToList();

			IAtom atom;

			// starting index of current node's text within the full text
			int nodeStart;
			// ending index of current node's text within the full text
			int nodeEnd = -1;
			// the remaining char count to address in the text
			int remaining = searchLength;
			// the index of the last char of the match in the text
			int searchEnd = searchIndex + searchLength;

			int i = 0;
			while (i < nodes.Count && nodeEnd < searchEnd)
			{
				atom = AtomicFactory.MakeAtom(nodes[i]);
				nodeStart = nodeEnd + 1;
				nodeEnd += atom.Length;

				if (searchIndex >= nodeStart && searchIndex <= nodeEnd)
				{
					// found node containing start or all of the match
					var index = searchIndex - nodeStart;
					var chars = Math.Min(remaining, atom.Length - index);

					if (elementValue != null)
					{
						atom.Replace(index, chars, elementValue);
					}
					else
					{
						atom.Replace(index, chars, stringValue);
					}

					remaining -= chars;
				}
				else if (searchIndex < nodeStart && searchEnd > nodeStart)
				{
					// found node containing middle/end of match
					var chars = Math.Min(atom.Length, remaining);

					atom.Remove(0, chars);

					remaining -= chars;
				}
				else if (searchEnd < nodeStart)
				{
					break;
				}

				i++;
			}
		}


		/// If a one:T ends with a BR\n then OneNote will strip it but it typically appends
		/// a nbsp after it to preserve the line break. We do the same here to ensure we
		/// don't lose our line breaks
		private void PatchEndingBreaks(Page page)
		{
			var runs = page.Root.Elements(page.Namespace + "Outline")
				.Descendants(page.Namespace + "T")
				.Where(t => t.GetCData().Value.EndsWith("<br>\n"))
				.ToList();

			foreach (var run in runs)
			{
				var cdata = run.GetCData();
				cdata.Value = $"{cdata.Value}&nbsp;";
			}
		}
	}
}
