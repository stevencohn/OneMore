//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;


	internal class NoSpellCheckCommand : Command
	{
		public NoSpellCheckCommand ()
		{
		}


		public void Execute ()
		{
			logger.WriteLine("NoSpellCheckCommand.Execute()");

			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				if (page != null)
				{
					var ns = page.GetNamespaceOfPrefix("one");

					// remove all occurances of the "lang=" attribute across the entire page

					var list =
						from e in page.DescendantsAndSelf()
						where e.Attributes().Any(a => a.Name.LocalName.ToLower().Equals("lang"))
						select e;

					if (list?.Count() > 0)
					{
						foreach (var e in list)
						{
							e.Attribute("lang").Remove();
						}
					}

					// set lang=yo for the page and the page title

					page.Add(new XAttribute("lang", "yo"));
					page.Element(ns + "Title")?.Add(new XAttribute("lang", "yo"));

					manager.UpdatePageContent(page);
				}
			}
		}
	}
}
