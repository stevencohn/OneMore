//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


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


		public XElement ExtractSelectedContent()
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

			// no selections in body
			if (!runs.Any())
			{
				Anchor = null;
				return content;
			}

			// find best anchor, either previous node (OE/HTMLBlock) or closet parent (OEChildren)
			Anchor = runs[0].Parent.PreviousNode is XElement prev
				? prev
				: runs[0].Parent.Parent;

			Logger.Current.WriteLine("Anchor...");
			Logger.Current.WriteLine(Anchor);

			var snippets = CollectSnippets(runs);

			var stack = new Stack<XElement>();
			stack.Push(content);

			// construct a hierarchy of content based on snippet indent levels...
			var container = content;
			foreach (var snippet in snippets)
			{
				if (snippet.Depth > stack.Count)
				{
					var child = container.Elements(ns + "OE").FirstOrDefault();
					if (child is null)
					{
						child = new XElement(ns + "OE");
						container.Add(child);
					}

					var children = new XElement(ns + "OEChildren");
					child.Add(children);

					stack.Push(children);
					container = children;
				}
				else if (snippet.Depth < stack.Count)
				{
					container = stack.Pop();
				}

				if (snippet.Element.Attribute("objectID") is XAttribute attribute)
				{
					attribute.Remove();
				}

				container.Add(snippet.Element);
			}

			Logger.Current.WriteLine("--- preclean ---");
			Logger.Current.WriteLine(page.Root);

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

			Logger.Current.WriteLine("--- cleaned ---");
			Logger.Current.WriteLine(page.Root);

			return content;
		}


		private List<Snippet> CollectSnippets(List<XElement> runs)
		{
			var snippets = new List<Snippet>();
			foreach (var run in runs)
			{
				var depth = IndentLevel(run);

				XElement element;
				if ((run.PreviousNode is XElement prev && prev.Name.LocalName.In("T", "InkWord")) ||
					run.NextNode is not null)
				{
					var content = new List<XElement> { run };
					var parent = run.Parent;

					if (parent.Elements(ns + "List").FirstOrDefault() is XElement list)
					{
						content.Insert(0, XElement.Parse(list.ToString(SaveOptions.DisableFormatting)));
					}

					run.Remove();

					element = new XElement(ns + parent.Name.LocalName,
						parent.Attributes().Where(a => !a.Name.LocalName.In("objectID", "selected")),
						content
						);
				}
				else
				{
					element = run.Parent;
					run.Parent.Remove();
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
	}
}
