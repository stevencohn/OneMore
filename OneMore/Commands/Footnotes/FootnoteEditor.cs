//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class FootnoteEditor
	{
		private readonly ApplicationManager manager;
		private readonly ILogger logger;
		private XElement page;
		private XNamespace ns;

		private XElement divider;


		/// <summary>
		/// Initialize a new instance of the editor with the given manager
		/// </summary>
		/// <param name="manager">The OneNote application manager (abstraction)</param>
		public FootnoteEditor(ApplicationManager manager)
		{
			this.manager = manager;

			page = manager.CurrentPage();
			ns = page.GetNamespaceOfPrefix("one");

			logger = Logger.Current;
		}


		/// <summary>
		/// Adds a new footnote to the page, possibly renumbering existing footnotes
		/// to ensure all foonotes are in sequential order from top to bottom.
		/// </summary>
		public void AddFootnote()
		{
			var element = page.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.LastOrDefault();

			if (element == null)
			{
				logger.WriteLine($"{nameof(FootnoteEditor)} could not find a selected outline");
				return;
			}

			// last selected paragraph OE
			element = element.Parent;

			if (!EnsureFootnoteFooter())
			{
				return;
			}

			var label = WriteFootnoteText(element.Attribute("objectID").Value);

			if (WriteFootnoteRef(label))
			{
				RefreshLabels();

				manager.UpdatePageContent(page);
			}
		}


		/*
		  <one:OE style="font-family:'Courier New';font-size:10.0pt;color:#999696">
			<one:Meta name="onemore" content="footnotes" />
			<one:T><![CDATA[─ ─ . . . ─ ─]]></one:T>
		  </one:OE>
		*/
		private bool EnsureFootnoteFooter()
		{
			divider = page.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Descendants(ns + "Meta")
				.Where(e =>
					e.Attribute("name").Value.Equals("omfootnotes") &&
					e.Attribute("content").Value.Equals("divider"))
				.FirstOrDefault();

			if (divider != null)
			{
				// Meta parent's OE
				divider = divider.Parent;
				return true;
			}

			// else build a new divider...

			var line = string.Concat(Enumerable.Repeat("- ", 50));
			PageHelper.EnsurePageWidth(page, line, "Courier New", 10f, manager.WindowHandle);

			var content = page.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Elements(ns + "OEChildren")
				.FirstOrDefault();

			if (content != null)
			{
				// add a couple of empty lines for spacing
				content.Add(new XElement(ns + "OE", new XElement(ns + "T", string.Empty)));
				content.Add(new XElement(ns + "OE", new XElement(ns + "T", string.Empty)));

				// build the divider line
				divider = new XElement(ns + "OE",
					new XAttribute("style", "font-family:'Courier New';font-size:10.0pt;color:#999696"),
					new XElement(ns + "Meta",
						new XAttribute("name", "omfootnotes"),
						new XAttribute("content", "divider")
						),
					new XElement(ns + "T", new XCData(line))
					);

				// add the divider line
				content.Add(divider);

				return true;
			}

			logger.WriteLine(
				$"{nameof(FootnoteEditor)} could not add footnote section; OEChildren not found");

			return false;
		}


		private string WriteFootnoteText(string textId)
		{
			// find next footnote label
			var label = (divider.NodesAfterSelf()
				.OfType<XElement>().Elements(ns + "Meta")
				.Where(e => e.Attribute("name").Value.Equals("omfootnote"))
				.DefaultIfEmpty()  // avoids null ref exception
				.Max(e => e == null ? 0 : int.Parse(e.Attribute("content").Value))
				+ 1).ToString();

			/*
			 * Note, this relies on the Meta.omfootnote/content value to track IDs;
			 * these can get out of sync with the visible [ID] in the content if the user
			 * mucks around with the hyperlink. Need a refresh routine to ensure all IDs
			 * are in sync with their Meta labels.
			 */

			// find last footnote (sibling) element after which new note is to be added
			var last = divider.NodesAfterSelf()
				.OfType<XElement>().Elements(ns + "Meta")
				.Where(e => e.Attribute("name").Value.Equals("omfootnote"))
				.LastOrDefault();

			// divider is a valid precedesor sibling; otherwise last's Parent
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

			// reload the page and reset state variables...
			page = manager.CurrentPage();
			ns = page.GetNamespaceOfPrefix("one");

			_ = EnsureFootnoteFooter();

			return label;
		}


		private bool WriteFootnoteRef(string label)
		{
			// find the new footer by its label and get its new objectID
			var noteId = page.Descendants(ns + "Meta")
				.Where(e =>
					e.Attribute("name").Value.Equals("omfootnote") &&
					e.Attribute("content").Value.Equals(label))
				.Select(e => e.Parent.Attribute("objectID").Value)
				.FirstOrDefault();

			if (noteId == null)
			{
				return false;
			}

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
			var element = page.Elements(ns + "Outline")
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
					cdata.ReplaceWith(
						new XCData(cdata.Value += note.ToString(SaveOptions.DisableFormatting))
						);

					return true;
				}
			}

			return false;
		}


		//=======================================================================================

		/// <summary>
		/// Removes the footnote at the current cursor position, either located on a footnote
		/// reference or a footnote text.
		/// </summary>
		/// <remarks>
		/// A dialog is displayed if the cursor is not positioned over a footnote ref or text.
		/// </remarks>
		public void RemoveFootnote()
		{

		}


		//=======================================================================================

		private void RefreshLabels()
		{
			// find all references [superscript]
			var refs = page.Descendants(ns + "T").DescendantNodes().OfType<XCData>()
				.Select(CData => new
				{
					CData,
					match = Regex.Match(CData.Value, @"vertical-align:super[;'""].*>\[(\d+)\]</span>")
				})				
				.Where(o => o.match.Success)
				.Select(o => new
				{
					o.CData,
					Value = int.Parse(o.match.Groups[1].Value),
					o.match.Groups[1].Index,
					o.match.Groups[1].Length
				})
				.ToList();

			// find all footnotes
			var notes = page.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name").Value.Equals("omfootnote"))
				.Select(e => new
				{
					Element = e.Parent,
					Value = int.Parse(e.Attribute("content").Value)
				})
				.ToList();

			if (refs.Count != notes.Count)
			{
				// out of sync so just give up!
				return;
			}


			// reorder footnotes in sync with refs...

			var map = refs.Select(r => r.Value).ToList();
			notes = notes.OrderBy(n => map.IndexOf(n.Value)).ToList();

			// refresh labels...

			int count = 0;
			for (int i = 0, label = 1; i < refs.Count; i++, label++)
			{
				var note = notes
					.Where(n => n.Value == label)
					.FirstOrDefault();

				if (note == null)
				{
					// something is awry!
					continue;
				}

				if (refs[i].Value != label)
				{
					var cdata = refs[i].CData.Value.Remove(refs[i].Index, refs[i].Length);
					refs[i].CData.Value = cdata.Insert(refs[i].Index, label.ToString());
					count++;
				}

				if (notes[i].Value != label)
				{
					notes[i].Element.Element(ns + "Meta").Attribute("content").Value = label.ToString();
					count++;
				}

				var text = notes[i].Element.Elements(ns + "T").DescendantNodes().OfType<XCData>()
					.Select(CData => new
					{
						CData,
						match = Regex.Match(CData.Value, @"\[(\d+)\]")
					})
					.Where(r => r.match.Success)
					.Select(o => new
					{
						o.CData,
						Value = int.Parse(o.match.Groups[1].Value),
						o.match.Groups[1].Index,
						o.match.Groups[1].Length
					})
					.FirstOrDefault();

				if ((text != null) && (text.Value != label))
				{
					var cdata = text.CData.Value.Remove(text.Index, text.Length);
					text.CData.Value = cdata.Insert(text.Index, label.ToString());
					count++;
				}
			}

			if (count > 0)
			{
				// remove notes and readd in sorted order

				foreach (var note in notes)
				{
					note.Element.Remove();
				}

				var previous = divider;
				foreach (var note in notes)
				{
					previous.AddAfterSelf(note.Element);
					previous = note.Element;
				}
			}
		}
	}
}
