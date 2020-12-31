//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class InsertLineCommand : Command
	{
		private const int LineCharCount = 100;


		public InsertLineCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var c = (char)args[0];

			using (var one = new OneNote(out var page, out var ns))
			{
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

				page.EnsurePageWidth(line, "Courier New", 10f, one.WindowHandle);

				current.AddAfterSelf(
					new XElement(ns + "OE",
						new XElement(ns + "T",
							new XAttribute("style", $"font-family:'Courier New';font-size:10.0pt;color:{color}"),
							new XCData(line + "<br/>")
						)
					));

				await one.Update(page);
			}
		}
	}
}
