﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class FavoritesSheet : SheetBase
	{
		private class Favorite
		{
			public int Index { get; set; }
			public XElement Root { get; set; }
			public string Name { get; set; }
			public string Location { get; set; }
		}


		private readonly IRibbonUI ribbon;
		private readonly BindingList<Favorite> favorites;


		public FavoritesSheet(IRibbonUI ribbon) : base(null)
		{
			InitializeComponent();

			Name = "FavoritesSheet";
			Title = Resx.FavoritesSheet_Text;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introLabel",
					"optionsBox",
					"shortcutsBox",
					"sortButton",
					"upButton",
					"downButton",
					"deleteLabel",
					"deleteButton",
					"okButton",
					"cancelButton"
				});

				nameColumn.HeaderText = Resx.FavoritesSheet_nameColumn_HeaderText;
				locationColumn.HeaderText = Resx.FavoritesSheet_locationColumn_HeaderText;
			}

			// prevent VS designer from overriding
			toolStrip.ImageScalingSize = new Size(16, 16);

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			gridView.Columns[1].DataPropertyName = "Location";

			this.ribbon = ribbon;

			favorites = new BindingList<Favorite>(LoadFavorites());

			gridView.DataSource = favorites;
		}


		private List<Favorite> LoadFavorites()
		{
			var list = new List<Favorite>();
			var root = new FavoritesProvider(ribbon).LoadFavorites();
			var ns = root.GetDefaultNamespace();

			int index = 0;
			foreach (var element in root.Elements(ns + "splitButton").Elements(ns + "button"))
			{
				list.Add(new Favorite
				{
					Index = index++,
					Root = element.Parent,
					Name = element.Attribute("label").Value,
					Location = element.Attribute("screentip").Value
				});
			}

			return list;
		}


		private void gridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
		{
			var result = MessageBox.Show(
				string.Format(Resx.SearchEngineDialog_DeleteMessage, favorites[e.Row.Index].Name),
				"OneMore",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2,
				MessageBoxOptions.DefaultDesktopOnly);

			if (result != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}


		private void deleteButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex < favorites.Count)
				{
					var result = MessageBox.Show(
						string.Format(Resx.FavoritesSheet_DeleteMessage, favorites[rowIndex].Name),
						"OneMore",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2,
						MessageBoxOptions.DefaultDesktopOnly);

					if (result == DialogResult.Yes)
					{
						favorites.RemoveAt(rowIndex);

						if (rowIndex > 0)
						{
							rowIndex--;
						}

						gridView.Rows[rowIndex].Cells[colIndex].Selected = true;
					}
				}
			}
		}


		private void sortButton_Click(object sender, EventArgs e)
		{
			var ordered = favorites.OrderBy(f => f.Name).ToList();

			favorites.Clear();
			foreach (var fav in ordered)
			{
				favorites.Add(fav);
			}
		}


		private void upButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex > 0 && rowIndex < favorites.Count)
				{
					var item = favorites[rowIndex];
					favorites.RemoveAt(rowIndex);
					favorites.Insert(rowIndex - 1, item);

					gridView.Rows[rowIndex - 1].Cells[colIndex].Selected = true;
				}
			}
		}


		private void downButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex < favorites.Count - 1)
				{
					var item = favorites[rowIndex];
					favorites.RemoveAt(rowIndex);
					favorites.Insert(rowIndex + 1, item);

					gridView.Rows[rowIndex + 1].Cells[colIndex].Selected = true;
				}
			}
		}


		public override void CollectSettings()
		{
			// favorites are not stored along with other settings,
			// but save them here in their own file when other settings are saved...

			var updated = false;
			for (int i=0; i < favorites.Count; i++)
			{
				if (favorites[i].Index != i)
				{
					updated = true;
					break;
				}
			}

			if (updated)
			{
				var root = FavoritesProvider.MakeMenuRoot();
				foreach (var favorite in favorites)
				{
					root.Add(favorite.Root);
				}

				new FavoritesProvider(ribbon).SaveFavorites(root);
			}
		}
	}
}
