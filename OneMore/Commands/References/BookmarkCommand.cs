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


	internal class BookmarkCommand : Command
	{
		public const string CollectionName = "bookmark";
		public const string SettingName = "hideMessage";
		private const string OldCollection = "bilink";
		private const string OldSetting = "hideStartMessage";

		private static string pageId;
		private static string objectId;
		private static SelectionRange range;
		private static string text;


		public BookmarkCommand()
		{
			IsCancelled = true;
		}


		public static string PageId => pageId;
		public static string ObjectId => objectId;
		public static SelectionRange Range => range;
		public static string Text => text;


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out _);
	
			if (!Bookmark(page))
			{
				ShowError(Resx.BookmarkCommand_Invalid);
				return;
			}

			InformUser();
		}


		private bool Bookmark(Page page)
		{
			range = new SelectionRange(page);

			// get selected runs but preserve cursor if there is one so we can edit from it later
			var run = range.GetSelection(true);
			if (run is null)
			{
				logger.WriteLine("bookmark failed - no selected content");
				Clear();
				return false;
			}

			// anchor range is the surrounding OE
			range = new SelectionRange(run.Parent);

			objectId = range.ObjectId;
			if (string.IsNullOrEmpty(objectId))
			{
				logger.WriteLine("bookmark failed - missing objectID");
				Clear();
				return false;
			}

			pageId = page.PageId;
			text = new PageEditor(page).GetSelectedText();

			logger.WriteLine($"bookmarked {objectId}");

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
				using var dialog = new BiLinkDialog();
				dialog.SetAnchorText(text.Length > 20 ? $"{text.Substring(0, 20)}..." : text);
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
			pageId = null;
			objectId = null;
			range = null;
		}
	}
}
