//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System.Linq;
	using System.Threading.Tasks;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


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

			var kind = (string)null;
			if (node is null)
			{
				node = notebook.Descendants(ns + SectionGroupName)
					.LastOrDefault(e =>
						e.Attribute(isCurrentlyViewedAtt) is not null &&
						e.Attribute(isCurrentlyViewedAtt).Value == "true");

				kind = KindSectionGroup;
			}

			if (node is not null)
			{
				await base.Execute(one.CurrentNotebookId, node.Attribute("ID").Value, null, kind);
			}
		}
	}

	internal class AddFavoriteSectionGroupCommand : AddFavoriteCommand
	{
		public AddFavoriteSectionGroupCommand() : base() { }
		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();

			var sectionGroupId = one.GetParent(one.CurrentSectionId);
			if (string.IsNullOrEmpty(sectionGroupId))
			{
				return;
			}

			await base.Execute(one.CurrentNotebookId, sectionGroupId, null, KindSectionGroup);
		}
	}

	internal class AddFavoriteNotebookCommand : AddFavoriteCommand
	{
		public AddFavoriteNotebookCommand() : base() { }
		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();
			var notebookId = one.CurrentNotebookId;
			await base.Execute(notebookId, notebookId, null, KindNotebook);
		}
	}
	#endregion Wrappers


	internal class AddFavoriteCommand : Command
	{
		protected const string KindSectionGroup = "sectiongroup";
		protected const string KindNotebook = "notebook";

		public override async Task Execute(params object[] args)
		{
			var pageID = args.Length > 2 ? args[2] as string : null;
			var kind = args.Length > 3 ? args[3] as string : null;

			var favorite = new Favorite
			{
				NotebookID = args[0] as string,
				SectionID = args[1] as string,
				PageID = pageID,
				Kind = kind
			};

			await using var one = new OneNote();

			var info = kind == KindNotebook
				? await one.GetNotebookInfo(favorite.NotebookID)
				: pageID is null
					? await one.GetSectionInfo(favorite.SectionID)
					: await one.GetPageInfo(favorite.PageID);

			favorite.Name = info.Name;
			favorite.Location = info.Path;
			favorite.Uri = info.Link;

			var provider = new FavoritesProvider();
			if (provider.WriteFavorite(favorite, out var duplicate))
			{
				ribbon?.InvalidateControl(FavoritesMenu.MenuID);
			}
			else if (duplicate)
			{
				MoreMessageBox.ShowError(owner, Resx.AddFavoriteCommand_duplicate);
			}
			else
			{
				MoreMessageBox.ShowError(owner, Resx.AddFavoriteCommand_error);
			}
		}
	}
}
