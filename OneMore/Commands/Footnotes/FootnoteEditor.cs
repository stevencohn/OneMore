//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;


	internal class FootnoteEditor
	{
		private XElement page;
		private XNamespace ns;
		private ApplicationManager manager;


		public FootnoteEditor(XElement page, ApplicationManager manager)
		{
			this.page = page;
			ns = page.GetNamespaceOfPrefix("one");

			this.manager = manager;
		}


		public void AddFootnote()
		{
			var element = page.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.LastOrDefault();

			if (element != null)
			{
				// last selected paragraph OE
				element = element.Parent;

				var label = WriteFootnote(element.Attribute("objectID").Value);

				// find the new footer and get its new objectId
				var noteId = page.Descendants(ns + "Meta")
					.Where(e =>
						e.Attribute("name").Value.Equals("omfootnote") &&
						e.Attribute("content").Value.Equals(label))
					.Select(e => e.Parent.Attribute("objectID").Value)
					.FirstOrDefault();

				manager.Application.GetHyperlinkToObject(page.Attribute("ID").Value, noteId, out var link);

				// <a href="...">
				//  <span style='vertical-align:super'>[1]</span>
				// </a>

				var note = new XElement("a",
					new XAttribute("href", link),
					new XElement("span",
						new XAttribute("style", "vertical-align:super"),
						new XText($"[{label}]")
						)
					);

				// find the element in the new page instance of XML
				element = page.Elements(ns + "Outline")
					.Where(e => e.Attributes("selected").Any())
					.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.LastOrDefault();

				if (element != null)
				{
					// add footnote link to end of selection range paragraph
					var cdata = element.DescendantNodes().OfType<XCData>().Last();
					if (cdata != null)
					{
						cdata.Parent.ReplaceNodes(
							new XCData(cdata.Value += note.ToString(SaveOptions.DisableFormatting))
							);

						manager.UpdatePageContent(page);
					}
				}
			}
		}


		private string WriteFootnote(string textId)
		{
			var divider = EnsureFootnoteSection();

			// find next footnote label
			var label = (divider.NodesAfterSelf()
				.OfType<XElement>().Elements(ns + "Meta")
				.Where(e => e.Attribute("name").Value.Equals("omfootnote"))
				.DefaultIfEmpty() // needed to avoid null ref exception
				.Max(e => e == null ? 0 : int.Parse(e.Attribute("content").Value))
				+ 1).ToString();

			/*
			 * Note, this relies on the Meta.omfootnote/content value to track IDs;
			 * these can get out of sync with the visible [ID] in the content if the user
			 * mucks around with the hyperlink. Need a refresh routine to ensure all IDs
			 * are in sync with their Meta labels.
			 */

			// find previous sibiling element after which new note is to be added
			var last = divider.NodesAfterSelf()
				.OfType<XElement>().Elements(ns + "Meta")
				.Where(e => e.Attribute("name").Value.Equals("omfootnote"))
				.LastOrDefault();

			last = last == null ? divider : last.Parent;

			// insert new note

			/*
			<one:OE>
			  <one:Meta name="omfootnote" content="1" />
			  <one:T>
				<![CDATA[<a href=""><span style='font-family:"Calibri Light";font-size:11.0pt' lang=yo>[1]</span></a>
				<span style='font-family:"Calibri Light";font-size:11.0pt' lang=yo> Something said here</span>]]>
			  </one:T>
			</one:OE>
			*/

			manager.Application.GetHyperlinkToObject(page.Attribute("ID").Value, textId, out var link);

			var cdata = new XElement("wrapper",
				new XElement("a",
					new XAttribute("href", link),
					new XElement("span",
						new XAttribute("style", "font-family:'Calibri Light';font-size:11.0pt"),
						$"[{label}]"
					)
				),
				new XElement("span",
					new XAttribute("style", "font-family:'Calibri Light';font-size:11.0pt"),
					" new footnote"
				));

			var note = new XElement(ns + "OE",
				new XElement(ns + "Meta",
					new XAttribute("name", "omfootnote"),
					new XAttribute("content", label)
				),
				new XElement(ns + "T",
					new XCData(cdata.GetInnerXml())
					)
				);

			last.AddAfterSelf(note);

			// update the page so OneNote will generate a new objectID
			manager.UpdatePageContent(page);
			page = manager.CurrentPage();
			ns = page.GetNamespaceOfPrefix("one");

			return label;
		}


		/*
		  <one:OE style="font-family:'Courier New';font-size:10.0pt;color:#999696">
			<one:Meta name="onemore" content="footnotes" />
			<one:T><![CDATA[─ ─ . . . ─ ─]]></one:T>
		  </one:OE>
		*/
		private XElement EnsureFootnoteSection()
		{
			var divider = page.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Descendants(ns + "Meta")
				.Where(e =>
					e.Attribute("name").Value.Equals("omfootnotes") &&
					e.Attribute("content").Value.Equals("divider"))
				.FirstOrDefault();

			if (divider == null)
			{
				var line = string.Concat(Enumerable.Repeat("- ", 50));
				PageHelper.EnsurePageWidth(page, line, "Courier New", 10f, manager.WindowHandle);

				var content = page.Elements(ns + "Outline")
					.Where(e => e.Attributes("selected").Any())
					.Elements(ns + "OEChildren")
					.FirstOrDefault();

				if (content != null)
				{
					content.Add(new XElement(ns + "OE", new XElement(ns + "T", string.Empty)));
					content.Add(new XElement(ns + "OE", new XElement(ns + "T", string.Empty)));

					divider = new XElement(ns + "OE",
						new XAttribute("style", "font-family:'Courier New';font-size:10.0pt;color:#999696"),
						new XElement(ns + "Meta",
							new XAttribute("name", "omfootnotes"),
							new XAttribute("content", "divider")
							),
						new XElement(ns + "T", new XCData(line))
						);

					content.Add(divider);
				}

				return divider;
			}

			// Meta parent's OE
			return divider.Parent;
		}
	}
}
