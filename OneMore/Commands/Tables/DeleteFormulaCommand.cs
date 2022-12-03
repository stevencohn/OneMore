//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Removes formula from the selected cell(s) but retain the values displayed in those cell.
	/// </summary>
	internal class DeleteFormulaCommand : Command
	{

		public DeleteFormulaCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);

			// only delete formula from selected cell(s)

			var metas = page.Root.Descendants(ns + "Cell")
				// first dive down to find the selected T
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE")
				.Elements(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				// now move back up to the Cell
				.Select(e => e.Parent)
				.Where(e => e.Element(ns + "Meta") != null && e.Element(ns + "Meta").Attribute("name").Value == "omfx")
				.Select(e => e.Element(ns + "Meta"));

			if (metas?.Any() == true)
			{
				var tagIndex = page.GetTagDefIndex(AddFormulaCommand.BoltSymbol);

				var count = 0;
				foreach (var meta in metas.ToList())
				{
					if (tagIndex != null)
					{
						var tag = meta.Parent.Elements(ns + "Tag")
							.FirstOrDefault(e => e.Attribute("index").Value == tagIndex);

						tag?.Remove();
					}

					meta.Parent.Attribute("objectID").Remove();
					meta.Remove();
					count++;
				}

				await one.Update(page);

				UIHelper.ShowMessage(
					string.Format(Resx.DeleteFormulaCommand_Deleted, count));
			}
			else
			{
				UIHelper.ShowInfo(Resx.DeleteFormulaCommand_NoFormulas);
			}
		}
	}
}
