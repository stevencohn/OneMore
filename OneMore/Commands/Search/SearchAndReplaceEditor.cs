//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class SearchAndReplaceEditor
	{
		private readonly string search;
		private readonly string replace;
		private readonly XElement replaceElement;
		private readonly RegexOptions options;


		public SearchAndReplaceEditor(string search, string replace, bool useRegex, bool caseSensitive)
		{
			this.search = useRegex ? search : EscapeEscapes(search);
			this.replace = replace;
			options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
		}

		public SearchAndReplaceEditor(string search, XElement replaceElement, bool useRegex, bool caseSensitive)
		{
			this.search = useRegex ? search : EscapeEscapes(search);
			this.replaceElement = replaceElement;
			options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
		}


		/// <summary>
		/// Escape all control chars in given string. Typically used to escape the search
		/// string when not using regular expressions
		/// </summary>
		/// <param name="plain">The string to treat as plain text</param>
		/// <returns>A string in which all regular expression control chars are escaped</returns>
		public static string EscapeEscapes(string plain)
		{
			var codes = new char[] { '\\', '.', '*', '|', '?', '(', '[', '$', '^' };

			var builder = new StringBuilder();
			for (var i = 0; i < plain.Length; i++)
			{
				if (codes.Contains(plain[i]))
				{
					if (i == 0 || plain[i - 1] != '\\')
					{
						builder.Append('\\');
					}
				}

				builder.Append(plain[i]);
			}

			return builder.ToString();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public int SearchAndReplace(Page page)
		{
			var elements = page.GetSelectedElements();

			int count = 0;
			if (elements.Any())
			{
				foreach (var element in elements)
				{
					count += SearchAndReplace(element);
				}

				if (count > 0)
				{
					PatchEndingBreaks(page);
				}
			}

			return count;
		}


		// Replace all matches in the given T run
		private int SearchAndReplace(XElement element)
		{
			// get a cleaned-up wrapper of the CDATA that we can parse
			var cdata = element.GetCData();
			if (cdata.Value.Length == 0)
			{
				return 0;
			}

			var wrapper = cdata.GetWrapper();

			// find all distinct occurances of search string across all text of the run
			// regardless of internal SPANs; we'll compensate for those below...

			var matches = Regex.Matches(wrapper.Value, search, options);
			if (matches?.Count > 0)
			{
				// iterate backwards to avoid cumulative offets if Match length differs
				// from length of replacement text
				for (var i = matches.Count - 1; i >= 0; i--)
				{
					var match = matches[i];
					Replace(wrapper, match.Index, match.Length);
				}

				cdata.Value = wrapper.GetInnerXml();

				return matches.Count;
			}

			return 0;
		}


		// Replace a single match in the given text.
		// A wrapper consists of both XText nodes and XElement nodes where the elements are
		// most likely SPAN or BR tags. This routine ignores the SPAN XElements themsevles
		// and focuses only on the inner text of the SPAN.
		private void Replace(
			XElement wrapper,		// the wrapped CDATA of the run
			int searchIndex,		// the starting index of the match in the text
			int searchLength)		// the length of the match
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

					if (replace != null)
					{
						atom.Replace(index, chars, replace);
					}
					else
					{
						atom.Replace(index, chars, replaceElement);
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
