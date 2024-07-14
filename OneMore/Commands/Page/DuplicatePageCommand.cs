//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Simple and direct duplication of current page, inserting the new page immediate
	/// after the current page in the section. Adds (#) after the page title.
	/// </summary>
	internal class DuplicatePageCommand : Command
	{

		public DuplicatePageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// need All detail to copy images and Ink
			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);
			var originalId = page.PageId;
			var sectionId = one.CurrentSectionId;

			// create a new page with new ID and update its title
			one.CreatePage(sectionId, out var newId);

			// set the page ID to the new page's ID
			page.Root.Attribute("ID").Value = newId;

			// remove all objectID values and let OneNote generate new IDs
			page.Root.Descendants().Attributes("objectID").Remove();
			page = new Page(page.Root); // reparse to refresh PageId

			var section = await one.GetSection(sectionId);

			var editor = new SectionEditor(section);

			// restore Title if it's hidden; the Interop API doesn't let us delete Title!
			if (page.Title is null)
			{
				page.SetTitle(page.Root.Attribute("name").Value);
			}

			editor.SetUniquePageTitle(page);

			await one.Update(page);

			// FOLLOWING DOESN'T WORK; INTEROP API DOESN'T LET US HIDE (DELETE) Title
			//if (hiddenTitle)
			//{
			//	page = await one.GetPage(page.PageId);
			//	page.Root.Elements(ns + "Title").Remove();
			//	await one.Update(page);
			//}

			if (editor.MovePageAfterAnchor(page.PageId, originalId))
			{
				one.UpdateHierarchy(section);
			}
		}
	}
}
