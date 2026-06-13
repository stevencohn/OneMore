//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S2696

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using Resx = Properties.Resources;


	internal class Bookmark
	{
		public string PageId;
		public string ObjectId;
		public SelectionRange Range;
		public string Text;
	}



	internal class BookmarkCommand : Command
	{
		public const string CollectionName = "bookmark";
		public const string SettingName = "hideMessage";
		private const string OldCollection = "bilink";
		private const string OldSetting = "hideStartMessage";

		private static Bookmark bookmark;

		public BookmarkCommand()
		{
			IsCancelled = true;
		}


		public static Bookmark Bookmark => bookmark;


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out _);
	
			if (!MakeBookmark(page))
			{
				ShowError(Resx.BookmarkCommand_Invalid);
				Clear();
				return;
			}

			InformUser();
		}


		private bool MakeBookmark(Page page)
		{
			bookmark = new Bookmark();
			bookmark.Range = new SelectionRange(page);

			// get selected runs but preserve cursor if there is one so we can edit from it later
			var run = bookmark.Range.GetSelection(true);
			if (run is null)
			{
				logger.WriteLine("bookmark failed - no selected content");
				Clear();
				return false;
			}

			// anchor range is the surrounding OE
			bookmark.Range = new SelectionRange(run.Parent);

			bookmark.ObjectId = bookmark.Range.ObjectId;
			if (string.IsNullOrEmpty(bookmark.ObjectId))
			{
				logger.WriteLine("bookmark failed - missing objectID");
				Clear();
				return false;
			}

			bookmark.PageId = page.PageId;
			bookmark.Text = new PageEditor(page).GetSelectedText();

			logger.WriteLine($"bookmarked {bookmark.ObjectId}");

			return true;
		}


		private void InformUser()
		{
			var provider = new SettingsProvider();

			// this is the new collection
			var settings = provider.GetCollection(CollectionName);
			bool hide;
			if (settings.Contains(SettingName))
			{
				hide = true;
			}
			else
			{
				// this is the old collection for backward compatibility
				settings = provider.GetCollection(OldCollection);
				hide = settings.Contains(OldSetting);
			}

			if (!hide)
			{
				using var dialog = new BookmarkedDialog(bookmark.Text.Length > 50 
					? $"{bookmark.Text.Substring(0, 50)}..."
					: bookmark.Text);

				dialog.ShowDialog(owner);
			}
		}


		/// <summary>
		/// Clear the bookmark cache. Should be called once the bookmark is used by a
		/// subsequent command so it be ready for the next command. This does prevent
		/// reuse of the same bookmark unless the user explicitly recreates the same one.
		/// </summary>
		public static void Clear()
		{
			bookmark = null;
		}
	}
}
