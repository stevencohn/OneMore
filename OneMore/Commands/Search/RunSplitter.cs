//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// The outcome of splitting a run so that a match is its own selected run.
	/// </summary>
	internal sealed class RunSplit
	{
		/// <summary>The objectID of the OE holding the split runs</summary>
		public string ObjectId { get; set; }

		/// <summary>The run as it was before it was split</summary>
		public XElement Original { get; set; }

		/// <summary>The CDATA of each run it was split into</summary>
		public List<string> Pieces { get; set; }
	}


	/// <summary>
	/// Splits a T run into before / match / after runs so that exactly the matched characters
	/// are a run marked selected="all", keeping span styles on each part.
	/// </summary>
	internal static class RunSplitter
	{
		/// <summary>
		/// Splits the run of the given match and selects the matched characters.
		/// </summary>
		/// <returns>Details of the split or null if the run could not be split</returns>
		public static RunSplit Split(MatchInfo match)
		{
			if (match.Length == 0)
			{
				return null;
			}

			var run = match.TElement;
			var cdata = run.GetCData();
			if (cdata is null)
			{
				return null;
			}

			var wrapper = cdata.GetWrapper();
			var total = wrapper.Value.Length;
			var start = match.StartIndex;
			var end = Math.Min(start + match.Length, total);

			var oe = run.Parent;
			while (oe is not null && oe.Name.LocalName != "OE")
			{
				oe = oe.Parent;
			}

			var objectId = oe?.Attribute("objectID")?.Value;
			if (objectId is null)
			{
				return null;
			}

			var original = new XElement(run);
			var pieces = new List<string>();
			var runs = new List<XElement>();

			if (start > 0)
			{
				var before = KeepBreak(Slice(wrapper, 0, start, false), wrapper, start);
				pieces.Add(before);
				runs.Add(MakeRun(run, before, false));
			}

			var inside = Slice(wrapper, start, end, end == total);
			if (end < total)
			{
				inside = KeepBreak(inside, wrapper, end);
			}

			pieces.Add(inside);
			runs.Add(MakeRun(run, inside, true));

			if (end < total)
			{
				var after = Slice(wrapper, end, total, true);
				pieces.Add(after);
				runs.Add(MakeRun(run, after, false));
			}

			run.ReplaceWith(runs.Cast<object>().ToArray());

			return new RunSplit { ObjectId = objectId, Original = original, Pieces = pieces };
		}


		// OneNote drops a line break that ends a run, so when the text just before the split
		// point is a line break (<br> followed by a newline), add a trailing nbsp to preserve
		// it, as OneNote itself does and as SearchAndReplaceEditor.PatchEndingBreaks does
		private static string KeepBreak(string piece, XElement wrapper, int end)
		{
			return end > 0 && wrapper.Value[end - 1] == '\n'
				? $"{piece}&nbsp;"
				: piece;
		}


		private static XElement MakeRun(XElement template, string cdata, bool selected)
		{
			var run = new XElement(template.Name);

			foreach (var attribute in template.Attributes().Where(a => a.Name.LocalName != "selected"))
			{
				run.Add(new XAttribute(attribute.Name, attribute.Value));
			}

			if (selected)
			{
				run.Add(new XAttribute("selected", "all"));
			}

			run.Add(new XCData(cdata));
			return run;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Slicing the inner XML of a run by character range, keeping span styles on each part

		private static string Slice(XElement wrapper, int from, int to, bool last)
		{
			var slice = new XElement("cdata");
			var offset = 0;

			foreach (var node in wrapper.Nodes())
			{
				var piece = Trim(node, offset, from, to, last);
				if (piece is not null)
				{
					slice.Add(piece);
				}

				offset += TextLength(node);
			}

			return slice.GetInnerXml();
		}


		private static int TextLength(XNode node)
		{
			return node is XText text ? text.Value.Length : ((XElement)node).Value.Length;
		}


		// Copies the part of the node within [from, to), where offset is where the node starts
		private static XNode Trim(XNode node, int offset, int from, int to, bool last)
		{
			if (node is XText text)
			{
				var lo = Math.Max(from - offset, 0);
				var hi = Math.Min(to - offset, text.Value.Length);
				return lo < hi ? new XText(text.Value.Substring(lo, hi - lo)) : null;
			}

			var element = (XElement)node;
			var length = element.Value.Length;

			if (length == 0)
			{
				// a line break has no text, so it belongs to whichever part it falls in
				return offset >= from && (offset < to || (last && offset <= to))
					? new XElement(element)
					: null;
			}

			var elo = Math.Max(from - offset, 0);
			var ehi = Math.Min(to - offset, length);
			if (elo >= ehi)
			{
				return null;
			}

			var copy = new XElement(element.Name, element.Attributes());
			var childOffset = 0;

			foreach (var child in element.Nodes())
			{
				var piece = Trim(child, childOffset, elo, ehi, last && ehi == length);
				if (piece is not null)
				{
					copy.Add(piece);
				}

				childOffset += TextLength(child);
			}

			return copy;
		}
	}
}
