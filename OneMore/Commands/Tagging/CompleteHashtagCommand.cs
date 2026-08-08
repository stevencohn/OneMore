//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Extensions;
	using River.OneMoreAddIn.Models;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Shows a small popup, anchored near the current word, letting the user pick or type
	/// a hashtag to replace that word with. Bound to Alt+G and the Complete Hashtag ribbon
	/// button.
	/// </summary>
	internal class CompleteHashtagCommand : Command
	{
		private static CompleteHashtagDialog dialog;
		private static bool commandIsActive;


		public CompleteHashtagCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive)
			{
				return;
			}

			commandIsActive = true;

			try
			{
				if (dialog != null)
				{
					// single instance
					dialog.Elevate();
					return;
				}

				string word;
				System.IntPtr windowHandle;

				await using (var one = new OneNote(out var page, out var ns))
				{
					word = PeekCaretWord(page, ns);
					windowHandle = one.WindowHandle;
				}

				using var provider = new HashtagProvider();
				var names = provider.ReadTagNames().ToArray();
				var recent = provider.ReadLatestTagNames().ToArray();

				dialog = new CompleteHashtagDialog(word, names, recent);
				dialog.FormClosed += Dialog_FormClosed;

				var anchor = CaretLocator.Locate(windowHandle);
				var location = Screen.FromPoint(anchor.Location)
					.GetBoundedLocationNear(anchor, dialog.Size);

				dialog.RunModeless(location, async (sender, e) =>
				{
					var d = sender as CompleteHashtagDialog;
					if (d.DialogResult == DialogResult.OK)
					{
						await ApplyTag(d.SelectedTag);
					}
				});
			}
			finally
			{
				commandIsActive = false;
			}
		}


		private async Task ApplyTag(string tag)
		{
			await using var one = new OneNote(out var page, out var ns);

			if (!ReplaceCaretWord(page, ns, tag))
			{
				new PageEditor(page).InsertOrReplace(tag);
			}

			await one.Update(page);
		}


		private void Dialog_FormClosed(object sender, System.EventArgs e)
		{
			if (dialog != null)
			{
				dialog.FormClosed -= Dialog_FormClosed;
				dialog.Dispose();
				dialog = null;
			}
		}


		/// <summary>
		/// Read-only peek at the word straddling the empty text cursor, used to seed the
		/// popup's text box. Does not modify the page.
		/// </summary>
		private static string PeekCaretWord(Page page, XNamespace ns)
		{
			var selection = FindCaretSelection(page, ns);
			if (selection is null)
			{
				return string.Empty;
			}

			var (prev, next) = FindAdjoiningRuns(selection);

			var builder = new StringBuilder();
			if (prev is not null)
			{
				builder.Append(prev.Value.SplitAtLastWord().Item1);
			}
			if (next is not null)
			{
				builder.Append(next.Value.SplitAtFirstWord().Item1);
			}

			return builder.ToString();
		}


		/// <summary>
		/// Removes the word straddling the empty text cursor, following the same pattern as
		/// ApplyStyleCommand.StylizeWords, and replaces it with the given tag text.
		/// </summary>
		/// <returns>True if a word was found and replaced; otherwise false</returns>
		private static bool ReplaceCaretWord(Page page, XNamespace ns, string tag)
		{
			var selection = FindCaretSelection(page, ns);
			if (selection is null)
			{
				return false;
			}

			var (prev, next) = FindAdjoiningRuns(selection);
			if (prev is null && next is null)
			{
				return false;
			}

			var word = new StringBuilder();

			if (prev is not null)
			{
				word.Append(prev.ExtractLastWord());
				if (prev.GetCData().Value.Length == 0)
				{
					prev.Remove();
				}
			}

			if (next is not null)
			{
				word.Append(next.ExtractFirstWord());
				if (next.GetCData().Value.Length == 0)
				{
					next.Remove();
				}
			}

			if (word.Length == 0)
			{
				return false;
			}

			selection.DescendantNodes().OfType<XCData>()
				.First()
				.ReplaceWith(new XCData(tag));

			return true;
		}


		// finds the empty-CDATA T run marking the text cursor position
		private static XElement FindCaretSelection(Page page, XNamespace ns)
		{
			var selection = page.Root.Descendants(ns + "T")
				.FirstOrDefault(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

			if (selection is null)
			{
				return null;
			}

			var cdata = selection.GetCData();
			return cdata is not null && cdata.IsEmpty() ? selection : null;
		}


		// finds the sibling T runs adjoining the cursor that are part of the same word,
		// i.e. not separated from the cursor by whitespace
		private static (XElement prev, XElement next) FindAdjoiningRuns(XElement selection)
		{
			var prev = selection.PreviousNode as XElement;
			if (prev is not null && prev.GetCData().EndsWithWhitespace())
			{
				prev = null;
			}

			var next = selection.NextNode as XElement;
			if (next is not null && next.GetCData().StartsWithWhitespace())
			{
				next = null;
			}

			return (prev, next);
		}
	}
}
