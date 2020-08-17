//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;


	internal class DeleteFormulaCommand : Command
	{

		public DeleteFormulaCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());
				var ns = page.Namespace;

				// only delete formula from selected cell(s)

				var cells = page.Root.Descendants(ns + "Cell")?
					.Select(o => new { cell = o, selected = o.Attribute("selected")?.Value })
					.Where(o => o.selected == "all" || o.selected == "partial")
					.Select(o => o.cell);

				if (cells?.Count() > 0)
				{
					foreach (var cell in cells)
					{
						var metas = cell.Descendants(ns + "Meta")
							.Where(e => e.Attribute("name").Value == "omfx");

						if (metas?.Count() > 0)
						{
							foreach (var meta in metas.ToList())
							{
								meta.Parent.Attribute("objectID").Remove();
								meta.Remove();
							}
						}
					}

					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
