//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Removes "Authored by" annotations from a page, also removing related
	/// attributes from all content on the page
	/// </summary>
	internal class RemoveAuthorsCommand : Command
	{
		public RemoveAuthorsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);
			logger.StartClock();

			var count = 0;

			// these are all the elements that might have editedByAttributes
			var elements = page.Root.Descendants().Where(d =>
				d.Name.LocalName == "Table" ||
				d.Name.LocalName == "Row" ||
				d.Name.LocalName == "Cell" ||
				d.Name.LocalName == "Outline" ||
				d.Name.LocalName == "OE")
				.ToList();

			var editedByAttributes = KnownSchemaAttributes.GetEditedByAttributes();

			foreach (var element in elements)
			{
				var attributes = element.Attributes()
					.Where(a => editedByAttributes.Contains(a.Name.LocalName))
					.ToList();

				count += attributes.Count;
				attributes.ForEach(a => a.Remove());
			}

			// TODO: This is removing authorship from OEs that wrap Images but
			// OneNote isn't saving those changes. I don't know why...

			if (count > 0)
			{
				logger.WriteTime($"removed {count} author-related attributes; saving...", true);
				await one.Update(page);
			}

			logger.WriteTime("removed authors completed");
		}
	}
}
