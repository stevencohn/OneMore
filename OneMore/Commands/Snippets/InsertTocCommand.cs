﻿//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S6605 // Collection-specific "Exists" method should be used

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
	using Resx = Properties.Resources;


	[CommandService]
	internal class InsertTocCommand : Command
	{
		public enum TitleStyles
		{
			StandardPageTitle = 0,
			StandardHeading1 = 1,
			StandardHeading2 = 2,
			StandardHeading3 = 3,
			CustomPageTitle = 4,
			CustomHeading1 = 5,
			CustomHeading2 = 6,
			CustomHeading3 = 7
		}


		private const string TocMeta = "omToc";
		private const string LongDash = "\u2015";
		private const string RefreshStyle = "font-style:italic;font-size:9.0pt;color:#808080";
		private const int MinProgress = 25;

		private Style cite;
		private UI.ProgressDialog progress;


		public InsertTocCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (args.Length > 0 && args[0] is string refresh && refresh == "refresh")
			{
				await RefreshTableOfContents(args);
				return;
			}

			using var dialog = new InsertTocDialog();
			if (dialog.ShowDialog(owner) == DialogResult.Cancel)
			{
				return;
			}

			try
			{
				await (dialog.Scope switch
				{
					OneNote.Scope.Self =>
						InsertTableOfContents(
							dialog.AddTopLinks, dialog.RightAlign,
							dialog.InsertHere, dialog.TitleStyle),

					OneNote.Scope.Pages =>
						MakePageIndexPage(dialog.PreviewPages),

					_ => MakeSectionIndexPage(dialog.SectionPages, dialog.PreviewPages)
				});
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error executing {nameof(InsertTocCommand)}", exc);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region InsertTableOfContents
		private async Task RefreshTableOfContents(object[] args)
		{
			// expected arguments: [/links[/align][/top]/[/style`n]
			// need to interpret URL with backwards compatibility...

			var addTopLinks = false;
			var rightAlign = false;
			var insertHere = false;
			var titleStyle = TitleStyles.StandardPageTitle;

			for (var i = 1; i < args.Length; i++)
			{
				if (args[i] is string value)
				{
					if (value == "links")
					{
						addTopLinks = true;
					}
					else if (value == "align")
					{
						rightAlign = true;
					}
					else if (value == "here")
					{
						insertHere = true;
					}
					else if (value.StartsWith("style") && value.Length > 5)
					{
						Enum.TryParse(value.Substring(5), out titleStyle);
					}
				}
			}

			try
			{
				await InsertTableOfContents(addTopLinks, rightAlign, insertHere, titleStyle);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error refreshing table of contents", exc);
			}
		}


		/// <summary>
		/// Inserts a table of contents at the top of the current page,
		/// of all headings on the page
		/// </summary>
		/// <param name="addTopLinks"></param>
		/// <param name="one"></param>
		private async Task InsertTableOfContents(
			bool addTopLinks, bool rightAlign, bool insertHere, TitleStyles titleStyle)
		{
			using var one = new OneNote();
			var page = one.GetPage(OneNote.PageDetail.Selection);
			var ns = page.Namespace;

			if (!ValidatePage(one, page, ns, out var top, out var headings, out var titleID))
			{
				return;
			}

			PageNamespace.Set(ns);

			var container = LocateInsertionPoint(page, ns, top, insertHere);

			// build new TOC...

			var cmd = "onemore://InsertTocCommand/refresh";
			if (addTopLinks) cmd = $"{cmd}/links";
			if (rightAlign) cmd = $"{cmd}/align";
			if (insertHere) cmd = $"{cmd}/here";
			cmd = $"{cmd}/style{(int)titleStyle}";

			var refresh = $"<a href=\"{cmd}\"><span style='{RefreshStyle}'>{Resx.word_Refresh}</span></a>";

			// be sure to emit this with ToRBGHtml() otherwise OneNote may normalize White/Black
			// color names, removing them against Dark/Light backgrounds respectively
			var textColor = page.GetBestTextColor();

			// "Table of Contents" line
			var toc = new Paragraph(
				$"<span style='font-weight:bold'>{Resx.InsertTocCommand_TOC}</span> " +
				$"<span style='{RefreshStyle}'>[{refresh}]</span>"
				)
				.SetStyle($"font-size:16.0pt;color:{textColor.ToRGBHtml()}");

			var content = new XElement(ns + "OEChildren");
			var index = 0;
			// use the minimum indent level
			var minlevel = headings.Min(e => e.Level);
			var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;

			BuildHeadings(content, headings, ref index, minlevel, dark);

			var table = new Table(ns, 3, 1) { BordersVisible = false };
			table[0][0].SetContent(toc);
			table[1][0].SetContent(content);
			table[2][0].SetContent(string.Empty);

			container.Add(
				new Meta(TocMeta, string.Empty),
				table.Root
				);

			// add top-of-page link to each header...

			if (addTopLinks)
			{
				BuildJumpLinks(one, page, titleID, headings, rightAlign);
			}

			await one.Update(page);
		}


		private bool ValidatePage(OneNote one, Page page, XNamespace ns,
			out XElement top, out List<Heading> headings, out string titleID)
		{
			headings = null;
			titleID = null;

			top = page.Root
				.Elements(ns + "Outline")
				.FirstOrDefault(e => !e.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank))?
				.Element(ns + "OEChildren");

			if (top == null)
			{
				UIHelper.ShowError(Resx.InsertTocCommand_NoHeadings);
				return false;
			}

			headings = page.GetHeadings(one);
			if (!headings.Any())
			{
				UIHelper.ShowError(Resx.InsertTocCommand_NoHeadings);
				return false;
			}

			// need the title OE ID to make a link back to the top of the page
			titleID = page.Root
				.Elements(ns + "Title")
				.Elements(ns + "OE")
				.Attributes("objectID")
				.FirstOrDefault()?.Value;

			if (titleID == null)
			{
				UIHelper.ShowError(Resx.InsertTocCommand_NoHeadings);
				return false;
			}

			return true;
		}


		private XElement LocateInsertionPoint(
			Page page, XNamespace ns, XElement top, bool insertHere)
		{
			XElement container;

			var meta = top
				.Descendants(ns + "Meta")
				.FirstOrDefault(e =>
					e.Attribute("name") is XAttribute attr && attr.Value == TocMeta);

			if (meta == null)
			{
				// make new and add to page...

				container = new XElement(ns + "OE");

				if (insertHere)
				{
					page.AddNextParagraph(container);
				}
				else
				{
					top.AddFirst(container);
				}
			}
			else
			{
				// reuse old and clear out to prepare for new table...

				container = meta.Parent;
				container.Elements().Remove();

				// if user wants it at top of page, make sure that's where it is
				if (!insertHere && container.ElementsBeforeSelf(ns + "OE").Any())
				{
					container.Remove();
					top.AddFirst(container);
				}
				else if (insertHere)
				{
					container.Remove();
					page.AddNextParagraph(container);
				}
			}

			return container;
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
					if (!container.Elements().Any())
					{
						container.Add(new Paragraph());
					}

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
			OneNote one, Page page, string titleID,
			List<Heading> headings, bool alignlinks)
		{
			var titleLink = one.GetHyperlink(page.PageId, titleID);
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
		#endregion InsertTableOfContents


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region MakePageIndexPage
		private async Task MakePageIndexPage(bool withPreviews)
		{
			using var one = new OneNote();
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

			var pageCount = elements.Count();
			if (pageCount > MinProgress)
			{
				progress = new UI.ProgressDialog();
				progress.SetMaximum(pageCount);
				progress.Show();
			}

			try
			{
				BuildSectionToc(one, container, elements.ToArray(), ref index, 1, withPreviews);
			}
			finally
			{
				if (progress != null)
				{
					progress.Close();
					progress.Dispose();
				}
			}

			var title = page.Root.Elements(ns + "Title").First();
			title.AddAfterSelf(new XElement(ns + "Outline", container));
			await one.Update(page);

			// move TOC page to top of section...

			// get current section again after new page is created
			section = one.GetSection();

			var entry = section.Elements(ns + "Page")
				.First(e => e.Attribute("ID").Value == pageId);

			entry.Remove();
			section.AddFirst(entry);
			one.UpdateHierarchy(section);

			await one.NavigateTo(pageId);
		}


		private void BuildSectionToc(
			OneNote one, XElement container, XElement[] elements,
			ref int index, int level, bool withPreviews)
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
					BuildSectionToc(one,children, elements, ref index, pageLevel, withPreviews);
					container.Elements().Last().Add(children);
				}
				else if (pageLevel == level)
				{
					var link = one.GetHyperlink(pageID, string.Empty);
					var name = element.Attribute("name").Value;

					if (progress != null)
					{
						progress.SetMessage(name);
						progress.Increment();
					}

					var text = withPreviews
						? $"<a href=\"{link}\">{name}</a> {GetPagePreview(one, pageID, css)}"
						: $"<a href=\"{link}\">{name}</a>";

					container.Add(new Paragraph(text));

					index++;
				}
				else
				{
					break;
				}
			}
		}


		private string GetPagePreview(OneNote one, string pageID, string css)
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
			var preview = outline.Descendants(ns + "T")?.Nodes().OfType<XCData>()
				.Select(c => c.GetWrapper().Value).Aggregate(string.Empty, (a, b) => $"{a} {b}");

			if (preview == null || preview.Length == 0)
			{
				return string.Empty;
			}

			if (preview.Length > 80)
			{
				preview = preview.Substring(0, 80);

				// cheap HTML encoding...
				if (preview.IndexOf('<') >= 0)
				{
					preview = preview.Replace("<", "&lt;");
				}

				if (preview.IndexOf('>') >= 0)
				{
					preview = preview.Replace(">", "&gt;");
				}

				preview = $"{preview}...";
			}

			return $"<span style=\"{css}\">{LongDash} {preview}</span>";
		}
		#endregion MakePageIndexPage


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region MakeSectionIndexPage
		private async Task MakeSectionIndexPage(bool includePages, bool withPreviews)
		{
			using var one = new OneNote();
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

			var pageCount = notebook.Descendants(ns + "Page").Count();
			if (pageCount > MinProgress)
			{
				progress = new UI.ProgressDialog();
				progress.SetMaximum(pageCount);
				progress.Show();
			}


			try
			{
				BuildSectionTable(one, ns, container, notebook.Elements(), includePages, withPreviews, 1);
			}
			finally
			{
				if (progress != null)
				{
					progress.Close();
					progress.Dispose();
				}
			}

			var title = page.Root.Elements(ns + "Title").First();
			title.AddAfterSelf(new XElement(ns + "Outline", container));

			await one.Update(page);

			// move TOC page to top of section...

			// get current section again after new page is created
			section = one.GetSection();

			var entry = section.Elements(ns + "Page")
				.First(e => e.Attribute("ID").Value == pageId);

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
							// this is a Folder icon... but doesn't look great
							// <span style='font-family:Segoe UI Emoji'>&#128194; </span>
							new XCData($"<span style='font-weight:bold'>{name}</span>"))
						));

					BuildSectionTable(
						one, ns, indent, element.Elements(), includePages, withPreviews, level + 1);

					container.Add(
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

						BuildSectionToc(one, indent, pages.ToArray(), ref index, 1, withPreviews);

						container.Add(new XElement(ns + "OE",
							new XElement(ns + "T", new XCData($"§ <a href=\"{link}\">{name}</a>")),
							indent
							));
					}
					else
					{
						container.Add(new XElement(ns + "OE",
							new XElement(ns + "T", new XCData($"§ <a href=\"{link}\">{name}</a>")
							)));
					}
				}
			}
		}
		#endregion MakeSectionIndexPage
	}
}
