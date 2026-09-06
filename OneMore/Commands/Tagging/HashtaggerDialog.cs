//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class HashtaggerDialog : MoreForm
	{
		private static readonly char[] TagSeparators = { ' ', '\t', '\r', '\n', ',' };

		private readonly Page page;
		private readonly MoreAutoCompleteList palette;
		private List<PageReader.CountedWord> commonWords;


		public HashtaggerDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Hashtags;

				Localize(new string[]
				{
					"tagsLabel",
					"bankBox",
					"findLabel",
					"commonWordsButton",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			palette = new MoreAutoCompleteList
			{
				FreeText = true,
				WordChars = new[] { '#' }
			};

			palette.SetAutoCompleteList(findBox);

			findBox.PreviewKeyDown += (sender, e) =>
			{
				if (e.KeyCode == Keys.Enter ||
					(e.KeyCode == Keys.Escape && palette.IsPopupVisible))
				{
					e.IsInputKey = true;
				}
			};

			findBox.KeyDown += DoFindBoxKeyDown;

			DefaultControl = tagsBox;
		}


		public HashtaggerDialog(Page page)
			: this()
		{
			this.page = page;
		}


		/// <summary>
		/// Gets a value indicating that tags should be added to the page tag bank.
		/// Default is to add at the insertion point.
		/// </summary>
		public bool AddToBank => bankBox.Checked;


		/// <summary>
		/// Gets the string containing the selected tags, normalized so tags entered in
		/// tagsBox separated by spaces, commas, or both are each prefaced with at least
		/// one '#' and separated by single spaces.
		/// </summary>
		public string Tags => string.Join(" ",
			tagsBox.Text
				.Split(TagSeparators, StringSplitOptions.RemoveEmptyEntries)
				.Select(t => t[0] == '#' ? t : $"#{t}"));


		private void LoadTagsOnLoad(object sender, EventArgs e)
		{
			commonWords = new PageReader(page).ReadCommonWords().ToList();
			PopulateFindPalette();
			RefreshCommonWordsAvailability();
		}


		private void PopulateFindPalette()
		{
			var existing = ExtractHashtags(tagsBox.Text);

			var provider = new HashtagProvider();
			var names = provider.ReadTagNames().Where(t => !existing.Contains(t)).ToArray();
			var recent = provider.ReadLatestTagNames().Where(t => !existing.Contains(t)).ToArray();

			palette.LoadCommands(names, recent);
		}


		private static HashSet<string> ExtractHashtags(string text)
		{
			return new HashSet<string>(
				Regex.Matches(text, @"#\w+").Cast<Match>().Select(m => m.Value),
				StringComparer.OrdinalIgnoreCase);
		}


		private void DoFindBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				AppendFindTag();
			}

			// Escape: MoreAutoCompleteList's own KeyDown handler, subscribed before ours via
			// SetAutoCompleteList, already hid the popup and set e.Handled when a popup was open
		}


		private void AppendFindTag()
		{
			var text = findBox.Text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				AppendTag(text);
			}

			findBox.Clear();
			findBox.Focus();
		}


		private void AppendTag(string text)
		{
			// add # to a common word or bare tag name
			if (text[0] != '#')
			{
				text = $"#{text}";
			}

			if (!ExtractHashtags(tagsBox.Text).Contains(text))
			{
				tagsBox.Text = string.IsNullOrWhiteSpace(tagsBox.Text)
					? text
					: $"{tagsBox.Text} {text}";
			}
		}


		private void SuppressTagsBoxEnter(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}


		private void DoTagsBoxChanged(object sender, EventArgs e)
		{
			okButton.Enabled = !string.IsNullOrWhiteSpace(tagsBox.Text);
			RefreshCommonWordsAvailability();
			PopulateFindPalette();
		}


		private void RefreshCommonWordsAvailability()
		{
			var existing = ExtractHashtags(tagsBox.Text);
			commonWordsButton.Enabled = commonWords.Any(w => !existing.Contains($"#{w.Word}"));
		}


		private void ShowCommonWordsMenu(object sender, EventArgs e)
		{
			// populate before Show() so the menu's size is already finalized when it is
			// displayed; populating in the Opening event instead requires a second click
			// before the drop-down actually renders
			PopulateCommonWordsMenu();

			commonWordsMenu.Show(commonWordsButton, new Point(
				-(commonWordsMenu.Width - commonWordsButton.Width),
				commonWordsButton.Height));
		}


		private void PopulateCommonWordsMenu()
		{
			commonWordsMenu.Items.Clear();

			var existing = ExtractHashtags(tagsBox.Text);
			foreach (var word in commonWords.Where(w => !existing.Contains($"#{w.Word}")))
			{
				var item = new MoreMenuItem($"#{word.Word} ({word.Count})");
				item.Click += (s, ev) => AppendTag(word.Word);
				commonWordsMenu.Items.Add(item);
			}
		}
	}
}
