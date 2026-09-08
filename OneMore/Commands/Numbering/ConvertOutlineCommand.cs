//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Converts the top levels of a bulleted or numbered list into headings, leaving
	/// deeper levels as sub-lists nested beneath the new headings.
	/// </summary>
	internal class ConvertOutlineCommand : Command
	{
		private const string QuickStyleAttribute = "quickStyleIndex";
		private const string StyleAttribute = "style";

		private XNamespace ns;


		public ConvertOutlineCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			using var dialog = new ConvertOutlineDialog();
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			await using var one = new OneNote(out var page, out ns);
			if (!page.IsValid)
			{
				return;
			}

			var eligible = GetEligibleOEs(page);

			var runs = new List<List<XElement>>();
			foreach (var outline in page.Root.Elements(ns + "Outline"))
			{
				var kids = outline.Element(ns + "OEChildren");
				if (kids is not null)
				{
					FindRuns(kids, eligible, runs);
				}
			}

			if (!runs.Any())
			{
				ShowInfo(Resx.ConvertOutlineCommand_NoList);
				return;
			}

			var theme = dialog.UseCustomTheme ? new ThemeProvider().Theme : null;

			foreach (var run in runs)
			{
				ConvertRun(page, run, dialog.Depth, theme);
			}

			// UpdatePageContent does not reliably honor removal of existing tracked
			// content just because it's absent from the submitted XML (see the same
			// workaround in CaptionAttachmentsCommand/AddCaptionCommand); dropping the
			// objectID of each affected list's container and everything beneath it
			// forces OneNote to treat that whole subtree as new content rather than a
			// partial patch of what it already has cached as "a list"
			foreach (var run in runs)
			{
				run[0].Parent.DescendantsAndSelf().Attributes("objectID").Remove();
			}

			await one.Update(page);
		}


		/// <summary>
		/// Determines the set of OE elements eligible for conversion based on the current
		/// selection. Returns null if there is no active selection, meaning the entire
		/// page is eligible; otherwise, only the touched OEs and their descendants are.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell",
			"S1168:Empty arrays and collections should be returned instead of null",
			Justification = "null means unrestricted (whole page eligible), distinct from an empty set")]
		private HashSet<XElement> GetEligibleOEs(Page page)
		{
			var range = new Models.SelectionRange(page);
			var selections = range.GetSelections();

			if (range.Scope is SelectionScope.TextCursor or SelectionScope.None)
			{
				return null;
			}

			var touched = selections
				.Select(t => t.Parent)
				.Where(e => e?.Name.LocalName == "OE")
				.ToList();

			var eligible = new HashSet<XElement>(touched);
			foreach (var oe in touched)
			{
				foreach (var descendant in oe.Descendants(ns + "OE"))
				{
					eligible.Add(descendant);
				}
			}

			return eligible;
		}


		/// <summary>
		/// Recursively scans a container of one:OE siblings, splitting bulleted/numbered
		/// list items into independent runs. A run ends at any non-list sibling or any
		/// sibling outside the eligible scope; that separator's own children (and any
		/// table cells it contains) are searched for further, independent runs.
		/// </summary>
		private void FindRuns(XElement container, HashSet<XElement> eligible, List<List<XElement>> runs)
		{
			List<XElement> current = null;

			foreach (var oe in container.Elements(ns + "OE"))
			{
				var isListItem = oe.Element(ns + "List") is not null;
				var inScope = eligible is null || eligible.Contains(oe);

				if (isListItem && inScope)
				{
					(current ??= new List<XElement>()).Add(oe);
					continue;
				}

				if (current is not null)
				{
					runs.Add(current);
					current = null;
				}

				var kids = oe.Element(ns + "OEChildren");
				if (kids is not null)
				{
					FindRuns(kids, eligible, runs);
				}

				foreach (var cell in oe.Elements(ns + "Table").Elements(ns + "Row").Elements(ns + "Cell"))
				{
					var cellKids = cell.Element(ns + "OEChildren");
					if (cellKids is not null)
					{
						FindRuns(cellKids, eligible, runs);
					}
				}
			}

			if (current is not null)
			{
				runs.Add(current);
			}
		}


		/// <summary>
		/// Converts one independent run of top-level list items, promoting converted
		/// items (and their converted descendants) to flat siblings the way OneNote
		/// natively represents multiple heading levels, while leaving deeper, unconverted
		/// items nested in place beneath their nearest surviving heading.
		/// </summary>
		private void ConvertRun(Page page, List<XElement> topLevel, int depth, Theme theme)
		{
			if (!topLevel.Any())
			{
				return;
			}

			var container = topLevel[0].Parent;

			foreach (var oe in topLevel)
			{
				FlattenConvert(page, oe, 1, depth, theme);
			}

			// a container's "indent" attribute tells OneNote this block is still an
			// indented list, causing it to backfill bullets onto List-less children
			// that remain inside; once none of its direct children are list items
			// any more, that attribute is stale and must be dropped too
			if (container.Attribute("indent") is not null &&
				!container.Elements(ns + "OE").Any(e => e.Element(ns + "List") is not null))
			{
				container.Attribute("indent").Remove();
			}
		}


		/// <summary>
		/// Converts 'oe' to a heading if its level qualifies, then promotes any of its
		/// descendants that also qualify for conversion to become flat siblings of 'oe'
		/// (and of each other) in document order, matching how OneNote natively represents
		/// multiple heading levels as flat, quickStyleIndex-driven siblings rather than
		/// physically nested paragraphs. Content beyond the chosen depth is left nested,
		/// in place, beneath its nearest surviving heading ancestor.
		/// </summary>
		/// <returns>
		/// The element now positioned last among 'oe' and its promoted descendants, in
		/// oe's (original) parent container, so a caller can chain further insertions
		/// after it.
		/// </returns>
		private XElement FlattenConvert(Page page, XElement oe, int level, int depth, Theme theme)
		{
			if (level <= depth && oe.Element(ns + "List") is not null)
			{
				ConvertToHeading(page, oe, level, theme);
			}

			var kids = oe.Element(ns + "OEChildren");
			if (kids is null)
			{
				return oe;
			}

			var lastSibling = oe;
			var children = kids.Elements(ns + "OE").ToList();

			foreach (var child in children)
			{
				var childLevel = level + 1;
				var childConverts = childLevel <= depth && child.Element(ns + "List") is not null;

				if (childConverts)
				{
					child.Remove();
					lastSibling.AddAfterSelf(child);
					lastSibling = FlattenConvert(page, child, childLevel, depth, theme);
				}
				else
				{
					// this is the top of a sub-list left behind under a new heading;
					// restart its bullet numbering from 1 rather than keeping the
					// glyph it had at its old, deeper nesting level
					RenumberBullets(child);
				}
			}

			if (!kids.Elements(ns + "OE").Any())
			{
				kids.Remove();
			}

			return lastSibling;
		}


		/// <summary>
		/// OneNote's own bullet glyph cycle, indexed by nesting depth (1-based); depths
		/// beyond the length of this table repeat its last entry. These are glyph IDs,
		/// not depth counts, so they can't be arithmetically shifted - only looked up.
		/// </summary>
		private static readonly int[] BulletCycle = { 2, 3, 13, 14, 9, 7, 15, 26, 8 };


		/// <summary>
		/// Re-glyphs 'top' and its descendants against OneNote's bullet cycle as if 'top'
		/// were now nesting depth 1, so a sub-list left behind under a new heading looks
		/// exactly like a freshly-started list rather than carrying glyphs from its old,
		/// deeper position.
		/// </summary>
		private void RenumberBullets(XElement top)
		{
			void Walk(XElement oe, int relativeLevel)
			{
				var bullet = oe.Element(ns + "List")?.Element(ns + "Bullet");
				if (bullet is not null)
				{
					var index = System.Math.Min(relativeLevel, BulletCycle.Length) - 1;
					bullet.SetAttributeValue("bullet", BulletCycle[index].ToString());
				}

				var kids = oe.Element(ns + "OEChildren");
				if (kids is not null)
				{
					foreach (var child in kids.Elements(ns + "OE"))
					{
						Walk(child, relativeLevel + 1);
					}
				}
			}

			Walk(top, 1);
		}


		private void ConvertToHeading(Page page, XElement oe, int level, Theme theme)
		{
			// no longer a list item
			oe.Element(ns + "List")?.Remove();

			var standard = (StandardStyles)(level - 1);
			var builtin = page.GetQuickStyle(standard);
			oe.SetAttributeValue(QuickStyleAttribute, builtin.Index.ToString());

			if (theme is null)
			{
				// let the QuickStyleDef's own default rendering apply; strip any
				// leftover paragraph-level CSS the bullet item might have carried
				oe.Attribute(StyleAttribute)?.Remove();
				return;
			}

			var custom = FindThemeHeading(theme, level) ?? builtin;
			oe.SetAttributeValue(StyleAttribute, custom.ToCss());
		}


		private static Style FindThemeHeading(Theme theme, int level)
		{
			return theme.GetStyles()
				.Where(s => s.StyleType == StyleType.Heading)
				.FirstOrDefault(s => Regex.IsMatch(s.Name, $@"^[Hh](?:ead)?.*?{level}$"));
		}
	}
}
