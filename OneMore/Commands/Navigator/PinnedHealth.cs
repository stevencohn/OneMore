//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using HistoryRecord = OneNote.HierarchyInfo;


	/// <summary>
	/// Remembers which items of the reading list the background healer could not find, so that the
	/// Navigator can show them as such. It is only what the last cycle saw, so it is not stored.
	/// </summary>
	internal static class PinnedHealth
	{
		private static readonly object locker = new();
		private static Dictionary<string, string> problems = new(StringComparer.Ordinal);


		/// <summary>
		/// Raised when the problems changed, or when the healer saved a change to the list and a
		/// Navigator that is showing it should read it again. May be raised on any thread.
		/// </summary>
		public static event EventHandler Changed;


		/// <summary>
		/// Gets what to show the user about an item that cannot be found, or null if it is fine.
		/// </summary>
		public static string ProblemOf(HistoryRecord record)
		{
			lock (locker)
			{
				return problems.TryGetValue(KeyOf(record.PageId, record.ObjectId), out var message)
					? message
					: null;
			}
		}


		/// <summary>
		/// Replaces what is known with what the last cycle found.
		/// </summary>
		/// <param name="found">The items that cannot be found, as a page ID, a paragraph ID or
		/// null, and what to tell the user</param>
		/// <param name="saved">True if the healer saved changes to the list</param>
		public static void Update(IEnumerable<(string PageId, string ObjectId, string Message)> found, bool saved)
		{
			var next = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (var (pageID, objectID, message) in found)
			{
				next[KeyOf(pageID, objectID)] = message;
			}

			bool different;
			lock (locker)
			{
				different = next.Count != problems.Count ||
					next.Any(n => !problems.TryGetValue(n.Key, out var m) || m != n.Value);

				problems = next;
			}

			if (different || saved)
			{
				Changed?.Invoke(null, EventArgs.Empty);
			}
		}


		private static string KeyOf(string pageID, string objectID)
		{
			return pageID + "|" + (objectID ?? string.Empty);
		}
	}
}
