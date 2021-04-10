//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class SpellCheckCommand : Command
	{
		public SpellCheckCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var disable = !(bool)args[0];

			logger.StartClock();

			using (var one = new OneNote(out var page, out var ns))
			{
				if (page != null)
				{
					if (page.GetTextCursor() == null)
					{
						// update only selected text...

						if (disable)
						{
							page.Root.Elements(ns + "Outline")
								.Descendants(ns + "T")
								.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")))
								.ForEach(e => e.SetAttributeValue("lang", "yo"));
						}
						else
						{
							page.Root.Elements(ns + "Outline")
								.Descendants(ns + "T")
								.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")))
								.ForEach(e => e.SetAttributeValue("lang", Thread.CurrentThread.CurrentUICulture.Name));
						}
					}
					else
					{
						// remove all occurances of the "lang=" attribute across the entire page

						page.Root.DescendantsAndSelf()
							.Attributes("lang")
							.Remove();

						if (disable)
						{
							// set lang=yo for the page and the page title

							page.Root.Add(new XAttribute("lang", "yo"));
							page.Root.Element(ns + "Title")?.Add(new XAttribute("lang", "yo"));
						}
					}

					await one.Update(page);
				}
			}

			logger.WriteTime($"spell check {(disable ? "disabled" : "enabled")}");
		}
	}
}
