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
		private const string NoLang = "yo";


		public SpellCheckCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var disable = !(bool)args[0];

			bool? ok = null;

			logger.StartClock();

			using (var one = new OneNote(out var page, out var ns))
			{
				if (page != null)
				{
					ok = false;

					if (page.GetTextCursor() == null)
					{
						// update only selected text...

						var selections = page.Root.Elements(ns + "Outline")
							.Descendants(ns + "T")
							.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

						if (selections.Any())
						{
							var cultureName = disable ? NoLang : Thread.CurrentThread.CurrentUICulture.Name;
							selections.ForEach(e => e.SetAttributeValue("lang", cultureName));
							ok = true;
						}
						else
						{
							logger.WriteLine("selections not found, setting page");
						}
					}

					if (ok != true)
					{
						// remove all occurances of the "lang=" attribute across the entire page

						page.Root.DescendantsAndSelf()
							.Where(e => e.Name.LocalName != "OCRData")
							.Attributes("lang")
							.Remove();

						var cultureName = disable ? NoLang : Thread.CurrentThread.CurrentUICulture.Name;
						page.Root.Add(new XAttribute("lang", cultureName));
						page.Root.Element(ns + "Title")?.Add(new XAttribute("lang", cultureName));
					}

					//logger.WriteLine(page.Root);

					await one.Update(page);
				}
			}

			if (ok == null)
			{
				logger.WriteTime($"spell check cancelled, invalid context");
			}
			else
			{
				var area = ok == true ? "selection" : "full page";
				logger.WriteTime($"spell check on {area} {(disable ? "disabled" : "enabled")}");
			}
		}
	}
}
