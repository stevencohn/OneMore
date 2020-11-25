//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Dialogs;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;


	internal partial class TaggedDialog : LocalizableForm
	{
		public TaggedDialog()
		{
			InitializeComponent();

			filterBox.PressedEnter += AcceptInput;
			scopeBox.SelectedIndex = 0;
		}


		private void ChangeScope(object sender, System.EventArgs e)
		{
			tagsFlow.Controls.Clear();

			OneNote.Scope scope = OneNote.Scope.Notebooks;
			switch (scopeBox.SelectedIndex)
			{
				case 1: scope = OneNote.Scope.Sections; break;
				case 2: scope = OneNote.Scope.Pages; break;
			}

			var tags = TagHelpers.FetchRecentTags(scope, 30);

			if (tags.Count > 0)
			{
				var sorted = tags.OrderBy(k => k.Key.StartsWith("#") ? k.Key.Substring(1) : k.Key);

				foreach (var s in sorted)
				{
					var tag = new TagCheckBox(s.Value);
					tag.CheckedChanged += ChangeTagSelection;
					tagsFlow.Controls.Add(tag);
				}
			}
		}


		private void ChangeTagSelection(object sender, System.EventArgs e)
		{
			var box = sender as TagCheckBox;
			if (box.Checked)
			{
				if (filterBox.Text.Trim().Length == 0)
				{
					filterBox.Text = box.Text;
				}
				else
				{
					filterBox.Text = $"{FormatFilter(filterBox.Text)}, {box.Text}";
				}
				clearLabel.Enabled = true;
			}
			else
			{
				var text = Regex.Replace(
					FormatFilter(filterBox.Text), $@"(?:\s|^)\-?{box.Text}(?:,|$)", string.Empty);

				// removing entry at end of string will leave a comma at end of string
				filterBox.Text = text.EndsWith(",")
					? text.Substring(0, text.Length - 1)
					: text;

				var count = 0;
				foreach (TagCheckBox tag in tagsFlow.Controls)
				{
					if (tag.Checked)
					{
						count++;
						break;
					}
				}

				if (count == 0)
				{
					clearLabel.Enabled = false;
				}
			}
		}


		private string FormatFilter(string filter)
		{
			// collapse multiple spaces to single space
			var text = Regex.Replace(filter.Trim(), @"[ ]{2,}", " ");

			// clean up spaces preceding commas
			text = Regex.Replace(text, @"\s+,", ",");

			// clean up spaces after the negation operator
			text = Regex.Replace(text, @"\-\s+", "-");

			// clean up extra commas at start or end of string
			text = Regex.Replace(text, @"(^\s?,\s?)|(\s?,\s?$)", string.Empty);

			return text;
		}


		private void ClearFilters(object sender, System.EventArgs e)
		{
			filterBox.Text = string.Empty;
			foreach (TagCheckBox tag in tagsFlow.Controls)
			{
				tag.Checked = false;
			}
		}


		private void AcceptInput(object sender, System.EventArgs e)
		{
			var tags = FormatFilter(filterBox.Text)
				.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

			var negtags = tags.Where(t => t[0] == '-').ToList();
			tags = tags.Except(negtags).ToList();
		}


		private void Cancel(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
