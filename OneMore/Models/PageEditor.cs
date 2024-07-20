﻿//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
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
		private readonly string[] OHeaders;
		private readonly string[] OECHeaders;
		private readonly string[] OECContent;
		private readonly string[] OEHeaders;
		private readonly string[] OEContent;


		public PageEditor(Page page)
		{
			this.page = page;
			ns = page.Namespace;

			// OE Schema...

			OEHeaders = new string[]
			{
				"MediaIndex", "Tag", "OutlookTask"/*+Tag?*/, "Meta", "List"
			};

			OEContent = new string[]
			{
				"Image", "Table", "InkDrawing", "InsertedFile",
				"MediaFile", "InkParagraph", "FutureObject",
				"T", "InkWord",
				"OEChildren",
				"LinkedNote"
			};

			// OEChildren Schema...

			OECHeaders = new string[]
			{
				"ChildOELayout"
			};

			OECContent = new string[]
			{
				"OE", "HTMLBlock"
			};

			// Outline Schema...

			OHeaders = new string[]
			{
				"Postion", "Size", "Meta", "Indents"
			};
			// - OContent: OEChildren
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
		/// Given some text, insert it at the current cursor location or replace the selected
		/// text on the page.
		/// </summary>
		/// <param name="text">The text to insert</param>
		public void InsertOrReplace(string text)
		{
			var cursor = page.GetTextCursor(allowPageTitle: true);

			if (page.SelectionScope == SelectionScope.Region || page.SelectionSpecial)
			{
				// replace region or hyperlink/MathML
				var content = new XElement(ns + "T", new XCData(text));
				page.ReplaceSelectedWithContent(content);
			}
			else if (cursor == null) // && page.SelectionScope == SelectionScope.Empty)
			{
				// can't find cursor so append to page
				var content = new XElement(ns + "T", new XCData(text));
				page.AddNextParagraph(content);
			}
			else
			{
				var line = page.Root.Descendants(ns + "T")
					.FirstOrDefault(e =>
						e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

				if (line is null)
				{
					// this case should not happen; should be handled above
					var content = new XElement(ns + "T", new XCData(text));
					page.AddNextParagraph(content);
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
						var content = new XElement(ns + "T", new XCData(text));
						page.ReplaceSelectedWithContent(content);
					}
				}
			}
		}


		/// <summary>
		/// Extracts all selected content as a single OEChildren element, preserving relative
		/// indents, table content, etc. The selected content is removedd from the page.
		/// </summary>
		/// <param name="targetOutline">
		/// Optional Outline element to process.
		/// Default is to process all page content.
		/// </param>
		/// <returns>An OEChildren XElement</returns>
		public async Task<XElement> ExtractSelectedContent(XElement targetOutline = null)
		{
			var content = new XElement(ns + "OEChildren");
			//	new XAttribute(XNamespace.Xmlns + OneNote.Prefix, ns.ToString())
			//);

			// An OE can contain either a sequence of T elements (or any in OEContent) or
			// one of Image, Table, InkDrawing, InsertedFile, MediaFile, InkParagraph.
			// These may by led by a sequence of MediaIndex, Tag, Meta, List.

			// IMPORTANT: Within an OE, no more than exactly one OEContent element
			// can be selected at a time!

			var paragraphs = targetOutline is null
				? page.Root.Elements(ns + "Outline").Descendants(ns + "OE")
				: targetOutline.Descendants(ns + "OE");

			var runs = paragraphs
				.Elements()
				.Where(e =>
					// tables are handled via their cell contents
					e.Name.LocalName != "Table" &&
					e.Name.LocalName != "OEChildren" &&
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

			// no selections found in body
			if (!runs.Any())
			{
				Anchor = null;
				return content;
			}

			FindBestAnchorPoint(runs[0]);

			//var oid = Anchor.Name.LocalName == "OE"
			//	? Anchor.Attribute("objectID").Value
			//	: Anchor.Elements(ns + "OE").Select(e => e.Attribute("objectID").Value).FirstOrDefault();
			//logger.WriteLine($"first anchor ({oid})", Anchor);

			var snippets = await ExtractSnippets(runs);
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

				if (Anchor.Name.LocalName.In(OHeaders) &&
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


		private async Task<List<Snippet>> ExtractSnippets(List<XElement> runs)
		{
			await using var one = new OneNote();
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
					(prev is null || prev.Name.LocalName.In(OEHeaders));

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
					headerNames = OEHeaders;
					contentNames = OEContent;
				}
				else
				{
					headerNames = OECHeaders;
					contentNames = OECContent;
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


		private int IndentLevel(XElement element)
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
				else if (snippet.Depth < depth)
				{
					// outdent...
					current = current.Parent.Parent;
					depth--;
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
					!e.Elements().Any(c => c.Name.LocalName.In(OEContent)));

			//logger.WriteLine($"cleaning ~~> {(items.Any() ? items.Count() : 0)} OE");
			items.Remove();

			// clean up orphaned OEChildren elements
			items = page.Root.Descendants(ns + "OEChildren")
				.Where(e => e != Anchor && !e.Elements().Any());

			//logger.WriteLine($"cleaning ~~> {(items.Any() ? items.Count() : 0)} OEChildren");
			items.Remove();

			// clean up selected attributes; keep only select snippets
			page.Root.DescendantNodes().OfType<XAttribute>()
				.Where(a => a.Name.LocalName == "selected")
				.Remove();

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
			else if (Anchor.Name.LocalName.In(OHeaders))
			{
				// if whole table is boxed at top of page then anchor might be an OHeader
				// so position after headers on next available OEChildren

				Anchor = page.EnsureContentContainer(false);
			}
		}


		public async Task<bool> ReplaceSelectedContent(XElement replacement)
		{
			var content = await ExtractSelectedContent();

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
	}
}
