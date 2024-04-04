//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Reads or extracts content from a Page.
	/// </summary>
	internal class PageReader : Loggable
	{
		private sealed class Snippet
		{
			public XElement Element;  // OE containing selected T run
			public int Depth;         // depth from Outline
		}

		private readonly Page page;
		private readonly XNamespace ns;


		public PageReader(Page page)
		{
			this.page = page;
			ns = page.Namespace;
		}


		/// <summary>
		/// Gets the anchor point for reintroducing collated content.
		/// If this is an OE or HTMLBlock then consumers may want to insert after that.
		/// Otherwise, consumers may want to insert at end of anchor container.
		/// </summary>
		public XElement Anchor { get; private set; }


		/// <summary>
		/// Extracts all selected content as a single OEChildren element, preserving relative
		/// indents, table content, etc. The selected content is removedd from the page.
		/// </summary>
		/// <returns>An OEChildren XElement</returns>
		public async Task<XElement> ExtractSelectedContent()
		{
			var content = new XElement(ns + "OEChildren");
			//	new XAttribute(XNamespace.Xmlns + OneNote.Prefix, ns.ToString())
			//);

			// An OE can contain either a sequence of T/InkWord elements or
			// one of Image, Table, InkDrawing, InsertedFile, MediaFile, InkParagraph.
			// These may follow a sequence of MediaIndex, Tag, Meta, List.
			// If the OE contains T/InkWord elements, no more than one can be selected!

			var runs = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "OE")
				.Elements()
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"))
				.ToList();

			// no selections found in body
			if (!runs.Any())
			{
				Anchor = null;
				return content;
			}

			// find best anchor, either previous node (OE/HTMLBlock) or closet parent (OEChildren)
			if (runs[0].Parent.PreviousNode is XElement prev)
			{
				Anchor = prev;
			}
			else
			{
				Anchor = runs[0].Parent.Parent;

				// if a descendent of a Cell then check if the entire table is selected and
				// adjust the anchor point based on that, otherwise we loose the table
				if (Anchor.FirstAncestor(ns + "Cell") is XElement cell &&
					WholeTableSelected(cell) is XElement table)
				{
					Anchor = table.Parent.PreviousNode is XElement pprev
						// previous 
						? pprev
						: table.Parent.Parent;
				}
			}

			var snippets = await ExtractSnippets(runs);

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

			CleanupOrphanedElements();

			return content;
		}


		private XElement WholeTableSelected(XElement cell)
		{
			var table = cell.FirstAncestor(ns + "Table");
			if (table.Descendants(ns + "Cell")
				.All(c => c.Attribute("selected")?.Value == "all"))
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


			System.Diagnostics.Debugger.Launch();


			// Runs are captured in document-order. So by reversing this collection, we should
			// be able discard nested elements as they are emptied; otherwise, doing this in
			// document-order would make it very difficult to look-ahead to determine if the
			// element should be discarded.
			runs.Reverse();

			foreach (var run in runs)
			{
				var prev = run.PreviousNode as XElement;
				var next = run.NextNode as XElement;
				var parent = run.Parent; // OE
				var grand = parent.Parent; // OEChildren

				var depth = IndentLevel(run);
				XElement element = null;

				var alone =
					// anything after run means there is more content: not alone
					next is null &&
					// informational elements don't count towards "content" (all of them?)
					(prev is null ||
					prev.Name.LocalName.In("MediaIndex", "Tag", "OutlookTask", "Meta", "List"));

				if (alone)
				{
					// alone means that there is exactly one content element in this OE
					// so we could reclaim the OE, unless its parent depends on it

					var inCell = grand.Parent.Name.LocalName == "Cell";

					if (inCell && WholeTableSelected(grand) is XElement table)
					{
						// special case to handle entire table selection, where we want to wrap
						// the table, possibly with other content. So do not count individual
						// runs within the table

						if (!tables.Contains(table))
						{
							element = new XElement(ns + "OE",
								table.Parent.Attributes()
									.Where(a => !a.Name.LocalName.In("objectID", "selected"))
								);

							table.Remove();
							tables.Add(table);

							element.Add(table);
						}
					}
					else
					{
						element = parent;
						parent.Remove();

						if (inCell && !grand.HasElements)
						{
							// don't leave table cells empty; must have default content
							var cell = new TableCell(grand.Parent);
							cell.SetContent(string.Empty);
						}
					}
				}
				else
				{
					// copy parent attributes to preserve quickstyles
					element = new XElement(ns + parent.Name.LocalName,
						parent.Attributes()
							.Where(a => !a.Name.LocalName.In("objectID", "selected"))
						);

					// copy list configuration if any
					if (parent.Elements(ns + "List").FirstOrDefault() is XElement list)
					{
						element.Add(XElement.Parse(list.ToString(SaveOptions.DisableFormatting)));
					}

					element.Add(run);

					run.Remove();
				}

				if (grand is not null && !grand.Elements().Any())
				{
					if (grand.Parent is XElement greatpar &&
						!greatpar.Name.LocalName.In("Cell", "Outline"))
					{
						logger.WriteLine("removing ~~ grandparent");
						grand.Remove();
					}
				}

				if (element is not null)
				{
					// when moving an Image from a page loaded with Basic or Selection scope,
					// it can get unwired internally and loose its data so explicitly replace
					// the CallbackID with raw Base64 Data
					if (run.Name.LocalName == "Image" &&
						run.Elements(ns + "CallbackID").FirstOrDefault() is XElement callback)
					{
						var data = one.GetPageContent(page.PageId,
							callback.Attribute("callbackID").Value);

						callback.ReplaceWith(new XElement(ns + "Data", data));
					}

					snippets.Insert(0, new Snippet
					{
						Element = element,
						Depth = depth
					});
				}
			}

			return snippets;
		}


		private int IndentLevel(XElement element)
		{
			if (element is null)
			{
				return 0;
			}

			var count = 0;
			var node = element.Parent;

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


		private void CleanupOrphanedElements()
		{
			// clean up orphaned OE elements
			IEnumerable<XElement> items;

			items = page.Root.Descendants(ns + "OE")
				.Where(e => e != Anchor &&
					!e.Elements().Any(c =>
						c.Name.LocalName.In(
							"T", "Table", "Image", "InkParagraph", "InkWord",
							"InsertedFile", "MediaFile", "OEChildren", "OutlookTask",
							"LinkedNote", "FutureObject")));

			logger.WriteLine($"cleaning ~~> {(items.Any() ? items.Count() : 0)} OE");
			items.Remove();

			// clean up orphaned OEChildren elements
			items = page.Root.Descendants(ns + "OEChildren")
				.Where(e => e != Anchor && !e.Elements().Any());

			logger.WriteLine($"cleaning ~~> {(items.Any() ? items.Count() : 0)} OEChildren");
			items.Remove();

			// clean up selected attributes; keep only select snippets
			page.Root.DescendantNodes().OfType<XAttribute>()
				.Where(a => a.Name.LocalName == "selected")
				.Remove();

			// patch any empty cells, cheap but effective!
			foreach (var item in page.Root.Descendants(ns + "Cell")
				.Where(e => !e.Elements().Any()))
			{
				logger.WriteLine("cleaning ~~> default cell");
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
						logger.WriteLine("cleaning ~~> orphaned List");
						list.Remove();
					}
				}
			}
		}
	}
}
