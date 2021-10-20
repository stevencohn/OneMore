//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
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

			using (var one = new OneNote(out var page, out _))
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

				var editor = new SearchAndReplaceEditor(whatText, withText,
					useRegex: useRegex,
					caseSensitive: matchCase
					);

				var count = editor.SearchAndReplace(page);

				if (count > 0)
				{
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
