//************************************************************************************************
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
	using System.Drawing;
	using System.IO;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SnippetsSheet : SheetBase
	{
		private class Snippet
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
			Title = "Manage My Custom Snippets"; // Resx.FavoritesSheet_Text; // translate

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introLabel",
					"deleteLabel",
					"deleteButton",
					"okButton",
					"cancelButton"
				});

				nameColumn.HeaderText = Resx.FavoritesSheet_nameColumn_HeaderText;
			}

			// prevent VS designer from overriding
			toolStrip.ImageScalingSize = new Size(16, 16);

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";

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

			var result = MessageBox.Show(
				string.Format("Delete snippet \"{0}\"?", snippet.Name), // translate
				"OneMore",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2,
				MessageBoxOptions.DefaultDesktopOnly);

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


		public override void CollectSettings()
		{
			if (updated)
			{
				ribbon.InvalidateControl("ribFavoritesMenu");
			}
		}
	}
}
