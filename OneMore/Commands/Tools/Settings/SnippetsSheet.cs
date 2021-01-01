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
		private bool updated = false;


		public SnippetsSheet(SettingsProvider provider)
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

			snippets = new BindingList<Snippet>(LoadSnippets());
			gridView.DataSource = snippets;
		}


		private List<Snippet> LoadSnippets()
		{
			var paths = new SnippetsProvider().GetPaths();
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
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex < snippets.Count)
				{
					var result = MessageBox.Show(
						string.Format("Delete {0}?", snippets[rowIndex].Name),
						"OneMore",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2,
						MessageBoxOptions.DefaultDesktopOnly);

					if (result == DialogResult.Yes)
					{
						snippets.RemoveAt(rowIndex);
						updated = true;

						if (rowIndex > 0)
						{
							rowIndex--;
						}

						gridView.Rows[rowIndex].Cells[colIndex].Selected = true;
					}
				}
			}
		}
	}
}
