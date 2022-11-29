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
	using System.Windows;
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

			var container = FindScopedContainer(anchor);
			var runs = CollectRuns(container);

			//Debug.Assert(anchor == runs[0], "anchor does not match first selected T run");

			Join(runs);

			// clean up any left-over elements; must be in this order:
			Cleanup(page);

			// insert caret position
			var caret = new XElement(ns + "T", new XCData(string.Empty));
			caret.SetAttributeValue("selected", "all");
			anchor.AddBeforeSelf(caret);

			logger.WriteLine(page.Root);
			await one.Update(page);
		}


		private XElement FindAnchor(Page page)
		{
			// anchor is first selected T run, regardless of content

			return page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.FirstOrDefault(e => 
					e.Attributes().Any(a => a.Name.LocalName == "selected" && a.Value == "all"));
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


		private List<XElement> CollectRuns(XElement container)
		{
			// collect all selected T runs plus immediate siblings in containing OEs so entire
			// paragraphs are joined, not just selected portions; this may not be precisely per
			// requirements but it is infinitely easier to do and far less cases to decide

			// find all selected T runs
			var runs = container.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			if (runs.Any())
			{
				// include sibling runs prior to first
				runs.InsertRange(0, runs.First().ElementsBeforeSelf(ns + "T"));

				if (runs.Count > 1)
				{
					// include sibling runs prior to first
					runs.AddRange(runs.Last().ElementsAfterSelf(ns + "T"));
				}
			}

			return runs;
		}


		private void Join(List<XElement> runs)
		{
			System.Diagnostics.Debugger.Launch();

			var first = runs.First();

			// parent OE to which everything is appended
			var parent = first.Parent;
			first.Attributes().Where(a => a.Name == "selected").Remove();

			if (runs.Count == 1)
			{
				var cdata = first.GetCData();
				// collapse soft-breaks
				cdata.Value = cdata.Value.Replace("<br>\n", " ").Trim();
				return;
			}

			// let OneNote combine and optimize so we don't have to...
			var prev = parent;

			foreach(var run in runs.Skip(1))
			{
				run.Parent.GetAttributeValue("style", out var pstyle);
				if (pstyle != null)
				{
					// inhert style from OE
					run.GetAttributeValue("style", out var style);
					run.SetAttributeValue("style", style == null ? pstyle : $"{pstyle};{style}");
				}

				run.Attributes().Where(a => a.Name == "selected").Remove();

				var cdata = run.GetCData();

				// collapse soft-breaks
				var text = cdata.Value.Replace("<br>\n", " ").Trim();

				if (run.NextNode == null || run.Parent != prev)
				{
					cdata.Value = $"{text} ";
				}
				else
				{
					cdata.Value = text;
				}

				// remove run from current parent
				prev = run.Parent;
				run.Remove();

				// add run into OE of first run
				parent.Add(run);
			}
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
