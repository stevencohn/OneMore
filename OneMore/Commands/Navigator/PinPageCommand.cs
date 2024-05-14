//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	/// <summary>
	/// Add the current page to the Navigator reading list.
	/// </summary>
	internal class PinPageCommand : Command
	{

		public PinPageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();
			var pageId = one.CurrentPageId;

			var provider = new NavigationProvider();
			var pinned = await provider.ReadPinned();

			if (!pinned.Exists(p => p.PageId == pageId))
			{
				var info = await one.GetPageInfo(pageId);
				pinned.Add(info);

				await provider.SavePinned(pinned);
			}
		}
	}
}
