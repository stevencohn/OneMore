//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class FavoritesSheet : SheetBase
	{
		private sealed class Favorite
		{
			public int Index { get; set; }
			public XElement Root { get; set; }
			public string Name { get; set; }
			public string Location { get; set; }
		}


		private readonly IRibbonUI ribbon;
		private readonly BindingList<Favorite> favorites;
		private readonly bool shortcuts;
		private bool updated = false;


		public FavoritesSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
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

			toolStrip.Rescale();

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			gridView.Columns[1].DataPropertyName = "Location";

			this.ribbon = ribbon;

			shortcuts = provider.GetCollection(Name).Get<bool>("kbdshorts");
			shortcutsBox.Checked = shortcuts;

			favorites = new BindingList<Favorite>(LoadFavorites());

			gridView.DataSource = favorites;
		}


		private List<Favorite> LoadFavorites()
		{
			var list = new List<Favorite>();
			var root = new FavoritesProvider(ribbon).LoadFavoritesMenu();
			var ns = root.Name.Namespace;

			// filter out the add/manage/shortcuts buttons
			var elements = root.Elements(ns + "button")
				.Where(e => e.Attribute("onAction")?.Value == FavoritesProvider.GotoFavoriteCmd);

			int index = 0;
			foreach (var element in elements)
			{
				list.Add(new Favorite
				{
					Index = index++,
					Root = element,
					Name = element.Attribute("label").Value,
					Location = element.Attribute("screentip").Value
				});
			}

			return list;
		}


		private void DeleteItem(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= favorites.Count)
				return;

			var result = MessageBox.Show(
				string.Format(Resx.FavoritesSheet_DeleteMessage, favorites[rowIndex].Name),
				"OneMore",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2,
				MessageBoxOptions.DefaultDesktopOnly);

			if (result != DialogResult.Yes)
				return;

			favorites.RemoveAt(rowIndex);
			updated = true;

			rowIndex--;
			if (rowIndex >= 0)
			{
				gridView.Rows[rowIndex].Cells[0].Selected = true;
			}
		}


		private void MoveItemDown(object sender, EventArgs e)
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


		private void MoveItemUp(object sender, EventArgs e)
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


		private void SortItems(object sender, EventArgs e)
		{
			var ordered = favorites.OrderBy(f => f.Name).ToList();

			favorites.Clear();
			foreach (var fav in ordered)
			{
				favorites.Add(fav);
			}
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			settings.Add("kbdshorts", shortcutsBox.Checked);

			provider.SetCollection(settings);

			// the actual favorites are stored in a separate file...

			// if nothing was deleted then check if they reordered
			if (!updated)
			{
				for (int i = 0; i < favorites.Count; i++)
				{
					if (favorites[i].Index != i)
					{
						updated = true;
						break;
					}
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
			else if (shortcuts != shortcutsBox.Checked)
			{
				ribbon.InvalidateControl("ribFavoritesMenu");
			}

			return false;
		}
	}
}
