//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
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
		private readonly string highlightColor;


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


		/// <summary>
		/// Initializes a new editor that highlights matches rather than replacing them,
		/// invoked by the SearchAndHighlight command
		/// </summary>
		/// <param name="pattern">A string or regular expression</param>
		/// <param name="enableRegex">Determines whether to treat pattern as a regular expression</param>
		/// <param name="caseSensitive">Determines whether to be case sensitive</param>
		/// <param name="highlightColor">The background color to apply to each match</param>
		public SearchAndReplaceEditor(
			string pattern, bool enableRegex, bool caseSensitive, string highlightColor)
		{
			regex = new Regex(
				enableRegex ? pattern : pattern.EscapeForRegex(),
				caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase
				);

			this.highlightColor = highlightColor;

			CaseSensitive = caseSensitive;
			EnableRegex = enableRegex;
			SearchPattern = pattern;
		}


		public bool CaseSensitive { get; private set; }

		public bool EnableRegex { get; private set; }

		public string ReplacementString => replacementString;
		// ?? replacementElement.ToString(SaveOptions.DisableFormatting);

		public string SearchPattern { get; private set; }


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Global search and replace

		/// <summary>
		/// Search selected content on the given page and replace matches with the
		/// specified replacement value.
		/// </summary>
		/// <param name="page">The page to modify</param>
		/// <returns>The number of successful replacements</returns>
		public int SearchAndReplace(Page page)
		{
			int count = 0;

			var runs = GetRuns(page);
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


		// Gets the selected runs, or every run if nothing is selected
		private static IEnumerable<XElement> GetRuns(Page page)
		{
			var range = new SelectionRange(page);
			var runs = range.GetSelections(defaulToAnytIfNoRange: true);

			if (range.Scope == SelectionScope.TextCursor)
			{
				// OneNote splits the run at a caret into previous, empty cursor, and next runs,
				// so the word under the caret is split and would never match; rejoin them
				range.Deselect();
				runs = page.Root.Descendants(page.Namespace + "T");
			}

			return runs;
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


		/// <summary>
		/// Finds the objectID of the nearest OE ancestor of the first match within the given
		/// scope, without applying any replacements. Used to link back to the specific
		/// paragraph a match was found in rather than just the containing page.
		/// </summary>
		/// <param name="scope">The element to search, e.g. a page Outline or OE</param>
		/// <returns>An objectID or null if no match was found</returns>
		public string FindFirstMatchObjectId(XElement scope)
		{
			var ns = scope.GetNamespaceOfPrefix(OneNote.Prefix);

			foreach (var element in scope.Descendants(ns + "T"))
			{
				// never touch mathML equations
				if (element.IsMathML())
				{
					continue;
				}

				var cdata = element.GetCData();
				if (cdata.Value.Length == 0)
				{
					continue;
				}

				var wrapper = cdata.GetWrapper();
				var rawtext = wrapper.Value.Replace(' ', ' ');
				var hasReplaceableMatch = regex.Matches(rawtext).Cast<Match>().Any(match =>
				{
					var groupIndex = match.Groups.Count > 1 ? match.Groups[1].Index : match.Index;
					var groupLength = match.Groups.Count > 1 ? match.Groups[1].Length : match.Length;
					return !OverlapsExistingLink(wrapper, groupIndex, groupLength);
				});

				if (!hasReplaceableMatch)
				{
					continue;
				}

				var oe = element.Parent;
				while (oe != null && oe.Name.LocalName != "OE")
				{
					oe = oe.Parent;
				}

				return oe?.Attribute("objectID")?.Value;
			}

			return null;
		}


		// Replace all matches in the given T run
		private int ScanElement(XElement element)
		{
			// never touch mathML equations
			if (element.IsMathML())
			{
				return 0;
			}

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

				var replaced = 0;

				for (var i = matches.Count - 1; i >= 0; i--)
				{
					var match = matches[i];

					// if there is exactly one group then the regex has neither capturing nor
					// non-capturing groups; otherwise there must be at least one group in the
					// regex so iterate the captures

					var groupIndex = match.Groups.Count > 1 ? match.Groups[1].Index : match.Index;
					var groupLength = match.Groups.Count > 1 ? match.Groups[1].Length : match.Length;

					// when replacing with a hyperlink element, never overwrite or nest inside
					// text that is already a hyperlink
					if (replacementElement != null && OverlapsExistingLink(wrapper, groupIndex, groupLength))
					{
						continue;
					}

					if (highlightColor != null)
					{
						if (groupLength == 0)
						{
							continue;
						}

						Highlight(wrapper, groupIndex, groupLength);
					}
					else if (match.Groups.Count == 1)
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

					replaced++;
				}

				if (replaced > 0)
				{
					cdata.Value = wrapper.GetInnerXml();
				}

				return replaced;
			}

			return 0;
		}


		// Determines whether the given match range falls within (or covers) an existing
		// hyperlink element, so we don't overwrite or nest a new link inside it.
		private bool OverlapsExistingLink(XElement wrapper, int searchIndex, int searchLength)
		{
			var searchEnd = searchIndex + searchLength;
			var nodeEnd = -1;

			foreach (var node in wrapper.Nodes())
			{
				var atom = AtomicFactory.MakeAtom(node);
				var nodeStart = nodeEnd + 1;
				nodeEnd += atom.Length;

				if (searchIndex <= nodeEnd && searchEnd > nodeStart)
				{
					if (node is XElement element &&
						(element.Name.LocalName == "a" ||
						 element.Descendants().Any(d => d.Name.LocalName == "a")))
					{
						return true;
					}
				}

				if (nodeEnd >= searchEnd)
				{
					break;
				}
			}

			return false;
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


		// Applies the highlight color to the characters in [index, index+length) of the wrapper.
		// Existing style attributes are preserved; only the background is set or replaced.
		private void Highlight(XElement wrapper, int index, int length)
		{
			var end = index + length;
			var offset = 0;

			foreach (var node in wrapper.Nodes().ToList())
			{
				var nodeLength = node is XText text ? text.Value.Length : ((XElement)node).Value.Length;
				if (offset >= end)
				{
					break;
				}

				var lo = Math.Max(index - offset, 0);
				var hi = Math.Min(end - offset, nodeLength);
				if (lo < hi)
				{
					HighlightNode(node, lo, hi);
				}

				offset += nodeLength;
			}

			HoistTrailingWhitespace(wrapper);
		}


		// OneNote treats a whitespace-only span as style-neutral and folds it into the run
		// before it, so a space left standing alone after a highlighted word would come back
		// highlighted. Move that space to the start of the following text so it keeps its
		// place without being a run of its own.
		private void HoistTrailingWhitespace(XElement wrapper)
		{
			var nodes = wrapper.Nodes().ToList();

			for (var i = 0; i + 2 < nodes.Count; i++)
			{
				if (!IsHighlight(nodes[i]))
				{
					continue;
				}

				var space = nodes[i + 1];
				if (!IsWhitespaceOnly(space) || IsHighlight(nodes[i + 2]))
				{
					continue;
				}

				var text = nodes[i + 2] switch
				{
					XText t => t,
					XElement e when e.FirstNode is XText first => first,
					_ => null
				};

				if (text is null)
				{
					continue;
				}

				text.Value = space is XText whitespace
					? whitespace.Value + text.Value
					: ((XElement)space).Value + text.Value;

				space.Remove();
				i++;
			}
		}


		private bool IsHighlight(XNode node)
		{
			return node is XElement element &&
				element.Name.LocalName == "span" &&
				element.Attribute("style")?.Value.IndexOf(
					$"background:{highlightColor}", StringComparison.OrdinalIgnoreCase) >= 0;
		}


		private static bool IsWhitespaceOnly(XNode node)
		{
			return node switch
			{
				XText text => text.Value.Length > 0 && string.IsNullOrWhiteSpace(text.Value),
				XElement element => element.Name.LocalName == "span" &&
					element.Nodes().All(n => n is XText) &&
					element.Value.Length > 0 && string.IsNullOrWhiteSpace(element.Value),
				_ => false
			};
		}


		// Highlights characters [lo, hi) of the node's text. Text is wrapped in a span; a span
		// wholly covered has its background set; a partially covered span of plain text is split
		// into sibling spans; other partially covered elements recurse so that inner text keeps
		// the formatting it inherits from the enclosing element.
		private void HighlightNode(XNode node, int lo, int hi)
		{
			if (node is XText text)
			{
				var value = text.Value;
				var parts = new List<object>();

				if (lo > 0)
				{
					parts.Add(value.Substring(0, lo));
				}

				parts.Add(HighlightRotator.Wrap(new XText(value.Substring(lo, hi - lo)), highlightColor));

				if (hi < value.Length)
				{
					parts.Add(value.Substring(hi));
				}

				text.ReplaceWith(parts.ToArray());
				return;
			}

			var element = (XElement)node;

			if (element.Name.LocalName == "span" && lo == 0 && hi == element.Value.Length)
			{
				HighlightRotator.SetBackground(element, highlightColor);
				return;
			}

			// split a span of plain text into consecutive sibling spans that each keep the
			// original style; nesting the highlight inside the span would have OneNote fold
			// the trailing whitespace into the highlighted text when it normalizes the nesting
			if (element.Name.LocalName == "span" && element.Nodes().All(n => n is XText))
			{
				var value = element.Value;
				var parts = new List<XElement>();

				if (lo > 0)
				{
					parts.Add(new XElement(element.Name, element.Attributes(), value.Substring(0, lo)));
				}

				var inside = new XElement(element.Name, element.Attributes(), value.Substring(lo, hi - lo));
				HighlightRotator.SetBackground(inside, highlightColor);
				parts.Add(inside);

				if (hi < value.Length)
				{
					parts.Add(new XElement(element.Name, element.Attributes(), value.Substring(hi)));
				}

				element.ReplaceWith(parts.Cast<object>().ToArray());
				return;
			}

			var offset = 0;
			foreach (var child in element.Nodes().ToList())
			{
				var childLength = child is XText t ? t.Value.Length : ((XElement)child).Value.Length;
				if (offset >= hi)
				{
					break;
				}

				var clo = Math.Max(lo - offset, 0);
				var chi = Math.Min(hi - offset, childLength);
				if (clo < chi)
				{
					HighlightNode(child, clo, chi);
				}

				offset += childLength;
			}
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


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Interactive session search and replace

		/// <summary>
		/// Finds all matches on the given page and returns them in document order.
		/// Used by the step-through replacement session.
		/// </summary>
		public List<MatchInfo> FindAllMatches(Page page)
		{
			var results = new List<MatchInfo>();

			var runs = GetRuns(page);
			foreach (var run in runs)
			{
				CollectMatches(run, results);
			}

			return results;
		}


		// Collect all regex matches within a single T element, appending to results
		private void CollectMatches(XElement element, List<MatchInfo> results)
		{
			// never touch mathML equations
			if (element.IsMathML())
			{
				return;
			}

			var cdata = element.GetCData();
			if (cdata.Value.Length == 0) return;

			var wrapper = cdata.GetWrapper();
			var rawtext = wrapper.Value.Replace(' ', ' ');

			var matches = regex.Matches(rawtext);
			if (matches.Count == 0) return;

			var oe = element.Parent;
			while (oe != null && oe.Name.LocalName != "OE")
			{
				oe = oe.Parent;
			}
			var objectId = oe?.Attribute("objectID")?.Value ?? string.Empty;

			foreach (Match m in matches)
			{
				results.Add(new MatchInfo
				{
					TElement = element,
					StartIndex = m.Index,
					Length = m.Length,
					MatchedText = m.Value,
					ParagraphObjectId = objectId
				});
			}
		}


		/// <summary>
		/// Replaces a single match as described by the given MatchInfo, modifying the T element
		/// in place. Rewrites the CDATA of the T element after replacement.
		/// </summary>
		/// <returns>
		/// The index within the run's text just after the inserted replacement, or -1 if the
		/// match could not be replaced
		/// </returns>
		public int ReplaceMatch(MatchInfo match)
		{
			// never touch mathML equations
			if (match.TElement.IsMathML())
			{
				return -1;
			}

			var cdata = match.TElement.GetCData();
			if (cdata.Value.Length == 0) return -1;

			var wrapper = cdata.GetWrapper();
			var rawtext = wrapper.Value.Replace(' ', ' ');

			var matches = regex.Matches(rawtext);

			for (var i = 0; i < matches.Count; i++)
			{
				var m = matches[i];
				if (m.Index != match.StartIndex || m.Length != match.Length) continue;

				var end = m.Index;

				if (m.Groups.Count == 1)
				{
					Replace(wrapper, m.Index, m.Length,
						stringValue: replacementString,
						elementValue: replacementElement);

					end += replacementString?.Length ?? replacementElement?.Value.Length ?? 0;
				}
				else
				{
					end = m.Groups[1].Index;

					if (replacementElement != null)
					{
						var xml = ExpandSubstitutions(m,
							replacementElement.ToString(SaveOptions.DisableFormatting));

						var element = XElement.Parse(xml);
						Replace(wrapper, m.Groups[1].Index, m.Groups[1].Length,
							elementValue: element);

						end += element.Value.Length;
					}
					else
					{
						var text = ExpandSubstitutions(m, replacementString);
						Replace(wrapper, m.Groups[1].Index, m.Groups[1].Length,
							stringValue: text);

						end += text.Length;
					}
				}

				cdata.Value = wrapper.GetInnerXml();
				return end;
			}

			return -1;
		}


		/// <summary>
		/// Highlights a single match as described by the given MatchInfo, modifying the T
		/// element in place. Used by the step-through session of Search and Highlight.
		/// </summary>
		/// <returns>
		/// The index within the run's text just after the highlighted text, or -1 if the
		/// match could not be highlighted
		/// </returns>
		public int HighlightMatch(MatchInfo match)
		{
			// never touch mathML equations
			if (highlightColor is null || match.TElement.IsMathML())
			{
				return -1;
			}

			var cdata = match.TElement.GetCData();
			if (cdata.Value.Length == 0) return -1;

			var wrapper = cdata.GetWrapper();
			var rawtext = wrapper.Value.Replace(' ', ' ');

			foreach (Match m in regex.Matches(rawtext))
			{
				if (m.Index != match.StartIndex || m.Length != match.Length) continue;

				var index = m.Groups.Count > 1 ? m.Groups[1].Index : m.Index;
				var length = m.Groups.Count > 1 ? m.Groups[1].Length : m.Length;
				if (length == 0)
				{
					return index;
				}

				Highlight(wrapper, index, length);
				cdata.Value = wrapper.GetInnerXml();
				return index + length;
			}

			return -1;
		}
	}
}