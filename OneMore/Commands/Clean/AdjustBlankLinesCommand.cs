//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Describes how blank lines between paragraphs are adjusted.
	/// </summary>
	internal enum BlankLineMode
	{
		/// <summary>Remove all blank lines between paragraphs</summary>
		RemoveAll = 0,

		/// <summary>Collapse consecutive blank lines into a single blank line</summary>
		KeepOne = 1,

		/// <summary>
		/// Collapse consecutive blank lines and insert a blank line between plain paragraphs
		/// that are not separated by one
		/// </summary>
		ExactlyOne = 2
	}


	/// <summary>
	/// Adjusts blank lines between paragraphs: removes all of them, collapses consecutive lines
	/// into one, or ensures exactly one separates plain paragraphs. Blank lines after headings are
	/// handled separately: one is always kept when requested, otherwise none are kept. Empty headings, custom and standard, are removed when
	/// removing all lines; otherwise they are converted to normal blank lines and collapsed.
	/// Also outdents and re-indents empty lines so related paragraphs stay grouped.
	/// </summary>
	internal class AdjustBlankLinesCommand : Command, ICliPageCommand
	{
		private Page page;
		private XNamespace ns;
		private IEnumerable<XElement> runs;
		private BlankLineMode mode = BlankLineMode.KeepOne;
		private bool headingBlank;

		private List<Style> quickStyles;
		private List<Style> headingQuickStyles;
		private List<Style> headingCustomStyles;


		public AdjustBlankLinesCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "AdjustBlankLines";

		public string Description =>
			"Adjust blank lines between paragraphs: remove all, keep one, or ensure exactly one";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false)
			.AddBoolean("all", "Remove all empty lines instead of keeping one between paragraphs",
				required: false, defaultValue: false)
			.AddString("mode",
				"Blank lines between paragraphs: remove, keep (at most one), or exactly-one; " +
				"overrides 'all'", required: false)
			.AddBoolean("headingBlank", "Put one blank line after each heading; otherwise none are kept",
				required: false, defaultValue: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				cliParams.TryGet("pageId", out string pageId);
				if (string.IsNullOrWhiteSpace(pageId)) { return; }

				cliParams.TryGet("all", out bool cliAll);
				cliParams.TryGet("mode", out string cliMode);
				cliParams.TryGet("headingBlank", out bool cliHeading);

				mode = ParseMode(cliMode, cliAll);
				headingBlank = cliHeading;

				await using var one = new OneNote();
				page = await one.GetPage(pageId, OneNote.PageDetail.All);
				ns = page.Namespace;

				var range = new Models.SelectionRange(page);
				runs = range.GetSelections(defaulToAnytIfNoRange: true);

				await Run(one);
				return;
			}

			using var guard = EnterOnce();
			if (guard is null) { return; }

			using var dialog = new AdjustBlankLinesDialog();
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			mode = dialog.Mode;
			headingBlank = dialog.HeadingBlank;

			logger.StartClock();

			await RunInteractive();
		}


		private static BlankLineMode ParseMode(string value, bool all)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return all ? BlankLineMode.RemoveAll : BlankLineMode.KeepOne;
			}

			value = value.Trim();

			if (value.Equals("remove", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("all", StringComparison.OrdinalIgnoreCase))
			{
				return BlankLineMode.RemoveAll;
			}

			if (value.StartsWith("exact", StringComparison.OrdinalIgnoreCase))
			{
				return BlankLineMode.ExactlyOne;
			}

			return BlankLineMode.KeepOne;
		}


		private async Task RunInteractive()
		{
			await using var one = new OneNote();
			page = await one.GetPage(OneNote.PageDetail.Selection);
			ns = page.Namespace;

			var range = new Models.SelectionRange(page);
			runs = range.GetSelections(defaulToAnytIfNoRange: true);
			logger.WriteLine($"found {runs.Count()} runs, scope={range.Scope}");

			await Run(one);
		}


		private async Task Run(OneNote one)
		{
			quickStyles = page.GetQuickStyles();

			headingQuickStyles = quickStyles
				.Where(s => s.StyleType == StyleType.Heading)
				.ToList();

			headingCustomStyles = new ThemeProvider().Theme.GetStyles()
				.Where(e => e.StyleType == StyleType.Heading)
				.ToList();

			// snapshot the paragraphs in scope before any are moved or removed
			var scope = runs
				.Select(r => r.Parent)
				.Where(p => p is not null)
				.Distinct()
				.ToList();

			var modified = OutdentEmptyLines();
			modified = CollapseEmptyLines() || modified;

			if (mode == BlankLineMode.ExactlyOne)
			{
				modified = InsertBlankLines(scope) || modified;
			}

			modified = AdjustHeadingBlanks(scope) || modified;

			modified = IndentEmptyLines() || modified;

			logger.WriteTime("saving", true);

			if (modified)
			{
				await one.Update(page);
				logger.WriteTime("saved");
			}
		}


		private bool OutdentEmptyLines()
		{
			/* Outdent empty indented lines so we can then easily check if there are consecutive
			 * empty lines that need to be collapse. An indended paragraph pattern is:
			 *
			 * <OEChildren>
			 *   <OE>
			 *     <OEChildren> indented OE...
			 */

			var children = runs.Descendants(ns + "OEChildren").ToList();
			if (children.Any())
			{
				return OutdentEmptyLines(children[0].Parent, children);
			}

			return false;
		}


		private bool OutdentEmptyLines(XElement parent, List<XElement> children)
		{
			// recursively find empty indented lines and outdent them

			var modified = false;

			for (var i = 0; i < children.Count; i++)
			{
				var child = children[i];
				if (child.HasElements)
				{
					OutdentEmptyLines(child, child.Elements(ns + "OE").Elements(ns + "OEChildren").ToList());

					if (child.TextValue().Trim() == string.Empty)
					{
						// move contents of OEChildren to the containing OE
						// and remove the OEChildren element

						var kids = child.Elements();
						child.Remove();
						parent.Add(kids);
						modified = true;
					}
				}
			}

			return modified;
		}


		public bool CollapseEmptyLines()
		{
			// find consecutive empty paragraphs that need to be collapsed...

			var elements = runs
				.Select(e => e.Parent)
				.Distinct()
				.Where(e => e.TextValue().Trim().Length == 0 && !e.IsMathML())
				.ToList();

			if (!elements.Any())
			{
				//logger.WriteLine("no blank lines found");
				return false;
			}

			//logger.WriteLine($"found {elements.Count} collapsable lines");

			var modified = false;

			foreach (var element in elements)
			{
				if (mode == BlankLineMode.RemoveAll)
				{
					element.Remove();
					modified = true;
					continue;
				}

				// is this a known standard or custom Heading style?
				if (IsHeading(element))
				{
					// convert empty heading to a normal blank line, which may then be
					// collapsed below with adjacent blank lines
					ClearHeadingStyle(element);
					modified = true;
				}

				// is this an empty paragraph preceded by an empty paragraph?
				if (element.PreviousNode is XElement prev && prev.Name.LocalName == "OE")
				{
					// is the previous paragraph entirely empty? note a paragraph with text can
					// end with an empty run, e.g. the one holding the text cursor
					var t = prev.Elements().Last();
					if (t.Name.LocalName == "T" && prev.TextValue().Trim().Length == 0)
					{
						// remove consecutive empty line
						prev.Remove();
						modified = true;
					}
				}
			}

			// clean up left-over empty OEChildrens
			page.Root.Descendants(ns + "OEChildren")
				.Where(e => !e.HasElements)
				.Remove();

			return modified;
		}


		/// <summary>
		/// Inserts a blank line between adjacent plain paragraphs, skipping list items, tables
		/// and code blocks, so that exactly one blank line separates all plain paragraphs.
		/// </summary>
		private bool InsertBlankLines(List<XElement> scope)
		{
			var modified = false;

			foreach (var oe in scope)
			{
				if (oe.Parent is null || !IsPlainParagraph(oe))
				{
					continue;
				}

				var next = oe.ElementsAfterSelf().FirstOrDefault();
				if (next is null || !IsPlainParagraph(next))
				{
					continue;
				}

				// a paragraph whose last nested line is blank, or one that starts with an empty
				// run above its indented children, already supplies the visible blank line
				if (HasBlankText(LastLine(oe)) || HasBlankText(next))
				{
					continue;
				}

				oe.AddAfterSelf(NewBlankLine());
				modified = true;
			}

			return modified;
		}


		/// <summary>
		/// Adjusts the blank lines that follow every non-empty heading regardless of the
		/// between-paragraphs mode: ensures exactly one blank line when headingBlank is set,
		/// otherwise removes them since the heading style is visual distinction enough. Runs
		/// after blank lines are collapsed or removed so the result is not undone by that pass.
		/// </summary>
		private bool AdjustHeadingBlanks(List<XElement> scope)
		{
			var modified = false;

			foreach (var oe in scope)
			{
				if (oe.Parent is null || oe.Name != ns + "OE" || IsBlankLine(oe) || !IsHeading(oe))
				{
					continue;
				}

				// the lines following a heading are either the paragraphs nested beneath it
				// or its next sibling paragraphs
				var children = oe.Elements(ns + "OEChildren").FirstOrDefault();
				var nested = children is not null && children.Elements(ns + "OE").Any();

				var following = nested
					? children.Elements(ns + "OE").ToList()
					: oe.ElementsAfterSelf(ns + "OE").ToList();

				var blanks = following.TakeWhile(IsBlankLine).ToList();

				if (headingBlank)
				{
					if (blanks.Count == 0)
					{
						if (following.Count > 0)
						{
							if (nested)
							{
								children.AddFirst(NewBlankLine());
							}
							else
							{
								oe.AddAfterSelf(NewBlankLine());
							}

							modified = true;
						}
					}
					else if (blanks.Count > 1)
					{
						// keep only the first
						blanks.Skip(1).ToList().ForEach(b => b.Remove());
						modified = true;
					}
				}
				else if (blanks.Count > 0)
				{
					blanks.ForEach(b => b.Remove());
					modified = true;

					if (nested && !children.HasElements)
					{
						children.Remove();
					}
				}
			}

			return modified;
		}

		private XElement NewBlankLine()
		{
			return new XElement(ns + "OE",
				new XElement(ns + "T", new XCData(string.Empty)));
		}


		/// <summary>
		/// Gets the last visible line of the paragraph, descending into its nested children.
		/// </summary>
		private XElement LastLine(XElement oe)
		{
			while (true)
			{
				var last = oe.Elements(ns + "OEChildren")
					.LastOrDefault()?.Elements(ns + "OE").LastOrDefault();

				if (last is null)
				{
					return oe;
				}

				oe = last;
			}
		}


		/// <summary>
		/// Determines if the paragraph's own line is blank, regardless of any nested children.
		/// </summary>
		private bool HasBlankText(XElement oe)
		{
			return oe.Elements(ns + "T").Any() &&
				oe.Elements().All(e => e.Name == ns + "T" || e.Name == ns + "OEChildren") &&
				oe.Elements(ns + "T").All(e => e.TextValue().Trim().Length == 0) &&
				!oe.IsMathML();
		}

		private bool IsBlankLine(XElement oe)
		{
			// a blank line is an OE containing only empty T runs; images, tables, lists,
			// tags, nested paragraphs, etc. make it a non-blank paragraph

			return oe.Name == ns + "OE" &&
				oe.HasElements &&
				oe.Elements().All(e => e.Name == ns + "T") &&
				oe.TextValue().Trim().Length == 0 &&
				!oe.IsMathML();
		}


		private bool IsPlainParagraph(XElement oe)
		{
			if (oe.Name != ns + "OE" || IsBlankLine(oe) || oe.IsMathML())
			{
				return false;
			}

			// must have text and not be a list item, tag, image, table, etc.
			if (!oe.Elements(ns + "T").Any() ||
				oe.Elements().Any(e => e.Name != ns + "T" && e.Name != ns + "OEChildren"))
			{
				return false;
			}

			if (oe.Descendants(ns + "Table").Any() ||
				oe.Descendants(ns + "List").Any() ||
				oe.Ancestors(ns + "Table").Any())
			{
				return false;
			}

			return !IsCodeBlock(oe);
		}


		private bool IsCodeBlock(XElement oe)
		{
			if (oe.GetAttributeValue("quickStyleIndex", out int index, -1))
			{
				var quick = quickStyles.Find(s => s.Index == index);
				if (quick?.Name?.IndexOf("code", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}

			var font = oe.Elements(ns + "T").FirstOrDefault()?.CollectStyleProperties(true);
			return font is not null &&
				font.TryGetValue("font-family", out var family) &&
				(family.IndexOf("Consolas", StringComparison.OrdinalIgnoreCase) >= 0 ||
				 family.IndexOf("Courier", StringComparison.OrdinalIgnoreCase) >= 0);
		}


		private bool IsHeading(XElement oe)
		{
			// standard heading quick style?
			var attr = oe.Attribute("quickStyleIndex");
			if (attr is not null &&
				int.TryParse(attr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) &&
				headingQuickStyles.Exists(s => s.Index == index))
			{
				return true;
			}

			// custom heading style?
			var style = new Style(oe.CollectStyleProperties(true));
			if (headingCustomStyles.Exists(s => s.Equals(style)))
			{
				return true;
			}

			// for non-empty paragraphs the style is carried on the text run
			if (!IsBlankLine(oe))
			{
				var run = oe.Elements(ns + "T").FirstOrDefault();
				if (run is not null)
				{
					style = new Style(run.CollectStyleProperties(true));
					return headingCustomStyles.Exists(s => s.Equals(style));
				}
			}

			return false;
		}


		private static void ClearHeadingStyle(XElement element)
		{
			// reset an empty paragraph to a normal, unstyled blank line

			element.Attribute("quickStyleIndex")?.Remove();
			element.Attribute("style")?.Remove();
			element.Attribute("spaceBefore")?.Remove();
			element.Attribute("spaceAfter")?.Remove();
			element.Attribute("spaceBetween")?.Remove();

			foreach (var run in element.Elements().Where(e => e.Name.LocalName == "T"))
			{
				run.Attribute("style")?.Remove();

				// the run is known to be empty/whitespace, so drop any styled span wrapper
				var cdata = run.Nodes().OfType<XCData>().FirstOrDefault();
				if (cdata is not null)
				{
					cdata.Value = string.Empty;
				}
			}
		}


		public bool IndentEmptyLines()
		{
			/* Indent outdented empty lines so "section" or related paragraphs can be collapsed
			 * together under a shared heading. The pattern is as follows, where the empty T is
			 * left outdented from para2 but should be indented to the same level:
			 *
			 * <OE>
			 *   <OEChildren>...</>
			 * </OE>
			 * <OE>
			 *   <T> -empty-and-outdented- </T>
			 *   <OEChildren>
			 *     para2
			 *   </OEChildren>
			 * </OE>
			 *
			 * This needs to be "flattened" to:
			 *
			 * <OE>
			 *   <OEChildren>...</>
			 *   <OEChildren><OE><T> -empty- </T></OE></>
			 *   <OEChildren> para2 </>
			 * </OE>
			 */

			var elements = runs
				.Where(e => e.PreviousNode == null && e.TextValue().Trim().Length == 0)
				.ToList();

			if (!elements.Any())
			{
				return false;
			}

			var modified = false;

			foreach (var element in elements)
			{
				if (element.NextNode is XElement next && next.Name.LocalName == "OEChildren")
				{
					var prev = element.Parent.PreviousNode as XElement;
					if (prev?.Name.LocalName == "OE")
					{
						if (mode != BlankLineMode.RemoveAll)
						{
							// move empty line to its own paragraph
							prev.Add(new XElement(ns + "OEChildren",
								new XElement(ns + "OE", element)
								));
						}
						// else when removing all blank lines, drop the empty line altogether
						// and just let its indented content join the previous paragraph

						// move remaining siblings to previous container
						prev.Add(element.NodesAfterSelf());

						// remove the offending paragraph
						element.Parent.Remove();

						modified = true;
					}
				}
			}

			return modified;
		}
	}
}
