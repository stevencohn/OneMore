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
			bool addSectionPages;

			using (var dialog = new TocDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return;
				}

				scope = dialog.Scope;
				addTopLinks = dialog.TopLinks;
				addSectionPages = dialog.SectionPages;
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
							InsertSectionsTable(manager, addSectionPages);
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

			page.PageName = "Table of Contents"; // TODO: resx

			var children = new XElement(ns + "OEChildren");

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

				children.Add(new XElement(ns + "OE",
					new XElement(ns + "T", new XCData(text.ToString())
					)));
			}

			var title = page.Root.Elements(ns + "Title").FirstOrDefault();

			title.AddAfterSelf(new XElement(ns + "Outline", children));

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

		private void InsertSectionsTable(ApplicationManager manager, bool addSectionPages)
		{
			// to do...
		}
	}
}
