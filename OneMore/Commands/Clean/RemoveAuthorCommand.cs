//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Linq;


	internal class RemoveAuthorsCommand : Command
	{
		public RemoveAuthorsCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var count = 0;
				var page = new Page(manager.CurrentPage());

				System.Diagnostics.Debugger.Launch();

				// these are all the elements that might have editedByAttributes
				var elements = page.Root.Descendants().Where(d => 
					d.Name.LocalName == "Table" ||
					d.Name.LocalName == "Row" ||
					d.Name.LocalName == "Cell" ||
					d.Name.LocalName == "Outline" ||
					d.Name.LocalName == "OE")
					.ToList();

				foreach (var element in elements)
				{
					// editedByAttributes attributeGroup
					var atts = element.Attributes().Where(a =>
						a.Name == "author" ||
						a.Name == "authorInitials" ||
						a.Name == "authorResolutionID" ||
						a.Name == "lastModifiedBy" ||
						a.Name == "lastModifiedByInitials" ||
						a.Name == "lastModifiedByResolutionID"
						)
						.ToList();

					count += atts.Count;
					atts.ForEach(a => a.Remove());
				}

				// TODO: This is removing authorship from OEs that wrap Images but
				// OneNote isn't saving those changes. I don't know why...

				if (count > 0)
				{
					logger.WriteLine($"clean {count} author attributes");
					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
