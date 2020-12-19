//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

//#pragma warning disable IDE0079 // Remove unnecessary suppression
//#pragma warning disable S2589 // Boolean expressions should not be gratuitous
//#pragma warning disable S2583 // Conditionally executed code should be reachable

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class SearchAndReplaceEditor
	{
		private readonly XNamespace ns;
		private readonly string search;
		private readonly string replace;
		private readonly bool caseSensitive;

		public SearchAndReplaceEditor(
			XNamespace ns, string search, string replace, bool caseSensitive)
		{
			this.ns = ns;
			this.search = search;
			this.replace = replace;
			this.caseSensitive = caseSensitive;
		}


		public int SearchAndReplace(XElement element)
		{
			var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

			// get a cleaned-up wrapper of the CDATA that we can parse
			var cdata = element.GetCData();
			var wrapper = cdata.GetWrapper();

			// find all distinct occurances of search string across all text of the run
			// regardless of internal SPANs; we'll compensate for those below...

			var matches = Regex.Matches(wrapper.Value, search, options);
			if (matches?.Count > 0)
			{
				// run backwards to avoid cumulative offets if match length differs from length
				// of replacement text
				for (var i = matches.Count - 1; i >= 0; i--)
				{
					var match = matches[i];
					Replace(wrapper, match.Index, match.Length, replace);
				}

				cdata.Value = wrapper.GetInnerXml();

				return matches.Count;
			}

			return 0;
		}


		/// <summary>
		/// Replace a single match in the given text.
		/// </summary>
		/// <param name="wrapper">The wrapped CDATA of the run</param>
		/// <param name="searchIndex">The starting index of the match in the text</param>
		/// <param name="searchLength">The length of the match</param>
		/// <param name="replace">The replacement text</param>
		/// <remarks>
		/// A wrapper consists of both XText nodes and XElement nodes where the elements are
		/// most likely SPAN or BR tags. This routine ignores the SPAN XElements themsevles
		/// and focuses only on the inner text of the SPAN.
		/// </remarks>
		private static void Replace(
			XElement wrapper, int searchIndex, int searchLength, string replace)
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

					atom.Replace(index, chars, replace);

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
	}
}
