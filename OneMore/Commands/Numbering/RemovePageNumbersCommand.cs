//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text.RegularExpressions;


	internal class RemovePageNumbersCommand : Command
	{
		public RemovePageNumbersCommand()
		{
		}


		public void Execute()
		{
			var npattern = new Regex(@"^(\(\d+\)\s).+");
			var apattern = new Regex(@"^(\([a-z]+\)\s).+");
			var ipattern = new Regex(@"^(\((?:xc|xl|l?x{0,3})(?:ix|iv|v?i{0,3})\)\s).+");

			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				if (section != null)
				{
					var ns = section.GetNamespaceOfPrefix("one");

					var pageIds = section.Elements(ns + "Page")
						.Select(e => e.Attribute("ID").Value);

					foreach (var pageId in pageIds)
					{
						var page = manager.GetPage(pageId, Microsoft.Office.Interop.OneNote.PageInfo.piBasic);
						var name = page.Attribute("name").Value;

						if (string.IsNullOrEmpty(name))
						{
							continue;
						}

						// numeric 1.
						var match = npattern.Match(name);

						// alpha a.
						if (!match.Success)
						{
							match = apattern.Match(name);
						}

						// alpha i.
						if (!match.Success)
						{
							match = ipattern.Match(name);
						}

						if (match.Success)
						{
							page.Element(ns + "Title")
								.Element(ns + "OE")
								.Element(ns + "T")
								.GetCData().Value = name.Substring(match.Groups[1].Length);

							manager.UpdatePageContent(page);
						}
					}
				}
			}
		}
	}
}
