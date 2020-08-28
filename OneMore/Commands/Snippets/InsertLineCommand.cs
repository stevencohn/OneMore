//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Xml.Linq;


	internal class InsertLineCommand : Command
	{
		private const int LineCharCount = 100;


		public InsertLineCommand() : base()
		{
		}


		public void Execute(char c)
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


		private void InsertLine(char c)
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());
				var ns = page.Namespace;

				if (!page.ConfirmBodyContext(true))
				{
					return;
				}

				var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
				var color = dark ? "#D0D0D0" : "#202020";

				var current =
					(from e in page.Root.Descendants(ns + "OE")
					 where e.Elements(ns + "T").Attributes("selected").Any(a => a.Value.Equals("all"))
					 select e).FirstOrDefault();

				string line = string.Empty.PadRight(LineCharCount, c);

				page.EnsurePageWidth(line, "Courier New", 10f, manager.WindowHandle);

				current.AddAfterSelf(
					new XElement(ns + "OE",
						new XElement(ns + "T",
							new XAttribute("style", $"font-family:'Courier New';font-size:10.0pt;color:{color}"),
							new XCData(line + "<br/>")
						)
					));

				manager.UpdatePageContent(page.Root);
			}
		}
	}
}
