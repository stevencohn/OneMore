//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Diagnostics;
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
		private XNamespace ns;


		public JoinParagraphCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out ns);


			System.Diagnostics.Debugger.Launch();


			var anchor = FindAnchor(page);
			if (anchor == null)
			{
				UIHelper.ShowInfo(Resx.JoinParagraphCommand_Select);
				return;
			}

			var container = FindScopedContainer(anchor);

			// find all selected T runs
			var runs = container.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			Debug.Assert(anchor == runs[0], "anchor does not match first selected T run");

			// join...
			Join(runs);

			// unselect all, including any beyond the scoped container
			page.Root.DescendantNodes().OfType<XAttribute>()
				.Where(a => a.Name.LocalName == "selected")
				.Remove();

			// clean up any empty OEs left over
			page.Root.Descendants(ns + "OE")
				.Where(e => !e.HasElements)
				.Remove();

			// insert caret position
			var caret = new XElement(ns + "T", new XCData(string.Empty));
			caret.SetAttributeValue("selected", "all");
			anchor.AddBeforeSelf(caret);

			logger.WriteLine(page.Root);
			await one.Update(page);
		}


		// anchor is first selected T run, regardless of content
		private XElement FindAnchor(Page page)
		{
			return page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.FirstOrDefault(e => 
					e.Attributes().Any(a => a.Name.LocalName == "selected" && a.Value == "all"));
		}


		// closest containing ancestor that is an Outline or Cell (Page is a catch-all)
		private XElement FindScopedContainer(XElement element)
		{
			var container = element.Parent;

			while (container.Name.LocalName != "Outline"
				&& container.Name.LocalName != "Cell"
				&& container.Name.LocalName != "Page")
			{
				container = container.Parent;
			}

			return container;
		}


		private void Join(List<XElement> runs)
		{
			// parent OE to which everything is appended
			var parent = runs[0].Parent;
			runs[0].Attributes().Where(a => a.Name == "selected").Remove();

			/*
			// if text is in the middle of a soft-break block then need to split the block
			// into two so the text can be spliced, maintaining its relative position
			if (runs[runs.Count - 1].NextNode != null)
			{
				var nextNodes = runs[runs.Count - 1].NodesAfterSelf().ToList();
				nextNodes.Remove();

				firstParent.AddAfterSelf(new XElement(ns + "OE",
					firstParent.Attributes(),
					nextNodes
					));
			}
			*/

			// let OneNote combine and optimize them so we don't have to...

			runs.Skip(1).ForEach(run =>
			{
				run.Parent.GetAttributeValue("style", out var pstyle);
				if (pstyle != null)
				{
					// inhert style from OE
					run.GetAttributeValue("style", out var style);
					run.SetAttributeValue("style", style == null ? pstyle : $"{pstyle};{style}");
				}

				// remove run from current parent
				run.Remove();

				// add run into new OE
				run.Attributes().Where(a => a.Name == "selected").Remove();

				var cdata = run.GetCData();
				cdata.Value = cdata.Value == string.Empty
					? " "
					: Regex.Replace(Regex.Replace(cdata.Value, @"<br>\n([^$])", " $1"), @"\s{2,}", " ");

				if (cdata.Value.EndsWith("<br>\n"))
				{
					// double it up
					cdata.Value = $"{cdata.Value}<br>\n";
				}

				if (cdata.Value.Length > 0 && !cdata.EndsWithWhitespace())
				{
					cdata.Value = $"{cdata.Value} ";
				}

				parent.Add(run);
			});
		}
	}
}
