//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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
					var count = 0;
					foreach (var meta in metas.ToList())
					{
						meta.Parent.Attribute("objectID").Remove();
						meta.Remove();
						count++;
					}

					manager.UpdatePageContent(page.Root);

					UIHelper.ShowMessage(
						string.Format(Resx.DeleteFormulaCommand_Deleted, count));
				}
				else
				{
					UIHelper.ShowMessage(Resx.DeleteFormulaCommand_NoFormulas);
				}
			}
		}
	}
}
