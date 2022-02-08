//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class InsertTocCommand : Command
	{
		private const string TocMeta = "omToc";
		// TODO: deprecated
		private const string TocOptionsMeta = "omTocOptions";

		private const string LongDash = "\u2015";

		private const string RefreshStyle = "font-style:italic;font-size:9.0pt;color:#808080";
		private const string Indent8 = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";

		private OneNote one;
		private Style cite;


		public InsertTocCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			bool jumplinks;
			bool alignlinks;

			if (args.Length > 0 && args[0] is string refresh && refresh == "refresh")
			{
				jumplinks = args.Any(a => a as string == "links");
				alignlinks = args.Any(a => a as string == "align");

				if (await RefreshToc(jumplinks, alignlinks))
				{
					// successfully updated
					return;
				}
			}

			OneNote.Scope scope;
			bool withPreviews;
			bool withPages;

			using (var dialog = new InsertTocDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return;
				}

				scope = dialog.Scope;
				jumplinks = dialog.TopLinks;
				alignlinks = dialog.RightAlignTopLinks;
				withPreviews = dialog.PreviewPages;
				withPages = dialog.SectionPages;
			}

			try
			{
				using (one = new OneNote())
				{
					switch (scope)
					{
						case OneNote.Scope.Self:
							await InsertToc(one.GetPage(), jumplinks, alignlinks);
							break;

						case OneNote.Scope.Pages:
							await InsertPagesTable(withPreviews);
							break;

						case OneNote.Scope.Sections:
							await InsertSectionsTable(withPages, withPreviews);
							break;
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error executing {nameof(InsertTocCommand)}", exc);
			}
		}


		// TODO: deprecated!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		//       This routine can be removed and just replaced with a direct call to InsertToc
		//       from the main Execute method
		private async Task<bool> RefreshToc(bool jumplinks, bool alignlinks)
		{
			using (one = new OneNote(out var page, out var ns))
			{
				var meta = page.Root.Descendants(ns + "Meta")
					.FirstOrDefault(e => e.Attribute("name").Value == TocMeta);

				if (meta != null)
				{
					// remove container OE so it can be regenerated
					meta.Parent.Remove();
					// regenerate TOC
					await InsertToc(page, jumplinks, alignlinks);
					return true;
				}

				// TODO: deprecated...

				meta = page.Root.Descendants(ns + "Meta")
					.FirstOrDefault(e => e.Attribute("name").Value == TocOptionsMeta);

				if (meta != null)
				{
					var parts = meta.Attribute("content").Value.Split(';');
					var options = parts.Select(p => p.Split('=')).ToDictionary(s => s[0], s => s[1]);
					jumplinks = options.ContainsKey("addTopLinks") && options["addTopLinks"] == "True";
					alignlinks = options.ContainsKey("rightAlignTopLinks") && options["rightAlignTopLinks"] == "True";

					meta.Parent.Remove();
					await InsertToc(page, jumplinks, alignlinks);
					return true;
				}

				return false;
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertHeadingsTable
		/// <summary>
		/// Inserts a table of contents at the top of the current page,
		/// of all headings on the page
		/// </summary>
		/// <param name="jumplinks"></param>
		/// <param name="one"></param>
		private async Task InsertToc(Page page, bool jumplinks, bool alignlinks)
		{
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			var top = page.Root.Elements(ns + "Outline")
				.FirstOrDefault(e => !e.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank))?
				.Element(ns + "OEChildren");

			if (top == null)
			{
				UIHelper.ShowError(Resx.InsertTocCommand_NoHeadings);
				return;
			}

			var headings = page.GetHeadings(one);
			if (!headings.Any())
			{
				UIHelper.ShowError(Resx.InsertTocCommand_NoHeadings);
				return;
			}

			// an anchor at the title OE is used in links to jump back to the top of the page...
			var title = page.Root.Elements(ns + "Title").Elements(ns + "OE").FirstOrDefault();
			if (title == null)
			{
				UIHelper.ShowError(Resx.InsertTocCommand_NoHeadings);
				return;
			}

			// erase existing TOC...

			var meta = page.Root.Descendants(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name") is XAttribute attr &&
					(attr.Value == TocMeta || attr.Value == TocOptionsMeta));

			if (meta != null)
			{
				meta.Parent.Remove();
			}

			// build new TOC...

			var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
			var textColor = dark ? "#FFFFFF" : "#000000";

			var cmd = "onemore://InsertTocCommand/refresh";
			if (jumplinks) cmd = $"{cmd}/links";
			if (alignlinks) cmd = $"{cmd}/align";

			var refresh = $"<a href=\"{cmd}\"><span style='{RefreshStyle}'>{Resx.InsertTocCommand_Refresh}</span></a>";

			// "Table of Contents" line
			var toc = new Paragraph(
				$"<span style='font-weight:bold'>{Resx.InsertTocCommand_TOC}</span> " +
				$"<span style='{RefreshStyle}'>[{refresh}]</span>"
				)
				.SetStyle($"font-size:16.0pt;color:{textColor}");

			// use the minimum intent level
			var minlevel = headings.Min(e => e.Level);

			var container = new XElement(ns + "OEChildren");
			var index = 0;

			BuildHeadings(container, headings, ref index, minlevel, dark);

			var table = new Table(ns, 3, 1) { BordersVisible = false };
			table[0][0].SetContent(toc);
			table[1][0].SetContent(container);
			table[2][0].SetContent(string.Empty);

			// insert the TOC at the top of the page
			top.AddFirst(new XElement(ns + "OE",
				new Meta(TocMeta, string.Empty),
				table.Root)
				);

			// add top-of-page link to each header...

			if (jumplinks)
			{
				BuildJumpLinks(page, title, headings, alignlinks);
			}

			await one.Update(page);
		}


		private void BuildHeadings(
			XElement container, List<Heading> headings, ref int index, int level, bool dark)
		{
			while (index < headings.Count)
			{
				var heading = headings[index];

				if (heading.Level > level)
				{
					var children = new XElement(PageNamespace.Value + "OEChildren");
					BuildHeadings(children, headings, ref index, heading.Level, dark);
					container.Elements().Last().Add(children);
					index--;
				}
				else if (heading.Level == level)
				{
					var text = heading.Text;
					if (!string.IsNullOrEmpty(heading.Link))
					{
						var linkColor = dark ? " style='color:#5B9BD5'" : string.Empty;
						var clean = RemoveHyperlinks(heading.Text);
						text = $"<a href=\"{heading.Link}\"{linkColor}>{clean}</a>";
					}

					var textColor = dark ? "#FFFFFF" : "#000000";
					container.Add(new Paragraph(text).SetStyle($"color:{textColor}"));
				}
				else
				{
					break;
				}

				index++;
			}
		}


		private string RemoveHyperlinks(string text)
		{
			// removes hyperlinks from the text of a heading so the TOC hyperlink can be applied

			// clean up illegal directives; can be caused by using "Clip to OneNote" from Edge
			var wrapper = new XCData(text).GetWrapper();
			var links = wrapper.Elements("a").ToList();
			foreach (var link in links)
			{
				link.ReplaceWith(link.Value);
			}

			return wrapper.ToString(SaveOptions.DisableFormatting);
		}


		private void BuildJumpLinks(
			Page page, XElement title, List<Heading> headings, bool alignlinks)
		{
			var titleLink = one.GetHyperlink(page.PageId, title.Attribute("objectID").Value);
			var titleLinkText = $"<a href=\"{titleLink}\"><span " +
				$"style='font-style:italic'>{Resx.InsertTocCommand_Top}</span></a>";

			var ns = page.Namespace;

			foreach (var heading in headings)
			{
				if (!heading.HasTopLink)
				{
					if (alignlinks)
					{
						var table = new Table(ns);
						table.AddColumn(400, true);
						table.AddColumn(100, true);
						var row = table.AddRow();
						row.Cells.ElementAt(0).SetContent(heading.Root);

						row.Cells.ElementAt(1).SetContent(
							new Paragraph(titleLinkText).SetAlignment("right"));

						// heading.Root is the OE
						heading.Root.ReplaceNodes(table.Root);
					}
					else
					{
						var run = heading.Root.Elements(ns + "T").Last();

						run.AddAfterSelf(
							new XElement(ns + "T", new XCData(" ")),
							new XElement(ns + "T", new XCData(
								$"<span style=\"font-size:9pt;\">[{titleLinkText}]</span>"
								))
							);
					}
				}
			}
		}
		#endregion InsertHeadingsTable


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertPagesTables
		private async Task InsertPagesTable(bool withPreviews)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = one.GetPage(pageId);
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			page.Title = string.Format(Resx.InsertTocCommand_TOCSections, section.Attribute("name").Value);
			cite = page.GetQuickStyle(StandardStyles.Citation);

			var container = new XElement(ns + "OEChildren");

			var elements = section.Elements(ns + "Page");
			var index = 0;

			BuildSectionToc(container, elements.ToArray(), ref index, 1, withPreviews);

			var title = page.Root.Elements(ns + "Title").FirstOrDefault();
			title.AddAfterSelf(new XElement(ns + "Outline", container));
			await one.Update(page);

			// move TOC page to top of section...

			// get current section again after new page is created
			section = one.GetSection();

			var entry = section.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == pageId);

			entry.Remove();
			section.AddFirst(entry);
			one.UpdateHierarchy(section);

			await one.NavigateTo(pageId);
		}


		private void BuildSectionToc(
			XElement container, XElement[] elements, ref int index, int level, bool withPreviews)
		{
			string css = null;
			if (withPreviews)
			{
				cite.IsItalic = true;
				css = cite.ToCss();
			}

			while (index < elements.Length)
			{
				var element = elements[index];
				var pageID = element.Attribute("ID").Value;

				var pageLevel = int.Parse(element.Attribute("pageLevel").Value);
				if (pageLevel > level)
				{
					var children = new XElement(PageNamespace.Value + "OEChildren");
					BuildSectionToc(children, elements, ref index, pageLevel, withPreviews);
					container.Elements().Last().Add(children);
				}
				else if (pageLevel == level)
				{
					var link = one.GetHyperlink(pageID, string.Empty);
					var name = element.Attribute("name").Value;

					var text = withPreviews
						? $"<a href=\"{link}\">{name}</a> {GetPagePreview(pageID, css)}"
						: $"<a href=\"{link}\">{name}</a>";

					container.Add(new Paragraph(text));
				}
				else
				{
					break;
				}

				index++;
			}
		}


		private string GetPagePreview(string pageID, string css)
		{
			var page = one.GetPage(pageID, OneNote.PageDetail.Basic);
			var ns = page.Namespace;

			var outline = page.Root.Elements(ns + "Outline")
				.FirstOrDefault(e => !e.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank));

			if (outline == null)
			{
				return string.Empty;
			}

			logger.WriteLine($"page {page.Title}");

			// sanitize the content, extracting only raw text and aggregating lines
			var preview = outline.Descendants(ns + "T").Nodes().OfType<XCData>()
				.Select(c => c.GetWrapper().Value).Aggregate((a, b) => $"{a} {b}");

			if (preview.Length > 80) { preview = preview.Substring(0, 80) + "..."; }

			return $"<span style=\"{css}\">{LongDash} {preview}</span>";
		}
		#endregion InsertPagesTables


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertSectionsTable
		private async Task InsertSectionsTable(bool includePages, bool withPreviews)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = one.GetPage(pageId);
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			var scope = includePages ? OneNote.Scope.Pages : OneNote.Scope.Sections;
			var notebook = await one.GetNotebook(scope);

			page.Title = string.Format(Resx.InsertTocCommand_TOCNotebook, notebook.Attribute("name").Value);
			cite = page.GetQuickStyle(StandardStyles.Citation);

			var container = new XElement(ns + "OEChildren");

			BuildSectionTable(one, ns, container, notebook.Elements(), includePages, withPreviews, 1);

			var title = page.Root.Elements(ns + "Title").FirstOrDefault();
			title.AddAfterSelf(new XElement(ns + "Outline", container));

			await one.Update(page);

			// move TOC page to top of section...

			// get current section again after new page is created
			section = one.GetSection();

			var entry = section.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == pageId);

			entry.Remove();
			section.AddFirst(entry);
			one.UpdateHierarchy(section);

			await one.NavigateTo(pageId);
		}


		private void BuildSectionTable(
			OneNote one, XNamespace ns, XElement container,
			IEnumerable<XElement> elements, bool includePages, bool withPreviews, int level)
		{
			foreach (var element in elements)
			{
				var notBin = element.Attribute("isRecycleBin") == null;

				if (element.Name.LocalName == "SectionGroup" && notBin)
				{
					// SectionGroup

					var name = element.Attribute("name").Value;

					var indent = new XElement(ns + "OEChildren");

					indent.Add(new XElement(ns + "OE",
						new XElement(ns + "T",
							new XCData($"<span style='font-weight:bold'>{name}</span>"))
						));

					BuildSectionTable(
						one, ns, indent, element.Elements(), includePages, withPreviews, level + 1);

					container.Add(
						new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))),
						new XElement(ns + "OE", indent)
						);
				}
				else if (element.Name.LocalName == "Section" && notBin)
				{
					// Section

					var link = one.GetHyperlink(element.Attribute("ID").Value, string.Empty);
					var name = element.Attribute("name").Value;
					var pages = element.Elements(ns + "Page");

					if (includePages && pages.Any())
					{
						var indent = new XElement(ns + "OEChildren");
						var index = 0;

						BuildSectionToc(indent, pages.ToArray(), ref index, 1, withPreviews);

						container.Add(new XElement(ns + "OE",
							new XElement(ns + "T", new XCData($"<a href=\"{link}\">{name}</a>")),
							indent
							));
					}
					else
					{
						container.Add(new XElement(ns + "OE",
							new XElement(ns + "T", new XCData($"<a href=\"{link}\">{name}</a>")
							)));
					}
				}
			}
		}
		#endregion InsertSectionsTable
	}
}
