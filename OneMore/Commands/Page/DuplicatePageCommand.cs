//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
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
			// capture page and section IDs before handing off to background thread
			await using var ctx = new OneNote();
			var originalId = ctx.CurrentPageId;
			var sectionId = ctx.CurrentSectionId;

			var progress = new UI.ProgressDialog(async (dialog, token) =>
			{
				dialog.SetMaximum(4);

				// need All detail to copy images and Ink
				dialog.SetMessage("Getting page content...");
				await using var one = new OneNote();
				var page = await one.GetPage(originalId, OneNote.PageDetail.All);
				dialog.Increment();

				if (token.IsCancellationRequested) { dialog.Close(); return; }

				dialog.SetMessage("Creating duplicate page...");

				// create a new page with new ID and update its title
				one.CreatePage(sectionId, out var newId);

				// set the page ID to the new page's ID
				page.Root.Attribute("ID").Value = newId;

				// ensure unique OneMore page ID
				if (page.GetMetaContent(MetaNames.PageID) is not null)
				{
					page.SetMeta(MetaNames.PageID, Guid.NewGuid().ToString("N"));
				}

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
				dialog.Increment();

				if (token.IsCancellationRequested) { dialog.Close(); return; }

				dialog.SetMessage("Uploading duplicate page...");
				await one.Update(page);
				dialog.Increment();

				// FOLLOWING DOESN'T WORK; INTEROP API DOESN'T LET US HIDE (DELETE) Title
				//if (hiddenTitle)
				//{
				//	page = await one.GetPage(page.PageId);
				//	page.Root.Elements(ns + "Title").Remove();
				//	await one.Update(page);
				//}

				if (token.IsCancellationRequested) { dialog.Close(); return; }

				dialog.SetMessage("Reorganizing section...");
				if (editor.MovePageAfterAnchor(page.PageId, originalId))
				{
					one.UpdateHierarchy(section);
				}

				dialog.Increment();
				dialog.Close();
			});

			progress.RunModeless();
		}
	}
}
