//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Scans every text run on a page for matches of a regular expression and applies a
	/// given style to each match, wrapping matched text in a styled span while preserving
	/// any pre-existing span structure within the run.
	/// </summary>
	internal class ConditionalFormatEditor
	{
		private readonly Regex regex;
		private readonly Style style;
		private readonly string css;


		/// <summary>
		/// Initialize a new editor with the given pattern and style.
		/// </summary>
		/// <param name="regex">The compiled regular expression to search for</param>
		/// <param name="style">The style to apply to each match</param>
		public ConditionalFormatEditor(Regex regex, Style style)
		{
			this.regex = regex;
			this.style = style;

			// generate css string here to optimize usage
			css = style.ToCss();
		}


		/// <summary>
		/// Scans every outline on the given page and applies the style to every regex match
		/// found within each one:T run, regardless of any active text selection. Matches are
		/// always scoped to a single run and never span across separate one:T elements.
		/// </summary>
		/// <param name="page">The page to scan</param>
		/// <returns>The total number of matches styled</returns>
		public int Apply(Page page)
		{
			var runs = page.BodyOutlines.Descendants(page.Namespace + "T").ToList();

			var count = 0;
			foreach (var run in runs)
			{
				count += ScanElement(run);
			}

			return count;
		}


		private int ScanElement(XElement element)
		{
			var cdata = element.GetCData();
			if (cdata is null || cdata.Value.Length == 0)
			{
				return 0;
			}

			var wrapper = cdata.GetWrapper();

			// replace unicode no-break space with normal space
			var rawtext = wrapper.Value.Replace(' ', ' ');

			var matches = regex.Matches(rawtext);
			if (matches.Count == 0)
			{
				return 0;
			}

			// iterate backwards so styling one match never invalidates the character
			// offsets of matches earlier in the string still queued for processing

			var styled = 0;
			for (var i = matches.Count - 1; i >= 0; i--)
			{
				var match = matches[i];
				if (match.Length == 0)
				{
					continue;
				}

				WrapMatch(wrapper, match.Index, match.Length);
				styled++;
			}

			if (styled > 0)
			{
				cdata.Value = wrapper.GetInnerXml();
			}

			return styled;
		}


		/// <summary>
		/// Wraps the given [start, start+length) character range of the run's flattened
		/// text in a styled span. A match that straddles an existing span/text node
		/// boundary is wrapped as adjacent sibling spans, one per overlapping node,
		/// rather than as a single span nested across multiple nodes; this renders as
		/// visually contiguous styled text without flattening whatever markup existed
		/// in the straddled nodes.
		/// </summary>
		private void WrapMatch(XElement wrapper, int start, int length)
		{
			var end = start + length;
			var nodeStart = 0;

			foreach (var node in wrapper.Nodes().ToList())
			{
				var nodeLength = GetLength(node);
				var nodeEnd = nodeStart + nodeLength;

				if (nodeEnd > start && nodeStart < end)
				{
					var segStart = Math.Max(start, nodeStart) - nodeStart;
					var segEnd = Math.Min(end, nodeEnd) - nodeStart;
					WrapSegment(node, segStart, segEnd - segStart);
				}

				nodeStart = nodeEnd;
				if (nodeStart >= end)
				{
					break;
				}
			}
		}


		private void WrapSegment(XNode node, int segStart, int segLength)
		{
			string value;
			string existingStyle = null;

			if (node is XText text)
			{
				value = text.Value;
			}
			else if (node is XElement element && element.Name.LocalName == "span")
			{
				value = element.Value;
				existingStyle = element.Attribute("style")?.Value;
			}
			else
			{
				// br or other non-text node has no content to style
				return;
			}

			var before = value.Substring(0, segStart);
			var matched = value.Substring(segStart, segLength);
			var after = value.Substring(segStart + segLength);

			var replacements = new List<XNode>();

			if (before.Length > 0)
			{
				replacements.Add(MakeNode(existingStyle, before));
			}

			// always build a fresh span for the matched text, seeded with whatever
			// style the original node carried, then merge this editor's style onto it
			var matchedSpan = new XElement("span", matched);
			if (!string.IsNullOrEmpty(existingStyle))
			{
				matchedSpan.Add(new XAttribute("style", existingStyle));
			}

			ApplyCss(matchedSpan);
			replacements.Add(matchedSpan);

			if (after.Length > 0)
			{
				replacements.Add(MakeNode(existingStyle, after));
			}

			node.ReplaceWith(replacements);
		}


		private static XNode MakeNode(string existingStyle, string text)
		{
			return string.IsNullOrEmpty(existingStyle)
				? new XText(text)
				: new XElement("span", new XAttribute("style", existingStyle), text);
		}


		/// <summary>
		/// Merges this editor's style onto the given span, preserving whatever style
		/// attribute it already carries. Mirrors the merge idiom used by Stylizer.
		/// </summary>
		private void ApplyCss(XElement span)
		{
			var attr = span.Attribute("style");
			if (attr is null)
			{
				span.Add(new XAttribute("style", css));
			}
			else
			{
				var merged = new Style(attr.Value);
				merged.Merge(style);
				attr.Value = merged.ToCss();
			}
		}


		private static int GetLength(XNode node)
		{
			return node switch
			{
				XText text => text.Value.Length,
				XElement element => element.Value.Length,
				_ => 0
			};
		}
	}
}
