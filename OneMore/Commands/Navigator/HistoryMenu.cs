//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using HistoryRecord = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	/// <summary>
	/// Builds the ribbon XML for the Navigator dynamic menu: an "Open Navigator" entry
	/// followed by a separator and the recently-visited-page history, modeled on
	/// FavoritesMenu.
	/// </summary>
	internal static class HistoryMenu
	{
		public const string OpenAction = "NavigatorCmd";
		public const string GotoAction = "GotoHistoryCmd";

		private static readonly XNamespace ns = "http://schemas.microsoft.com/office/2009/07/customui";
		private static readonly string OpenButtonId = "omOpenNavigatorButton";
		private static readonly string SeparatorId = "omHistorySeparator";

		// the dropdown must stay short even if the user allows a much larger stored
		// history; the History dialog, with its own filter box, is not similarly capped
		private const int MaxItems = 40;


		/// <summary>
		/// Loads the current history and builds the ribbon XML for the dropdown.
		/// </summary>
		/// <returns></returns>
		public static XElement LoadMenu()
		{
			var settings = new SettingsProvider().GetCollection(nameof(NavigatorSheet));
			var disabled = settings.Get("disabled", false);

			// when the service is disabled there is no history to show; NavigatorCmd
			// itself reports the "disabled" message when the Open Navigator entry is used
			if (disabled)
			{
				return BuildMenu(new List<HistoryRecord>(), 0);
			}

			var depth = settings.Get("depth", NavigationService.DefaultHistoryDepth);

			using var provider = new NavigationProvider();

			// run on the threadpool, off the ribbon/UI thread's SynchronizationContext, so
			// blocking here on the async file read below cannot deadlock against its own
			// continuation (getContent callbacks are inherently synchronous)
			var log = Task.Run(() => provider.ReadHistoryLog()).GetAwaiter().GetResult();

			return BuildMenu(log.History, Math.Min(MaxItems, depth));
		}


		/// <summary>
		/// Builds the ribbon XML for the given history, capped at the given number of
		/// items. Pure XML construction, no I/O, so this is the entry point for unit tests.
		/// </summary>
		internal static XElement BuildMenu(List<HistoryRecord> history, int max)
		{
			// the root element returned from a dynamicMenu's getContent callback is typed
			// as CT_MenuRoot, which carries no id attribute - only its children may have one
			var root = new XElement(ns + "menu", MakeOpenButton());

			if (history.Count > 0)
			{
				root.Add(new XElement(ns + "menuSeparator", new XAttribute("id", SeparatorId)));

				var index = 0;
				foreach (var record in history.Take(max))
				{
					root.Add(MakeHistoryButton(record, index++));
				}
			}

			return root;
		}


		private static XElement MakeOpenButton()
		{
			return new XElement(ns + "button",
				new XAttribute("id", OpenButtonId),
				new XAttribute("onAction", OpenAction),
				new XAttribute("getImage", "GetNavigatorMenuImage"),
				new XAttribute("label", Resx.ribNavigatorButton_Label),
				new XAttribute("screentip", Resx.ribNavigatorButton_Screentip)
				);
		}


		private static XElement MakeHistoryButton(HistoryRecord record, int index)
		{
			// PageId contains characters (braces, hyphens) that aren't safe as a ribbon
			// control id, so index the id instead; the page identity travels in tag=Link
			return new XElement(ns + "button",
				new XAttribute("id", $"omHistory{index}"),
				new XAttribute("onAction", GotoAction),
				new XAttribute("imageMso", "GroupInsertLinks"),
				new XAttribute("label", Chop(record.Name)),
				new XAttribute("tag", record.Link),
				new XAttribute("screentip", record.Path)
				);
		}


		private static string Chop(string s)
		{
			return s.Length <= 45 ? s : s.Substring(0, 42) + "...";
		}
	}
}
