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

				kind = Favorite.KindSectionGroup;
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

			await base.Execute(one.CurrentNotebookId, sectionGroupId, null, Favorite.KindSectionGroup);
		}
	}

	internal class AddFavoriteNotebookCommand : AddFavoriteCommand
	{
		public AddFavoriteNotebookCommand() : base() { }
		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();
			var notebookId = one.CurrentNotebookId;
			await base.Execute(notebookId, notebookId, null, Favorite.KindNotebook);
		}
	}
	#endregion Wrappers


	internal class AddFavoriteCommand : Command
	{
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

			// GetHyperlinkToObject is unreliable for notebook/section-group hierarchy IDs
			// (see OneNote.GetHyperlink), so those favorites navigate by raw ID instead;
			// only look up a Name/Location and skip the fragile Link lookup for them.
			if (kind == Favorite.KindNotebook || kind == Favorite.KindSectionGroup)
			{
				var info = kind == Favorite.KindNotebook
					? await one.GetNotebookInfo(favorite.NotebookID)
					: await one.GetSectionInfo(favorite.SectionID);

				favorite.Name = info.Name;
				favorite.Location = info.Path;
				favorite.Uri = favorite.SectionID;
			}
			else
			{
				var info = pageID is null
					? await one.GetSectionInfo(favorite.SectionID)
					: await one.GetPageInfo(favorite.PageID);

				favorite.Name = info.Name;
				favorite.Location = info.Path;
				favorite.Uri = info.Link;
			}

			var provider = new FavoritesProvider();
			favorite.SortOrder = provider.GetNextSortOrder(favorite.FolderID);

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
