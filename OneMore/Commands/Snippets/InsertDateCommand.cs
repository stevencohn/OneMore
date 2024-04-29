//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class InsertDateCommand : Command
	{

		public InsertDateCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			var includeTime = (bool)args[0];
			var text = DateTime.Now.ToString(includeTime ? "yyy-MM-dd hh:mm tt" : "yyy-MM-dd");
			var content = new XElement(ns + "T", new XCData(text));

			var cursor = page.GetTextCursor(allowPageTitle: true);

			if (page.SelectionScope == SelectionScope.Region || page.SelectionSpecial)
			{
				// replace region or hyperlink/MathML
				page.ReplaceSelectedWithContent(content);
			}
			else if (cursor == null) // && page.SelectionScope == SelectionScope.Empty)
			{
				// can't find cursor to append to page
				page.AddNextParagraph(content);
			}
			else
			{
				var line = page.Root .Descendants(ns + "T")
					.FirstOrDefault(e => 
						e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

				if (line is null)
				{
					// this case should not happen; should be handled above
					page.AddNextParagraph(content);
				}
				else
				{
					if (line.FirstAncestor(ns + "Title", ns + "Outline") is XElement title)
					{
						// special case to insert date before heading;
						// if cursor is before first char of title, the entire title is "selected"
						// so rather than replace the title, just insert the date before it
						var first = title.Elements(ns + "OE").Elements(ns + "T").First();
						var cdata = first.GetCData();
						cdata.Value = $"{text} {cdata.Value}";
					}
					else if (line.Value.Length == 0)
					{
						// empty cdata, unselected cursor so just insert
						line.GetCData().Value = text;
					}
					else
					{
						// this case should not happen; should be handled above
						// something is selected so replace it
						page.ReplaceSelectedWithContent(content);
					}
				}
			}

			await one.Update(page);
		}
	}
}
