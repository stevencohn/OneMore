//************************************************************************************************
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

		public InsertTocCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			OneNote.Scope scope;
			bool addTopLinks;
			bool includePages;

			using (var dialog = new InsertTocDialog())
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
				using (var one = new OneNote())
				{
					switch (scope)
					{
						case OneNote.Scope.Self:
							await InsertHeadingsTable(one, addTopLinks);
							break;

						case OneNote.Scope.Pages:
							await InsertPagesTable(one);
							break;

						case OneNote.Scope.Sections:
							await InsertSectionsTable(one, includePages);
							break;
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error executing {nameof(InsertTocCommand)}", exc);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertHeadingsTable
		/// <summary>
		/// Inserts a table of contents at the top of the current page,
		/// of all headings on the page
		/// </summary>
		/// <param name="addTopLinks"></param>
		/// <param name="one"></param>
		private async Task InsertHeadingsTable(OneNote one, bool addTopLinks)
		{
			var page = one.GetPage();
			var ns = page.Namespace;

			var headings = page.GetHeadings(one);

			var top = page.Root.Element(ns + "Outline")?.Element(ns + "OEChildren");
			if (top == null)
			{
				return;
			}

			var title = page.Root.Elements(ns + "Title").Elements(ns + "OE").FirstOrDefault();
			if (title == null)
			{
				return;
			}

			var titleLink = one.GetHyperlink(page.PageId, title.Attribute("objectID").Value);
			var titleLinkText = $"<a href=\"{titleLink}\"><span style='font-style:italic'>{Resx.InsertTocCommand_Top}</span></a>";

			var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
			var textColor = dark ? "#FFFFFF" : "#000000";

			var toc = new List<XElement>
			{
				// "Table of Contents"
				new XElement(ns + "OE",
				new XAttribute("style", $"font-size:16.0pt;color:{textColor}"),
				new XElement(ns + "T",
					new XCData($"<span style='font-weight:bold'>{Resx.InsertTocCommand_TOC}</span>")
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

				if (addTopLinks)
				{
					var table = new Table(ns);
					table.AddColumn(400, true);
					table.AddColumn(100, true);
					var row = table.AddRow();
					row.Cells.ElementAt(0).SetContent(heading.Root);
					row.Cells.ElementAt(1).SetContent(
						new XElement(ns + "OE",
							new XAttribute("alignment", "right"),
							new XElement(ns + "T", new XCData(titleLinkText)))
						);

					// heading.Root is the OE
					heading.Root.ReplaceNodes(table.Root);
				}
			}

			// empty line after the TOC
			toc.Add(new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))));
			top.AddFirst(toc);

			await one.Update(page);
		}
		#endregion InsertHeadingsTable


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertPagesTables
		private async Task InsertPagesTable(OneNote one)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = one.GetPage(pageId);
			var ns = page.Namespace;

			page.Title = string.Format(Resx.InsertTocCommand_TOCSections, section.Attribute("name").Value);

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

				var link = one.GetHyperlink(element.Attribute("ID").Value, string.Empty);

				var name = element.Attribute("name").Value;
				text.Append($"<a href=\"{link}\">{name}</a>");

				container.Add(new XElement(ns + "OE",
					new XElement(ns + "T", new XCData(text.ToString())
					)));
			}

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
		#endregion InsertPagesTables


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertSectionsTable
		private async Task InsertSectionsTable(OneNote one, bool includePages)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = one.GetPage(pageId);
			var ns = page.Namespace;

			var scope = includePages ? OneNote.Scope.Pages : OneNote.Scope.Sections;
			var notebook = one.GetNotebook(scope);

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
								text.Append(". . ");
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
