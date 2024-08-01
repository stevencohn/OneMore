//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Media;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class FootnoteEditor : Loggable
	{
		private const string FootnotesMeta = "omfootnotes";
		private const string FootnoteMeta = "omfootnote";
		private const string DividerContent = "divider";
		private const string EmptyContent = "empty";
		private const string RefreshStyle = "font-style:italic;font-size:9.0pt;color:#808080";

		private readonly OneNote one;
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
			page = Task.Run(async () => { return await one.GetPage(); }).Result;
			ns = page.Namespace;
			PageNamespace.Set(ns);

			dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
		}


		/// <summary>
		/// Determines if the cursor context is appropriate for footnote operations
		/// </summary>
		/// <returns>True if the cursor is positioned in the body of the page</returns>
		public bool ValidContext()
		{
			if (!page.ConfirmBodyContext())
			{
				MoreMessageBox.ShowError(one.Window, Resx.Error_BodyContext);
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

			if (element is null)
			{
				MoreMessageBox.ShowError(one.Window, Resx.Error_BodyContext);
				return;
			}

			// containing paragraph of selected run
			var parent = element.Parent;

			// ensure RTL is set appropriately
			var lang = parent.Attribute("lang")?.Value;
			if (!string.IsNullOrEmpty(lang))
			{
				var culture = new CultureInfo(lang);
				if (culture.TextInfo.IsRightToLeft)
				{
					parent.SetAttributeValue("RTL", "true");
				}
			}

			// rtl paragraph, rtl page, or rtl Windows language
			rightToLeft = parent.Attribute("RTL")?.Value == "true" || page.IsRightToLeft();

			if (!EnsureFootnoteFooter())
			{
				MoreMessageBox.ShowError(one.Window, Resx.Error_BodyContext);
				return;
			}

			var label = await WriteFootnoteText(parent.Attribute("objectID").Value);

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

			if (outline is null)
			{
				logger.WriteLine($"could not add footnote footer; no selected outline");
				return false;
			}

			var container = outline.Elements(ns + "OEChildren").FirstOrDefault();
			if (container is null)
			{
				logger.WriteLine($"could not add footnote footer; no outline container");
				return false;
			}

			divider = outline.Descendants(ns + "Meta").FirstOrDefault(e =>
				e.Attribute("name").Value == FootnotesMeta &&
				e.Attribute("content").Value == DividerContent);

			if (divider is not null)
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
				$"<span style='{RefreshStyle}'>{Resx.word_Refresh}</span></a>" +
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
				.Where(e => e.Attribute("name").Value.Equals(FootnoteMeta))
				.DefaultIfEmpty()  // avoids null ref exception
				.Max(e => e is null ? 0 : int.Parse(e.Attribute("content").Value))
				+ 1).ToString();

			// find last footnote (sibling) element after which new note is to be added
			var last = divider.NodesAfterSelf()
				.OfType<XElement>().Elements(ns + "Meta")
				.LastOrDefault(e => e.Attribute("name").Value.Equals(FootnoteMeta));

			// divider is a valid precedesor sibling; otherwise last's Parent
			last = last is null ? divider : last.Parent;

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
						"< "),
					cdatalink
					);
			}
			else
			{
				cdata = new XElement("wrapper", cdatalink,
					new XElement("span",
						new XAttribute("style", "font-family:'Calibri Light';font-size:11.0pt"),
						" >")
					);
			}

			color = dark ? "#BFBFBF" : "#151515";

			var note = new Paragraph(
				new XAttribute("style", $"color:{color}"),
				new XElement(ns + "Meta",
					new XAttribute("name", FootnoteMeta),
					new XAttribute("content", label)
				),
				new XElement(ns + "T",
					new XCData(cdata.GetInnerXml())
					)
				);

			if (rightToLeft)
			{
				note.SetRTL(rightToLeft);
			}

			last.AddAfterSelf(note);

			// update the page so OneNote will generate a new objectID
			await one.Update(page);

			// reload the page and reset state variables...
			page = await one.GetPage();
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
					e.Attribute("name").Value.Equals(FootnoteMeta) &&
					e.Attribute("content").Value.Equals(label))
				.Select(e => e.Parent.Attribute("objectID").Value)
				.FirstOrDefault();

			if (noteId is null)
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

			if (element is not null)
			{
				// add footnote link to end of selection range paragraph
				var cdata = element.DescendantNodes().OfType<XCData>().Last();
				if (cdata is not null)
				{
					cdata.Value =
						$"{cdata.Value}{note.ToString(SaveOptions.DisableFormatting)}";

					// set insertion just after footnote ref
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
			var refs = FindSelectedReferences(
				page.Root.Descendants(ns + "T").InDocumentOrder(),
				true);

			if (refs?.Any() != true)
			{
				// no refs so just give up!
				return;
			}

			// find all footnotes
			var notes = page.Root.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name").Value.Equals(FootnoteMeta))
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
				var note = notes.Find(n => n.Label == refs[i].Label);
				if (note is null)
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

				if (text is not null && (text.Label != label))
				{
					var cdata = text.CData.Value.Remove(text.Index, text.Length);
					text.CData.Value = cdata.Insert(text.Index, label.ToString());
					count++;
				}
			}

			if (count > 0)
			{
				// divider will be null if this routine is invoked independently from Refresh cmd
				if (divider is null)
				{
					divider = page.Root.Elements(ns + "Outline")
						.Where(e => e.Attributes("selected").Any())
						.Descendants(ns + "Meta")
						.FirstOrDefault(e =>
							e.Attribute("name").Value.Equals(FootnotesMeta) &&
							e.Attribute("content").Value.Equals(DividerContent))?
						.Parent;

					if (divider is null)
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
				for (var i = 0; i < notes.Count; i++)
				{
					var note = notes[i];
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
				? @"vertical-align:super[;'""][^>]*>\[(\d+)\]</span>"
				: @"\[(\d+)\]";

			var regex = new Regex(pattern);

			// there could be multiple references in each text run...

			// find selected "[\d]" labels in body of page
			var data = roots.DescendantNodes().OfType<XCData>()
				.Select(CData => new
				{
					CData,
					matches = regex.Matches(CData.Value)
				})
				.Where(o => o.matches.Count > 0);

			var list = new List<FootnoteReference>();
			foreach (var datum in data)
			{
				foreach (Match match in datum.matches)
				{
					list.Add(new FootnoteReference
					{
						CData = datum.CData,
						Label = int.Parse(match.Groups[1].Value),
						Index = match.Groups[1].Index,
						Length = match.Groups[1].Length
					});
				}
			}

			// find selected footnote text lines in footer of page
			foreach (var root in roots)
			{
				var meta = root.Parent.Elements(ns + "Meta")
					.Where(e => e.Attribute("name").Value.Equals(FootnoteMeta))
					.Select(e => new
					{
						CData = e.Parent.Element(ns + "T").DescendantNodes().OfType<XCData>().FirstOrDefault(),
						Label = int.Parse(e.Attribute("content").Value)
					})
					.FirstOrDefault();

				if ((meta is not null) && !list.Exists(e => e.Label == meta.Label))
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
		/// If the cursor is not positioned over a reference or text then display a message
		/// asking the user to move the cursor.
		/// </remarks>
		public async Task RemoveFootnote()
		{
			var range = new SelectionRange(page);
			var cursor = range.GetSelection();

			if (range.Scope != SelectionScope.TextCursor &&
				range.Scope != SelectionScope.SpecialCursor)
			{
				logger.WriteLine("could not delete footnote, cursor not found");
				SystemSounds.Exclamation.Play();
				return;
			}

			string label = null;
			int index = -1;
			int length;

			var meta = cursor.Parent.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == FootnoteMeta);

			if (meta is not null)
			{
				// cursor must be positioned on a footnote text item
				label = meta.Attribute("content").Value;
				logger.WriteLine($"found note [{label}]");
			}
			else if (range.Scope == SelectionScope.SpecialCursor) // URL
			{
				// cursor is on a hyperlink, check that it matches the [label] syntax
				var match = Regex.Match(cursor.Value,
					@"vertical-align:super[;'""][^>]*>\[(\d+)\]<\/span>");

				if (match.Success)
				{
					label = match.Groups[1].Value;
					index = match.Groups[1].Index;
					length = match.Groups[1].Length;
					logger.WriteLine($"found link is [{label}] @{index}..{length}");
				}
			}

			if (string.IsNullOrWhiteSpace(label))
			{
				logger.WriteLine("could not delete footnote, cursor not positioned");
				SystemSounds.Exclamation.Play();
				return;
			}

			if (index < 0)
			{
				// find reference and remove it
				var cdata = page.Root.Elements(ns + "Outline")
					.DescendantNodes()
					.OfType<XCData>()
					.FirstOrDefault(c => Regex.IsMatch(
						c.Value,
						$@"vertical-align:super[;'""][^>]*>\[{label}\]<\/span>"));

				if (cdata is not null)
				{
					RemoveReference(cdata, label);
				}

				// found note, remove it
				cursor.Parent.Remove();
			}
			else
			{
				// found reference, remove it
				var cdata = cursor.DescendantNodes().OfType<XCData>().First();
				RemoveReference(cdata, label);

				// find note and remove it
				page.Root.Descendants(ns + "Meta")
					.Where(e =>
						e.Attribute("name").Value.Equals(FootnoteMeta) &&
						e.Attribute("content").Value.Equals(label))
					.Select(e => e.Parent)
					.FirstOrDefault()?
					.Remove();
			}

			// make sure divider is set
			_ = EnsureFootnoteFooter();

			var remaining = divider.NodesAfterSelf().OfType<XElement>().Elements(ns + "Meta")
				.Any(e => e.Attribute("name").Value.Equals(FootnoteMeta));

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

				if (empties.Any())
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
		private static void RemoveReference(XCData data, string label)
		{
			var wrapper = data.GetWrapper();
			var a = wrapper.Elements("a").Elements("span")
				.Where(e =>
					e.Attribute("style").Value.Contains("vertical-align:super") &&
					e.Value.Equals($"[{label}]"))
				.Select(e => e.Parent)
				.FirstOrDefault();

			if (a is not null)
			{
				a.Remove();
				data.Value = wrapper.GetInnerXml();
			}
		}

		#endregion Delete
	}
}
