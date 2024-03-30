//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
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

			// no selections in body or only zero-length insertion cursor
			if (!runs.Any() || (runs.Count == 1 && runs[0].GetCData().Value.Length == 0))
			{
				Anchor = null;
				return content;
			}

			// first parent is considered the anchor point, to which the
			// consumer may choose to insert the newly collated content
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
					var children = new XElement(ns + "OEChildren");
					container.Add(new XElement(ns + "OE", children));
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

			// clean up orphaned OEChildren
			page.Root.Descendants(ns + "OEChildren")
				.Where(e => e != Anchor && !e.Elements().Any())
				.Remove();

			return content;
		}


		private List<Snippet> CollectSnippets(List<XElement> runs)
		{
			var snippets = new List<Snippet>();
			foreach (var run in runs)
			{
				var depth = IndentLevel(run.Parent);

				XElement element;
				if ((run.PreviousNode is XElement prev && 
					(prev.Name.LocalName == "T" || prev.Name.LocalName == "InkWord")) ||
					run.NextNode is not null)
				{
					var content = new List<XElement> { run };
					var parent = run.Parent;

					if (parent.Elements(ns + "List").FirstOrDefault() is XElement list)
					{
						content.Add(XElement.Parse(list.ToString(SaveOptions.DisableFormatting)));
					}

					run.Remove();

					element = new XElement(ns + parent.Name.LocalName,
						parent.Attributes().Where(a =>
							a.Name.LocalName != "objectID" && a.Name.LocalName != "selected"),
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
			var count = 0;
			var node = element;

			if (node is not null)
			{
				while (node.Parent != null && node.Parent.Name != "Outline")
				{
					node = node.Parent;
					if (node.Name.LocalName == "OEChildren")
					{
						count++;
					}
				}
			}

			return node is null ? -1 : count;
		}
	}
}
