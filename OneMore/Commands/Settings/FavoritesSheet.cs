//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Favorite = FavoritesProvider.Favorite;
	using FavoriteStatus = FavoritesProvider.FavoriteStatus;
	using Resx = Properties.Resources;


	internal partial class FavoritesSheet : SheetBase
	{

		private readonly IRibbonUI ribbon;
		private readonly bool shortcuts;
		private BindingList<Favorite> favorites;
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
					"optionsBox=word_Options",
					"shortcutsBox",
					"sortButton",
					"upButton",
					"downButton",
					"deleteLabel",
					"deleteButton"
				});

				nameColumn.HeaderText = Resx.word_Name;
				locationColumn.HeaderText = Resx.FavoritesSheet_locationColumn_HeaderText;
			}

			toolStrip.Rescale();

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			gridView.Columns[1].DataPropertyName = "Location";

			this.ribbon = ribbon;

			shortcuts = provider.GetCollection(Name).Get<bool>("kbdshorts");
			shortcutsBox.Checked = shortcuts;
		}


		private async void LoadData(object sender, EventArgs e)
		{
			using var provider = new FavoritesProvider(null);
			var list = await provider.LoadFavorites();
			favorites = new BindingList<Favorite>(list);

			gridView.DataSource = favorites;
		}


		private void FormatCell(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (gridView.Rows[e.RowIndex].DataBoundItem is Favorite favorite)
			{
				if (favorite.Status == FavoriteStatus.Unknown)
				{
					e.CellStyle.BackColor = Color.Pink;
					e.FormattingApplied = true;
				}
				else if (favorite.Status == FavoriteStatus.Suspect)
				{
					e.CellStyle.BackColor = Color.LightGoldenrodYellow;
					e.FormattingApplied = true;
				}
			}
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

				using var provider = new FavoritesProvider(ribbon);
				provider.SaveFavorites(root);
			}
			else if (shortcuts != shortcutsBox.Checked)
			{
				ribbon.InvalidateControl("ribFavoritesMenu");
			}

			return false;
		}
	}
}
