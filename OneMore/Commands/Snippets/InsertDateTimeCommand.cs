//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class InsertDateTimeCommand : Command
	{

		public InsertDateTimeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var elements = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all");

				var text = DateTime.Now.ToString("yyy-MM-dd hh:mm tt");
				var content = new XElement(ns + "T", new XCData(text));

				if (elements == null)
				{
					// empty page so add new content
					page.AddNextParagraph(content);
				}
				else if (elements.Count() > 1)
				{
					// selected multiple runs so replace them all
					page.ReplaceSelectedWithContent(content);
				}
				else
				{
					var line = elements.First();
					if (line.Parent.Parent.Name.LocalName == "Title" &&
						line.Value == line.Parent.Value)
					{
						// special case to insert date before heading;
						// if cursor is before first char of title, the entire title is "selected"
						// so rather than replace the title, just insert the date before it
						var cdata = line.GetCData();
						cdata.Value = $"{text} {cdata.Value}";
					}
					else if (line.Value.Length == 0)
					{
						// empty cdata, unselected cursor so just insert
						line.GetCData().Value = text;
					}
					else
					{
						// something is selected so replace it
						page.ReplaceSelectedWithContent(content);
					}
				}

				await one.Update(page);
			}
		}
	}
}
