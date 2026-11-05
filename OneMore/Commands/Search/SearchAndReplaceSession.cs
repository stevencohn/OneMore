//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// The outcome of one step of a <see cref="SearchAndReplaceSession"/>.
	/// </summary>
	internal class SessionResult
	{
		/// <summary>Gets a value indicating whether a match is now selected on the page</summary>
		public bool Found { get; set; }

		/// <summary>Gets the 1-based position of the selected match among all matches</summary>
		public int Ordinal { get; set; }

		/// <summary>Gets the number of matches on the page</summary>
		public int Total { get; set; }

		/// <summary>Gets a value indicating whether the search wrapped past the page end</summary>
		public bool Wrapped { get; set; }

		/// <summary>Gets the number of replacements or highlights applied by this step</summary>
		public int Changed { get; set; }
	}


	/// <summary>
	/// Steps through the matches on one page for Find Next, Replace and Highlight, similar to
	/// Visual Studio's find and replace. To show a match, its run is split so that exactly
	/// the matched characters are selected in OneNote. That structure is put back the next
	/// time the session touches the page, and when the session is released, so stepping
	/// past a match without changing it leaves the page as it was.
	/// </summary>
	internal class SearchAndReplaceSession
	{
		private sealed class Pending
		{
			public string ObjectId;             // the OE holding the split runs
			public XElement Original;           // the run before it was split
			public List<string> Pieces;         // the CDATA of each run it was split into
		}

		private readonly string pageId;
		private readonly Action<Page> stamp;

		private int cursorRun;
		private int cursorIndex;
		private (int Run, int Start, int Length, string Text)? current;
		private Pending pending;


		/// <summary>
		/// Initialize a new session on the given page.
		/// </summary>
		/// <param name="pageId">The page to search</param>
		/// <param name="stamp">
		/// Called on a page just before it is saved with a replacement or highlight applied
		/// </param>
		public SearchAndReplaceSession(string pageId, Action<Page> stamp = null)
		{
			this.pageId = pageId;
			this.stamp = stamp;
		}


		/// <summary>
		/// Forget the current match and restart from the top of the page, such as after the
		/// search criteria have changed.
		/// </summary>
		public void Reset()
		{
			cursorRun = 0;
			cursorIndex = 0;
			current = null;
		}


		/// <summary>
		/// Selects the next match after the current one, wrapping at the end of the page.
		/// </summary>
		public Task<SessionResult> FindNext(SearchAndReplaceEditor editor)
		{
			return Step(editor, null);
		}


		/// <summary>
		/// Replaces the current match, if any, and selects the next one.
		/// </summary>
		public Task<SessionResult> Replace(SearchAndReplaceEditor editor)
		{
			return Step(editor, editor.ReplaceMatch);
		}


		/// <summary>
		/// Highlights the current match, if any, and selects the next one.
		/// </summary>
		public Task<SessionResult> Highlight(SearchAndReplaceEditor editor)
		{
			return Step(editor, editor.HighlightMatch);
		}


		/// <summary>
		/// Replaces or highlights every match on the page.
		/// </summary>
		/// <returns>The number of matches changed</returns>
		public async Task<int> ApplyAll(SearchAndReplaceEditor editor)
		{
			await using var one = new OneNote();
			var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
			if (page is null)
			{
				return 0;
			}

			var restored = Restore(page);
			var count = editor.SearchAndReplace(page);

			if (count > 0)
			{
				stamp?.Invoke(page);
			}

			if (count > 0 || restored)
			{
				await one.Update(page);
			}

			Reset();
			return count;
		}


		/// <summary>
		/// Puts the page back the way it was before a match was selected.
		/// </summary>
		public async Task Release()
		{
			if (pending is null)
			{
				return;
			}

			await using var one = new OneNote();
			var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
			if (page is not null && Restore(page))
			{
				await one.Update(page);
			}

			pending = null;
		}


		private async Task<SessionResult> Step(
			SearchAndReplaceEditor editor, Func<MatchInfo, int> apply)
		{
			var result = new SessionResult();

			await using var one = new OneNote();
			var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
			if (page is null)
			{
				return result;
			}

			var dirty = Restore(page);

			var runs = page.Root.Descendants(page.Namespace + "T").ToList();
			var order = new Dictionary<XElement, int>();
			for (var i = 0; i < runs.Count; i++)
			{
				order[runs[i]] = i;
			}

			var matches = editor.FindAllMatches(page);

			if (apply is not null && current is not null)
			{
				var cur = current.Value;
				var match = matches.FirstOrDefault(m =>
					order[m.TElement] == cur.Run &&
					m.StartIndex == cur.Start &&
					m.Length == cur.Length &&
					m.MatchedText == cur.Text);

				if (match is not null)
				{
					var end = apply(match);
					if (end >= 0)
					{
						result.Changed = 1;
						cursorRun = cur.Run;
						cursorIndex = end;
						dirty = true;

						stamp?.Invoke(page);

						// text offsets have shifted
						matches = editor.FindAllMatches(page);
					}
				}
			}

			current = null;
			result.Total = matches.Count;

			if (matches.Count > 0)
			{
				var index = matches.FindIndex(m =>
					order[m.TElement] > cursorRun ||
					(order[m.TElement] == cursorRun && m.StartIndex >= cursorIndex));

				if (index < 0)
				{
					index = 0;
					result.Wrapped = true;
				}

				var target = matches[index];
				var run = order[target.TElement];

				current = (run, target.StartIndex, target.Length, target.MatchedText);
				cursorRun = run;
				cursorIndex = target.StartIndex + Math.Max(target.Length, 1);

				result.Found = true;
				result.Ordinal = index + 1;

				dirty |= Select(target);

				if (!string.IsNullOrEmpty(target.ParagraphObjectId))
				{
					// scroll to the paragraph first; the update then sets the selection
					await one.NavigateTo(pageId, target.ParagraphObjectId);
				}
			}

			if (dirty)
			{
				await one.Update(page);
			}

			return result;
		}


		// Splits the run so exactly the matched characters are a selected run
		private bool Select(MatchInfo match)
		{
			var split = RunSplitter.Split(match);
			if (split is null)
			{
				return false;
			}

			pending = new Pending
			{
				ObjectId = split.ObjectId,
				Original = split.Original,
				Pieces = split.Pieces
			};

			return true;
		}


		// Puts back the original run in place of the runs a previous Select split it into.
		// If OneNote has already recombined them, or the user changed them, do nothing.
		private bool Restore(Page page)
		{
			var old = pending;
			if (old is null)
			{
				return false;
			}

			pending = null;

			var ns = page.Namespace;
			var oe = page.Root.Descendants(ns + "OE")
				.FirstOrDefault(e => e.Attribute("objectID")?.Value == old.ObjectId);

			if (oe is null)
			{
				return false;
			}

			var runs = oe.Elements(ns + "T").ToList();

			for (var i = 0; i + old.Pieces.Count <= runs.Count; i++)
			{
				var same = true;
				for (var j = 0; j < old.Pieces.Count && same; j++)
				{
					same = runs[i + j].GetCData()?.Value == old.Pieces[j];
				}

				if (same)
				{
					runs[i].AddBeforeSelf(new XElement(old.Original));
					for (var j = 0; j < old.Pieces.Count; j++)
					{
						runs[i + j].Remove();
					}

					return true;
				}
			}

			return false;
		}
	}
}
