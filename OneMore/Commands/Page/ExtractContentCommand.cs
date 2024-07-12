//************************************************************************************************
// Copyright © 2024 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Threading.Tasks;
	using System.Xml.Linq;


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
		private OneNote one;
		private Page page;
		private XNamespace ns;


		public ExtractContentCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			bool move = (bool)args[0];

			await using (one = new OneNote(out page, out ns, OneNote.PageDetail.All))
			{
				var editor = new PageEditor(page);
				var content = await editor.ExtractSelectedContent();
				if (content.HasElements)
				{
					var target = await MakeTargetPage(page.Title, content);

					if (move)
					{
						await one.Update(page);
					}

					//await one.NavigateTo(page.PageId);
				}
			}
		}


		private async Task<Page> MakeTargetPage(string title, XElement content)
		{
			// create a page in this section to capture heading content
			one.CreatePage(one.CurrentSectionId, out var pageId);
			var target = await one.GetPage(pageId);

			target.Title = $"{title}_0";

			var container = target.EnsureContentContainer();
			container.Add(content.Elements());
			await one.Update(target);

			return target;
		}
	}
}
