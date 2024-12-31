//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using NStandard;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Reads, extracts, or edits content on a Page.
	/// </summary>
	internal class PageEditor : Loggable
	{
		private sealed class Snippet
		{
			public XElement Element;  // OE containing selected T run
			public int Depth;         // depth from Outline
		}


		private readonly Page page;
		private readonly XNamespace ns;
		private PageSchema schema;


		public PageEditor(Page page)
		{
			this.page = page;
			ns = page.Namespace;
		}


		/// <summary>
		/// Gets or sets and indication whether all page content should be chosen.
		/// The default is to process only the selected content.
		/// </summary>
		public bool AllContent { get; set; }


		/// <summary>
		/// Gets the anchor point for reintroducing collated content.
		/// If this is an OE or HTMLBlock then consumers may want to insert after that.
		/// Otherwise, consumers may want to insert at end of anchor container.
		/// </summary>
		public XElement Anchor { get; private set; }


		/// <summary>
		/// Gets or sets a Boolean indicating whether to maintain the selected state of
		/// extracted content. Default is to remove selected state.
		/// </summary>
		public bool KeepSelected { get; set; }


		/// <summary>
		/// Signals EditSelected(), EditNode() and, by dependency, GetSelectedText() methods
		/// that editor scanning should be done in reverse doc-order. This must be set prior
		/// to calling one of those method to take effect.
		/// </summary>
		public bool ReverseScanning { get; set; }


		/// <summary>
		/// Adds the given content after the selected insertion point; this will not
		/// replace selected regions.
		/// </summary>
		/// <param name="content">The content to add</param>
		public void AddNextParagraph(XElement content)
		{
			InsertParagraph(content, false);
		}


		public void AddNextParagraph(params XElement[] content)
		{
			// consumer will build content array in document-order but InsertParagraph inserts
			// just prior to the insertion point which will reverse the order of content items
			// so insert them in reverse order intentionally so they show up correctly
			for (var i = content.Length - 1; i >= 0; i--)
			{
				InsertParagraph(content[i], false);
			}
		}


		/// <summary>
		/// Removes the selected attribute from the page
		/// </summary>
		public void Deselect(XElement root = null)
		{
			// clean up selected attributes; keep only select snippets

			(root ?? page.Root).Descendants().Attributes()
				.Where(a => a.Name == "selected")
				.Remove();
		}


		public void FollowWithCurosr(XElement root)
		{
			var last = root.Descendants()
				.Attributes("selected")
				.Where(a => a.Value == "all")
				.Select(a => a.Parent)
				.LastOrDefault();

			if (last is not null)
			{
				Deselect(root);

				// Within an OE, you're allowed one image, one table, inserted file,
				// or a mix of Ink and Text pieces...

				if (last.Name.LocalName.In("T", "InkWord"))
				{
					if (last.GetCData().Value == string.Empty)
					{
						last.SetAttributeValue("selected", "all");
					}
					else
					{
						last.AddAfterSelf(new XElement(ns + "T",
							new XAttribute("selected", "all"),
							new XCData(string.Empty))
							);
					}
				}
				else
				{
					// selected item might be Image, so move up to Parent
					if (last.Parent.Name.LocalName == "OE")
					{
						last = last.Parent;
					}

					last.AddAfterSelf(new XElement(ns + "OE",
						new XElement(ns + "T",
							new XAttribute("selected", "all"),
							new XCData(string.Empty))
						));
				}
			}
		}


		/// <summary>
		/// Gets the currently selected text. If the text cursor is positioned over a word but
		/// with zero selection length then that word is returned; othewise, text from the selected
		/// region is returned.
		/// </summary>
		/// <returns>A string of the selected text</returns>
		public string GetSelectedText()
		{
			var builder = new StringBuilder();

			// not editing... just using EditSelected to extract the current text,
			// ignoring inline span styling

			EditSelected((s) =>
			{
				if (s is XText text)
				{
					if (ReverseScanning)
						builder.Insert(0, text.Value);
					else
						builder.Append(text.Value);
				}
				else if (s is not XComment)
				{
					if (ReverseScanning)
						builder.Insert(0, ((XElement)s).Value);
					else
						builder.Append(((XElement)s).Value);
				}

				return s;
			});

			return builder.ToString();
		}


		/// <summary>
		/// Inserts the given content immediately after the anchor point. This might be after
		/// the anchor element if the anchor is an OE or HTMLBlock or as its first element
		/// if the anchor is an OEChildren or Outline.
		/// </summary>
		/// <param name="content">The content to insert.</param>
		public void InsertAtAnchor(XElement content)
		{
			if (Anchor.Name.LocalName.In("OE", "HTMLBlock", "ChildOELayout"))
			{
				Anchor.AddAfterSelf(content.Name.LocalName.In("OE", "HTMLBlock")
					? content
					: new XElement(ns + "OE", content));
			}
			else if (Anchor.Name.LocalName == "OEChildren")
			{
				Anchor.AddFirst(content.Name.LocalName.In("OE", "HTMLBlock")
					? content
					: new XElement(ns + "OE", content));
			}
			else if (Anchor.Name.LocalName == "Outline")
			{
				if (content.Name.LocalName == "OEChildren")
				{
					Anchor.AddFirst(content);
				}
				else if (content.Name.LocalName == "OE")
				{
					Anchor.AddFirst(new XElement(ns + "OEChildren", content));
				}
				else
				{
					Anchor.AddFirst(new XElement(ns + "OEChildren", new XElement(ns + "OE", content)));
				}
			}
		}


		/// <summary>
		/// Adds the given content immediately before or after the selected insertion point;
		/// this will not replace selected regions.
		/// </summary>
		/// <param name="content">The content to insert</param>
		/// <param name="before">
		/// If true then insert before the insertion point; otherwise insert after the insertion point
		/// </param>
		public void InsertParagraph(XElement content, bool before = true)
		{
			// TODO: 7/29/24 How does this compare to InsertAtAnchor?!

			// TODO: 7/29/24 when called consecutively, e.g. within a loop, this will continually
			// find the selected anchor. Should it track the last inserted OE so subsequent calls
			// can reference from there? Would need a correlation-id/context-ticket thing...

			var current = page.Root.Descendants(ns + "OE")
				.LastOrDefault(e =>
					e.Elements(ns + "T").Attributes("selected").Any(a => a.Value == "all"));

			if (current != null)
			{
				if (content.Name.LocalName != "OE")
				{
					content = new XElement(ns + "OE", content);
				}

				if (before)
					current.AddBeforeSelf(content);
				else
					current.AddAfterSelf(content);
			}
		}


		/// <summary>
		/// Given some text, insert it at the current cursor location or replace the selected
		/// text on the page.
		/// </summary>
		/// <param name="text">The text to insert</param>
		public void InsertOrReplace(string text)
		{
			var range = new SelectionRange(page);
			range.GetSelections(allowPageTitle: true);

			if (range.Scope == SelectionScope.Range ||
				range.Scope == SelectionScope.Run)
			{
				// replace region
				ReplaceSelectedWith(new XElement(ns + "T", new XCData(text)));
			}
			else if (range.Scope == SelectionScope.SpecialCursor)
			{
				// do not replace hyperlink/MathML!
				// impossible to determine exact cursor location so add immediately before
				InsertParagraph(new XElement(ns + "T", new XCData(text)), true);
			}
			else if (
				range.Scope != SelectionScope.TextCursor)
			{
				// can't find cursor so append to page
				var container = page.EnsureContentContainer();
				container.Add(new XElement(ns + "T", new XCData(text)));
			}
			else
			{
				// empty text cursor, simple insert...

				var line = page.Root.Descendants(ns + "T")
					.FirstOrDefault(e =>
						e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

				if (line is null)
				{
					// this case should not happen; should be handled above
					var content = new XElement(ns + "T", new XCData(text));
					InsertParagraph(content, false);
				}
				else
				{
					if (line.FirstAncestor(ns + "Title", ns + "Outline") is XElement title)
					{
						// special case to insert before page Title, used by InsertDate;
						// if cursor is before first char of title, the entire title is "selected"
						// so rather than replace the title, just insert before it
						var first = title.Elements(ns + "OE").Elements(ns + "T").First();
						var cdata = first.GetCData();
						cdata.Value = $"{text} {cdata.Value}";
					}
					else if (line.Value.Length == 0)
					{
						// empty cdata, unselected cursor so just insert
						line.GetCData().Value = text;
					}
					else
					{
						// this case should not happen; should be handled above
						// something is selected so replace it
						ReplaceSelectedWith(new XElement(ns + "T", new XCData(text)));
					}
				}
			}
		}


		/// <summary>
		/// Given content, insert above/below or replace selected content, possibly splitting
		/// the current paragraph to insert one or more new paragraphs.
		/// </summary>
		/// <param name="content">
		/// The content to insert. This can be any valid child of Outline, relative to the
		/// current text cursor context.
		/// </param>
		/// <param name="above">
		/// True to insert above, otherwise below. This only applies when there is no
		/// selection range to replace, otherwise the selected content is replace in-place
		/// and this parameter is ignored.
		/// </param>
		public bool InsertOrReplace(XElement content, bool above = true)
		{
			var range = new SelectionRange(page);
			_ = range.GetSelections(allowPageTitle: true);

			//var selections = range.GetSelections(allowPageTitle: true);
			//if (!selections.Any() && range.Scope == SelectionScope.TextCursor)
			//{
			//	// cursor focus on neither body nor title
			//	return false;
			//}

			//if (cursor)
			return true;
		}


		/// <summary>
		/// Replaces the selected range on the page with the given content, keeping
		/// the cursor after the newly inserted content.
		/// <para>
		/// This attempts to replicate what Word might do when pasting content in a
		/// document with a selection range.
		/// </para>
		/// </summary>
		/// <param name="content">The content to insert</param>
		public SelectionScope ReplaceSelectedWith(XElement content)
		{
			var elements = page.Root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if ((elements.Count() == 1) &&
				(elements.First().GetCData().Value.Length == 0))
			{
				// zero-width selection so insert just before cursor
				elements.First().AddBeforeSelf(content);
				return SelectionScope.TextCursor;
			}

			// replace one or more [one:T @select=all] with status, placing cursor after
			var element = elements.Last();
			element.AddAfterSelf(content);
			elements.Remove();

			content.AddAfterSelf(new XElement(ns + "T",
				new XAttribute("selected", "all"),
				new XCData(string.Empty)
				));

			return SelectionScope.Range;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Editors
		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region Editors
		/// <summary>
		/// Invokes an edit function on the selected text. The selection may be either infered
		/// from the current cursor position or explicitly highlighted as a selected region.
		/// No assumptions are made as to the resultant content or the order in which parts of
		/// context are edited.
		/// </summary>
		/// <param name="edit">
		/// A Func that accepts an XNode and returns an XNode. The accepted XNode is either an
		/// XText or a "span" XElement. The returned XNode can be either the original unmodified,
		/// the original modified, or a new XNode. Regardless, the returned XNode will replace
		/// the current XNode in the content.
		/// </param>
		/// <returns></returns>
		public bool EditSelected(Func<XNode, XNode> edit)
		{
			var range = new SelectionRange(page);
			var selections = range.GetSelections(allowPageTitle: true);

			if (range.Scope == SelectionScope.TextCursor ||
				range.Scope == SelectionScope.SpecialCursor)
			{
				return EditNode(selections.First(), edit);
			}

			return EditSelected(page.Root, edit);
		}


		public bool EditSelected(XElement root, Func<XNode, XNode> edit)
		{
			// detect all selected text (cdata within T runs)
			var cdatas = root.Descendants(ns + "T")
				.Where(e => e.Attributes("selected").Any(a => a.Value == "all")
					&& e.FirstNode?.NodeType == XmlNodeType.CDATA)
				.Select(e => e.FirstNode as XCData);

			if (!cdatas.Any())
			{
				return false;
			}

			foreach (var cdata in cdatas)
			{
				// edit every XText and SPAN in the T wrapper
				var wrapper = cdata.GetWrapper();

				// use ToList, otherwise enumeration will stop after first ReplaceWith
				foreach (var node in wrapper.Nodes().ToList())
				{
					node.ReplaceWith(edit(node));
				}

				var text = wrapper.GetInnerXml();

				// special case for <br> + EOL
				text = text.Replace("<br>", "<br>\n");

				// build CDATA with editing content
				cdata.Value = text;
			}

			return true;
		}


		public bool EditNode(XElement cursor, Func<XNode, XNode> edit)
		{
			var updated = false;

			// T elements can only be a child of an OE but can also have other T siblings...
			// Each T will have one CDATA with one or more XText and SPAN XElements.
			// OneNote handles nested spans by normalizing them into consecutive spans

			// Just FYI, because we use XElement.Parse to build the DOM, XText nodes will be
			// singular but may be surrounded by SPAN elements; i.e., there shouldn't be two
			// consecutive XText nodes next to each other

			// indicate to GetSelectedText() that we're scanning in reverse
			ReverseScanning = true;

			// is there a preceding T?
			if ((cursor.PreviousNode is XElement prev) && !prev.GetCData().EndsWithWhitespace())
			{
				var cdata = prev.GetCData();
				var wrapper = cdata.GetWrapper();
				var nodes = wrapper.Nodes().ToList();

				// reverse, extracting text and stopping when matching word delimiter
				for (var i = nodes.Count - 1; i >= 0; i--)
				{
					if (nodes[i] is XText text)
					{
						// ends with delimiter so can't be part of current word
						if (text.Value.EndsWithWhitespace())
							break;

						// extract last word and pump through the editor
						var pair = text.Value.SplitAtLastWord();
						if (pair.Item1 == null)
						{
							// entire content of this XText
							edit(text);
						}
						else
						{
							// last word of this XText
							text.Value = pair.Item2;
							text.AddAfterSelf(edit(new XText(pair.Item1)));
						}

						// remaining text has a word delimiter
						if (text.Value.StartsWithWhitespace())
							break;
					}
					else if (nodes[i] is XElement span)
					{
						// ends with delimiter so can't be part of current word
						if (span.Value.EndsWithWhitespace())
							break;

						// extract last word and pump through editor
						var word = span.ExtractLastWord();
						if (word == null)
						{
							// edit entire contents of SPAN
							edit(span);
						}
						else
						{
							// last word of this SPAN
							var spawn = new XElement(span.Name, span.Attributes(), word);
							edit(spawn);
							span.AddAfterSelf(spawn);
						}

						// remaining text has a word delimiter
						if (span.Value.StartsWithWhitespace())
							break;
					}
				}

				// rebuild CDATA with edited content
				cdata.Value = wrapper.GetInnerXml();
				updated = true;
			}

			// indicate to GetSelectedText() that we're scanning forward
			ReverseScanning = false;

			// is there a following T?
			if ((cursor.NextNode is XElement next) && !next.GetCData().StartsWithWhitespace())
			{
				var cdata = next.GetCData();
				var wrapper = cdata.GetWrapper();
				var nodes = wrapper.Nodes().ToList();

				// extract text and stop when matching word delimiter
				for (var i = 0; i < nodes.Count; i++)
				{
					if (nodes[i] is XText text)
					{
						// starts with delimiter so can't be part of current word
						if (text.Value.StartsWithWhitespace())
							break;

						// extract first word and pump through editor
						var pair = text.Value.SplitAtFirstWord();
						if (pair.Item1 == null)
						{
							// entire content of this XText
							edit(text);
						}
						else
						{
							// first word of this XText
							text.Value = pair.Item2;
							text.AddBeforeSelf(edit(new XText(pair.Item1)));
						}

						// remaining text has a word delimiter
						if (text.Value.EndsWithWhitespace())
							break;
					}
					else if (nodes[i] is XElement span)
					{
						// ends with delimiter so can't be part of current word
						if (span.Value.StartsWithWhitespace())
							break;

						// extract first word and pump through editor
						var word = span.ExtractFirstWord();
						if (word == null)
						{
							// eidt entire contents of SPAN
							edit(span);
						}
						else
						{
							// first word of this SPAN
							var spawn = new XElement(span.Name, span.Attributes(), word);
							edit(spawn);
							span.AddBeforeSelf(spawn);
						}

						// remaining text has a word delimiter
						if (span.Value.EndsWithWhitespace())
							break;
					}
				}

				// rebuild CDATA with edited content
				cdata.Value = wrapper.GetInnerXml();
				updated = true;
			}

			return updated;
		}
		#endregion Editors


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Extractors
		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region Extractors

		/// <summary>
		/// Extracts all selected content as a single OEChildren element, preserving relative
		/// indents, table content, etc. The selected content is removedd from the page.
		/// </summary>
		/// <param name="targetOutline">
		/// Optional Outline element to process.
		/// Default is to process all page content.
		/// </param>
		/// <returns>An OEChildren XElement</returns>
		public XElement ExtractSelectedContent(XElement targetOutline = null)
		{
			schema = new PageSchema();

			var content = new XElement(ns + "OEChildren");

			// An OE can contain either a sequence of T elements (or any in OEContent) or
			// one of Image, Table, InkDrawing, InsertedFile, MediaFile, InkParagraph.
			// These may by led by a sequence of MediaIndex, Tag, Meta, List.

			// IMPORTANT: Within an OE, no more than exactly one OEContent element
			// can be selected at a time!

			var paragraphs = targetOutline is null
				? page.Root.Elements(ns + "Outline").Descendants(ns + "OE")
				: targetOutline.Descendants(ns + "OE");

			// we only need first-gen content elements of the OE; will dive into OEChildren
			// later and tables are handled separately, ~= runs|img|ink|files|tables|children
			var excludes = schema.OeHeaders.Concat(new[] { "Table", "OEChildren" }).ToArray();

			var runs = paragraphs
				.Elements()
				.Where(e => !e.Name.LocalName.In(excludes) &&
					(AllContent || e.Attributes().Any(a => a.Name == "selected" && a.Value == "all")))
				.ToList();

			if (runs.Any() && AllContent)
			{
				// filter out the blank cursor to avoid inadvertently inserting a newline
				runs = runs.Except(runs.Where(e =>
					e.Attributes().Any(a => a.Name == "selected" && a.Value == "all") &&
					e.GetCData().Value == string.Empty))
					.ToList();
			}

			// no selections found in body; could be possible if only caret text cursor
			if (!runs.Any())
			{
				runs = paragraphs.Elements()
					.Where(e => !e.Name.LocalName.In(excludes))
					.ToList();

				if (!runs.Any())
				{
					Anchor = null;
					return content;
				}
			}

			FindBestAnchorPoint(runs[0]);

			//var oid = Anchor.Name.LocalName == "OE"
			//	? Anchor.Attribute("objectID").Value
			//	: Anchor.Elements(ns + "OE").Select(e => e.Attribute("objectID").Value).FirstOrDefault();
			//logger.WriteLine($"first anchor ({oid})", Anchor);

			var snippets = ExtractSnippets(runs);
			RebuildContent(snippets, content);

			//logger.WriteLine($"content", content);

			CleanupOrphanedElements();

			return content;
		}


		private void FindBestAnchorPoint(XElement start)
		{
			// start might be a OEContent element or OE or OEChildren, so handle them all...

			if (start.Parent.Name.LocalName == "OE")
			{
				// move up from T to at least OE
				start = start.Parent;
			}

			if (start.Name.LocalName.In("OE", "OEChildren"))
			{
				// find best anchor, either prev node (OE/HTMLBlock) or closet parent (OEChildren)
				Anchor = start.PreviousNode is XElement prev
					? prev
					: start.Parent;

				if (Anchor.Name.LocalName.In(schema.OutHeaders) &&
					Anchor.Parent.Name.LocalName == "Outline")
				{
					Anchor = Anchor.Parent;
				}
			}

			// if a descendent of a Cell then check if the entire table is selected and
			// adjust the anchor point based on that, otherwise we loose the table
			var celement = Anchor.Name.LocalName == "Cell"
				? Anchor
				: Anchor.FirstAncestor(ns + "Cell");

			if (celement is not null)
			{
				if (WholeTableSelected(celement) is XElement table)
				{
					Anchor = table.Parent.PreviousNode is XElement prev
						? prev
						: table.Parent.Parent;
				}
			}
		}


		private XElement WholeTableSelected(XElement cell)
		{
			var table = cell.FirstAncestor(ns + "Table");
			if (AllContent ||
				table.Descendants(ns + "Cell").All(c => c.Attribute("selected")?.Value == "all"))
			{
				return table;
			}

			return null;
		}


		private List<Snippet> ExtractSnippets(List<XElement> runs)
		{
			var tables = new List<XElement>();

			var snippets = new List<Snippet>();

			// Runs are captured in document-order so, by reversing this collection, we should
			// be able discard nested elements as they are emptied; otherwise, doing this in
			// document-order would make it very difficult to look-ahead into child elements
			// to determine if this element should be discarded.
			runs.Reverse();

			// List elements will be handled inline along with their associated content
			foreach (var run in runs.Where(e => e.Name.LocalName != "List"))
			{
				var snippet = new Snippet
				{
					Depth = IndentLevel(run)
				};

				var prev = run.PreviousNode as XElement;
				var next = run.NextNode as XElement;
				XElement parent = null;

				var fullParagraph =
					// anything after this content run means there is more content so not alone
					next is null &&
					// header elements don't count towards "content" (or should they!?)
					(prev is null || prev.Name.LocalName.In(schema.OeHeaders));

				if (fullParagraph)
				{
					// the select content covers the entire OE paragraph...

					var cell = run.FirstAncestor(ns + "Cell");
					if (cell is not null &&
						WholeTableSelected(cell) is XElement table)
					{
						if (!tables.Contains(table))
						{
							snippet.Element = new XElement(ns + "OE",
							table.Parent.Attributes()
								.Where(a => !a.Name.LocalName.In("objectID", "selected"))
							);

							parent = table.Parent;
							table.Remove();
							tables.Add(table);
							snippet.Element.Add(table);
						}
					}
					else
					{
						parent = run.Parent?.Parent;
						if (cell is not null)
							parent = null;

						snippet.Element = run.Parent;
						run.Parent.Remove();
					}
				}
				else
				{
					// the selected content is a portion of the OE paragraph so tease apart...

					var oe = run.Parent;

					// copy parent attributes to preserve quickstyles
					snippet.Element = new XElement(ns + oe.Name.LocalName,
						oe.Attributes().Where(a => !a.Name.LocalName.In("objectID", "selected"))
						);

					// preserve media index for inserted objects
					if (run.Name.LocalName.In("InsertedFile", "MediaFile") &&
						oe.Elements(ns + "MediaIndex") is XElement index)
					{
						// can only be one inserted object so remove the index here
						index.Remove();
						snippet.Element.Add(index);
					}

					// copy list configuration if any
					if (oe.Elements(ns + "List").FirstOrDefault() is XElement list)
					{
						snippet.Element.Add(
							XElement.Parse(list.ToString(SaveOptions.DisableFormatting)));
					}

					snippet.Element.Add(run);

					run.Remove();
				}

				if (parent is not null)
				{
					CleanupEmptyElements(parent);
				}

				if (snippet.Element is not null)
				{
					// when moving an Image from a page loaded with Basic or Selection
					// scope, it can get unwired internally and loose its data, so explicitly
					// replace the CallbackID with raw Base64 Data
					if (run.Name.LocalName == "Image" &&
						run.Elements(ns + "CallbackID").FirstOrDefault() is XElement callback)
					{
						using var one = new OneNote();
						var data = one.GetPageContent(page.PageId,
							callback.Attribute("callbackID").Value);

						callback.ReplaceWith(new XElement(ns + "Data", data));
					}

					snippets.Insert(0, snippet);
				}
			}

			return snippets;
		}


		private void CleanupEmptyElements(XElement element)
		{
			string[] headerNames;
			string[] contentNames;
			bool empty;

			void SetContext()
			{
				if (element.Name.LocalName == "OE")
				{
					headerNames = schema.OeHeaders;
					contentNames = schema.OeContent;
				}
				else
				{
					headerNames = schema.OecHeaders;
					contentNames = schema.OecContent;
				}

				empty =
					!element.HasElements ||
					!element.Elements().Any(e => e.Name.LocalName.In(contentNames));
			}

			SetContext();

			while (empty && !element.Name.LocalName.In("Cell", "Outline"))
			{
				if (Anchor == element)
				{
					FindBestAnchorPoint(element);
				}

				if (!element.Parent.Name.LocalName.In("Cell", "Outline"))
				{
					var trash = element;

					element = element.Parent;
					trash.Remove();
				}
				else
				{
					break;
				}

				SetContext();
			}

			// ensure we haven't lost our anchor
			if (Anchor.Name.LocalName == "Outline")
			{
				if (Anchor.Elements(ns + "OEChildren").FirstOrDefault() is XElement child)
				{
					Anchor = child;
				}
				else
				{
					Anchor = new XElement(ns + "OEChildren");
					element.Add(Anchor);
				}
			}
		}


		private static int IndentLevel(XElement element)
		{
			if (element is null || element.Parent is null)
			{
				return 0;
			}

			var count = 0;
			var node = element.Parent;

			// indents are relative to their closest container, Outline or Cell
			while (!node.Name.LocalName.In("Outline", "Cell") &&
				node.Parent is not null && !node.Parent.Name.LocalName.In("Outline", "Cell"))
			{
				node = node.Parent;
				if (node.Name.LocalName == "OEChildren")
				{
					count++;
				}
			}

			return count;
		}


		private void RebuildContent(List<Snippet> snippets, XElement content)
		{
			// construct a hierarchy of content based on snippet indent levels...

			var current = content;
			var depth = 1;
			foreach (var snippet in snippets)
			{
				logger.Debug($"snippet depth={snippet.Depth}:{depth} [{snippet.Element.Value}]");

				if (snippet.Depth > depth)
				{
					// indent...
					depth++;

					var child = current.Elements(ns + "OE").LastOrDefault();
					if (child is null)
					{
						child = new XElement(ns + "OE");
						current.Add(child);
					}

					var children = new XElement(ns + "OEChildren");
					child.Add(children);

					current = children;
				}
				else
				{
					while (snippet.Depth < depth)
					{
						// outdent...
						current = current.Parent.Parent;
						depth--;
					}
				}

				if (snippet.Element.Attribute("objectID") is XAttribute attribute)
				{
					attribute.Remove();
				}

				current.Add(snippet.Element);
			}
		}


		private void CleanupOrphanedElements()
		{
			// clean up orphaned OE elements
			IEnumerable<XElement> items;

			items = page.Root.Descendants(ns + "OE")
				.Where(e => e != Anchor &&
					!e.Elements().Any(c => c.Name.LocalName.In(schema.OeContent)));

			//logger.WriteLine($"cleaning ~~> {(items.Any() ? items.Count() : 0)} OE");
			items.Remove();

			// clean up orphaned OEChildren elements
			items = page.Root.Descendants(ns + "OEChildren")
				.Where(e => e != Anchor && !e.Elements().Any());

			//logger.WriteLine($"cleaning ~~> {(items.Any() ? items.Count() : 0)} OEChildren");
			items.Remove();

			if (!KeepSelected)
			{
				// clean up selected attributes; keep only select snippets
				Deselect();
			}

			// patch any empty cells, cheap but effective!
			foreach (var item in page.Root.Descendants(ns + "Cell")
				.Where(e => !e.Elements().Any()))
			{
				//logger.WriteLine("cleaning ~~> default cell");
				new TableCell(item).SetContent(string.Empty);
			}

			var outline = page.EnsureContentContainer();
			if (Anchor.Parent is null)
			{
				Anchor = outline;
			}

			if (Anchor.Name.LocalName == "OE")
			{
				var list = Anchor.Elements(ns + "List").FirstOrDefault();
				if (list is not null)
				{
					if (!Anchor.Elements().Any(e => e.Name.LocalName.In("T", "InkWord")))
					{
						//logger.WriteLine("cleaning ~~> orphaned List");
						list.Remove();
					}
				}
			}
			else if (Anchor.Name.LocalName.In(schema.OutHeaders))
			{
				// if whole table is boxed at top of page then anchor might be an OHeader
				// so position after headers on next available OEChildren

				Anchor = page.EnsureContentContainer(false);
			}
		}


		public bool ReplaceSelectedContent(XElement replacement)
		{
			var content = ExtractSelectedContent();

			if (!content.HasElements)
			{
				// no selection region found
				return false;
			}

			if (replacement.Name.LocalName != "OE")
			{
				replacement = new XElement(ns + "OE", replacement);
			}

			if (Anchor.Name.LocalName.In("OE", "HTMLBlock"))
			{
				Anchor.AddAfterSelf(replacement);
			}
			else
			{
				Anchor.AddFirst(replacement);
			}

			return true;
		}

		#endregion Extractors
	}
}
