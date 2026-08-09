//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Windows.Forms;


	/// <summary>
	/// A lightweight frameless popup, anchored near the current word, that lets the user
	/// pick or refine a hashtag from an autocomplete list to replace that word with.
	/// </summary>
	internal partial class CompleteHashtagDialog : MoreForm
	{
		private readonly MoreAutoCompleteList palette;


		/// <summary>
		/// Initialize a new dialog, seeded with the given word (without a leading '#')
		/// and populated with the given known/recent hashtag names (with a leading '#').
		/// </summary>
		public CompleteHashtagDialog(
			string word, IEnumerable<string> names, IEnumerable<string> recentNames)
		{
			InitializeComponent();

			DefaultControl = tagBox;

			palette = new MoreAutoCompleteList
			{
				FreeText = true,
				WordChars = new[] { '#' },
				ShowPopupOnStartup = true
			};

			palette.SetAutoCompleteList(tagBox);
			palette.LoadCommands(names, recentNames);

			tagBox.Text = string.IsNullOrEmpty(word) ? "#" : $"#{word}";
			tagBox.SelectionStart = tagBox.Text.Length;

			// deliberately NOT setting ElevatedWithOneNote here: that mechanism registers a
			// process-wide Automation.AddAutomationFocusChangedEventHandler to re-elevate a
			// non-topmost form whenever OneNote regains focus, and both that call and its
			// OnFormClosed-time Remove counterpart can take multiple seconds against
			// OneNote's automation tree. Since OnActivated below forces TopMost = true
			// unconditionally, this window already stays above OneNote in z-order
			// regardless of activation, making that (slow) mechanism unnecessary here - and
			// because RunModeless now runs on HotkeyManager's own thread, that multi-second
			// stall would otherwise block the whole thread's message loop, including the
			// next Alt+G's WM_HOTKEY, until it cleared.
			logger.Verbose($"CompleteHashtagDialog: constructed, hash={GetHashCode()}");
		}


		/// <summary>
		/// Gets the hashtag chosen or typed by the user, always prefixed with '#'.
		/// Only meaningful when DialogResult is OK.
		/// </summary>
		public string SelectedTag { get; private set; }


		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: OnActivated");

			// MoreForm.OnActivated calls Elevate(false), which leaves TopMost off; undo that
			// here so this popup can't disappear behind OneNote if the user clicks back into
			// it, which otherwise makes it easy to forget it's still open
			TopMost = true;
		}


		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);

			logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: OnDeactivate");

			// WM_ACTIVATE(WA_INACTIVE) is delivered to the losing window only after Windows
			// has already updated the foreground window, so this can run synchronously here
			// without racing GetForegroundWindow(); deferring via BeginInvoke instead queued
			// an extra message on the shared hotkey-thread message loop for no benefit
			CloseIfFocusLeftOneNote();
		}


		private void CloseIfFocusLeftOneNote()
		{
			if (IsDisposed)
			{
				logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: " +
					"CloseIfFocusLeftOneNote, already disposed, skipping");
				return;
			}

			var foreground = Native.GetForegroundWindow();
			if (foreground == IntPtr.Zero || foreground == Handle)
			{
				logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: " +
					$"CloseIfFocusLeftOneNote, foreground is self or zero, staying open");
				return;
			}

			Native.GetWindowThreadProcessId(foreground, out var pid);

			// staying within OneNote (elevated back to front) or within this own process
			// (e.g. the autocomplete drop-down, itself a separate top-level window) is fine;
			// only a genuinely different application, or the desktop, should dismiss this
			if (pid == (uint)Process.GetCurrentProcess().Id)
			{
				logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: " +
					$"CloseIfFocusLeftOneNote, foreground pid={pid} is this process, staying open");
				return;
			}

			try
			{
				var name = Process.GetProcessById((int)pid).ProcessName;
				if (name == "ONENOTE")
				{
					logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: " +
						"CloseIfFocusLeftOneNote, foreground is ONENOTE, staying open");
					return;
				}

				logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: " +
					$"CloseIfFocusLeftOneNote, foreground pid={pid} process={name}, closing");
			}
			catch (ArgumentException)
			{
				// process already exited; treat as "elsewhere" and fall through to close
				logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: " +
					$"CloseIfFocusLeftOneNote, foreground pid={pid} process exited, closing");
			}

			// focus moved to a different application (or the desktop) entirely; dismiss
			// this popup just as if the user had pressed Esc
			Close();
		}


		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				// unlike HashtagDialog, there's no other content here worth keeping open
				// after dismissing the autocomplete list, so a single Esc dismisses both
				logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: Escape, closing");

				if (palette.IsPopupVisible)
				{
					palette.HidePopup(this, EventArgs.Empty);
				}

				e.Handled = true;
				Close();
			}
			else if (e.KeyCode == Keys.Enter)
			{
				// MoreAutoCompleteList's PreviewKeyDown, which runs before this Form-level
				// KeyDown, already copied any selected autocomplete item into tagBox.Text
				e.Handled = true;
				Accept();
			}
		}


		private void Accept()
		{
			if (palette.IsPopupVisible)
			{
				palette.HidePopup(this, EventArgs.Empty);
			}

			var text = tagBox.Text.Trim();
			if (string.IsNullOrEmpty(text) || text == "#")
			{
				logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: Accept, empty text, closing");
				Close();
				return;
			}

			SelectedTag = AddHashtagCommand.NormalizeTags(text);
			logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: Accept, tag=[{SelectedTag}]");

			DialogResult = DialogResult.OK;
			Close();
		}


		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			logger.Verbose($"CompleteHashtagDialog[{GetHashCode()}]: OnFormClosed, " +
				$"DialogResult={DialogResult}");

			base.OnFormClosed(e);
		}
	}
}
