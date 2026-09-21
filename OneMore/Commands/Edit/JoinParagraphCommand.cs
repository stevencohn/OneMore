//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Join multiple lines of text into a single running paragraph, removing line breaks.
	/// </summary>
	internal class JoinParagraphCommand : Command
	{
		// OneNote commonly encodes preserved indentation as non-breaking-space
		// entities rather than plain ASCII spaces, so both must count as "space"
		// here (same convention as StringExtensions.StartsWithWhitespace)
		private const string Space = @"(?: |&#160;|&nbsp;)";
		private static readonly Regex SoftBreak = new Regex($@"<br>\n{Space}*", RegexOptions.Compiled);
		private static readonly Regex LeadingSpaces = new Regex($@"^{Space}+", RegexOptions.Compiled);

		private XNamespace ns;


		public JoinParagraphCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out ns);

			var cursorRun = FindSelectedRun(page);
			if (cursorRun == null)
			{
				ShowInfo(Resx.JoinParagraphCommand_Select);
				return;
			}

			XElement anchor;
			List<XElement> runs;
			XElement caret;
			bool preserveCaret;

			if (cursorRun.GetCData().Value.Length == 0)
			{
				// bare cursor, no drag-selected range; find the block of adjacent
				// same-style paragraphs (hard breaks) surrounding the cursor, along
				// with any soft-break siblings already in its own OE
				var block = FindBlock(cursorRun.Parent);
				runs = block.SelectMany(oe => oe.Elements(ns + "T")).ToList();

				if (runs.Count <= 1)
				{
					ShowInfo(Resx.JoinParagraphCommand_Select);
					return;
				}

				anchor = block[0].Elements(ns + "T").First();
				caret = cursorRun;
				preserveCaret = true;
			}
			else
			{
				// real drag-selected range, possibly already spanning multiple paragraphs
				anchor = cursorRun;
				var container = FindScopedContainer(anchor);
				runs = CollectRuns(container, out caret);
				preserveCaret = false;
			}

			// remember parent so we can add a new caret later
			var parent = anchor.Parent;

			Join(runs, caret);

			// clean up any left-over elements; must be in this order:
			Cleanup(page);

			if (preserveCaret)
			{
				// Join already relocated the caret to its original relative position
				// within the joined text; Cleanup's Deselect() strips its "selected"
				// marker along with everyone else's, so restore it here to give the
				// user continuity of cursor position after the join
				caret.SetAttributeValue("selected", "all");
			}
			else
			{
				// no single cursor position to restore; place a fresh caret at the
				// start of the joined paragraph, as before
				var newCaret = new XElement(ns + "T", new XCData(string.Empty));
				newCaret.SetAttributeValue("selected", "all");
				parent.AddFirst(newCaret);
			}

			await one.Update(page);
		}


		private XElement FindSelectedRun(Page page)
		{
			// first selected T run, regardless of content; this is either a real
			// selected range or the zero-width caret OneNote reports when nothing
			// is actually selected

			return page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.FirstOrDefault(e => e.Attribute("selected")?.Value == "all");
		}


		private List<XElement> FindBlock(XElement cursorOE)
		{
			// walk outward from the cursor's OE, collecting adjacent sibling OEs
			// that are plain, non-blank paragraphs sharing the same paragraph
			// style; stops at an empty paragraph, a style change (e.g. a
			// heading), or any non-paragraph content (table, list, and so on)

			var block = new List<XElement> { cursorOE };

			if (!IsPlainParagraph(cursorOE) || IsBlank(cursorOE))
			{
				return block;
			}

			var fingerprint = GetStyleFingerprint(cursorOE);

			var node = cursorOE.PreviousNode;
			while (node is XElement sibling && IsBlockMember(sibling, fingerprint))
			{
				block.Insert(0, sibling);
				node = sibling.PreviousNode;
			}

			node = cursorOE.NextNode;
			while (node is XElement sibling && IsBlockMember(sibling, fingerprint))
			{
				block.Add(sibling);
				node = sibling.NextNode;
			}

			return block;
		}


		private bool IsBlockMember(XElement oe, string fingerprint)
		{
			return IsPlainParagraph(oe) && !IsBlank(oe) &&
				GetStyleFingerprint(oe) == fingerprint;
		}


		private static bool IsPlainParagraph(XElement oe)
		{
			// an OE whose only children are T runs; excludes lists, tables,
			// tagged/task paragraphs, and paragraphs with nested sub-content
			return oe.Name.LocalName == "OE" &&
				oe.Elements().Any() &&
				oe.Elements().All(e => e.Name.LocalName == "T");
		}


		private static bool IsBlank(XElement oe)
		{
			return !oe.Elements(oe.Name.Namespace + "T")
				.Any(t => t.GetCData().Value.Trim().Length > 0);
		}


		private static string GetStyleFingerprint(XElement oe)
		{
			// quickStyleIndex is the real "paragraph type" signal (heading vs.
			// normal, etc.); the free-form "style" attribute captures incidental
			// per-paragraph formatting (e.g. syntax-highlighting color) that can
			// legitimately differ between lines of the same kind and must not
			// block the join
			return (string)oe.Attribute("quickStyleIndex");
		}


		private XElement FindScopedContainer(XElement element)
		{
			// closest containing ancestor that is an Outline or Cell (Page is a catch-all)

			var container = element.Parent;

			while (container.Name.LocalName != "Outline"
				&& container.Name.LocalName != "Cell"
				&& container.Name.LocalName != "Page")
			{
				container = container.Parent;
			}

			return container;
		}


		private List<XElement> CollectRuns(XElement container, out XElement caret)
		{
			// collect all selected T runs plus immediate siblings in containing OEs so entire
			// paragraphs are joined, not just selected portions; this may not be precisely per
			// requirements but it is infinitely easier to do and far less cases to decide

			// find all selected T runs
			var runs = container.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			caret = null;

			if (runs.Any())
			{
				var first = runs[0];
				if (runs.Count == 1 && first.GetCData().Value == string.Empty)
				{
					caret = first;
				}

				// include siblings before first
				var before = first.ElementsBeforeSelf(ns + "T");
				if (before.Any())
				{
					runs.InsertRange(0, before);
				}

				// include siblings after last
				var after = runs[runs.Count - 1].ElementsAfterSelf(ns + "T");
				if (after.Any())
				{
					runs.AddRange(after);
				}
			}

			return runs;
		}


		private void Join(List<XElement> runs, XElement caret)
		{
			var first = runs[0];

			// parent OE into which all runs are collated
			var parent = first.Parent;

			// snapshot each run's original OE before anything moves; runs get
			// relocated into "parent" as we go, so live .Parent lookups would
			// give the wrong answer for a run whose predecessor already moved
			var originalParents = runs.Select(r => r.Parent).ToList();

			if (first != caret)
			{
				Defrag(first, runs, 0, originalParents, parent);
			}

			// let OneNote combine and optimize so we don't have to...

			for (int i = 0; i < runs.Count; i++)
			{
				var run = runs[i];
				if (run == caret)
				{
					// relocate rather than remove: this preserves the run's own
					// "selected" state and its position relative to the other
					// joined text, so the caller can restore the user's cursor
					// to where it was after the join
					if (run.Parent != parent)
					{
						run.Remove();
						parent.Add(run);
					}
					continue;
				}

				Defrag(run, runs, i, originalParents, parent);

				// collate all runs into first run's parent
				if (run.Parent != parent)
				{
					run.Remove();
					parent.Add(run);
				}
			}
		}


		private void Defrag(
			XElement run, List<XElement> runs, int index, List<XElement> originalParents, XElement survivor)
		{
			// deselect
			run.Attributes().Where(a => a.Name == "selected").Remove();

			var cdata = run.GetCData();

			// collapse soft-breaks; a soft-break line other than the first may carry
			// meaningful leading indentation (e.g. a SQL sample) -- keep only a
			// single separating space rather than the original indentation
			var text = SoftBreak.Replace(cdata.Value, " ");

			if (index > 0 && originalParents[index] != originalParents[index - 1])
			{
				// this run is the first piece of a new hard-break paragraph (a
				// "line" other than the first); its own leading indentation is
				// redundant with the single separating space added below, so
				// drop it rather than let it pile up in the joined text
				text = LeadingSpaces.Replace(text, string.Empty);
			}

			if ((index < runs.Count - 1) &&
				(originalParents[index] != originalParents[index + 1]) &&
				!text.EndsWithWhitespace())
			{
				text = $"{text} ";
			}

			// this run's own paragraph may have carried a different style than
			// the paragraph it's being merged into (e.g. two lines of the same
			// quickStyle but with different syntax-highlighting colors); rather
			// than let it silently inherit the survivor's style, wrap it in a
			// span carrying its own original style, the same way OneNote itself
			// expresses mixed formatting within a single run of text
			var oe = originalParents[index];
			if (oe != survivor)
			{
				oe.GetAttributeValue("style", out var pstyle);
				survivor.GetAttributeValue("style", out var sstyle);

				if (!string.IsNullOrEmpty(pstyle) && pstyle != sstyle)
				{
					text = $"<span style=\"{pstyle}\">{text}</span>";
				}
			}

			cdata.Value = text;
		}


		private void Cleanup(Page page)
		{
			// must be removed in exactly this order - lower hierarhcy to upper hierarchy

			page.Root.Descendants(ns + "Bullet").Where(e => !e.HasElements).Remove();
			page.Root.Descendants(ns + "Number").Where(e => !e.HasElements).Remove();
			page.Root.Descendants(ns + "List").Where(e => !e.HasElements).Remove();
			page.Root.Descendants(ns + "OE").Where(e => !e.HasElements).Remove();
			page.Root.Descendants(ns + "OEChildren").Where(e => !e.HasElements).Remove();

			// unselect all, including any beyond the scoped container
			new PageEditor(page).Deselect();
		}
	}
}
