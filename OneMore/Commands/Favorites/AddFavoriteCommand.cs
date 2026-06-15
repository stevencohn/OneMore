//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System.Linq;
	using System.Threading.Tasks;
	using River.OneMoreAddIn.UI;


	#region Wrappers
	internal class AddFavoritePageCommand : AddFavoriteCommand
	{
		public AddFavoritePageCommand() : base() { }
		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();
			await base.Execute(one.CurrentNotebookId, one.CurrentSectionId, one.CurrentPageId);
		}
	}

	internal class AddFavoriteSectionCommand : AddFavoriteCommand
	{
		private const string SectionName = "Section";
		private const string SectionGroupName = "SectionGroup";
		private const string isCurrentlyViewedAtt = "isCurrentlyViewed";

		public AddFavoriteSectionCommand() : base() { }
		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();

			var notebook = await one.GetNotebook();
			var ns = one.GetNamespace(notebook);

			var node = notebook.Descendants(ns + SectionName)
				.LastOrDefault(e =>
					e.Attribute(isCurrentlyViewedAtt) is not null &&
					e.Attribute(isCurrentlyViewedAtt).Value == "true");

			node ??= notebook.Descendants(ns + SectionGroupName)
				.LastOrDefault(e =>
					e.Attribute(isCurrentlyViewedAtt) is not null &&
					e.Attribute(isCurrentlyViewedAtt).Value == "true");

			if (node is not null)
			{
				await base.Execute(one.CurrentNotebookId, node.Attribute("ID").Value);
			}
		}
	}
	#endregion Wrappers


	internal class AddFavoriteCommand : Command
	{
		public override async Task Execute(params object[] args)
		{
			var pageID = args.Length > 2 ? args[2] as string : null;

			var favorite = new Favorite
			{
				NotebookID = args[0] as string,
				SectionID = args[1] as string,
				PageID = pageID
			};

			await using var one = new OneNote();

			var info = pageID is null
				? await one.GetSectionInfo(favorite.SectionID)
				: await one.GetPageInfo(favorite.PageID);

			favorite.Name = info.Name;
			favorite.Alias = info.Name;
			favorite.Location = info.Path;
			favorite.Uri = info.Link;

			var provider = new FavoritesProvider();
			if (provider.WriteFavorite(favorite))
			{
				ribbon?.InvalidateControl(FavoritesMenu.MenuID);
			}
			else
			{
				MoreMessageBox.ShowError(owner, "Could not save favorite");
			}
		}
	}
}
