//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class HighlightFormulaCommand : Command
	{

		public HighlightFormulaCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			logger.StartClock();

			using (var one = new OneNote(out var page, out var ns))
			{
				// deselect any selected content in the page
				page.Root.Descendants().Attributes("selected").Remove();

				// highlight all formula cells in all tables on the page

				var tables = page.Root.Descendants(ns + "Cell")
					.Elements(ns + "OEChildren")
					.Elements(ns + "OE")
					.Elements(ns + "Meta")
					.Where(e => e.Attribute("name").Value == "omfx")
					.Select(e => e.FirstAncestor(ns + "Table"));

				if (tables?.Any() == true)
				{
					foreach (var table in tables)
					{
						// select all omfx cells
						var cells = table.Descendants(ns + "Meta")
							.Where(e => e.Attribute("name").Value == "omfx")
							.Select(e => e.Parent.Parent.Parent);

						foreach (var cell in cells)
						{
							cell.SetAttributeValue("selected", "all");
						}
					}

					await one.Update(page);
				}
				else
				{
					UIHelper.ShowInfo(Resx.HighlightFormulaCommand_NoFormulas);
				}
			}

			logger.WriteTime("highlight");
		}
	}
}
