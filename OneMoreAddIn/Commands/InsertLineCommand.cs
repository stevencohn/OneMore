//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;
	using Microsoft.Office.Interop.OneNote;


	internal class InsertLineCommand : Command
	{
		public InsertLineCommand () : base()
		{
		}


		public bool IsBodyContext ()
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var found = page
					.Elements(ns + "Outline")?
					.Descendants(ns + "T")?
					.Attributes("selected")?
					.Any(a => a.Value.Equals("all"));

				ribbon.Invalidate();

				return found ?? true;
			}
		}


		public void Execute (char c)
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var current =
					(from e in page.Descendants(ns + "OE")
					 where e.Elements(ns + "T").Attributes("selected").Any(a => a.Value.Equals("all"))
					 select e).FirstOrDefault();

				if (current != null)
				{
					current.AddAfterSelf(
						new XElement(ns + "OE",
							new XElement(ns + "T",
								new XAttribute("style", "font-family:'Courier New';font-size:10.0pt"),
								new XCData(string.Empty.PadRight(90, c) + "<br/>")
							)
						));
				}

				manager.UpdatePageContent(page);
			}
		}
	}
}
