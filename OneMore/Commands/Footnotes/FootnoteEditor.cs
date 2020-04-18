//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class FootnoteEditor
	{
		private readonly ApplicationManager manager;
		private readonly ILogger logger;
		private readonly bool dark;

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

			dark = new Page(page).GetPageColor(out _, out _).GetBrightness() < 0.5;

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
				logger.WriteLine($"{nameof(FootnoteEditor.AddFootnote)} could not find a selected outline");
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

			new Page(page).EnsurePageWidth(line, "Courier New", 10f, manager.WindowHandle);

			var content = page.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Elements(ns + "OEChildren")
				.FirstOrDefault();

			if (content != null)
			{
				// add a couple of empty lines for spacing
				content.Add(new XElement(ns + "OE",
					new XElement(ns + "Meta",
						new XAttribute("name", "omfootnotes"),
						new XAttribute("content", "empty")
						),
					new XElement(ns + "T", string.Empty)
					));

				content.Add(new XElement(ns + "OE",
					new XElement(ns + "Meta",
						new XAttribute("name", "omfootnotes"),
						new XAttribute("content", "empty")
						),
					new XElement(ns + "T", string.Empty)
					));

				// build the divider line
				var color = dark ? "#595959" : "#999696";

				divider = new XElement(ns + "OE",
					new XAttribute("style", $"font-family:'Courier New';font-size:10.0pt;color:{color}"),
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
				$"{nameof(FootnoteEditor.EnsureFootnoteFooter)} could not add footnote section; OEChildren not found");

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

			var color = dark ? ";color:#5B9BD5" : string.Empty;

			var cdata = new XElement("wrapper",
				new XElement("a",
					new XAttribute("href", link),
					new XElement("span",
						new XAttribute("style", $"font-family:'Calibri Light';font-size:11.0pt{color}"),
						$"[{label}]"
					)
				),
				new XElement("span",
					new XAttribute("style", "font-family:'Calibri Light';font-size:11.0pt"),
					" new footnote"
				));

			color = dark ? "#BFBFBF" : "#151515";

			var note = new XElement(ns + "OE",
				new XAttribute("style", $"color:{color}"),
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

			// TODO: color isn't applied correctly on dark pages, why?
			if (dark)
			{
				note.Add(new XAttribute("style", "color:'#5B9BD5'"));
			}

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

					element.Attribute("selected").Remove();

					element.AddAfterSelf(new XElement(ns + "T",
						new XAttribute("selected", "all"),
						new XCData(string.Empty))
						);

					return true;
				}
			}

			return false;
		}


		//=======================================================================================

		/// <summary>
		/// Refreshes the label numbers so that all references are sequentially ordered starting
		/// at 1 from the top of the page. This is needed when adding a new reference prior to
		/// existing ones or deleting a reference.
		/// </summary>
		private void RefreshLabels()
		{
			var refs = FindSelectedReferences(page.Descendants(ns + "T"), true);

			if (refs?.Any() != true)
			{
				// no refs so just give up!
				return;
			}

			// find all footnotes
			var notes = page.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name").Value.Equals("omfootnote"))
				.Select(e => new
				{
					Element = e.Parent,
					Label = int.Parse(e.Attribute("content").Value)
				})
				.ToList();

			if ((notes?.Any() != true) || (refs.Count != notes.Count))
			{
				// no notes or out of sync so just give up!
				return;
			}

			// reorder footnotes in sync with refs...

			var map = refs.Select(r => r.Label).ToList();
			notes = notes.OrderBy(n => map.IndexOf(n.Label)).ToList();

			// refresh labels...

			int count = 0;
			for (int i = 0, label = 1; i < refs.Count; i++, label++)
			{
				var note = notes
					.Where(n => n.Label == refs[i].Label)
					.FirstOrDefault();

				if (note == null)
				{
					// something is awry!
					continue;
				}

				if (refs[i].Label != label)
				{
					var cdata = refs[i].CData.Value.Remove(refs[i].Index, refs[i].Length);
					refs[i].CData.Value = cdata.Insert(refs[i].Index, label.ToString());
					count++;
				}

				if (notes[i].Label != label)
				{
					notes[i].Element.Element(ns + "Meta").Attribute("content").Value = label.ToString();
					count++;
				}

				var text = FindSelectedReferences(notes[i].Element.Elements(ns + "T"), false)?.FirstOrDefault();

				if ((text?.Label != label))
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


		private List<FootnoteReference> FindSelectedReferences(IEnumerable<XElement> roots, bool super)
		{
			var pattern = super
				? @"vertical-align:super[;'""].*>\[(\d+)\]</span>"
				: @"\[(\d+)\]";

			// find selected "[\d]" labels
			var list = roots.DescendantNodes().OfType<XCData>()
				.Select(CData => new
				{
					CData,
					match = Regex.Match(CData.Value, pattern)
				})
				.Where(o => o.match.Success)
				.Select(o => new FootnoteReference
				{
					CData = o.CData,
					Label = int.Parse(o.match.Groups[1].Value),
					Index = o.match.Groups[1].Index,
					Length = o.match.Groups[1].Length
				})
				.ToList();

			// find selected footnote text lines
			foreach (var root in roots)
			{
				var meta = root.Parent.Elements(ns + "Meta")
					.Where(e => e.Attribute("name").Value.Equals("omfootnote"))
					.Select(e => new
					{
						CData = e.Parent.Element(ns + "T").DescendantNodes().OfType<XCData>().FirstOrDefault(),
						Label = int.Parse(e.Attribute("content").Value)
					})
					.FirstOrDefault();

				if ((meta != null) && !list.Any(e => e.Label == meta.Label))
				{
					var match = Regex.Match(meta.CData.Value, @"\[(\d+)\]");
					if (match.Success)
					{
						list.Add(new FootnoteReference
						{
							CData = meta.CData,
							Label = meta.Label,
							Index = match.Groups[1].Index,
							Length = match.Groups[1].Length
						});
					}
				}
			}

			return list;
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
			// find all selected paragraph
			var elements = page.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (elements?.Any() != true)
			{
				logger.WriteLine($"{nameof(FootnoteEditor.RemoveFootnote)} could not find a selected outline");
				return;
			}

			// matches both context body refs and footer section text lines
			var selections = FindSelectedReferences(elements, false);
			if (selections?.Any() != true)
			{
				MessageBox.Show(manager.Window, "No footnotes selected");
				return;
			}

			foreach (var selection in selections)
			{
				var parent = selection.CData.Parent.Parent; // should be a one:OE

				var found = parent.Elements(ns + "Meta")
					.Where(e => e.Attributes("name").Any(a => a.Value.Equals("omfootnote")))
					.Any();

				if (found)
				{
					// found a footnote, so remove it and associated reference

					parent.Remove();

					// associated reference
					var nref = page.Elements(ns + "Outline")
						.Where(e => e.Attributes("selected").Any())
						.DescendantNodes()
						.OfType<XCData>()
						.Where(c => Regex.IsMatch(c.Value, $@"vertical-align:super[;'""].*>\[{selection.Label}\]</span>"))
						.FirstOrDefault();

					if (nref != null)
					{
						RemoveReference(nref, selection.Label);
					}
				}
				else
				{
					// found a reference, so remove it and associated footnote

					RemoveReference(selection.CData, selection.Label);

					// associated footnote
					var note = page.Descendants(ns + "Meta")
						.Where(e =>
							e.Attribute("name").Value.Equals("omfootnote") &&
							e.Attribute("content").Value.Equals(selection.Label.ToString()))
						.Select(e => e.Parent)
						.FirstOrDefault();

					if (note != null)
					{
						note.Remove();
					}
				}
			}

			// make sure divider is set
			_ = EnsureFootnoteFooter();

			var remaining = divider.NodesAfterSelf().OfType<XElement>().Elements(ns + "Meta")
				.Any(e => e.Attribute("name").Value.Equals("omfootnote"));

			if (remaining)
			{
				// must be some footnotes so resequence them
				RefreshLabels();
			}
			else
			{
				// remove blank lines before divider
				var empties = divider.Parent.Descendants(ns + "Meta")
					.Where(e =>
						e.Attribute("name").Value.Equals("omfootnotes") &&
						e.Attribute("content").Value.Equals("empty"))
					.Select(e => e.Parent);

				if (empties != null)
				{
					foreach (var empty in empties.ToList())
					{
						empty.Remove();
					}
				}

				// remove divider line
				divider.Remove();
			}

			manager.UpdatePageContent(page);
		}


		/*
		<a href="..."><span style='vertical-align:super'>[2]</span></a>
		*/

		private void RemoveReference(XCData data, int label)
		{
			var wrapper = data.GetWrapper();

			var a = wrapper.Elements("a").Elements("span")
				.Where(e => 
					e.Attribute("style").Value.Contains("vertical-align:super") &&
					e.Value.Equals($"[{label}]"))
				.Select(e => e.Parent)
				.FirstOrDefault();

			if (a != null)
			{
				a.Remove();
				data.Value = wrapper.GetInnerXml();
			}
		}
	}
}
