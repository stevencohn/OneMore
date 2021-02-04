//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class SearchAndReplaceEditor
	{
		private readonly string search;
		private readonly string replace;
		private readonly RegexOptions options;

		public SearchAndReplaceEditor(
			string search, string replace,bool caseSensitive, bool useRegex)
		{
			this.search = search;
			this.replace = replace;
			this.search = useRegex ? search : EscapeEscapes(search);
			options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
		}


		private string EscapeEscapes(string plain)
		{
			// if NOT using reg expression then must escape all regex control chars...

			var codes = new char[] { '\\', '.', '*', '|', '?', '(', '[' };

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


		public int SearchAndReplace(XElement element)
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
