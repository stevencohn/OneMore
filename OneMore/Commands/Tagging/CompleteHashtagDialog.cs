//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
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
				WordChars = new[] { '#' }
			};

			palette.SetAutoCompleteList(tagBox);
			palette.LoadCommands(names, recentNames);

			tagBox.Text = string.IsNullOrEmpty(word) ? "#" : $"#{word}";
			tagBox.SelectionStart = tagBox.Text.Length;

			ElevatedWithOneNote = true;
		}


		/// <summary>
		/// Gets the hashtag chosen or typed by the user, always prefixed with '#'.
		/// Only meaningful when DialogResult is OK.
		/// </summary>
		public string SelectedTag { get; private set; }


		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				if (palette.IsPopupVisible)
				{
					// let MoreAutoCompleteList's own KeyDown handle hiding the popup first;
					// a second Escape, once hidden, will close this dialog
					return;
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
				Close();
				return;
			}

			SelectedTag = AddHashtagCommand.NormalizeTags(text);

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
