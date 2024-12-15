//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Select all tables on the page
	/// </summary>
	internal class SelectTablesCommand : Command
	{

		public SelectTablesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			var cells = page.BodyOutlines
				.Descendants(ns + "Cell")?
				.ToList();

			if (cells.Any())
			{
				// deselect anything selected
				var selections = page.Root.Descendants()
					.Where(e => e.Attribute("selected")?.Value == "all")
					.ToList();

				selections.ForEach((e) =>
				{
					e.Attributes("selected").Remove();
				});

				foreach (var cell in cells)
				{
					cell.SetAttributeValue("selected", "all");
				}

				await one.Update(page);
			}
		}
	}
}
