//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

#define Verbose

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Reads or extracts content from a Page.
	/// </summary>
	internal class PageReader
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


		public XElement Anchor { get; private set; }


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
			Anchor = runs[0].Parent.PreviousNode is XElement prev
				? prev
				: runs[0].Parent.Parent;
#if Verbose
			Logger.Current.WriteLine("Anchor...");
			Logger.Current.WriteLine(Anchor);
#endif
			var snippets = await ExtractSnippets(runs);

			// construct a hierarchy of content based on snippet indent levels...
			var container = content;
			var depth = 1;
			foreach (var snippet in snippets)
			{
#if Verbose
				Logger.Current.WriteLine($"snippet depth={snippet.Depth}");
				Logger.Current.WriteLine(snippet.Element);
#endif
				if (snippet.Depth > depth)
				{
					depth++;

					var child = container.Elements(ns + "OE").LastOrDefault();
					if (child is null)
					{
						child = new XElement(ns + "OE");
						container.Add(child);
					}

					var children = new XElement(ns + "OEChildren");
					child.Add(children);

					container = children;
				}
				else if (snippet.Depth < depth)
				{
					container = container.Parent.Parent;
					depth--;
				}

				if (snippet.Element.Attribute("objectID") is XAttribute attribute)
				{
					attribute.Remove();
				}

				container.Add(snippet.Element);
			}

			CleanupOrphanedElements();

			return content;
		}


		private async Task<List<Snippet>> ExtractSnippets(List<XElement> runs)
		{
			await using var one = new OneNote();

			var snippets = new List<Snippet>();
			foreach (var run in runs)
			{
				var prev = run.PreviousNode as XElement;
				var next = run.NextNode as XElement;
				var parent = run.Parent;

				var depth = IndentLevel(run);
				XElement element;

				var alone =
					// anything after run means there is more content: not alone
					next is null &&
					(prev is null ||
					!prev.Name.LocalName.In("MediaIndex", "Tag", "OutlookTask", "Meta", "List"));

				if (alone)
				{
					var grand = parent.Parent;

					element = parent;
					parent.Remove();

					// don't leave empty cells
					if (grand.Name.LocalName == "Cell")
					{
						var cell = new TableCell(grand);
						cell.SetContent(string.Empty);
					}
				}
				else
				{
					element = new XElement(ns + parent.Name.LocalName,
						parent.Attributes()
							.Where(a => !a.Name.LocalName.In("objectID", "selected"))
						);

					if (parent.Elements(ns + "List").FirstOrDefault() is XElement list)
					{
						element.Add(XElement.Parse(list.ToString(SaveOptions.DisableFormatting)));
					}

					element.Add(run);

					run.Remove();
				}

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

				snippets.Add(new Snippet
				{
					Element = element,
					Depth = depth
				});
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
#if Verbose
			Logger.Current.WriteLine("--- preclean ---");
			Logger.Current.WriteLine(page.Root);
#endif
			// clean up orphaned OE elements
			page.Root.Descendants(ns + "OE")
				.Where(e => e != Anchor &&
					!e.Elements().Any(c =>
						c.Name.LocalName.In(
							"T", "Table", "Image", "InkParagraph", "InkWord",
							"InsertedFile", "MediaFile", "OEChildren", "OutlookTask",
							"LinkedNote", "FutureObject")))
				.Remove();

			// clean up orphaned OEChildren elements
			page.Root.Descendants(ns + "OEChildren")
				.Where(e => e != Anchor && !e.Elements().Any())
				.Remove();

			// clean up selected attributes; keep only select snippets
			page.Root.DescendantNodes().OfType<XAttribute>()
				.Where(a => a.Name.LocalName == "selected")
				.Remove();

			// patch any empty cells, cheap but effective!
			foreach (var item in page.Root.Descendants(ns + "Cell")
				.Where(e => !e.Elements().Any()))
			{
				new TableCell(item).SetContent(string.Empty);
			}

			if (Anchor.Name.LocalName == "OE")
			{
				var list = Anchor.Elements(ns + "List").FirstOrDefault();
				if (list is not null)
				{
					if (!Anchor.Elements().Any(e => e.Name.LocalName.In("T", "InkWord")))
					{
						list.Remove();
					}
				}
			}
#if Verbose
			Logger.Current.WriteLine("--- cleaned ---");
			Logger.Current.WriteLine(page.Root);
#endif
		}
	}
}
