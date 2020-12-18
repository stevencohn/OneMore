//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class SearchAndReplaceEditor
	{
		private readonly XNamespace ns;
		private readonly string search;
		private readonly string replace;
		private readonly bool caseSensitive;

		public SearchAndReplaceEditor(XNamespace ns, string search, string replace, bool caseSensitive)
		{
			this.ns = ns;
			this.search = search;
			this.replace = replace;
			this.caseSensitive = caseSensitive;
		}


		public int SearchAndReplace(XElement element)
		{

			System.Diagnostics.Debugger.Launch();


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
				var difference = replace.Length - search.Length;

				for (int i = 0; i < matches.Count; i++)
				{
					var match = matches[i];
					var index = match.Index + (difference * i);

					Replace(wrapper, index, match.Length, replace);
				}

				// TODO: preserve non-breaking whitespace at beginning of line

				cdata.Value = wrapper.GetInnerXml();

				return matches.Count;
			}

			return 0;
		}


		private static void Replace(XElement wrapper, int searchIndex, int searchLength, string replace)
		{
			int index = 0;
			int searchEnd = searchIndex + searchLength;

			var list = wrapper.Nodes().ToList();

			int i = 0;
			while (i < list.Count)
			{
				var node = list[i];
				var atom = AtomicFactory.MakeAtom(node);
				var atomLength = atom.Length;
				var atomEnd = index + atom.Length;
				var startIndex = searchIndex - index;

				// passed the start position so must have remnants to clean up
				if (startIndex < 0 && index < searchEnd)
				{
					// throw away remants of search string from within atom
					atom.Extract(0, searchEnd - searchIndex - 1);

					if (atom.Empty())
					{
						node.Remove();
						i--;
					}
				}
				// atom contains entire search string
				else if (index <= searchIndex && atomEnd >= searchEnd)
				{
					atom.Replace(startIndex, searchLength, replace);
				}
				// either atom is before searchIndex or
				// atom contains part of search
				else if (index <= searchIndex && (atomEnd > searchIndex && atomEnd <= searchEnd))
				{
					// replace beginning of search string
					atom.Replace(startIndex, atom.Length - startIndex, replace);
				}

				index += atomLength;
				i++;
			}
		}
	}
}
