//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Media;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class FootnoteEditor
	{
		private const string FootnotesMeta = "omfootnotes";
		private const string DividerContent = "divider";
		private const string EmptyContent = "empty";
		private const string RefreshStyle = "font-style:italic;font-size:9.0pt;color:#808080";

		private readonly OneNote one;
		private readonly ILogger logger;
		private readonly bool dark;

		private Page page;
		private XNamespace ns;
		private bool rightToLeft;

		private XElement divider;


		/// <summary>
		/// Initialize a new instance of the editor with the given manager
		/// </summary>
		/// <param name="one">The OneNote application manager (abstraction)</param>
		public FootnoteEditor(OneNote one)
		{
			this.one = one;

			page = one.GetPage();
			ns = page.Namespace;
			PageNamespace.Set(ns);

			dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;

			logger = Logger.Current;
		}


		/// <summary>
		/// Determines if the cursor context is appropriate for footnote operations
		/// </summary>
		/// <returns>True if the cursor is positioned in the body of the page</returns>
		public bool ValidContext()
		{
			if (!page.ConfirmBodyContext())
			{
				UIHelper.ShowError(Resx.Error_BodyContext);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Adds a new footnote to the page, possibly renumbering existing footnotes
		/// to ensure all foonotes are in sequential order from top to bottom.
		/// </summary>
		public async Task AddFootnote()
		{
			// find the selected run, the insertion point
			var element = page.Root.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Descendants(ns + "T")
				.LastOrDefault(e => e.Attribute("selected")?.Value == "all");

			if (element == null)
			{
				UIHelper.ShowError(one.Window, Resx.Error_BodyContext);
				return;
			}

			// last selected paragraph OE
			element = element.Parent;

			// rtl paragraph, rtl page, or rtl Windows language
			rightToLeft = element.Attribute("RTL")?.Value == "true" || page.IsRightToLeft();

			if (!EnsureFootnoteFooter())
			{
				UIHelper.ShowError(one.Window, Resx.Error_BodyContext);
				return;
			}

			var label = await WriteFootnoteText(element.Attribute("objectID").Value);

			if (WriteFootnoteRef(label))
			{
				await RefreshLabels();

				await one.Update(page);
			}
		}


		/*
		  <one:OE style="font-family:'Courier New';font-size:10.0pt;color:#999696">
			<one:Meta name="onemore" content="footnotes" />
			<one:T><![CDATA[─ ─ . . . ─ ─]]></one:T>
		  </one:OE>
		*/
		/// <summary>
		/// Looks for the footer paragraph, marked with the meta "omfootnotes". If not found,
		/// it will create this section.
		/// </summary>
		/// <returns>True if found or created; false is there was a problem</returns>
		private bool EnsureFootnoteFooter()
		{
			// the selected Outline must be the right context
			var outline = page.Root.Elements(ns + "Outline")
				.FirstOrDefault(e => e.Attributes("selected").Any());

			if (outline == null)
			{
				logger.WriteLine($"could not add footnote footer; no selected outline");
				return false;
			}

			var container = outline.Elements(ns + "OEChildren").FirstOrDefault();
			if (container == null)
			{
				logger.WriteLine($"could not add footnote footer; no outline container");
				return false;
			}

			divider = outline.Descendants(ns + "Meta").FirstOrDefault(e =>
				e.Attribute("name").Value == FootnotesMeta &&
				e.Attribute("content").Value == DividerContent);

			if (divider != null)
			{
				// found the divider so return its parent OE
				divider = divider.Parent;
				return true;
			}

			// else build a new divider...

			var line = string.Concat(Enumerable.Repeat("- ", 50));
			page.EnsurePageWidth(line, "Courier New", 10f, one.WindowHandle);

			// add a couple of empty lines for spacing
			container.Add(
				new Paragraph(
					new Meta(FootnotesMeta, EmptyContent),
					new XElement(ns + "T", string.Empty)
					).SetRTL(rightToLeft),
				new Paragraph(
					new Meta(FootnotesMeta, EmptyContent),
					new XElement(ns + "T", string.Empty)
					).SetRTL(rightToLeft)
				);

			// build the divider line
			line = line.Substring(0, 42) +
				$"<span stlye='{RefreshStyle}'>[</span><a href=\"onemore://RefreshFootnotesCommand/\">" +
				$"<span style='{RefreshStyle}'>{Resx.AddFootnoteCommand_Refresh}</span></a>" +
				$"<span style='{RefreshStyle}'>]</span>";

			var color = dark ? "#595959" : "#999696";
			var style = $"font-family:'Courier New';font-size:10.0pt;color:{color}";
			if (rightToLeft)
			{
				style = $"direction:rtl;{style};direction:rtl;unicode-bidi:embed";
			}

			var paragraph = new Paragraph(
				new Meta(FootnotesMeta, DividerContent),
				new XElement(ns + "T", new XCData(line))
				)
				.SetStyle(style)
				.SetRTL(rightToLeft);

			// add the new divider line
			divider = paragraph;
			container.Add(divider);

			return true;
		}


		/// <summary>
		/// Add a footnote to the footer section at the bottom of the outline
		/// </summary>
		/// <param name="textId">The objectId of the paragraph containing the cursor</param>
		/// <returns>The footnote label, the footnote ID#</returns>
		private async Task<string> WriteFootnoteText(string textId)
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
				.LastOrDefault(e => e.Attribute("name").Value.Equals("omfootnote"));

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

			var link = one.GetHyperlink(page.PageId, textId);

			var color = dark ? ";color:#5B9BD5" : string.Empty;

			var cdatalink = new XElement("a",
				new XAttribute("href", link),
				new XElement("span",
					new XAttribute("style", $"font-family:'Calibri Light';font-size:11.0pt{color}"),
					$"[{label}]"
				));

			XElement cdata;
			if (rightToLeft)
			{
				cdata = new XElement("wrapper",
					new XElement("span",
						new XAttribute("style", "font-family:'Calibri Light';font-size:11.0pt"),
						"new footnote "),
					cdatalink
					);
			}
			else
			{
				cdata = new XElement("wrapper", cdatalink,
					new XElement("span",
						new XAttribute("style", "font-family:'Calibri Light';font-size:11.0pt"),
						" new footnote")
					);
			}

			color = dark ? "#BFBFBF" : "#151515";

			var note = new Paragraph(
				new XAttribute("style", $"color:{color}"),
				new XElement(ns + "Meta",
					new XAttribute("name", "omfootnote"),
					new XAttribute("content", label)
				),
				new XElement(ns + "T",
					new XCData(cdata.GetInnerXml())
					)
				);

			if (rightToLeft)
			{
				note.SetAlignment("right");
			}

			last.AddAfterSelf(note);

			// update the page so OneNote will generate a new objectID
			await one.Update(page);

			// reload the page and reset state variables...
			page = one.GetPage();
			ns = page.Namespace;

			EnsureFootnoteFooter();

			return label;
		}


		/// <summary>
		/// Insert the linked label at the cursor insertion point within the body text
		/// </summary>
		/// <param name="label">The footnote label; it's label ID#</param>
		/// <returns></returns>
		private bool WriteFootnoteRef(string label)
		{
			// find the new footer by its label and get its new objectID
			var noteId = page.Root.Descendants(ns + "Meta")
				.Where(e =>
					e.Attribute("name").Value.Equals("omfootnote") &&
					e.Attribute("content").Value.Equals(label))
				.Select(e => e.Parent.Attribute("objectID").Value)
				.FirstOrDefault();

			if (noteId == null)
			{
				return false;
			}

			var link = one.GetHyperlink(page.PageId, noteId);

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
			var element = page.Root.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Descendants(ns + "T")
				.LastOrDefault(e => e.Attribute("selected")?.Value == "all");

			if (element != null)
			{
				// add footnote link to end of selection range paragraph
				var cdata = element.DescendantNodes().OfType<XCData>().Last();
				if (cdata != null)
				{
					cdata.ReplaceWith(
						new XCData(cdata.Value + note.ToString(SaveOptions.DisableFormatting))
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

		#region Refresh
		/// <summary>
		/// Refreshes the label numbers so that all references are sequentially ordered starting
		/// at 1 from the top of the page. This is needed when adding a new reference prior to
		/// existing ones or deleting a reference.
		/// </summary>
		public async Task RefreshLabels(bool updatePage = false)
		{
			var refs = FindSelectedReferences(page.Root.Descendants(ns + "T"), true);

			if (refs?.Any() != true)
			{
				// no refs so just give up!
				return;
			}

			// find all footnotes
			var notes = page.Root.Descendants(ns + "Meta")
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
					.FirstOrDefault(n => n.Label == refs[i].Label);

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

				if (text != null && (text.Label != label))
				{
					var cdata = text.CData.Value.Remove(text.Index, text.Length);
					text.CData.Value = cdata.Insert(text.Index, label.ToString());
					count++;
				}
			}

			if (count > 0)
			{
				// divider will be null if this routine is invoked independently from Refresh cmd
				if (divider == null)
				{
					divider = page.Root.Elements(ns + "Outline")
						.Where(e => e.Attributes("selected").Any())
						.Descendants(ns + "Meta")
						.FirstOrDefault(e =>
							e.Attribute("name").Value.Equals(FootnotesMeta) &&
							e.Attribute("content").Value.Equals(DividerContent))?
						.Parent;

					if (divider == null)
					{
						// no divider so just give up!
						return;
					}
				}

				// remove notes and re-add in sorted order

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

				if (updatePage)
				{
					await one.Update(page);
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
		#endregion Refresh


		//=======================================================================================

		#region Delete
		/// <summary>
		/// Removes the footnote at the current cursor position, either located on a footnote
		/// reference or a footnote text.
		/// </summary>
		/// <remarks>
		/// A dialog is displayed if the cursor is not positioned over a footnote ref or text.
		/// </remarks>
		public async Task RemoveFootnote()
		{
			// find all selected paragraph
			var elements = page.Root.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (elements?.Any() != true)
			{
				logger.WriteLine($"{nameof(FootnoteEditor.RemoveFootnote)} could not find a selected outline");
				SystemSounds.Exclamation.Play();
				return;
			}

			// matches both context body refs and footer section text lines
			var selections = FindSelectedReferences(elements, false);
			if (selections?.Any() != true)
			{
				logger.WriteLine($"{nameof(FootnoteEditor.RemoveFootnote)} could not find a selected reference");
				SystemSounds.Exclamation.Play();
				return;
			}

			foreach (var selection in selections)
			{
				var parent = selection.CData.Parent.Parent; // should be a one:OE

				var found = parent.Elements(ns + "Meta")
					.Any(e => e.Attributes("name").Any(a => a.Value.Equals("omfootnote")));

				if (found)
				{
					// found a footnote, so remove it and associated reference

					parent.Remove();

					// associated reference
					var nref = page.Root.Elements(ns + "Outline")
						.Where(e => e.Attributes("selected").Any())
						.DescendantNodes()
						.OfType<XCData>()
						.FirstOrDefault(c => Regex.IsMatch(
							c.Value,
							$@"vertical-align:super[;'""].*>\[{selection.Label}\]</span>"));

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
					var note = page.Root.Descendants(ns + "Meta")
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
				await RefreshLabels();
			}
			else
			{
				// remove blank lines before divider
				var empties = divider.Parent.Descendants(ns + "Meta")
					.Where(e =>
						e.Attribute("name").Value.Equals(FootnotesMeta) &&
						e.Attribute("content").Value.Equals(EmptyContent))
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

			await one.Update(page);
		}


		/*
		<a href="..."><span style='vertical-align:super'>[2]</span></a>
		*/

		private static void RemoveReference(XCData data, int label)
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
		#endregion Delete
	}
}
