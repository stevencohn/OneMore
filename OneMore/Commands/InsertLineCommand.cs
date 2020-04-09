//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
	using System.Xml.Linq;


	internal class InsertLineCommand : Command
	{
		private const int LineCharCount = 100;


		public InsertLineCommand () : base()
		{
		}


		public void Execute (char c)
		{
			try
			{
				InsertLine(c);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(InsertLineCommand)}", exc);
			}
		}


		private void InsertLine (char c)
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
					string line = string.Empty.PadRight(LineCharCount, c);

					PageHelper.EnsurePageWidth(page, line, "Courier New", 10f, manager.WindowHandle);

					current.AddAfterSelf(
						new XElement(ns + "OE",
							new XElement(ns + "T",
								new XAttribute("style", "font-family:'Courier New';font-size:10.0pt"),
								new XCData(line + "<br/>")
							)
						));

					manager.UpdatePageContent(page);
				}
			}
		}
	}
}
