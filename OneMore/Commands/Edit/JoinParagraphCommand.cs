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
			using var one = new OneNote(out var page, out ns);

			var anchor = FindAnchor(page);
			if (anchor == null)
			{
				UIHelper.ShowInfo(Resx.JoinParagraphCommand_Select);
				return;
			}

			// remember parent so we can add a new caret later
			var parent = anchor.Parent;

			var container = FindScopedContainer(anchor);
			var runs = CollectRuns(container, out var caret);

			//Debug.Assert(anchor == runs[0], "anchor does not match first selected T run");

			Join(runs, caret);

			// clean up any left-over elements; must be in this order:
			Cleanup(page);

			// insert caret position
			caret = new XElement(ns + "T", new XCData(string.Empty));
			caret.SetAttributeValue("selected", "all");
			parent.AddFirst(caret);

			await one.Update(page);
		}


		private XElement FindAnchor(Page page)
		{
			// anchor is first selected T run, regardless of content

			var anchor = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.FirstOrDefault(e =>
					e.Attributes().Any(a => a.Name.LocalName == "selected" && a.Value == "all"));

			// don't use the caret as an anchor because it's going to be removed later
			if (anchor?.GetCData().Value.Length == 0)
			{
				anchor = anchor.ElementsAfterSelf(ns + "T").FirstOrDefault()
					?? anchor.ElementsBeforeSelf(ns + "T").FirstOrDefault();
			}

			return anchor;
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
				var first = runs.First();
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
				var after = runs.Last().ElementsAfterSelf(ns + "T");
				if (after.Any())
				{
					runs.AddRange(after);
				}
			}

			return runs;
		}


		private void Join(List<XElement> runs, XElement caret)
		{
			var start = 0;
			var first = runs[start];

			// parent OE into which all runs are collated
			var parent = first.Parent;

			if (first == caret && runs.Count > 1)
			{
				first.Remove();
				start = 1;
				first = runs[start];
				parent = first.Parent;
			}

			Defrag(first, runs, start);

			if (runs.Count == start)
			{
				return;
			}

			// let OneNote combine and optimize so we don't have to...

			for (int i = start; i < runs.Count; i++)
			{
				var run = runs[i];
				if (run == caret)
				{
					run.Remove();
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

			// collapse soft-breaks
			var text = cdata.Value.Replace("<br>\n", " ").Trim();

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
			page.Root.DescendantNodes().OfType<XAttribute>()
				.Where(a => a.Name.LocalName == "selected")
				.Remove();
		}
	}
}
