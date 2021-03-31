//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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

					await one.Update(page);
				}
			}

			logger.WriteTime($"spell check {(disable ? "disabled" : "enabled")}");
		}
	}
}
