//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class TaggedDialog : LocalizableForm
	{
		public enum Commands
		{
			Index,
			Copy,
			Move
		}

		private readonly string separator;
		private readonly OneNote one;
		private bool editing = false;


		public TaggedDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.TaggedDialog_Title;

				Localize(new string[]
				{
					"introLabel",
					"tagsLabel",
					"clearLabel",
					"checkAllLabel",
					"clearAllLabel",
					"indexButton",
					"moveButton",
					"copyButton",
					"cancelButton"
				});

				scopeBox.Items.Clear();
				scopeBox.Items.AddRange(Resx.TaggedDialog_scopeBox_Items.Split(new char[] { '\n' }));
			}

			filterBox.PressedEnter += Search;
			scopeBox.SelectedIndex = 0;

			separator = AddIn.Culture.TextInfo.ListSeparator;
			SelectedPages = new List<string>();

			// disposed in Dispose()
			one = new OneNote();
		}


		public Commands Command { get; private set; }


		public List<string> SelectedPages { get; private set; }


		private void ChangedFilter(object sender, EventArgs e)
		{
			var enabled = filterBox.Text.Trim().Length > 0;
			searchButton.Enabled = enabled;
			clearLabel.Enabled = enabled;

			if (enabled)
			{
				var tags = FormatFilter(filterBox.Text.ToLower())
					.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries)
					.Select(v => Regex.Replace(v.Trim(), @"^-", string.Empty))
					.ToList();

				editing = true;
				foreach (TagCheckBox tag in tagsFlow.Controls)
				{
					tag.Checked = tags.Contains(tag.Text);
				}
				editing = false;
			}
		}


		// async event handlers should be be declared 'async void'
		private async void ChangeScope(object sender, EventArgs e)
		{
			tagsFlow.Controls.Clear();

			OneNote.Scope scope = OneNote.Scope.Notebooks;
			switch (scopeBox.SelectedIndex)
			{
				case 1: scope = OneNote.Scope.Sections; break;
				case 2: scope = OneNote.Scope.Pages; break;
			}

			var tags = await TagHelpers.FetchRecentTags(scope, 30);

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


		private void ChangeTagSelection(object sender, EventArgs e)
		{
			if (editing)
			{
				return;
			}

			var box = sender as TagCheckBox;
			if (box.Checked)
			{
				if (filterBox.Text.Trim().Length == 0)
				{
					filterBox.Text = box.Text;
				}
				else
				{
					// check if user already type in this tag
					var tags = filterBox.Text.Split(
						new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();

					if (!tags.Any(t => t.Equals(box.Text, StringComparison.CurrentCultureIgnoreCase)))
					{
						filterBox.Text = $"{FormatFilter(filterBox.Text)}{separator} {box.Text}";
					}
				}

				clearLabel.Enabled = true;
			}
			else
			{
				var text = Regex.Replace(
					FormatFilter(filterBox.Text), $@"(?:\s|^)\-?{box.Text}(?:{separator}|$)", string.Empty);

				// removing entry at end of string will leave a comma at end of string
				filterBox.Text = text.EndsWith(separator)
					? text.Substring(0, text.Length - 1).Trim()
					: text.Trim();

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
			text = Regex.Replace(text, $@"\s+{separator}", ",");

			// clean up spaces after the negation operator
			text = Regex.Replace(text, @"\-\s+", "-");

			// clean up extra commas at start or end of string
			text = Regex.Replace(text, $@"(^\s?{separator}\s?)|(\s?{separator}\s?$)", string.Empty);

			return text;
		}


		private void ClearFilters(object sender, EventArgs e)
		{
			filterBox.Text = string.Empty;
			foreach (TagCheckBox tag in tagsFlow.Controls)
			{
				tag.Checked = false;
			}
		}


		// async event handlers should be be declared 'async void'
		private async void ClickNode(object sender, TreeNodeMouseClickEventArgs e)
		{
			// thanksfully, Bounds specifies bounds of label
			var node = e.Node as HierarchyNode;
			if (node.Hyperlinked && e.Node.Bounds.Contains(e.Location))
			{
				var pageId = node.Root.Attribute("ID").Value;
				if (!pageId.Equals(one.CurrentPageId))
				{
					await one.NavigateTo(pageId);
				}
			}
		}


		// async event handlers should be be declared 'async void'
		private async void Search(object sender, EventArgs e)
		{
			checkAllLabel.Enabled = false;
			clearAllLabel.Enabled = false;
			resultTree.Nodes.Clear();

			var text = filterBox.Text.ToLower();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			var filters = FormatFilter(filterBox.Text.ToLower())
				.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries)
				.Select(v => v.Trim())
				.ToList();

			var includedTags = filters.Where(f => f[0] != '-').ToList();
			var excludedTags = filters.Where(f => f[0] == '-').Select(f => f.Substring(1)).ToList();

			var scopeId = string.Empty;
			switch (scopeBox.SelectedIndex)
			{
				case 1: scopeId = one.CurrentNotebookId; break;
				case 2: scopeId = one.CurrentSectionId; break;
			}

			var results = await one.SearchMeta(scopeId, MetaNames.TaggingLabels);
			var ns = one.GetNamespace(results);

			// remove recyclebin nodes
			results.Descendants()
				.Where(n => n.Name.LocalName == "UnfiledNotes" ||
							n.Attribute("isRecycleBin") != null ||
							n.Attribute("isInRecycleBin") != null)
				.Remove();

			// filter
			var metas = results.Descendants(ns + "Meta")
				.Where(m => m.Attribute("name").Value == MetaNames.TaggingLabels);

			if (metas == null)
			{
				return;
			}

			// filter out unmatched pages, keep track in separate list because metas can't be
			// modified while enumerating
			var dead = new List<XElement>();
			foreach (var meta in metas)
			{
				var tags = meta.Attribute("content").Value.ToLower()
					.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries)
					.Select(v => v.Trim())
					.ToList();

				if (tags.Count > 0)
				{
					if ((excludedTags.Count > 0 && tags.Any(t => excludedTags.Contains(t))) ||
						(includedTags.Count > 0 && !tags.Any(t => includedTags.Contains(t))))
					{
						dead.Add(meta.Parent);
					}
				}
			}

			// remove unmatched pages
			dead.ForEach(d => d.Remove());

			// remove empty leaf nodes
			var pruning = true;
			while (pruning)
			{
				var elements = results.Descendants()
					.Where(d => d.Name.LocalName != "Meta" && !d.HasElements);

				pruning = elements.Any();
				if (pruning)
				{
					elements.Remove();
				}
			}

			if (results.HasElements)
			{
				resultTree.Populate(results, one.GetNamespace(results));

				checkAllLabel.Enabled = true;
				clearAllLabel.Enabled = true;
			}
		}


		private void TreeAfterCheck(object sender, TreeViewEventArgs e)
		{
			var node = e.Node as HierarchyNode;
			var id = node.Root.Attribute("ID").Value;

			if (node.Checked)
			{
				if (!SelectedPages.Contains(id))
				{
					SelectedPages.Add(id);
				}
			}
			else if (SelectedPages.Contains(id))
			{
				SelectedPages.Remove(id);
			}

			indexButton.Enabled = copyButton.Enabled = moveButton.Enabled = SelectedPages.Count > 0;
		}


		private void ToggleChecks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ToggleChecks(resultTree.Nodes, sender == checkAllLabel);
		}


		private void ToggleChecks(TreeNodeCollection nodes, bool check)
		{
			foreach (HierarchyNode node in nodes)
			{
				if (node.ShowCheckBox)
				{
					node.Checked = check;
				}

				if (node.Nodes?.Count > 0)
				{
					ToggleChecks(node.Nodes, check);
				}
			}
		}


		private void IndexPressed(object sender, EventArgs e)
		{
			Command = Commands.Index;
			DialogResult = DialogResult.OK;
			Close();
		}


		private void CopyPressed(object sender, EventArgs e)
		{
			Command = Commands.Copy;
			DialogResult = DialogResult.OK;
			Close();
		}


		private void MovePressed(object sender, EventArgs e)
		{
			Command = Commands.Move;
			DialogResult = DialogResult.OK;
			Close();
		}


		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
