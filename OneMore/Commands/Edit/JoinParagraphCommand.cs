//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Join multiple lines of text into a single running paragraph, removing line breaks.
	/// </summary>
	internal class JoinParagraphCommand : Command
	{
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


		private bool IsBlockMember(XElement oe, (string quickStyle, string style) fingerprint)
		{
			return IsPlainParagraph(oe) && !IsBlank(oe) &&
				GetStyleFingerprint(oe).Equals(fingerprint);
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


		private static (string quickStyle, string style) GetStyleFingerprint(XElement oe)
		{
			return ((string)oe.Attribute("quickStyleIndex"), (string)oe.Attribute("style"));
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

			if (first != caret)
			{
				Defrag(first, runs, 0);
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

				Defrag(run, runs, i);

				// collate all runs into first run's parent
				if (run.Parent != parent)
				{
					run.Remove();
					parent.Add(run);
				}
			}
		}


		private void Defrag(XElement run, List<XElement> runs, int index)
		{
			// inhert style from OE
			run.Parent.GetAttributeValue("style", out var pstyle);
			if (pstyle != null)
			{
				run.GetAttributeValue("style", out var style);
				run.SetAttributeValue("style", style == null ? pstyle : $"{pstyle};{style}");
			}

			// deselect
			run.Attributes().Where(a => a.Name == "selected").Remove();

			var cdata = run.GetCData();

			// collapse soft-breaks; don't trim the result -- a run being reassembled
			// within the same paragraph (soft breaks, or a cursor split mid-sentence)
			// may carry a real, meaningful leading/trailing space that must survive
			var text = cdata.Value.Replace("<br>\n", " ");

			if ((index < runs.Count - 1) &&
				(run.Parent != runs[index + 1].Parent) &&
				!text.EndsWithWhitespace())
			{
				text = $"{text} ";
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
