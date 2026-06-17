//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Commands.Favorites;
	using Resx = Properties.Resources;


	internal partial class FavoritesSheet : SheetBase
	{

		private readonly IRibbonUI ribbon;
		private readonly bool shortcuts;
		private BindingList<Favorite> favorites;
		private Task validation;
		private bool updated = false;


		public FavoritesSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = "FavoritesSheet";
			Title = Resx.word_Favorites;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"optionsBox=word_Options",
					"shortcutsBox",
					"deleteButton=word_Delete"
				});

				nameColumn.HeaderText = Resx.word_Name;
				locationColumn.HeaderText = Resx.FavoritesSheet_locationColumn_HeaderText;
			}

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			gridView.Columns[1].DataPropertyName = "Location";

			this.ribbon = ribbon;

			shortcuts = provider.GetCollection(Name).Get<bool>("kbdshorts");
			shortcutsBox.Checked = shortcuts;
		}


		private async void LoadData(object sender, EventArgs e)
		{
			(_, float scaleY) = UI.Scaling.GetScalingFactors();
			gridView.RowTemplate.Height = (int)(16 * scaleY);

			using var provider = new FavoritesProvider();
			var collection = provider.ReadFavorites();

			// capture Task so it can be completed later in RowEnter
			await using var checker = new FavoritesChecker(logger);
			validation = checker.InvalidFavorites(collection.Items);

			favorites = new BindingList<Favorite>(collection.Items);

			gridView.DataSource = favorites;
		}


		private async void FinishValidationOnRowEnter(object sender, DataGridViewCellEventArgs e)
		{
			// executed once (by unsetting 'validator') to update the data source binding
			// after validation is completed

			if (validation is not null)
			{
				try
				{
					if (!(validation.IsCompleted || validation.IsFaulted || validation.IsCanceled))
					{
						await Task.WhenAll(validation);
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error awaiting in {nameof(FinishValidationOnRowEnter)}", exc);
				}

				validation = null;

				try
				{
					favorites.ResetBindings();
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error resetting in {nameof(FinishValidationOnRowEnter)}", exc);
				}
			}
		}


		private void FormatCell(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (gridView.Rows[e.RowIndex].DataBoundItem is Favorite favorite)
			{
				if (favorite.Status == FavoriteStatus.Unknown)
				{
					gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText =
						Resx.Favorites_unknown;

					e.CellStyle.ForeColor = manager.GetColor("ErrorText");
					e.FormattingApplied = true;
				}
				else if (favorite.Status == FavoriteStatus.Suspect)
				{
					gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText =
						Resx.Favorites_suspect;

					e.CellStyle.BackColor = manager.GetColor("Info");
					e.CellStyle.ForeColor = manager.GetColor("InfoText");
					e.FormattingApplied = true;
				}
			}
		}


		private void DeleteOnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				DeleteItems(sender, e);
			}
		}


		private void DeleteItems(object sender, EventArgs e)
		{
			if (gridView.SelectedRows.Count == 0)
				return;

			int first = gridView.SelectedRows[0].Index;
			if (first >= favorites.Count)
				return;

			var text = gridView.SelectedRows.Count == 1
				? favorites[first].Name
				: $"({gridView.SelectedRows.Count})";

			var result = UI.MoreMessageBox.Show(this,
				string.Format(Resx.FavoritesSheet_DeleteMessage, text),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
				return;

			using var provider = new FavoritesProvider();

			for (int i = gridView.SelectedRows.Count - 1; i >= 0; i--)
			{
				var index = gridView.SelectedRows[i].Index;
				if (provider.DeleteFavorite(favorites[index].ID))
				{
					favorites.RemoveAt(index);
				}
			}

			gridView.ClearSelection();

			updated = true;

			first--;
			if (first < 0 && gridView.Rows.Count > 0)
			{
				gridView.Rows[0].Selected = true;
			}
			else if (first < gridView.Rows.Count && first >= 0)
			{
				gridView.Rows[first].Selected = true;
			}
		}


		private void MoveItemDown(object sender, EventArgs e)
		{
			gridView.MoveSelectedItemDown(favorites);
		}


		private void MoveItemUp(object sender, EventArgs e)
		{
			gridView.MoveSelectedItemUp(favorites);
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

			// favorites are stored in the DB...

			// if nothing was deleted then check if they reordered
			if (!updated)
			{
				for (int i = 0; i < favorites.Count; i++)
				{
					if (favorites[i].SortOrder != i)
					{
						updated = true;
						break;
					}
				}
			}

			//if (updated)
			//{
			//	var root = FavoritesMenu.LoadMenu();
			//	foreach (var favorite in favorites)
			//	{
			//		root.Add(favorite.Root);
			//	}

			//	Task.Run(async () =>
			//	{
			//		var provider = new FavoritesProvider();
			//		//provider.WriteFavorite(root);
			//	});
			//}
			//else if (shortcuts != shortcutsBox.Checked)
			//{
			//	ribbon.InvalidateControl(FavoritesMenu.MenuID);
			//}

			return false;
		}
	}
}
