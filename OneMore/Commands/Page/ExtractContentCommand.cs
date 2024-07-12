//************************************************************************************************
// Copyright © 2024 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Threading.Tasks;


	#region Wrappers
	internal class CopyPageContentCommand : ExtractContentCommand
	{
		public CopyPageContentCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(false);
		}
	}
	internal class MovePageContentCommand : ExtractContentCommand
	{
		public MovePageContentCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(true);
		}
	}
	#endregion Wrappers

	/// <summary>
	/// Copy or move selected content from the current page into a new page, providing a
	/// quick and easy way to arbitrarily split up a page into mutliple subpages or extract
	/// portions of a page into new subpages.
	/// </summary>
	internal class ExtractContentCommand : Command
	{
		public ExtractContentCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			bool move = (bool)args[0];

			await using var one = new OneNote(out var page, out _, OneNote.PageDetail.All);

			var editor = new PageEditor(page);
			var content = await editor.ExtractSelectedContent();
			if (!content.HasElements)
			{
				return;
			}

			var sectionEditor = new SectionEditor(await one.GetSection());

			// current page is the initial anchor
			var anchorID = sectionEditor.FindLastIndexedPageIDByTitle(page.Title);

			// make target page...

			// create a page in this section to capture heading content
			one.CreatePage(one.CurrentSectionId, out var targetID);
			var target = await one.GetPage(targetID);
			target.Title = page.Title; // temp

			// refresh the editor
			sectionEditor = new SectionEditor(await one.GetSection());
			sectionEditor.SetUniquePageTitle(target);

			var container = target.EnsureContentContainer();
			container.Add(content.Elements());

			var map = target.MergeQuickStyles(page);
			target.ApplyStyleMapping(map, container);

			await one.Update(target);

			// place the page in index-order after the source page...

			if (anchorID is not null)
			{
				// refresh the editor
				sectionEditor = new SectionEditor(await one.GetSection());
				if (sectionEditor.MovePageAfterAnchor(target.PageId, anchorID))
				{
					one.UpdateHierarchy(sectionEditor.Section);
				}
			}

			if (move)
			{
				// update the source page, removing the moved content
				await one.Update(page);
			}

			//await one.NavigateTo(page.PageId);
		}
	}
}
