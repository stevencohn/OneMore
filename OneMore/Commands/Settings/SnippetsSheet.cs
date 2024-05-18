﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class SnippetsSheet : SheetBase
	{
		private sealed class Snippet
		{
			public string Name { get; set; }
			public string Path { get; set; }
		}


		private readonly IRibbonUI ribbon;
		private readonly BindingList<Snippet> snippets;
		private readonly SnippetsProvider snipsProvider;
		private bool updated = false;


		public SnippetsSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = "SnippetsSheet";
			Title = Resx.word_Snippets;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"renameButton=word_Rename",
					"deleteButton=word_Delete"
				});

				nameColumn.HeaderText = Resx.word_Name;
			}

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";

			(_, float scaleY) = UI.Scaling.GetScalingFactors();
			gridView.RowTemplate.Height = (int)(16 * scaleY);

			this.ribbon = ribbon;
			snipsProvider = new SnippetsProvider();

			snippets = new BindingList<Snippet>(LoadSnippets());
			gridView.DataSource = snippets;
		}


		private List<Snippet> LoadSnippets()
		{
			var paths = snipsProvider.GetPaths();
			var list = new List<Snippet>();

			foreach (var path in paths)
			{
				list.Add(new Snippet
				{
					Name = Path.GetFileNameWithoutExtension(path),
					Path = path
				});
			}

			return list;
		}


		private void DeleteItem(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= snippets.Count)
				return;

			var snippet = snippets[rowIndex];

			var result = UI.MoreMessageBox.Show(this,
				string.Format(Resx.SnippetsSheet_ConfirmDelete, snippet.Name),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
				return;

			snippets.RemoveAt(rowIndex);
			snipsProvider.Delete(snippet.Path);
			updated = true;

			rowIndex--;
			if (rowIndex >= 0)
			{
				gridView.Rows[rowIndex].Cells[0].Selected = true;
			}
		}


		public override bool CollectSettings()
		{
			if (updated)
			{
				ribbon.InvalidateControl("ribFavoritesMenu");
			}

			return false;
		}

		private void RenameItem(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= snippets.Count)
				return;

			var snippet = snippets[rowIndex];

			using var dialog = new SaveSnippetDialog();
			dialog.SnippetName = snippet.Name;

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				var path = snipsProvider.Rename(snippet.Path, dialog.SnippetName);
				if (!string.IsNullOrEmpty(path))
				{
					snippet.Name = dialog.SnippetName;
					snippet.Path = path;
					updated = true;
				}
			}
		}
	}
}
