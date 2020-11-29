//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Xml.Linq;


	internal class NoSpellCheckCommand : Command
	{
		public NoSpellCheckCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			logger.StartClock();

			using (var one = new OneNote(out var page, out var ns))
			{
				if (page != null)
				{
					// remove all occurances of the "lang=" attribute across the entire page

					page.Root.DescendantsAndSelf()
						.Attributes("lang")
						.Remove();

					// set lang=yo for the page and the page title

					page.Root.Add(new XAttribute("lang", "yo"));
					page.Root.Element(ns + "Title")?.Add(new XAttribute("lang", "yo"));

					one.Update(page);
				}
			}

			logger.WriteTime("language reset");
		}
	}
}
