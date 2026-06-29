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


		public NotebooksDialog()
		{
			InitializeComponent();

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
