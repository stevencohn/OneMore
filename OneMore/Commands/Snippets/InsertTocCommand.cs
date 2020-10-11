//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class InsertTocCommand : Command
	{

		public InsertTocCommand() : base()
		{
		}


		public void Execute()
		{
			HierarchyScope scope;
			bool addTopLinks;
			bool includePages;

			using (var dialog = new TocDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return;
				}

				scope = dialog.Scope;
				addTopLinks = dialog.TopLinks;
				includePages = dialog.SectionPages;
			}

			try
			{
				using (var manager = new ApplicationManager())
				{
					switch (scope)
					{
						case HierarchyScope.hsSelf:
							InsertHeadingsTable(manager, addTopLinks);
							break;

						case HierarchyScope.hsPages:
							InsertPagesTable(manager);
							break;

						case HierarchyScope.hsSections:
							InsertSectionsTable(manager, includePages);
							break;
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(InsertTocCommand)}", exc);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertHeadingsTable
		/// <summary>
		/// Inserts a table of contents at the top of the current page,
		/// of all headings on the page
		/// </summary>
		/// <param name="addTopLinks"></param>
		/// <param name="manager"></param>
		private void InsertHeadingsTable(ApplicationManager manager, bool addTopLinks)
		{
			var page = new Page(manager.CurrentPage());
			var ns = page.Namespace;

			var headings = page.GetHeadings(manager);

			var top = page.Root.Element(ns + "Outline")?.Element(ns + "OEChildren");
			if (top == null)
			{
				return;
			}

			var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
			var textColor = dark ? "#FFFFFF" : "#000000";

			var toc = new List<XElement>
			{
				// "Table of Contents"
				new XElement(ns + "OE",
				new XAttribute("style", $"font-size:16.0pt;color:{textColor}"),
				new XElement(ns + "T",
					new XCData("<span style='font-weight:bold'>Table of Contents</span>")
					)
				)
			};

			// use the minimum intent level
			var minlevel = headings.Min(e => e.Style.Index);

			foreach (var heading in headings)
			{
				var text = new StringBuilder();
				var count = minlevel;
				while (count < heading.Style.Index)
				{
					text.Append(". . ");
					count++;
				}

				if (!string.IsNullOrEmpty(heading.Link))
				{
					var linkColor = dark ? " style='color:#5B9BD5'" : string.Empty;
					text.Append($"<a href=\"{heading.Link}\"{linkColor}>{heading.Text}</a>");
				}
				else
				{
					text.Append(heading.Text);
				}

				toc.Add(new XElement(ns + "OE",
					new XAttribute("style", $"color:{textColor}"),
					new XElement(ns + "T", new XCData(text.ToString()))
					));
			}

			// empty line after the TOC
			toc.Add(new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))));

			top.AddFirst(toc);

			manager.UpdatePageContent(page.Root);
		}
		#endregion InsertHeadingsTable


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertPagesTables
		private void InsertPagesTable(ApplicationManager manager)
		{
			var section = manager.CurrentSection();
			var sectionId = section.Attribute("ID").Value;

			manager.Application.CreateNewPage(sectionId, out var pageId);

			var page = new Page(manager.GetPage(pageId));
			var ns = page.Namespace;

			page.PageName = "Table of Contents - Section " + section.Attribute("name").Value; // TODO: resx

			var container = new XElement(ns + "OEChildren");

			var elements = section.Elements(ns + "Page");
			foreach (var element in elements)
			{
				var text = new StringBuilder();
				var level = int.Parse(element.Attribute("pageLevel").Value);
				while (level > 0)
				{
					text.Append(". . ");
					level--;
				}

				manager.Application.GetHyperlinkToObject(
					element.Attribute("ID").Value, string.Empty, out var link);

				var name = element.Attribute("name").Value;
				text.Append($"<a href=\"{link}\">{name}</a>");

				container.Add(new XElement(ns + "OE",
					new XElement(ns + "T", new XCData(text.ToString())
					)));
			}

			var title = page.Root.Elements(ns + "Title").FirstOrDefault();
			title.AddAfterSelf(new XElement(ns + "Outline", container));
			manager.UpdatePageContent(page.Root);

			// move TOC page to top of section...

			// get current section again after new page is created
			section = manager.CurrentSection();

			var entry = section.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == pageId);

			entry.Remove();
			section.AddFirst(entry);
			manager.UpdateHierarchy(section);

			manager.Application.NavigateTo(pageId);
		}
		#endregion InsertPagesTables


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void InsertSectionsTable(ApplicationManager manager, bool includePages)
		{
			var section = manager.CurrentSection();
			var sectionId = section.Attribute("ID").Value;

			manager.Application.CreateNewPage(sectionId, out var pageId);

			var page = new Page(manager.GetPage(pageId));
			var ns = page.Namespace;

			var notebook = manager.CurrentNotebook();
			page.PageName = "Table of Contents - Notebook " + notebook.Attribute("name").Value; // TODO: resx

			var container = new XElement(ns + "OEChildren");

			BuildSectionTable(manager, ns, container, notebook.Elements(), includePages, 1);

			var title = page.Root.Elements(ns + "Title").FirstOrDefault();
			title.AddAfterSelf(new XElement(ns + "Outline", container));
			manager.UpdatePageContent(page.Root);

			// move TOC page to top of section...

			// get current section again after new page is created
			section = manager.CurrentSection();

			var entry = section.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == pageId);

			entry.Remove();
			section.AddFirst(entry);
			manager.UpdateHierarchy(section);

			manager.Application.NavigateTo(pageId);
		}


		private void BuildSectionTable(
			ApplicationManager manager, XNamespace ns, XElement container,
			IEnumerable<XElement> elements, bool includePages, int level)
		{
			foreach (var element in elements)
			{
				if (element.Name.LocalName == "SectionGroup")
				{
					// SectionGroup

					container.Add(new XElement(ns + "OE",
						new XElement(ns + "T", new XCData(element.Attribute("name").Value)
						)));

					BuildSectionTable(
						manager, ns, container, element.Elements(), includePages, level + 1);
				}
				else if (element.Name.LocalName == "Section" &&
					!element.Attributes().Any(a => a.Name == "isREcycleBin"))
				{
					// Section

					manager.Application.GetHyperlinkToObject(
						element.Attribute("ID").Value, string.Empty, out var link);

					var name = element.Attribute("name").Value;

					container.Add(new XElement(ns + "OE",
						new XElement(ns + "T", new XCData($"<a href=\"{link}\">{name}</a>")
						)));

					if (includePages)
					{
						//var elements = section.Elements(ns + "Page");
						//foreach (var element in elements)
						//{
						//	var text = new StringBuilder();
						//	var level = int.Parse(element.Attribute("pageLevel").Value);
						//	while (level > 0)
						//	{
						//		text.Append(". . ");
						//		level--;
						//	}

						//	manager.Application.GetHyperlinkToObject(
						//		element.Attribute("ID").Value, string.Empty, out var link);

						//	var name = element.Attribute("name").Value;
						//	text.Append($"<a href=\"{link}\">{name}</a>");

						//	container.Add(new XElement(ns + "OE",
						//		new XElement(ns + "T", new XCData(text.ToString())
						//		)));
						//}
					}
				}
			}
		}
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
