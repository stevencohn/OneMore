//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Snippets.TocGenerators
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal sealed class PageTocGenerator : TocGenerator
	{
		public const int MinToCWidth = 400;

		private Page page;
		private XNamespace ns;


		public PageTocGenerator(TocParameters parameters)
			: base(parameters)
		{
		}


		protected override string RefreshCmd => "refresh";


		public override async Task<bool> Build()
		{
			await using var one = new OneNote(out page, out ns);

			if (!ValidatePage(one, out var topElement, out var headings, out var titleID))
			{
				return false;
			}

			// build new TOC...

			PageNamespace.Set(ns);
			var container = LocateInsertionPoint(page, ns, topElement);

			var content = new XElement(ns + "OEChildren");
			var index = 0;
			// use the minimum indent level
			var minlevel = headings.Min(e => e.Level);
			var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;

			BuildHeadings(content, headings, ref index, minlevel, dark);

			var table = new Table(ns, 3, 1) { BordersVisible = false };

			var watt = container.Ancestors(ns + "Outline")
				.Elements(ns + "Size").Attributes("width").FirstOrDefault();

			var colwid = watt is not null && float.TryParse(
					watt.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
					out float width)
				? (float)Math.Round(Math.Max(width, MinToCWidth + 40) - 40, 2)
				: MinToCWidth;

			if (colwid < MinToCWidth || colwid > Table.MaxColumnWidth)
			{
				colwid = MinToCWidth;
			}

			table.SetColumnWidth(0, colwid);

			table[0][0].SetContent(MakeTitle(page));
			table[1][0].SetContent(content);
			table[2][0].SetContent(string.Empty);

			container.Add(
				new Meta(Toc.MetaName, string.Empty),
				table.Root
				);

			// add top-of-page link to each header...

			if (parameters.Contains("links"))
			{
				AddTopLinksToHeadings(one, titleID, headings);
			}
			else
			{
				RemoveTopLinksFromHeadings(headings);
			}

			await one.Update(page);
			return true;
		}


		private bool ValidatePage(OneNote one,
			out XElement topElement, out List<Heading> headings, out string titleID)
		{
			// check taht there are headings on the page...

			headings = null;
			titleID = null;

			topElement = page.Root
				.Elements(ns + "Outline")
				.FirstOrDefault(e => !e.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank))?
				.Element(ns + "OEChildren");

			if (topElement == null)
			{
				//ShowError(Resx.InsertTocCommand_NoHeadings);
				return false;
			}

			headings = page.GetHeadings(one);
			if (!headings.Any())
			{
				//ShowError(Resx.InsertTocCommand_NoHeadings);
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
				//ShowError(Resx.InsertTocCommand_NoHeadings);
				return false;
			}

			return true;
		}


		private void BuildHeadings(
			XElement container, List<Heading> headings, ref int index, int level, bool dark)
		{
			static string RemoveHyperlinks(string text)
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


		private void AddTopLinksToHeadings(OneNote one, string titleID, List<Heading> headings)
		{
			var titleLink = one.GetHyperlink(page.PageId, titleID);
			var titleLinkText = $"<a href=\"{titleLink}\"><span " +
				$"style='font-style:italic'>{Resx.InsertTocCommand_Top}</span></a>";

			var align = parameters.Contains("align");

			if ((align && !headings.Exists(h => h.IsRightAligned)) ||
				(!align && headings.Exists(h => h.IsRightAligned)))
			{
				RemoveTopLinksFromHeadings(headings);
			}

			foreach (var heading in headings)
			{
				if (!heading.HasTopLink)
				{
					if (align)
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


		private void RemoveTopLinksFromHeadings(List<Heading> headings)
		{
			foreach (var heading in headings)
			{
				if (heading.IsRightAligned)
				{
					var container = heading.Root.Ancestors(ns + "Table").First().Parent;
					container.ReplaceWith(heading.Root);
				}
				else if (heading.HasTopLink)
				{
					heading.Root.Elements().Remove();
					heading.Root.Add(new XElement(ns + "T", new XCData(heading.Text)));
				}

				heading.IsRightAligned = heading.HasTopLink = false;
			}
		}


		public override async Task<bool> Refresh()
		{
			try
			{
				await Build();
				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error refreshing table of contents", exc);
				return false;
			}
		}
	}
}
