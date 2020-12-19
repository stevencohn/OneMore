//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class SearchAndReplaceCommand : Command
	{

		/*
		 * KNOWN ISSUE
		 * . When replacing content in a line that begins with non-breaking whitespace
		 *   that whitespace will be removed; need to figure out how to preserve this
		 * 
		 */

		public SearchAndReplaceCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			string whatText;
			string withText;
			bool matchCase;

			using (var dialog = new SearchAndReplaceDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				whatText = dialog.WhatText;
				withText = dialog.WithText;
				matchCase = dialog.MatchCase;
			}

			using (var one = new OneNote(out var page, out var ns))
			{
				var cursor = page.GetTextCursor();
				if (cursor != null)
				{
					MergeRuns(cursor);
				}

				TransformSoftBreaks(page.Root, ns);

				IEnumerable<XElement> elements;
				if (cursor != null)
				{
					elements = page.Root.Elements(ns + "Outline")
						.Descendants(ns + "T");
				}
				else
				{
					elements = page.Root.Elements(ns + "Outline")
						.Descendants(ns + "T")
						.Where(e => e.Attribute("selected")?.Value == "all");
				}

				if (elements.Any())
				{
					int count = 0;
					var editor = new SearchAndReplaceEditor(ns, whatText, withText, matchCase);

					foreach (var element in elements)
					{
						count += editor.SearchAndReplace(element);
					}

					if (count > 0)
					{
						one.Update(page);
					}
				}
			}
		}


		// special handling to expand soft line breaks (Shift + Enter) into
		// hard breaks, splitting the line into multiple ines...
		private void TransformSoftBreaks(XElement root, XNamespace ns)
		{
			// get distinct OE elements that contain soft-breaks in one or more T runs
			var lines = root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Where(t => t.GetCData().Value.Contains("<br>"))
				.Select(e => e.Parent)
				.Distinct()
				.ToList();

			var regex = new Regex(@"<br>\n?");

			foreach (var line in lines)
			{
				var runs = line.Elements(ns + "T").ToList();
				for (var r = runs.Count - 1; r >= 0; r--)
				{
					var run = runs[r];

					var cdata = run.GetCData();
					if (cdata.Value.Contains("<br>"))
					{
						var text = cdata.Value;
						var parts = regex.Split(text);

						// update current cdata with first line
						cdata.Value = parts[0];

						// collect subsequent lines from soft-breaks
						var elements = new List<XElement>();
						for (int i = 1; i < parts.Length; i++)
						{
							elements.Add(new XElement(ns + "OE",
								run.Parent.Attributes(),
								new XElement(ns + "T", new XCData(parts[i])
								)));
						}

						run.Parent.AddAfterSelf(elements);
					}
				}
			}
		}


		// remove the empty CDATA[] cursor, combining the previous and next runs into one
		private void MergeRuns(XElement cursor)
		{
			if (cursor.PreviousNode is XElement prev)
			{
				if (cursor.NextNode is XElement next)
				{
					var cprev = prev.GetCData();
					var cnext = next.GetCData();
					cprev.Value = $"{cprev.Value}{cnext.Value}";
					next.Remove();
				}
			}

			cursor.Remove();
		}
	}
}
