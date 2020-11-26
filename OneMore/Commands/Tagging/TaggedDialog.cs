//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal partial class TaggedDialog : LocalizableForm
	{
		private readonly string separator;
		private readonly OneNote one;


		public TaggedDialog()
		{
			InitializeComponent();

			filterBox.PressedEnter += Search;
			scopeBox.SelectedIndex = 0;

			separator = AddIn.Culture.TextInfo.ListSeparator;

			// dispose in Dispose()
			one = new OneNote();
		}


		private void ChangeScope(object sender, EventArgs e)
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


		private void ChangeTagSelection(object sender, EventArgs e)
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


		private void ClickNode(object sender, TreeNodeMouseClickEventArgs e)
		{
			// thanksfully, Bounds specifies bounds of label
			var node = e.Node as HierarchyNode;
			if (node.Hyperlinked && e.Node.Bounds.Contains(e.Location))
			{
				var pageId = node.Root.Attribute("ID").Value;
				if (!pageId.Equals(one.CurrentPageId))
				{
					one.NavigateTo(pageId);
				}
			}
		}


		private void Search(object sender, EventArgs e)
		{
			resultTree.Nodes.Clear();

			var tags = FormatFilter(filterBox.Text)
				.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();

			var negtags = tags.Where(t => t[0] == '-').ToList();
			tags = tags.Except(negtags).ToList();

			var scopeId = string.Empty;
			switch (scopeBox.SelectedIndex)
			{
				case 1: scopeId = one.CurrentNotebookId; break;
				case 2: scopeId = one.CurrentSectionId; break;
			}

			var results = one.SearchMeta(scopeId, Page.TaggingMetaName);

			// remove recyclebin nodes
			results.Descendants()
				.Where(n => n.Name.LocalName == "UnfiledNotes" ||
							n.Attribute("isRecycleBin") != null ||
							n.Attribute("isInRecycleBin") != null)
				.Remove();

			if (results.HasElements)
			{
				//resultTree.BeginUpdate();
				DisplayResults(results, one.GetNamespace(results), resultTree.Nodes);
				if (resultTree.Nodes.Count > 0)
				{
					resultTree.ExpandAll();
					resultTree.Nodes[0].EnsureVisible();
				}
				//resultTree.EndUpdate();
			}
		}


		private void DisplayResults(XElement root, XNamespace ns, TreeNodeCollection nodes)
		{
			TreeNode node;

			if (root.Name.LocalName == "Page")
			{
				node = new HierarchyNode(root.Attribute("name").Value, root)
				{
					Hyperlinked = true
				};

				nodes.Add(node);
				return;
			}

			if (root.Name.LocalName == "Notebooks")
			{
				foreach (var element in root.Elements())
				{
					DisplayResults(element, ns, nodes);
				}
				return;
			}

			node = new HierarchyNode(root.Attribute("name")?.Value, root);
			nodes.Add(node);

			foreach (var element in root.Elements())
			{
				DisplayResults(element, ns, node.Nodes);
			}
		}
		/*
		<one:Notebooks xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote">
			<one:Notebook name="Personal" nickname="Personal" ID="{CAE56365-6026-4E6C-A313-667D6FEBE5D8}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/" lastModifiedTime="2020-11-25T18:12:56.000Z" color="#F6B078">
			<one:Section name="OneMore" ID="{B49560C5-7EEF-41CB-9B62-A1999A367EC1}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/OneMore.one" lastModifiedTime="2020-11-25T18:12:56.000Z" color="#B49EDE">
				<one:Page ID="{B49560C5-7EEF-41CB-9B62-A1999A367EC1}{1}{E1949357495378378527691939956701555580463441}" name="OneNoteTaggingKit" dateTime="2020-11-03T22:18:37.000Z" lastModifiedTime="2020-11-25T18:12:56.000Z" pageLevel="2">
				<one:Meta name="omTaggingLabels" content="OneNoteTaggingKit" />
				<one:Meta name="" content="" />
				</one:Page>
			</one:Section>
			</one:Notebook>
			<one:Notebook name="Flux" nickname="Flux" ID="{853348FB-82D9-44E4-93FE-60EBFBE72E9C}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Flux/" lastModifiedTime="2020-11-25T18:13:39.000Z" color="#F5F96F" isCurrentlyViewed="true">
			<one:Section name="King" ID="{D20A5AAE-3E10-08C5-0E4F-6806D7C9C11A}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Flux/King.one" lastModifiedTime="2020-11-25T18:13:34.000Z" color="#8AA8E4" isCurrentlyViewed="true">
				<one:Page ID="{D20A5AAE-3E10-08C5-0E4F-6806D7C9C11A}{1}{E1811519588813354923720177303100143448474111}" name="OTK at Title" dateTime="2020-03-03T23:35:47.000Z" lastModifiedTime="2020-11-25T18:13:28.000Z" pageLevel="1">
				<one:Meta name="omTaggingLabels" content="cheese,apples,cream" />
				<one:Meta name="TaggingKit.PageTags" content="#Cheese, #cheese, #Grape, #grape, #Orange, #orange, -âœ©-" />
				</one:Page>
				<one:Page ID="{D20A5AAE-3E10-08C5-0E4F-6806D7C9C11A}{1}{E1823918871045154751520113206231786064788801}" name="Splitter" dateTime="2020-03-03T23:36:27.000Z" lastModifiedTime="2020-11-25T14:52:58.000Z" pageLevel="1" isCurrentlyViewed="true">
				<one:Meta name="TaggingKit.PageTags" content="" />
				<one:Meta name="omLabels" content="fish;orange;" />
				<one:Meta name="omTaggingLabels" content="Wonder Woman, Apples, orange" />
				<one:Meta name="omHighlightIndex" content="1" />
				</one:Page>
			</one:Section>
			</one:Notebook>
		</one:Notebooks>
		*/

		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
