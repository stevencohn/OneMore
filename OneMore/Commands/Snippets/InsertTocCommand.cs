﻿//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class InsertTocCommand : Command
	{
		private const string TocMeta = "omToc";
		// TODO: deprecated
		private const string TocOptionsMeta = "omTocOptions";

		private const string RefreshStyle = "font-style:italic;font-size:9.0pt;color:#808080";
		private const string Indent8 = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";

		private OneNote one;
		private int citeIndex;


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
							await InsertSectionsTable(withPages);
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

			var titleLink = one.GetHyperlink(page.PageId, title.Attribute("objectID").Value);
			var titleLinkText = $"<a href=\"{titleLink}\"><span " +
				$"style='font-style:italic'>{Resx.InsertTocCommand_Top}</span></a>";

			var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
			var textColor = dark ? "#FFFFFF" : "#000000";

			var cmd = "onemore://InsertTocCommand/refresh";
			if (jumplinks) cmd = $"{cmd}/links";
			if (alignlinks) cmd = $"{cmd}/align";

			var refresh = $"<a href=\"{cmd}\"><span style='{RefreshStyle}'>{Resx.InsertTocCommand_Refresh}</span></a>";

			var toc = new List<XElement>
			{
				// "Table of Contents" line
				new Paragraph(
					$"<span style='font-weight:bold'>{Resx.InsertTocCommand_TOC}</span> " +
					$"<span style='{RefreshStyle}'>[{refresh}]</span>"
					)
					.SetStyle($"font-size:16.0pt;color:{textColor}")
			};

			// use the minimum intent level
			var minlevel = headings.Min(e => e.Level);

			foreach (var heading in headings)
			{
				var text = new StringBuilder();
				var count = minlevel;
				while (count < heading.Level)
				{
					text.Append(Indent8);
					count++;
				}

				if (!string.IsNullOrEmpty(heading.Link))
				{
					var linkColor = dark ? " style='color:#5B9BD5'" : string.Empty;
					var clean = RemoveHyperlinks(heading.Text);
					text.Append($"<a href=\"{heading.Link}\"{linkColor}>{clean}</a>");
				}
				else
				{
					text.Append(heading.Text);
				}

				//text.Append($"(count:{count}=level:{heading.Level})");

				toc.Add(new Paragraph(text.ToString()).SetStyle($"color:{textColor}"));

				if (jumplinks && !heading.HasTopLink)
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

			// empty line after the TOC
			toc.Add(new Paragraph(string.Empty));

			var container = new Table(ns, 1, 1) { BordersVisible = false };
			container[0][0].SetContent(new XElement(ns + "OEChildren", toc));

			top.AddFirst(new XElement(ns + "OE",
				new Meta(TocMeta, String.Empty),
				container.Root)
				);

			await one.Update(page);
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
			citeIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

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
			while (index < elements.Length)
			{
				var element = elements[index];
				var pageID = element.Attribute("ID").Value;
				var ns = element.GetNamespaceOfPrefix(OneNote.Prefix);

				var pageLevel = int.Parse(element.Attribute("pageLevel").Value);
				if (pageLevel > level)
				{
					var children = new XElement(ns + "OEChildren");
					BuildSectionToc(children, elements, ref index, pageLevel, withPreviews);
					container.Elements().Last().Add(children);
				}
				else if (pageLevel == level)
				{
					var link = one.GetHyperlink(pageID, string.Empty);
					var name = element.Attribute("name").Value;

					container.Add(new Paragraph($"<a href=\"{link}\">{name}</a>"));

					if (withPreviews)
					{
						AppendPreview(container, pageID);
					}
				}
				else
				{
					break;
				}

				index++;
			}
		}


		private void AppendPreview(XElement container, string pageID)
		{
			var page = one.GetPage(pageID, OneNote.PageDetail.Basic);
			var ns = page.Namespace;

			var ce = page.Root.Elements(ns + "Outline")
				.FirstOrDefault(e => !e.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank));

			var preview = ce == null ? string.Empty : ce.TextValue();
			if (preview.Length > 100) { preview = preview.Substring(0, 100) + "..."; }

			container.Add(new Paragraph(preview).SetQuickStyle(citeIndex));
			container.Add(new Paragraph(string.Empty));
		}
		#endregion InsertPagesTables


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertSectionsTable
		private async Task InsertSectionsTable(bool includePages)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = one.GetPage(pageId);
			var ns = page.Namespace;

			var scope = includePages ? OneNote.Scope.Pages : OneNote.Scope.Sections;
			var notebook = await one.GetNotebook(scope);

			page.Title = string.Format(Resx.InsertTocCommand_TOCNotebook, notebook.Attribute("name").Value);

			var container = new XElement(ns + "OEChildren");

			BuildSectionTable(one, ns, container, notebook.Elements(), includePages, 1);

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
			IEnumerable<XElement> elements, bool includePages, int level)
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
						one, ns, indent, element.Elements(), includePages, level + 1);

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

						foreach (var page in pages)
						{
							var text = new StringBuilder();
							var plevel = int.Parse(page.Attribute("pageLevel").Value);
							while (plevel > 0)
							{
								text.Append("\t");
								plevel--;
							}

							var plink = one.GetHyperlink(page.Attribute("ID").Value, string.Empty);

							var pname = page.Attribute("name").Value;
							text.Append($"<a href=\"{plink}\">{pname}</a>");

							indent.Add(new XElement(ns + "OE",
								new XElement(ns + "T", new XCData(text.ToString())
								)));
						}

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
/*
<one:Notebook xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote" name="Waters" nickname="Waters" ID="{CC6FC6F1-BD14-4FD6-A934-6A31BF8836E1}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Waters/" lastModifiedTime="2020-10-10T16:26:52.000Z" color="#8AB6E2" isCurrentlyViewed="true">
  <one:Section name="Notes" ID="{19D2987D-72BD-0D29-17FD-7D30C15F1FE2}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Waters/Notes.one" lastModifiedTime="2020-09-25T13:06:16.000Z" color="#FFD869">
    <one:Page ID="{19D2987D-72BD-0D29-17FD-7D30C15F1FE2}{1}{E188573882613585322981946966423193255375821}" name="Peer Impact Awards" dateTime="2008-04-04T15:54:03.000Z" lastModifiedTime="2016-06-07T15:13:41.000Z" pageLevel="1" />
  </one:Section>
  <one:SectionGroup name="OneNote_RecycleBin" ID="{4B379CD2-6D99-4149-BA1B-68B83028AF8C}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Waters/OneNote_RecycleBin/" lastModifiedTime="2020-04-18T14:56:02.000Z" isRecycleBin="true">
    <one:Section name="Deleted Pages" ID="{09209055-01BA-09A1-3B28-20B02B70E41B}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Waters/OneNote_RecycleBin/OneNote_DeletedPages.one" lastModifiedTime="2020-04-18T14:56:02.000Z" color="#E1E1E1" isInRecycleBin="true" isDeletedPages="true" />
  </one:SectionGroup>
  <one:SectionGroup name="g1" ID="{FB629CB1-E0D1-409A-92E8-752E72348537}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Waters/g1/" lastModifiedTime="2020-10-10T16:26:52.000Z">
    <one:Section name="foo" ID="{59D86390-9B9F-4673-A607-F77FA24FA5F0}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Waters/g1/foo.one" lastModifiedTime="2020-10-10T16:26:52.000Z" color="#F5F96F">
      <one:Page ID="{59D86390-9B9F-4673-A607-F77FA24FA5F0}{1}{E19531428620227426150320144405193344912910641}" name="Titled" dateTime="2020-10-10T16:26:41.000Z" lastModifiedTime="2020-10-10T16:26:48.000Z" pageLevel="1" />
    </one:Section>
  </one:SectionGroup>
</one:Notebook>
*/
