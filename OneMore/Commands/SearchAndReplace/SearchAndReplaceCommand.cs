//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
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

		public SearchAndReplaceCommand() : base()
		{
		}


		public void Execute()
		{
			try
			{
				SearchAndReplace();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(SearchAndReplaceCommand)}", exc);
			}
		}

		private void SearchAndReplace()
		{
			DialogResult result = DialogResult.None;
			string whatText;
			string withText;
			bool matchCase;

			using (var dialog = new Dialogs.SearchAndReplaceDialog())
			{
				result = dialog.ShowDialog(owner);

				whatText = dialog.WhatText;
				withText = dialog.WithText;
				matchCase = dialog.MatchCase;
			}

			if (result == DialogResult.OK)
			{
				using (var manager = new ApplicationManager())
				{
					var page = manager.CurrentPage();
					var ns = page.GetNamespaceOfPrefix("one");

					var elements = page.Elements(ns + "Outline").Descendants(ns + "T")
						.Select(e => e.Parent)
						.Distinct()
						.Cast<XElement>();

					if (elements.Any())
					{
						// if there is a selection range...
						var countRange = elements.Elements(ns + "T").Count(e =>
							e.Attribute("selected")?.Value == "all" &&
							e.FirstNode is XCData && (e.FirstNode as XCData).Value.Length > 0);

						if (countRange > 0)
						{
							// ...then further filter out only the selected range
							elements = elements.Elements(ns + "T")
								.Where(t => t.Attribute("selected")?.Value == "all")
								.Select(t => t.Parent);
						}
					}

					if (elements.Any())
					{
						int count = 0;
						var editor = new SearchAndReplaceEditor(ns, whatText, withText, matchCase);

						// use ToList to avoid null ref exception while updating IEnumerated collection
						var list = elements.ToList();
						for (var i = 0; i < list.Count(); i++)
						{
							count += editor.SearchAndReplace(list[i]);
						}

						manager.UpdatePageContent(page);

						//var msg = count == 1 ? "occurance was" : "occurances were";
						//MessageBox.Show($"{count} {msg} replaced", "Replaced",
						//	MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
		}
	}
}
