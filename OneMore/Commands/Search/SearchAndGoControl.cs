//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class SearchAndGoControl : MoreUserControl
	{
		private readonly OneNote one;


		public SearchAndGoControl()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introLabel",
					"findLabel",
					"moveButton=word_Move",
					"copyButton=word_Copy",
					"cancelButton=word_Cancel"
				});

				scopeBox.Items.Clear();
				scopeBox.Items.AddRange(Resx.phrase_scopeOptions.Split('\n'));
			}

			scopeBox.SelectedIndex = 0;
			SelectedPages = new List<string>();

			one = new OneNote();
		}


		public event EventHandler<SearchCloseEventArgs> SearchClosing;


		/// <summary>
		/// Localizes each of the named controls by looking up the appropriate resource string
		/// </summary>
		/// <param name="keys">An collection of strings specifying the control names</param>
		protected void Localize(string[] keys)
		{
			Translator.Localize(this, keys);
		}


		/// <summary>
		/// Determines if the sheet needs to be localized
		/// </summary>
		/// <returns>True if the sheets needs to be localized; language is not default 'en'</returns>
		protected static bool NeedsLocalizing()
		{
			return AddIn.Culture.TwoLetterISOLanguageName != "en";
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
			SearchClosing?.Invoke(this, new(DialogResult.OK));
		}


		private void MovePressed(object sender, EventArgs e)
		{
			CopySelections = false;
			SearchClosing?.Invoke(this, new(DialogResult.OK));
		}

		private void Nevermind(object sender, EventArgs e)
		{
			SearchClosing?.Invoke(this, new(DialogResult.Cancel));
		}
	}
}
