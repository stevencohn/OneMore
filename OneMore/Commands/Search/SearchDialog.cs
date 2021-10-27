//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Search
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SearchDialog : LocalizableForm
	{
		private readonly OneNote one;


		public SearchDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchDialog_Title;

				Localize(new string[]
				{
					"introLabel",
					"findLabel",
					"moveButton",
					"copyButton",
					"cancelButton=word_Cancel"
				});

				scopeBox.Items.Clear();
				scopeBox.Items.AddRange(Resx.SearchDialog_scopeBox_Items.Split(new char[] { '\n' }));
			}

			scopeBox.SelectedIndex = 0;
			SelectedPages = new List<string>();

			one = new OneNote();
		}


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }


		private void ChangeQuery(object sender, EventArgs e)
		{
			searchButton.Enabled = findBox.Text.Trim().Length > 0;
		}


		private void SearchOnKeydown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter &&
				findBox.Text.Trim().Length > 0)
			{
				Search(sender, e);
			}
		}


		private void Search(object sender, EventArgs e)
		{
			resultTree.Nodes.Clear();

			string startId = string.Empty;
			switch (scopeBox.SelectedIndex)
			{
				case 1: startId = one.CurrentNotebookId; break;
				case 2: startId = one.CurrentSectionId; break;
			}

			var results = one.Search(startId, findBox.Text);

			if (results.HasElements)
			{
				resultTree.Populate(results, one.GetNamespace(results));
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

			copyButton.Enabled = moveButton.Enabled = SelectedPages.Count > 0;
		}


		private void CopyPressed(object sender, EventArgs e)
		{
			CopySelections = true;
			DialogResult = DialogResult.OK;
			Close();
		}


		private void MovePressed(object sender, EventArgs e)
		{
			CopySelections = false;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void Nevermind(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
