//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Builds and applies snippets stored as OneNote page XML. A snippet package is a
	/// miniature page that contains only the selected content plus the QuickStyleDef and
	/// TagDef elements that the content references, so that all formatting is preserved
	/// and can be remapped onto any target page.
	/// </summary>
	internal static class SnippetPackage
	{
		public const string KindRuns = "runs";
		public const string KindParagraphs = "paragraphs";
		public const string KindTable = "table";

		private const string KindMeta = "omSnippetKind";
		public const string BodyTag = SnippetMacros.BodyTag;


		//========================================================================================
		// Build
		//========================================================================================

		/// <summary>
		/// Builds a snippet package from the selected content of the given page. The page
		/// itself is not modified.
		/// </summary>
		/// <param name="page">A page loaded with PageDetail.All</param>
		/// <param name="scope">The selection scope of the page</param>
		/// <returns>The package root element or null if there is no selected content</returns>
		public static XElement Build(Page page, SelectionScope scope)
		{
			var ns = page.Namespace;

			// the extractor removes selected content from the tree it is given
			// so work on a throw-away copy of the page
			var clone = new Page(XElement.Parse(page.Root.ToString(SaveOptions.DisableFormatting)));

			XElement content;
			string kind;

			SelectCollapsedChildren(clone, ns);

			var cells = ExtractCells(clone, ns);
			if (cells is not null)
			{
				content = new XElement(ns + "OEChildren", new XElement(ns + "OE", cells));
				kind = KindTable;
			}
			else
			{
				var editor = new PageEditor(clone);
				content = editor.ExtractSelectedContent(breakParagraph: scope != SelectionScope.Run);
				if (!content.HasElements)
				{
					return null;
				}

				kind = DetermineKind(content, ns, scope);
			}

			content.DescendantsAndSelf().Attributes("objectID").Remove();
			content.DescendantsAndSelf().Attributes("selected").Remove();

			var root = new XElement(ns + "Page",
				new XAttribute(XNamespace.Xmlns + OneNote.Prefix, ns));

			var tags = new HashSet<string>(content.Descendants(ns + "Tag")
				.Select(e => e.Attribute("index")?.Value)
				.Where(v => v is not null));

			root.Add(clone.Root.Elements(ns + "TagDef")
				.Where(e => tags.Contains(e.Attribute("index")?.Value))
				.Select(e => new XElement(e)));

			var styles = new HashSet<string>(content.DescendantsAndSelf()
				.Select(e => e.Attribute("quickStyleIndex")?.Value)
				.Where(v => v is not null));

			root.Add(clone.Root.Elements(ns + "QuickStyleDef")
				.Where(e => styles.Contains(e.Attribute("index")?.Value))
				.Select(e => new XElement(e)));

			root.Add(new XElement(ns + "Meta",
				new XAttribute("name", KindMeta),
				new XAttribute("content", kind)));

			root.Add(new XElement(ns + "Outline", content));

			return root;
		}


		/// <summary>
		/// OneNote does not mark the content hidden inside a collapsed paragraph as selected
		/// when the paragraph itself is selected, so the extractor would drop those children.
		/// For each selected collapsed paragraph, select all of its hidden descendants so they
		/// travel with it, preserving the collapsed state of the paragraph.
		/// </summary>
		private static void SelectCollapsedChildren(Page clone, XNamespace ns)
		{
			var schema = new PageSchema();
			var skip = new[] { "OEChildren", "Table" };

			var collapsed = clone.Root.Elements(ns + "Outline").Descendants(ns + "OE")
				.Where(oe => oe.Attribute("collapsed")?.Value is "1" or "true" &&
					oe.Elements().Any(e => e.Name.LocalName.In(schema.OeContent) &&
						!e.Name.LocalName.In(skip) &&
						e.Attribute("selected")?.Value == "all"))
				.ToList();

			foreach (var oe in collapsed)
			{
				foreach (var child in oe.Elements(ns + "OEChildren"))
				{
					foreach (var e in child.Descendants())
					{
						var name = e.Name.LocalName;
						if (name == "Cell" ||
							(name.In(schema.OeContent) && !name.In(skip)))
						{
							e.SetAttributeValue("selected", "all");
						}
					}
				}
			}
		}


		private static string DetermineKind(XElement content, XNamespace ns, SelectionScope scope)
		{
			var oes = content.Elements(ns + "OE").ToList();
			if (oes.Count == 1)
			{
				var oe = oes[0];
				var schema = new PageSchema();

				var body = oe.Elements()
					.Where(e => !e.Name.LocalName.In(schema.OeHeaders))
					.ToList();

				if (body.Count == 1 && body[0].Name.LocalName == "Table")
				{
					return KindTable;
				}

				if (scope == SelectionScope.Run &&
					body.Count > 0 && body.All(e => e.Name.LocalName == "T"))
				{
					return KindRuns;
				}
			}

			return KindParagraphs;
		}


		/// <summary>
		/// When only some of the cells of a table are selected, the extractor would return
		/// just the content of those cells, losing the table. Instead, build a new table
		/// from the selected cells, preserving column widths, borders, and cell shading.
		/// </summary>
		/// <returns>A new Table element or null if the selection is not a partial table</returns>
		private static XElement ExtractCells(Page clone, XNamespace ns)
		{
			var outlines = clone.Root.Elements(ns + "Outline");

			var table = outlines.Descendants(ns + "Table")
				.FirstOrDefault(t => t.Elements(ns + "Row").Elements(ns + "Cell")
					.Any(c => c.Attribute("selected")?.Value == "all"));

			if (table is null)
			{
				return null;
			}

			// all selected text runs must live in this table; otherwise the selection
			// spans beyond the table and the extractor is the right tool
			var outside = outlines.Descendants(ns + "T")
				.Any(t => t.Attribute("selected")?.Value == "all" && !t.Ancestors().Contains(table));

			if (outside)
			{
				return null;
			}

			var rows = table.Elements(ns + "Row").ToList();
			var picks = new List<(int Row, int Col)>();

			for (var r = 0; r < rows.Count; r++)
			{
				var cells = rows[r].Elements(ns + "Cell").ToList();
				for (var c = 0; c < cells.Count; c++)
				{
					if (cells[c].Attribute("selected")?.Value == "all")
					{
						picks.Add((r, c));
					}
				}
			}

			var total = rows.Sum(r => r.Elements(ns + "Cell").Count());

			// a single cell is treated as text content; the whole table is handled by the extractor
			if (picks.Count < 2 || picks.Count >= total)
			{
				return null;
			}

			var rowIndexes = picks.Select(p => p.Row).Distinct().OrderBy(i => i).ToList();
			var colIndexes = picks.Select(p => p.Col).Distinct().OrderBy(i => i).ToList();

			var sub = new XElement(ns + "Table",
				table.Attributes().Where(a => !a.Name.LocalName.In("objectID", "selected")));

			if (!rowIndexes.Contains(0))
			{
				sub.Attribute("hasHeaderRow")?.Remove();
			}

			var columns = new XElement(ns + "Columns");
			var sourceColumns = table.Element(ns + "Columns")?.Elements(ns + "Column").ToList()
				?? new List<XElement>();

			var index = 0;
			foreach (var c in colIndexes)
			{
				if (c < sourceColumns.Count)
				{
					var column = new XElement(sourceColumns[c]);
					if (column.Attribute("index") is not null)
					{
						column.SetAttributeValue("index", index.ToString(CultureInfo.InvariantCulture));
					}

					columns.Add(column);
					index++;
				}
			}

			sub.Add(columns);

			foreach (var r in rowIndexes)
			{
				var row = new XElement(ns + "Row",
					rows[r].Attributes().Where(a => !a.Name.LocalName.In("objectID", "selected")));

				var cells = rows[r].Elements(ns + "Cell").ToList();
				foreach (var c in colIndexes)
				{
					if (c < cells.Count)
					{
						row.Add(new XElement(cells[c]));
					}
				}

				sub.Add(row);
			}

			return sub;
		}


		//========================================================================================
		// Apply
		//========================================================================================

		/// <summary>
		/// Parses the XML of a snippet package, validating its structure
		/// </summary>
		/// <param name="xml">The text of a .snpx file</param>
		/// <param name="kind">The kind of content, one of the Kind constants</param>
		/// <returns>The content as an OEChildren element, or null if invalid</returns>
		public static XElement Parse(string xml, XNamespace ns, out string kind, out XElement root)
		{
			kind = KindParagraphs;
			root = null;

			try
			{
				root = XElement.Parse(xml);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error parsing snippet", exc);
				return null;
			}

			kind = root.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name")?.Value == KindMeta)?
				.Attribute("content")?.Value ?? KindParagraphs;

			return root.Element(ns + "Outline")?.Element(ns + "OEChildren");
		}


		/// <summary>
		/// Replaces macros such as {=DATETIME(fmt)} and {=QUARTER} within the text of the
		/// snippet content
		/// </summary>
		public static void ExpandMacros(XElement content, SnippetMacros macros)
		{
			foreach (var cdata in content.DescendantNodes().OfType<XCData>().ToList())
			{
				if (cdata.Value.IndexOf("{=", StringComparison.Ordinal) < 0)
				{
					continue;
				}

				cdata.Value = macros.Expand(cdata.Value);
			}
		}


		/// <summary>
		/// Determines if the snippet content contains the {SNIPPET_BODY} token
		/// </summary>
		public static bool HasBodyToken(XElement content)
		{
			return content.DescendantNodes().OfType<XCData>()
				.Any(c => c.Value.ContainsICIC(BodyTag));
		}


		/// <summary>
		/// Replaces the paragraph containing the {SNIPPET_BODY} token with the given body
		/// </summary>
		public static void ExpandBody(XElement content, XNamespace ns, XElement body)
		{
			var token = content.Descendants(ns + "T")
				.FirstOrDefault(t => t.GetCData().Value.ContainsICIC(BodyTag));

			if (token is null || token.Parent is null)
			{
				return;
			}

			var oe = token.Parent;
			token.GetCData().Value = token.GetCData().Value.ReplaceIgnoreCase(BodyTag, string.Empty);

			var empty = oe.Elements(ns + "T").All(t => string.IsNullOrWhiteSpace(t.GetCData().Value));

			var items = body.Elements().ToList();
			if (items.Count == 0)
			{
				// nothing was selected so keep the now-blank paragraph; removing it could leave
				// its container, e.g. a table cell, with an empty OEChildren which is invalid
				return;
			}

			oe.AddAfterSelf(items);
			if (empty)
			{
				oe.Remove();
			}
		}


		/// <summary>
		/// Merges the quick styles and tags of the package into the target page and rewrites
		/// the index references of the content to match the target page.
		/// </summary>
		/// <param name="page">The target page</param>
		/// <param name="root">The snippet package root, source of the definitions</param>
		/// <param name="content">The snippet content to adjust</param>
		public static void MergeDefinitions(Page page, XElement root, XElement content)
		{
			var ns = page.Namespace;

			// quick styles...

			var targets = page.Root.Elements(ns + "QuickStyleDef").ToList();
			var next = targets.Any()
				? targets.Max(e => (int)e.Attribute("index")) + 1
				: 0;

			var map = new Dictionary<string, string>();
			foreach (var source in root.Elements(ns + "QuickStyleDef"))
			{
				var sourceStyle = new QuickStyleDef(source);
				var sourceIndex = source.Attribute("index").Value;

				var match = targets.Find(t => new QuickStyleDef(t).Equals(sourceStyle));
				if (match is not null)
				{
					map[sourceIndex] = match.Attribute("index").Value;
				}
				else
				{
					var def = new XElement(source);
					def.SetAttributeValue("index", next.ToString(CultureInfo.InvariantCulture));
					page.AddQuickStyleDef(def);
					targets.Add(def);

					map[sourceIndex] = next.ToString(CultureInfo.InvariantCulture);
					next++;
				}
			}

			foreach (var attribute in content.DescendantsAndSelf().Attributes("quickStyleIndex").ToList())
			{
				if (map.TryGetValue(attribute.Value, out var value))
				{
					attribute.Value = value;
				}
			}

			// tags...

			var tagTargets = page.Root.Elements(ns + "TagDef").ToList();
			var nextTag = tagTargets.Any()
				? tagTargets.Max(e => (int)e.Attribute("index")) + 1
				: 0;

			var tagMap = new Dictionary<string, string>();
			foreach (var source in root.Elements(ns + "TagDef"))
			{
				var sourceIndex = source.Attribute("index").Value;

				var match = tagTargets.Find(t =>
					t.Attribute("symbol")?.Value == source.Attribute("symbol")?.Value &&
					t.Attribute("fontColor")?.Value == source.Attribute("fontColor")?.Value &&
					t.Attribute("highlightColor")?.Value == source.Attribute("highlightColor")?.Value &&
					t.Attribute("name")?.Value == source.Attribute("name")?.Value);

				if (match is not null)
				{
					tagMap[sourceIndex] = match.Attribute("index").Value;
				}
				else
				{
					var def = new XElement(source);
					def.SetAttributeValue("index", nextTag.ToString(CultureInfo.InvariantCulture));
					page.Root.AddFirst(def);
					tagTargets.Add(def);

					tagMap[sourceIndex] = nextTag.ToString(CultureInfo.InvariantCulture);
					nextTag++;
				}
			}

			foreach (var attribute in content.Descendants(ns + "Tag").Attributes("index").ToList())
			{
				if (tagMap.TryGetValue(attribute.Value, out var value))
				{
					attribute.Value = value;
				}
			}
		}


		/// <summary>
		/// Inserts the snippet content into the page at the current cursor, or after the
		/// given anchor if specified.
		/// </summary>
		/// <param name="page">The target page</param>
		/// <param name="content">The OEChildren snippet content, already merged</param>
		/// <param name="kind">The kind of snippet</param>
		/// <param name="anchor">
		/// Optional anchor, as provided by PageEditor.Anchor; if null then the selected
		/// cursor of the page is used
		/// </param>
		public static void Insert(Page page, XElement content, string kind, XElement anchor = null)
		{
			var ns = page.Namespace;
			var schema = new PageSchema();

			content.DescendantsAndSelf().Attributes("objectID").Remove();

			var items = content.Elements().ToList();
			if (items.Count == 0)
			{
				return;
			}

			// detach so the same instances are added to the page, not copies
			foreach (var item in items)
			{
				item.Remove();
			}

			if (anchor is not null)
			{
				InsertAtAnchor(page, anchor, items);
				return;
			}

			var range = new SelectionRange(page);
			range.GetSelection(true);

			if (range.Scope == SelectionScope.Range && !range.SingleParagraph)
			{
				// selection spans paragraphs; remove it and insert where it was
				var editor = new PageEditor(page);
				editor.ExtractSelectedContent(breakParagraph: true);
				if (editor.Anchor is not null)
				{
					InsertAtAnchor(page, editor.Anchor, items);
					return;
				}
			}

			var cursor = page.Root.Elements(ns + "Outline").Descendants()
				.LastOrDefault(e => e.Attribute("selected")?.Value == "all" &&
					e.Parent?.Name.LocalName == "OE");

			if (cursor is null)
			{
				page.EnsureContentContainer().Add(items);
				PlaceCursor(page, items);
				return;
			}

			var current = cursor.Parent;

			// a non-empty selection within one paragraph is replaced; reduce it to an empty
			// run so it behaves just like the text cursor
			var replace = range.Scope is SelectionScope.Run or SelectionScope.Range;
			if (replace)
			{
				var selection = current.Elements(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.ToList();

				for (var i = 1; i < selection.Count; i++)
				{
					selection[i].Remove();
				}

				cursor = selection[0];
				cursor.GetCData().Value = string.Empty;
			}

			// selected state moves to the end of the inserted snippet
			new PageEditor(page).Deselect();

			var runs = items.Count == 1 ? items[0].Elements(ns + "T").ToList() : null;

			// content before and after the insert point, ignoring headers and nested children
			var before = cursor.ElementsBeforeSelf().ToList();
			if (!replace && !(cursor.Name.LocalName == "T" && cursor.GetCData().Value.Length == 0))
			{
				// zero-length selection of a link or a selected image; insert after it
				before.Add(cursor);
			}

			var after = cursor.ElementsAfterSelf().ToList();

			var beforeBlank = before.All(e => IsBlank(e, schema));
			var afterBlank = after.All(e => IsBlank(e, schema) || e.Name.LocalName == "OEChildren");
			var hasChildren = after.Any(e => e.Name.LocalName == "OEChildren");

			var blank = beforeBlank && afterBlank && !hasChildren;

			// merge runs inline only into existing text; on a blank line insert the whole
			// paragraph so its quick style, color, font and alignment are preserved
			if (kind == KindRuns && !(beforeBlank && afterBlank) &&
				cursor.Name.LocalName == "T" && runs?.Count > 0)
			{
				// merge inline at the cursor position
				var copies = runs.Select(r => new XElement(r)).ToList();
				cursor.AddAfterSelf(copies);
				copies[copies.Count - 1].AddAfterSelf(new XElement(ns + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)));

				return;
			}

			if (beforeBlank && !afterBlank)
			{
				// cursor at the start of a paragraph so insert ahead of it
				current.AddBeforeSelf(items);
			}
			else if (!beforeBlank && !afterBlank)
			{
				// cursor within a paragraph so split it, moving the trailing content,
				// including nested children, into a new paragraph following the snippet
				var tail = new XElement(ns + "OE",
					current.Attributes().Where(a => !a.Name.LocalName.In("objectID", "selected")));

				foreach (var node in after)
				{
					node.Remove();
					tail.Add(node);
				}

				current.AddAfterSelf(items);
				items[items.Count - 1].AddAfterSelf(tail);
			}
			else
			{
				current.AddAfterSelf(items);
				if (blank)
				{
					// paste over an empty line rather than leaving a gap
					current.Remove();
				}
			}

			PlaceCursor(page, items);
		}


		private static bool IsBlank(XElement element, PageSchema schema)
		{
			return element.Name.LocalName.In(schema.OeHeaders) ||
				(element.Name.LocalName == "T" && string.IsNullOrWhiteSpace(element.GetCData().Value));
		}


		private static void InsertAtAnchor(Page page, XElement anchor, List<XElement> items)
		{
			if (anchor.Name.LocalName.In("OE", "HTMLBlock"))
			{
				anchor.AddAfterSelf(items);
			}
			else
			{
				anchor.AddFirst(items);
			}

			PlaceCursor(page, items);
		}


		private static void PlaceCursor(Page page, List<XElement> items)
		{
			var ns = page.Namespace;
			var last = items[items.Count - 1];

			var runs = last.Elements().Where(e => !e.Name.LocalName.In(new PageSchema().OeHeaders)).ToList();
			if (runs.Count > 0 && runs.All(e => e.Name.LocalName == "T"))
			{
				// text paragraph, place cursor at its end
				runs[runs.Count - 1].SetAttributeValue("selected", "all");
			}
			else
			{
				// table, image, etc. so add a paragraph to continue typing
				last.AddAfterSelf(new XElement(ns + "OE",
					new XElement(ns + "T",
						new XAttribute("selected", "all"),
						new XCData(string.Empty))));
			}
		}
	}
}
