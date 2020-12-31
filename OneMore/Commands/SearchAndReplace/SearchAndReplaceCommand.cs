//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class SearchAndReplaceCommand : Command
	{

		public SearchAndReplaceCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			string whatText;
			string withText;
			bool matchCase;

			using (var one = new OneNote(out var page, out var ns))
			{
				var text = page.GetSelectedText();

				using (var dialog = new SearchAndReplaceDialog())
				{
					if (text.Length > 0)
					{
						dialog.WhatText = text;
					}

					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					whatText = dialog.WhatText;
					withText = dialog.WithText;
					matchCase = dialog.MatchCase;
				}

				// let user insert a newline char
				withText = withText.Replace("\\n", "\n");

				var cursor = page.GetTextCursor();
				if (cursor != null)
				{
					MergeRuns(cursor);
				}

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
					var editor = new SearchAndReplaceEditor(whatText, withText, matchCase);

					foreach (var element in elements)
					{
						count += editor.SearchAndReplace(element);
					}

					if (count > 0)
					{
						PatchEndingBreaks(page.Root, ns);

						logger.WriteLine($"found {count} matches");
						await one.Update(page);
					}
					else
					{
						logger.WriteLine("no matches found");
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


		private void PatchEndingBreaks(XElement root, XNamespace ns)
		{
			// if a <t> ends with <br>\n then OneNote will strip it but it typically
			// appends a &nbsp; after it to preserve the line break. We do the same
			// here to make sure we don't loose our line breaks

			var runs = root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Where(t => t.GetCData().Value.EndsWith("<br>\n"))
				.ToList();

			foreach (var run in runs)
			{
				var cdata = run.GetCData();
				cdata.Value = $"{cdata.Value}&nbsp;";
			}
		}
	}
}
