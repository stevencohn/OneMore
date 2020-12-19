//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S2589 // Boolean expressions should not be gratuitous
#pragma warning disable S2583 // Conditionally executed code should be reachable

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
				// if length of "search" and "replace" differ then the match indexes will
				// offset cumulatively at each replacement so need to adjust for that
				var shift = replace.Length - search.Length;

				for (int i = 0; i < matches.Count; i++)
				{
					var match = matches[i];
					var index = match.Index + (shift * i);

					Replace(wrapper, index, match.Length, replace);
				}

				// TODO: preserve non-breaking whitespace at beginning of line

				cdata.Value = wrapper.GetInnerXml();

				return matches.Count;
			}

			return 0;
		}


		/// <summary>
		/// Replace a single match in the given text.
		/// </summary>
		/// <param name="wrapper">The wrapped CDATA of the run</param>
		/// <param name="searchIndex">The starting index of the text replacement</param>
		/// <param name="searchLength">The length of the text replacement</param>
		/// <param name="replace">The replacement text</param>
		private static void Replace(
			XElement wrapper, int searchIndex, int searchLength, string replace)
		{
			var nodes = wrapper.Nodes().ToList();

			IAtom atom;
			int nodeStart;
			int nodeEnd = -1;
			int chars;
			int remaining = searchLength;
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
					chars = Math.Min(remaining, atom.Length - index);

					atom.Replace(index, chars, replace);

					remaining -= chars;
				}
				else if (searchIndex < nodeStart && searchEnd > nodeStart)
				{
					// found node containing middle/end of match
					chars = Math.Min(atom.Length, remaining);

					atom.Remove(0, chars);

					remaining -= chars;
				}
				else if (searchEnd < nodeStart)
				{
					Logger.Current.WriteLine($"replace break");
					break;
				}

				i++;
			}
		}
	}
}
