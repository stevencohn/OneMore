//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


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
			using (var one = new OneNote(out var page, out ns))
			{
				var content = CollectContent(page, out var firstParent);
				if (content != null)
				{
					if (firstParent.HasElements)
					{
						// selected text was a subset of runs under an OE
						firstParent.AddAfterSelf(content);
					}
					else
					{
						// selected text was all of an OE
						firstParent.Add(content.Elements());
					}

					//logger.WriteLine(page.Root);
					await one.Update(page);
				}
			}
		}


		private XElement CollectContent(Page page, out XElement firstParent)
		{
			firstParent = null;

			var runs = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"))
				.ToList();

			if (runs.Count == 0)
			{
				return null;
			}

			// content will eventually be added after the first parent
			firstParent = runs[0].Parent;

			var content = new XElement(ns + "OE",
				runs[0].Parent.Attributes().Where(a => a.Name != "selected")
				);

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

			// collect the content by collating all T runs into a single OE,
			// let OneNote combine and optimize them so we don't have to...

			foreach (var run in runs)
			{
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

				content.Add(run);
			}

			return content;
		}
	}
}
