//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


	internal partial class NotebooksDialog : MoreForm
	{
		private readonly MoreCheckList booksList;
		private readonly bool forIndex;


		/// <summary>
		/// Initialize a dialog that chooses the notebooks scanned for hashtags
		/// </summary>
		public NotebooksDialog()
			: this(false)
		{
		}


		/// <summary>
		/// Initialize a dialog that chooses notebooks
		/// </summary>
		/// <param name="forIndex">True to choose the notebooks that are in the text index; false
		/// to choose the notebooks that are scanned for hashtags</param>
		public NotebooksDialog(bool forIndex)
		{
			InitializeComponent();

			this.forIndex = forIndex;

			if (NeedsLocalizing())
			{
				Text = Resx.NotebooksDialog_Title;

				Localize(new string[]
				{
					"infoLabel=NotebooksDialog_infoLabel",
					"notebooksLabel=word_Notebooks",
					"selectAllLink=phrase_SelectAll",
					"selectNoneLink=phrase_SelectNone",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			if (forIndex)
			{
				Text = Resx.IndexNotebooksDialog_Title;
				infoLabel.Text = Resx.IndexNotebooksDialog_infoLabel;
			}

			booksList = new MoreCheckList
			{
				Dock = DockStyle.Fill,
				HeaderStyle = ColumnHeaderStyle.None,
				MultiSelect = false,
				SelectedBackColorKey = "LinkHighlight",
				SelectedForeColorKey = "ControlText"
			};

			booksList.Columns.Add(string.Empty);
			booksList.SetColumnProportions(1f);
			booksPanel.Controls.Add(booksList);
		}


		protected override async void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AlignHyperlinks();

			if (forIndex)
			{
				await LoadIndexNotebooks();
				FocusButtons();
				return;
			}

			// load scanned notebooks from DB for inclusion state and scanned indicator
			HashtagNotebooks knownNotebooks = null;
			if (HashtagProvider.CatalogExists())
			{
				using var provider = new HashtagProvider();
				knownNotebooks = provider.ReadKnownNotebooks();
			}

			await using var one = new OneNote();
			var books = await one.GetNotebooks();
			var ns = one.GetNamespace(books);

			booksList.BeginUpdate();
			foreach (var book in books.Elements(ns + "Notebook"))
			{
				var id = book.Attribute("ID").Value;
				var name = book.Attribute("name").Value;

				var known = knownNotebooks?.FirstOrDefault(n => n.NotebookID == id);
				var isIncluded = known?.Included ?? true;
				var displayName = known != null
					? $"{name} {Resx.NotebooksDialog_scanned}"
					: name;

				booksList.Items.Add(new ListViewItem(displayName)
				{
					Tag = (id, name),
					Checked = isIncluded
				});
			}
			booksList.EndUpdate();

			FocusButtons();
		}


		// lists the open notebooks with the choice made for the text index; a notebook that has
		// not been chosen is not indexed. The index knows a notebook by its path or name, as the
		// identity catalog does, not by the ID OneNote gives it, which changes when it is reopened
		private async System.Threading.Tasks.Task LoadIndexNotebooks()
		{
			var choices = new System.Collections.Generic.Dictionary<string, bool>();
			if (TextIndex.IndexProvider.IsAvailable && TextIndex.IndexProvider.CatalogExists())
			{
				using var index = new TextIndex.IndexProvider();
				foreach (var notebook in index.ReadNotebooks())
				{
					choices[notebook.NotebookKey] = notebook.Included;
				}
			}

			await using var one = new OneNote();
			var books = await one.GetNotebooks();
			var ns = one.GetNamespace(books);

			booksList.BeginUpdate();
			foreach (var book in books.Elements(ns + "Notebook"))
			{
				var name = book.Attribute("name").Value;
				var path = book.Attribute("path")?.Value;
				var key = Identity.PageIdentityKeys.NotebookKey(path, name);

				booksList.Items.Add(new ListViewItem(name)
				{
					Tag = (key, name),
					Checked = choices.TryGetValue(key, out var included) && included
				});
			}
			booksList.EndUpdate();
		}


		private void AlignHyperlinks()
		{
			var noneSize = TextRenderer.MeasureText(selectNoneLink.Text, selectNoneLink.Font);
			selectNoneLink.Left = notebooksPanel.Width - notebooksPanel.Padding.Right - noneSize.Width;

			var sep1Size = TextRenderer.MeasureText(sep1.Text, sep1.Font);
			sep1.Left = selectNoneLink.Left - selectNoneLink.Margin.Left - sep1Size.Width - sep1.Margin.Right;

			var allSize = TextRenderer.MeasureText(selectAllLink.Text, selectAllLink.Font);
			selectAllLink.Left = sep1.Left - sep1.Margin.Left - allSize.Width - selectAllLink.Margin.Right;
		}


		private void FocusButtons()
		{
			AcceptButton = okButton;
			okButton.Focus();
		}


		private void SelectAllNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (ListViewItem item in booksList.Items)
			{
				item.Checked = true;
			}
			booksList.Invalidate();
		}


		private void SelectNoneNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (ListViewItem item in booksList.Items)
			{
				item.Checked = false;
			}
			booksList.Invalidate();
		}


		private void SaveInclusion(object sender, EventArgs e)
		{
			if (forIndex)
			{
				using var index = new TextIndex.IndexProvider();
				var chosen = new System.Collections.Generic.HashSet<string>();

				foreach (ListViewItem item in booksList.Items)
				{
					var (key, _) = ((string, string))item.Tag;
					index.WriteNotebook(key, item.Checked);

					if (item.Checked)
					{
						chosen.Add(key);
					}
				}

				// a notebook that is no longer chosen keeps nothing in the index; this dialog is
				// only offered when the user has not chosen every notebook
				index.PruneNotebooks(chosen.Contains);

				DialogResult = DialogResult.OK;
				Close();
				return;
			}

			using var provider = new HashtagProvider();
			foreach (ListViewItem item in booksList.Items)
			{
				var (id, name) = ((string, string))item.Tag;
				provider.WriteNotebookInclusion(id, name, item.Checked);
			}

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
