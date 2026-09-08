//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Converts headings (h1..h6) back into a single nested bulleted list, rebuilding
	/// their h1/h2/h3... hierarchy as physical OEChildren nesting. The reverse of
	/// ConvertOutlineCommand.
	/// </summary>
	internal class ConvertHeadingsCommand : Command
	{
		private const string QuickStyleAttribute = "quickStyleIndex";
		private const string StyleAttribute = "style";

		// same OneNote bullet glyph cycle used by ConvertOutlineCommand.RenumberBullets
		private static readonly int[] BulletCycle = { 2, 3, 13, 14, 9, 7, 15, 26, 8 };

		private XNamespace ns;


		public ConvertHeadingsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote(out var page, out ns);
			if (!page.IsValid)
			{
				return;
			}

			var eligible = GetEligibleOEs(page);

			var headings = page.GetHeadings(one, secondary: false);

			var targets = eligible is null
				? headings
				: headings.Where(h => eligible.Contains(h.Root)).ToList();

			if (!targets.Any())
			{
				ShowInfo(Resx.ConvertHeadingsCommand_NoHeadings);
				return;
			}

			var headingOEs = new HashSet<XElement>(targets.Select(h => h.Root));

			IndentBodyContent(headings, headingOEs);

			var roots = NestHeadings(targets);

			foreach (var root in roots)
			{
				ConvertToBullets(page, root, 1, headingOEs);
			}

			// the forward conversion strips "indent" from a container once it holds no
			// more list items (see ConvertOutlineCommand); restore it now that this
			// container holds list items again, matching what OneNote itself sets on a
			// genuine top-level bulleted list
			foreach (var root in roots)
			{
				var container = root.Parent;
				if (container is not null && container.Attribute("indent") is null)
				{
					container.SetAttributeValue("indent", "2");
				}
			}

			// UpdatePageContent does not reliably honor structural changes against
			// content it still has cached by objectID (see the same workaround in
			// ConvertOutlineCommand and CaptionAttachmentsCommand/AddCaptionCommand)
			foreach (var root in roots)
			{
				root.DescendantsAndSelf().Attributes("objectID").Remove();
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
		/// Moves each target heading's trailing flat-sibling content, up to the next
		/// heading of any kind (eligible or not, so it stays a correct boundary), into
		/// the heading's own OEChildren - same shape as OutlineCommand.IndentContent.
		/// </summary>
		private void IndentBodyContent(List<Heading> headings, HashSet<XElement> headingOEs)
		{
			for (var i = 0; i < headings.Count; i++)
			{
				var element = headings[i].Root;
				if (!headingOEs.Contains(element))
				{
					continue;
				}

				var next = i < headings.Count - 1 ? headings[i + 1].Root : null;

				var siblings = element.ElementsAfterSelf()?.ToList();
				if (siblings is null || siblings.Count == 0)
				{
					continue;
				}

				var kids = element.Element(ns + "OEChildren");
				var isNew = kids is null;
				kids ??= new XElement(ns + "OEChildren");

				var any = false;
				foreach (var sibling in siblings)
				{
					if (sibling == next)
					{
						break;
					}

					sibling.Remove();
					kids.Add(sibling);
					any = true;
				}

				if (any && isNew)
				{
					element.Add(kids);
				}
			}
		}


		/// <summary>
		/// Physically nests deeper-level target headings under their nearest shallower
		/// target ancestor, returning the roots of the resulting forest.
		/// </summary>
		private List<XElement> NestHeadings(List<Heading> targets)
		{
			var roots = new List<XElement>();
			var i = 0;

			while (i < targets.Count)
			{
				roots.Add(targets[i].Root);
				i = Nest(targets, i, targets[i].Level);
			}

			return roots;
		}


		private int Nest(List<Heading> targets, int index, int level)
		{
			var oe = targets[index].Root;
			index++;

			while (index < targets.Count && targets[index].Level > level)
			{
				var childLevel = targets[index].Level;
				var childOe = targets[index].Root;

				childOe.Remove();

				var kids = oe.Element(ns + "OEChildren");
				if (kids is null)
				{
					kids = new XElement(ns + "OEChildren");
					oe.Add(kids);
				}

				kids.Add(childOe);

				index = Nest(targets, index, childLevel);
			}

			return index;
		}


		/// <summary>
		/// Walks a converted heading's (now nested) tree, turning every OE into a
		/// bulleted list item at the appropriate relative depth. Former heading OEs also
		/// have their heading quickStyleIndex/style reverted to Normal; body-content OEs
		/// pulled in by IndentBodyContent keep whatever paragraph style they already had.
		/// </summary>
		private void ConvertToBullets(Page page, XElement oe, int relativeLevel, HashSet<XElement> headingOEs)
		{
			if (headingOEs.Contains(oe))
			{
				var normal = page.GetQuickStyle(StandardStyles.Normal);
				oe.SetAttributeValue(QuickStyleAttribute, normal.Index.ToString());
				oe.Attribute(StyleAttribute)?.Remove();
			}

			var bulletValue = BulletCycle[System.Math.Min(relativeLevel, BulletCycle.Length) - 1];
			oe.Element(ns + "List")?.Remove();
			AddListMarker(oe, new XElement(ns + "List",
				new XElement(ns + "Bullet", new XAttribute("bullet", bulletValue.ToString()))));

			var kids = oe.Element(ns + "OEChildren");
			if (kids is not null)
			{
				foreach (var child in kids.Elements(ns + "OE").ToList())
				{
					ConvertToBullets(page, child, relativeLevel + 1, headingOEs);
				}
			}
		}


		/// <summary>
		/// Inserts a List element in schema order - after any Tag/Meta children, before
		/// the OE's content (T, Image, Table, ...).
		/// </summary>
		private void AddListMarker(XElement oe, XElement list)
		{
			var anchor = oe.Elements(ns + "Meta").LastOrDefault()
				?? oe.Elements(ns + "Tag").LastOrDefault();

			if (anchor is not null)
			{
				anchor.AddAfterSelf(list);
			}
			else
			{
				oe.AddFirst(list);
			}
		}
	}
}
