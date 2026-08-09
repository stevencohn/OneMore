//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Extensions;
	using River.OneMoreAddIn.Models;
	using System.Diagnostics;
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
		private static System.IDisposable pauseHandle;


		public CompleteHashtagCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			logger.Verbose(
				$"CompleteHashtagCommand.Execute: commandIsActive={commandIsActive}, " +
				$"dialog={(dialog is null ? "null" : $"non-null,IsDisposed={dialog.IsDisposed}")}");

			if (dialog != null)
			{
				// Single instance. This must be checked first, independent of
				// commandIsActive below: RunModeless can block synchronously for the
				// dialog's ENTIRE lifetime (observed via logging - when
				// Application.MessageLoop is false at the point RunModeless is called,
				// it starts its own nested message loop and doesn't return until the
				// dialog closes). If commandIsActive were still guarding at that point,
				// every repeated Alt+G pressed while the popup is open would bail out
				// below instead of ever reaching this elevate.
				logger.Verbose("CompleteHashtagCommand.Execute: elevating existing dialog");
				dialog.Elevate();
				return;
			}

			if (commandIsActive)
			{
				// another invocation is concurrently in the brief setup below
				logger.Verbose("CompleteHashtagCommand.Execute: commandIsActive, bailing out");
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

				// signal the background HashtagScanner to back off (longer inter-page
				// throttle) while we need uncontended SQLite access, same as HashtagCommand
				// does for HashtagDialog; the scanner writes to the same database these
				// reads hit, and can otherwise hold it busy long enough to be felt as a
				// multi-second hang here
				pauseHandle = HashtagServicePause.Hold();

				var watch = Stopwatch.StartNew();
				using var provider = new HashtagProvider();
				var names = provider.ReadTagNames().ToArray();
				var recent = provider.ReadLatestTagNames().ToArray();
				logger.Verbose($"CompleteHashtagCommand.Execute: read {names.Length} tag names, " +
					$"{recent.Length} recent, in {watch.ElapsedMilliseconds}ms");

				dialog = new CompleteHashtagDialog(word, names, recent);
				dialog.FormClosed += Dialog_FormClosed;
				logger.Verbose($"CompleteHashtagCommand.Execute: created new dialog, seed=[{word}]");

				var anchor = CaretLocator.Locate(windowHandle);
				var location = Screen.FromPoint(anchor.Location)
					.GetBoundedLocationNear(anchor, dialog.Size);

				logger.Verbose($"CompleteHashtagCommand.Execute: showing dialog at {location}, " +
					$"Application.MessageLoop={Application.MessageLoop}");

				// setup is done; release the guard now, BEFORE calling RunModeless, since
				// that call may not return until the dialog closes (see comment above) -
				// dialog is already non-null by this point, so a repeated Alt+G from here
				// on correctly reaches the elevate branch above instead of bailing out here
				commandIsActive = false;

				// CommandFactory.RunCore runs every command's Execute() inside Task.Run,
				// i.e. on a throwaway threadpool thread with no message loop of its own.
				// Calling RunModeless directly from there forces it into a slow, blocking
				// nested Application.Run for every single show. Marshaling onto
				// HotkeyManager's own persistent message-pump thread instead lets
				// RunModeless see an already-running loop and take its lightweight,
				// non-blocking Show() path - this call returns as soon as Show() does, it
				// does not wait for the dialog to close.
				HotkeyManager.InvokeOnMessageThread(() =>
				{
					logger.Verbose("CompleteHashtagCommand.Execute: on message thread, " +
						$"Application.MessageLoop={Application.MessageLoop}");

				dialog.RunModeless(location, async (sender, e) =>
				{
						logger.Verbose("CompleteHashtagCommand: RunModeless closedAction invoked, " +
							$"DialogResult={(sender as CompleteHashtagDialog)?.DialogResult}");

					var d = sender as CompleteHashtagDialog;
					if (d.DialogResult == DialogResult.OK)
					{
							try
							{
						await ApplyTag(d.SelectedTag);
					}
							catch (System.Exception exc)
							{
								// this callback runs detached from Execute()'s call stack, after
								// the dialog has already closed, so an unhandled exception here
								// would otherwise vanish silently instead of surfacing anywhere
								logger.WriteLine("CompleteHashtagCommand: error applying tag", exc);
							}
						}
				});
				});
			}
			finally
			{
				// redundant in the success path (already reset above); still needed to
				// clear the guard if something threw during setup, before that reset ran
				commandIsActive = false;
				logger.Verbose("CompleteHashtagCommand.Execute: commandIsActive reset to false");

				if (dialog is null)
				{
					// setup failed before the dialog could be created/shown, so
					// Dialog_FormClosed will never fire to release this
					pauseHandle?.Dispose();
					pauseHandle = null;
				}
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
			logger.Verbose($"CompleteHashtagCommand.Dialog_FormClosed: fired, " +
				$"sender==dialog={ReferenceEquals(sender, dialog)}, dialog is null={dialog is null}");

			if (dialog != null)
			{
				dialog.FormClosed -= Dialog_FormClosed;
				dialog.Dispose();
				dialog = null;
				pauseHandle?.Dispose();
				pauseHandle = null;
				logger.Verbose("CompleteHashtagCommand.Dialog_FormClosed: dialog cleared");
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

				// '#' isn't a \w character so ExtractLastWord leaves a leading '#' behind
				// uneaten; consume it too or the replacement ends up doubled, e.g.
				// "#onemore" -> extracts "onemore", leaves "#" -> "##another"
				var cdata = prev.GetCData();
				var trimmed = cdata.Value.TrimEnd('#');
				if (trimmed.Length < cdata.Value.Length)
				{
					word.Insert(0, '#');
					cdata.Value = trimmed;
				}

				if (prev.GetCData().Value.Length == 0)
				{
					prev.Remove();
				}
			}

			if (next is not null)
			{
				word.Append(next.ExtractFirstWord());

				var cdata = next.GetCData();
				var trimmed = cdata.Value.TrimStart('#');
				if (trimmed.Length < cdata.Value.Length)
				{
					word.Append('#');
					cdata.Value = trimmed;
				}

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
