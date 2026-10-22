//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Snippets.Toc
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Builds or refreshes a TOC for both custom and standard headings on the current page
	/// </summary>
	internal sealed class PageTocGenerator : TocGenerator
	{
		/*
		TBC...

		internal enum TocLocation
		{
			// values must match index of InsertPageTocDialog.locationBox items
			//
			// At top of first outline
			// At top of page, inserted
			// At top of page, overlayed
			// At current cursor
		

			TopOutline,
			//TopInserted,
			TopOverlay,
			Cursor
		}
		*/

		public const int MinToCWidth = 400;
		private const string TodoChars = "☐⭕🟥🟦🟧🟨🟩🟪🟫"; // Segoe UI Emoji

		private Page page;
		private XNamespace ns;
		private string pageId;
		private int todoIndex = -1;
		private List<string> todos;
		private Dictionary<string, XElement> savedEntries;


		public PageTocGenerator(TocParameters parameters, string pageId = null)
			: base(parameters)
		{
			this.pageId = pageId;
		}


		protected override string RefreshCmd => "refresh";


		protected override string PrimaryTitle => Resx.InsertTocCommand_TOC;


		public override async Task<bool> Build()
		{
			await using var one = new OneNote();

			page = pageId is null ? await one.GetPage() : await one.GetPage(pageId);
			ns = page.Namespace;

			var secondary = parameters.Any(p => p.Equals("secondary"));

			var headings = CollectHeadings(one, secondary, out var titleID);
			if (!headings.Any())
			{
				await ClearToC(one);
				return false;
			}

			var level = parameters.FirstOrDefault(p => p.StartsWith("level"));
			if (level is not null)
			{
				var levels = int.Parse(level.Substring(5));
				headings = headings.Where(h => h.Level <= levels).ToList();
			}

			var todo = parameters.FirstOrDefault(p => p.StartsWith("todo"));
			if (todo is not null)
			{
				todoIndex = int.Parse(todo.Substring(4));
				todos = MakeEmojiList();
			}

			// build new TOC...

			var op = parameters.Contains(RefreshCmd) ? "refresh" : "build";
			logger.WriteLine($"{op} toc for page {page.Title}");

			PageNamespace.Set(ns);

			// a refresh preserves the user's formatting of existing entries
			if (refreshing)
			{
				savedEntries = CaptureEntries();
			}

			var container = LocateBestContainerOE();

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

			var segments = string.Empty;
			if (parameters.Contains("links")) segments = $"{segments}/links";
			if (parameters.Contains("align")) segments = $"{segments}/align";
			if (parameters.Contains("here")) segments = $"{segments}/here";
			if (parameters.Contains("over")) segments = $"{segments}/over";
			if (todo is not null) segments = $"{segments}/{todo}";
			if (level is not null) segments = $"{segments}/{level}";
			if (secondary) segments = $"{segments}/secondary";

			table[0][0].SetContent(MakeTitle(page, segments));
			table[1][0].SetContent(content);
			table[2][0].SetContent(string.Empty);

			// meta...

			if (!parameters.Contains("page")) parameters.Insert(0, "page");
			var segs = parameters.Aggregate((a, b) => $"{a}/{b}");

			container.Add(
				new Meta(Toc.MetaName, segs),
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

			await one.UpdateWithProgress(page);
			return true;
		}


		private List<string> MakeEmojiList()
		{
			var list = new List<string>();
			var e = StringInfo.GetTextElementEnumerator(TodoChars);

			while (e.MoveNext())
				list.Add(e.GetTextElement());

			return list;
		}


		private List<Heading> CollectHeadings(OneNote one, bool secondary, out string titleID)
		{
			// check that there are headings on the page...

			titleID = null;

			// must have a body outline
			if (page.BodyOutlines?.Elements(ns + "OEChildren").FirstOrDefault() is null)
			{
				return new();
			}

			// must have headings
			var headings = page.GetHeadings(one, secondary: secondary);
			if (!headings.Any())
			{
				return headings;
			}

			// need the title OE ID to make a link back to the top of the page
			titleID = page.Root
				.Elements(ns + "Title")
				.Elements(ns + "OE")
				.Attributes("objectID")
				.FirstOrDefault()?.Value;

			if (titleID is null)
			{
				headings.Clear();
				return headings;
			}

			return headings;
		}


		private async Task ClearToC(OneNote one)
		{
			if (FindMetaElement() is XElement meta)
			{
				var result = UI.MoreMessageBox.ShowQuestion(
					one.OwnerWindow, Resx.InsertTocForPage_ClearToc);

				if (result == System.Windows.Forms.DialogResult.Yes)
				{
					meta.Parent.Remove();
					page.EnsureContentContainer();
					await one.UpdateWithProgress(page);
				}
			}
			else
			{
				logger.WriteLine($"{nameof(PageTocGenerator)} found no headings");
				UI.MoreMessageBox.ShowError(one.OwnerWindow, Resx.InsertTocCommand_NoHeadings);
			}
		}


		private XElement FindMetaElement()
		{
			return page.BodyOutlines
				.Descendants(ns + "Meta")
				.FirstOrDefault(e =>
					e.Attribute("name") is XAttribute attr && attr.Value == Toc.MetaName);
		}


		private XElement LocateBestContainerOE()
		{
			XElement container;

			// allow exactly one TOC on a page so if found, just replace it...

			var meta = FindMetaElement();
			if (meta is not null)
			{
				container = meta.Parent;
				container.Elements().Remove();
				return container;
			}

			// creating new TOC...

			if (parameters.Contains("over"))
			{
				return CreateOverlayContainer();
			}

			container = new XElement(ns + "OE");

			// try to find selection; need to find T runs and then OE because OE will be
			// marked partially if not all of its runs are selected
			var anchor = page.BodyOutlines
				.Descendants(ns + "T")
				.Where(e => e.Attribute("selected") is XAttribute a && a.Value == "all")
				.Select(e => e.Parent)
				.FirstOrDefault();

			if (parameters.Contains("here"))
			{
				if (anchor is not null)
				{
					/*
					 * 
					 * 
					 * TODO: this is tempoary. Need to extend PageEditor to split a paragraph
					 * 
					 * this is the old code:
					 * 
						if (page.GetSelectedElements() is not null &&
							page.SelectionScope != SelectionScope.Unknown)
						{
							container.Remove();
							page.AddNextParagraph(container);
						}
					 * 
					 * 
					 * 
					 * Need another Insert Top of Outline option in dialog???
					 * 
					 * 
					 * 
					 */

					// add container before selected paragraph for now
					anchor.AddBeforeSelf(container);
					return container;
				}
			}

			// if not here or can't find selection then:
			anchor = anchor is null
				// add to top of first container on page
				? page.EnsureContentContainer()
				// add to top of current container (!here)
				: anchor.FirstAncestor(ns + "Outline");

			// NOTE: I don't think this can break?!
			if (anchor.Name.LocalName == "OEChildren")
			{
				anchor.AddFirst(container);
			}
			else
			{
				anchor.Elements(ns + "OEChildren").First().AddFirst(container);
			}

			return container;
		}


		private XElement CreateOverlayContainer()
		{
			// <one:Position x="36.0" y="86.0" z="0" />
			// <one:Size width="286.3078918457031" height="375.5271911621094" isSetByUser="true" />

			var outlines = page.Root.Elements(ns + "Outline");

			var x = (int)outlines.Max(o =>
				float.Parse(o.Element(ns + "Position").Attribute("x").Value) +
				float.Parse(o.Element(ns + "Size").Attribute("width").Value));

			var y = (int)outlines.Elements(ns + "Position").Min(p => float.Parse(p.Attribute("y").Value));

			x = Math.Max(x - MinToCWidth, 200);
			y = Math.Min(y + 30, 125);

			var container = new XElement(ns + "OE");
			var outline = new XElement(ns + "Outline",
				new XElement(ns + "Position",
					new XAttribute("x", $"{x}.0"),
					new XAttribute("y", $"{y}.0")),
				new XElement(ns + "OEChildren",
					container)
				);

			page.Root.Add(outline);

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
					string clean = null;
					if (!string.IsNullOrEmpty(heading.Link))
					{
						var linkColor = dark ? " style='color:#5B9BD5'" : string.Empty;
						clean = CleanHeadingText(heading.Text);

						text = $"<a href=\"{heading.Link}\"{linkColor}>{clean}</a>";
					}

					var todoSuffix = string.Empty;
					if (todoIndex >= 0)
					{
						var incomplete = heading.Root.Elements(ns + "Tag")
							.Count(t => t.Attribute("completed").Value == "false");

						if (incomplete > 0)
						{
							todoSuffix = $" <span style='font-family:\"Segoe UI Emoji\"'>{todos[todoIndex]}</span>";
							text = $"{text}{todoSuffix}";
						}
					}

					var textColor = dark ? "#FFFFFF" : "#000000";
					var paragraph = new Paragraph(text).SetStyle($"color:{textColor}");

					if (savedEntries is not null && clean is not null &&
						LinkKey(heading.Link) is string key &&
						savedEntries.TryGetValue(key, out var previous))
					{
						KeepFormatting(paragraph, previous, heading.Link, clean, todoSuffix);
					}

					container.Add(paragraph);
				}
				else
				{
					break;
				}

				index++;
			}
		}


		/// <summary>
		/// Gets the object ID of the heading targeted by the given hyperlink or markup.
		/// </summary>
		private static string LinkKey(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return null;
			}

			// the GUID is shared by many objects; the short suffix after it identifies the object
			var match = Regex.Match(html,
				@"object-id=(\{[0-9A-Fa-f\-]+\})(?:&amp;|&)([0-9A-Fa-f]+)");

			return match.Success
				? $"{match.Groups[1].Value}&{match.Groups[2].Value}".ToUpperInvariant()
				: null;
		}


		/// <summary>
		/// Collects the entries of the existing TOC, keyed by the heading each links to,
		/// so their user-applied formatting can be reapplied to the rebuilt entries.
		/// </summary>
		private Dictionary<string, XElement> CaptureEntries()
		{
			var entries = new Dictionary<string, XElement>();

			if (FindMetaElement()?.Parent is not XElement container)
			{
				return entries;
			}

			foreach (var oe in container.Descendants(ns + "OE"))
			{
				// the first run holds the link to the heading; skip the title with its
				// refresh links, which also has an anchor
				var cdata = oe.Elements(ns + "T").Nodes().OfType<XCData>().FirstOrDefault();
				if (cdata is null || cdata.Value.Contains(Toc.RefreshUri))
				{
					continue;
				}

				var key = LinkKey(cdata.Value);
				if (key is not null && !entries.ContainsKey(key))
				{
					entries.Add(key, oe);
				}
			}

			return entries;
		}


		/// <summary>
		/// Applies the formatting of a previous TOC entry to its rebuilt replacement
		/// while keeping the new heading text and link.
		/// </summary>
		private void KeepFormatting(
			Paragraph paragraph, XElement previous, string link, string clean, string todoSuffix)
		{
			// paragraph-level formatting...

			// NOTE: the OneNote API cannot preserve text color; it keeps bold, italic, font,
			// font size and highlight, so a user-applied color is lost on save

			foreach (var name in new[] { "style", "alignment", "quickStyleIndex" })
			{
				if (previous.Attribute(name) is XAttribute attr)
				{
					paragraph.SetAttributeValue(name, attr.Value);
				}
			}

			// run-level formatting...

			var oldRun = previous.Elements(ns + "T").FirstOrDefault();
			var newRun = paragraph.Elements(ns + "T").FirstOrDefault();
			if (oldRun?.FirstNode is not XCData cdata || newRun is null)
			{
				return;
			}

			var wrapper = cdata.GetWrapper();
			var anchor = wrapper.Descendants("a").FirstOrDefault(
				a => a.Attribute("href")?.Value.StartsWith("onenote:") == true);

			if (anchor is null)
			{
				return;
			}

			anchor.SetAttributeValue("href", link);

			// anything after the link is stale, such as the todo marker
			var top = anchor;
			while (top.Parent is not null && top.Parent != wrapper)
			{
				top = top.Parent;
			}

			top.NodesAfterSelf().ToList().ForEach(n => n.Remove());

			// descend to the innermost element wrapping the text, which holds the formatting
			var target = anchor;
			while (target.Elements().Count() == 1 &&
				target.Nodes().OfType<XText>().All(t => string.IsNullOrWhiteSpace(t.Value)))
			{
				target = target.Elements().First();
			}

			// CleanHeadingText returns its own "cdata" wrapper element; keep only its content
			var content = new XCData(clean).GetWrapper();
			if (content.Elements().Count() == 1 && content.Elements().First().Name == "cdata")
			{
				content = content.Elements().First();
			}

			target.ReplaceNodes(content.Nodes());

			var html = string.Concat(wrapper.Nodes().Select(n => n.ToString(SaveOptions.DisableFormatting)));

			foreach (var attr in oldRun.Attributes().Where(a => a.Name.LocalName != "selected"))
			{
				newRun.SetAttributeValue(attr.Name, attr.Value);
			}

			newRun.ReplaceNodes(new XCData($"{html}{todoSuffix}"));
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


		public override async Task<RefreshOption> RefreshExistingPage()
		{
			await Task.Yield();
			return RefreshOption.Build;
		}
	}
}
