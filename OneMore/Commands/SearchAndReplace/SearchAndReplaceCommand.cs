//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
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
			bool useRegex;

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
					useRegex = dialog.UseRegex;
				}

				// let user insert a newline char
				withText = withText.Replace("\\n", "\n");

				var cursor = page.GetTextCursor();
				if (cursor != null && cursor.GetCData().Value == string.Empty)
				{
					// only merge if empty [], note cursor could be a hyperlink <a>..</a>
					MergeRuns(cursor);
				}

				IEnumerable<XElement> elements;
				if (cursor != null)
				{
					elements = page.Root.Elements(ns + "Outline")
						.Descendants(ns + "T")
						.ToList();
				}
				else
				{
					elements = page.Root.Elements(ns + "Outline")
						.Descendants(ns + "T")
						.Where(e => e.Attribute("selected")?.Value == "all")
						.ToList();
				}

				if (elements.Any())
				{
					int count = 0;
					var editor = new SearchAndReplaceEditor(whatText, withText, matchCase, useRegex);

					foreach (var element in elements)
					{
						count += editor.SearchAndReplace(element);
					}

					if (count > 0)
					{
						PatchEndingBreaks(page.Root, ns);

						logger.WriteLine($"found {count} matches");
						await one.Update(page);

						SaveSettings(whatText, withText, matchCase, useRegex);
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


		private void SaveSettings(string whatText, string withText, bool matchCase, bool useRegex)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("SearchReplace");

			// remember WhatText and options...

			var whats = settings.Get<XElement>("whats");
			if (whats == null)
			{
				whats = new XElement("whats");
				settings.Add("whats", whats);
			}

			// only add if not already present
			if (!whats.Elements().Any(e => e.Value == whatText))
			{
				if (whats.Elements().Count() > 8)
				{
					whats.Elements().Last().Remove();
				}

				whats.AddFirst(new XElement("whatText",
					new XAttribute("matchCase", matchCase.ToString()),
					new XAttribute("useRegex", useRegex.ToString()),
					HttpUtility.HtmlEncode(whatText)
					));
			}

			// remember WithText...

			var withs = settings.Get<XElement>("withs");
			if (withs == null)
			{
				withs = new XElement("withs");
				settings.Add("withs", withs);
			}

			// only add if not already present
			if (!withs.Elements().Any(e => e.Value == withText))
			{
				if (withs.Elements().Count() > 8)
				{
					withs.Elements().Last().Remove();
				}

				withs.AddFirst(new XElement("withText",
					HttpUtility.HtmlEncode(withText)
					));
			}

			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
