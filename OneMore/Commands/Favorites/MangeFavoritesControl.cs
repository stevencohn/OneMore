//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Hosted by ManageFavoritesDialog. Lets the user organize favorites into folders,
	/// reorder them, rename their displayed alias, and delete favorites and folders. All
	/// edits are staged in memory; nothing is written to the database until Save() is
	/// called by the hosting dialog's OK handler.
	/// </summary>
	internal partial class MangeFavoritesControl : MoreUserControl
	{
		/// <summary>
		/// Marks a row that represents a folder rather than a favorite. FolderID is the
		/// real database ID, or a negative sentinel for a folder created during this
		/// session but not yet persisted.
		/// </summary>
		private sealed class FolderRow
		{
			public int FolderID { get; set; }
			public string Name { get; set; }
		}


		private const int FavoriteIndent = 20;

		private readonly HashSet<int> deletedFavoriteIDs = new();
		private readonly HashSet<int> deletedFolderIDs = new();
		private List<(int FolderID, string Name)> originalFolders = new();
		private List<(int ID, int FolderID, string Alias, int Position)> originalFavorites = new();
		private int nextSentinelFolderID = -1;
		private bool isDirtyFromCheck;


		public MangeFavoritesControl()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				nameColumn.Text = Resx.word_Name;
				locationColumn.Text = Resx.MangeFavoritesControl_locationColumn_HeaderText;
				sortButton.ToolTipText = Resx.ManageFavoritesControl_sort;
				upButton.ToolTipText = Resx.NavigatorWindow_menuMoveUp;
				downButton.ToolTipText = Resx.NavigatorWindow_menuMoveDown;
				newFolderButton.ToolTipText = Resx.ManageFavoritesControl_newFolder;
				deleteButton.ToolTipText = Resx.word_Delete;
				renameButton.ToolTipText = Resx.ManageFavoritesControl_renameFavorite;
				renameMenuItem.Text = Resx.ManageFavoritesControl_renameFavorite;
				checkButton.ToolTipText = Resx.FavoritesDialog_checkButton_Text;
				importButton.ToolTipText = Resx.ManageFavoritesControl_import;
				exportButton.ToolTipText = Resx.ManageFavoritesControl_export;
			}

			listView.SetColumnProportions(0.4f, 0.6f);
			listView.CanDragItem = item => item.Tag is Favorite;
			listView.IsInsertionAnchor = item => item.Tag is FolderRow;
			listView.GetCellStyle = GetCellStyle;
		}


		/// <summary>
		/// Loads the current favorites/folders from the database into the working view.
		/// </summary>
		public void LoadFavorites()
		{
			using var provider = new FavoritesProvider();
			var collection = provider.ReadFavorites();

			LoadFavorites(collection);
		}


		private void LoadFavorites(FavoritesCollection collection)
		{
			listView.BeginUpdate();
			listView.Items.Clear();

			foreach (var folder in collection.Folders)
			{
				AddFolderRow(folder.FolderID, folder.Name);
				foreach (var favorite in folder.Items)
				{
					AddFavoriteRow(favorite);
				}
			}

			foreach (var favorite in collection.Items)
			{
				AddFavoriteRow(favorite);
			}

			listView.EndUpdate();

			RefreshFolderHints();
			originalFolders = CurrentFoldersShape();
			originalFavorites = CurrentFavoritesShape();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		/// <summary>
		/// Returns true if the user has made any change since LoadFavorites() was called.
		/// </summary>
		public bool IsDirty()
		{
			if (deletedFavoriteIDs.Count > 0 || deletedFolderIDs.Count > 0 || isDirtyFromCheck)
			{
				return true;
			}

			if (listView.Items.Cast<ListViewItem>().Any(i => i.Tag is FolderRow row && row.FolderID < 0))
			{
				return true;
			}

			return !CurrentFoldersShape().SequenceEqual(originalFolders) ||
				!CurrentFavoritesShape().SequenceEqual(originalFavorites);
		}


		/// <summary>
		/// Commits all staged additions, deletions, and updates to the database.
		/// </summary>
		/// <returns>True if every change was saved successfully</returns>
		public bool Save()
		{
			using var provider = new FavoritesProvider();

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is FolderRow row && row.FolderID < 0)
				{
					var newID = provider.CreateFolder(row.Name);
					if (newID == 0)
					{
						return false;
					}

					var oldID = row.FolderID;
					row.FolderID = newID;

					foreach (ListViewItem favoriteItem in listView.Items)
					{
						if (favoriteItem.Tag is Favorite favorite && favorite.FolderID == oldID)
						{
							favorite.FolderID = newID;
						}
					}
				}
				else if (item.Tag is FolderRow existingRow && existingRow.FolderID > 0)
				{
					if (!provider.RenameFolder(existingRow.FolderID, existingRow.Name))
					{
						return false;
					}
				}
			}

			// must persist remaining favorites' (possibly changed) FolderID before deleting
			// any folder below: DeleteFolder cascades by deleting every favorite row still
			// pointing at that folderID in the database, and a favorite the user just
			// dragged out of the folder still has its old, stale folderID in the database
			// until UpdateFavorite runs - deleting the folder first would catch it too.
			var positionWithinFolder = new Dictionary<int, int>();
			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is Favorite favorite)
				{
					var position = positionWithinFolder.TryGetValue(favorite.FolderID, out var p) ? p : 0;
					positionWithinFolder[favorite.FolderID] = position + 1;

					favorite.SortOrder = position;

					if (!provider.UpdateFavorite(favorite))
					{
						return false;
					}
				}
			}

			foreach (var folderID in deletedFolderIDs)
			{
				if (!provider.DeleteFolder(folderID))
				{
					return false;
				}
			}

			foreach (var favoriteID in deletedFavoriteIDs)
			{
				if (!provider.DeleteFavorite(favoriteID))
				{
					return false;
				}
			}

			return true;
		}


		private static MoreListView.CellStyle GetCellStyle(ListViewItem item, int columnIndex)
		{
			if (item.Tag is Favorite favorite)
			{
				var indent = columnIndex == 0 && favorite.FolderID != 0 ? FavoriteIndent : 0;
				var foreColorKey =
					favorite.Status is FavoriteStatus.Suspect or FavoriteStatus.Unknown
						? "ErrorText"
						: null;

				return new MoreListView.CellStyle(indent, false, foreColorKey);
			}

			if (columnIndex == 1 && item.SubItems.Count > 1 &&
				item.SubItems[1].Text == Resx.ManageFavoritesControl_emptyFolderHint)
			{
				return new MoreListView.CellStyle(0, true);
			}

			return MoreListView.CellStyle.Default;
		}


		private async void ImportFavorites(object sender, EventArgs e)
		{
			if (IsDirty() && MoreMessageBox.Show(this, Resx.ManageFavoritesDialog_discard,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			var command = new ImportFavoritesCommand();
			command.SetLogger(Logger.Current);
			command.SetOwner(this);
			await command.Execute();

			LoadFavorites();
		}


		private async void ExportFavorites(object sender, EventArgs e)
		{
			var command = new ExportFavoritesCommand();
			command.SetLogger(Logger.Current);
			command.SetOwner(this);
			await command.Execute();
		}


		private async void CheckFavorites(object sender, EventArgs e)
		{
			if (!listView.Items.Cast<ListViewItem>().Any(i => i.Tag is Favorite))
			{
				return;
			}

			var collection = RebuildCollection();

			await using var checker = new FavoritesChecker(Logger.Current);
			if (await checker.InvalidFavorites(collection))
			{
				LoadFavorites(collection);
				isDirtyFromCheck = true;
			}
		}


		/// <summary>
		/// Reconstructs a FavoritesCollection from the favorites/folders currently shown in
		/// the listview, reusing the same Favorite/FolderRow instances so in-place edits made
		/// by the caller (e.g. FavoritesChecker) are reflected immediately in the listview.
		/// </summary>
		private FavoritesCollection RebuildCollection()
		{
			var collection = new FavoritesCollection();
			var folders = new Dictionary<int, FavoritesFolder>();

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is FolderRow row)
				{
					var folder = new FavoritesFolder { FolderID = row.FolderID, Name = row.Name };
					collection.Folders.Add(folder);
					folders[row.FolderID] = folder;
				}
				else if (item.Tag is Favorite favorite)
				{
					if (favorite.FolderID != 0 && folders.TryGetValue(favorite.FolderID, out var folder))
					{
						folder.Items.Add(favorite);
					}
					else
					{
						collection.Items.Add(favorite);
					}
				}
			}

			return collection;
		}


		private void ListViewItemMoved(object sender, MoreListView.ItemMovedEventArgs e)
		{
			if (e.Item.Tag is Favorite favorite)
			{
				favorite.FolderID = e.MovedToEnd
					? 0
					: e.PrecedingItem?.Tag switch
					{
						FolderRow row => row.FolderID,
						Favorite precedingFavorite => precedingFavorite.FolderID,
						_ => 0
					};
			}

			RefreshFolderHints();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		private ListViewItem AddFolderRow(int folderID, string name)
		{
			return AddFolderRowAt(listView.Items.Count, folderID, name);
		}


		private ListViewItem AddFolderRowAt(int index, int folderID, string name)
		{
			var item = new ListViewItem(name)
			{
				Tag = new FolderRow { FolderID = folderID, Name = name },
				Font = new Font(listView.Font, FontStyle.Bold)
			};

			item.SubItems.Add(string.Empty);
			listView.Items.Insert(index, item);
			return item;
		}


		private ListViewItem AddFavoriteRow(Favorite favorite)
		{
			var item = new ListViewItem(favorite.Alias ?? favorite.Name) { Tag = favorite };
			item.SubItems.Add(favorite.Location);
			listView.Items.Add(item);
			return item;
		}


		private List<(int FolderID, string Name)> CurrentFoldersShape()
		{
			return listView.Items.Cast<ListViewItem>()
				.Where(i => i.Tag is FolderRow)
				.Select(i => (((FolderRow)i.Tag).FolderID, ((FolderRow)i.Tag).Name))
				.ToList();
		}


		private List<(int ID, int FolderID, string Alias, int Position)> CurrentFavoritesShape()
		{
			var result = new List<(int ID, int FolderID, string Alias, int Position)>();
			var positionWithinFolder = new Dictionary<int, int>();

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is Favorite favorite)
				{
					var position = positionWithinFolder.TryGetValue(favorite.FolderID, out var p) ? p : 0;
					positionWithinFolder[favorite.FolderID] = position + 1;

					result.Add((favorite.ID, favorite.FolderID, favorite.Alias, position));
				}
			}

			return result;
		}


		private void RefreshFolderHints()
		{
			var favoriteCountByFolder = new Dictionary<int, int>();
			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is Favorite favorite)
				{
					favoriteCountByFolder[favorite.FolderID] =
						favoriteCountByFolder.TryGetValue(favorite.FolderID, out var count) ? count + 1 : 1;
				}
			}

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is FolderRow row)
				{
					item.SubItems[1].Text = favoriteCountByFolder.ContainsKey(row.FolderID)
						? string.Empty
						: Resx.ManageFavoritesControl_emptyFolderHint;
				}
			}
		}


		private int FindFolderInsertIndex(string name)
		{
			var index = 0;
			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is FolderRow row)
				{
					if (string.Compare(name, row.Name, StringComparison.CurrentCultureIgnoreCase) < 0)
					{
						return index;
					}
				}
				else
				{
					// folders always precede favorites; nothing more to scan
					return index;
				}

				index++;
			}

			return index;
		}


		private ListViewItem GetSingleSelectedFavoriteItem()
		{
			if (listView.SelectedItems.Count != 1)
			{
				return null;
			}

			var item = listView.SelectedItems[0];
			return item.Tag is Favorite ? item : null;
		}


		private ListViewItem GetSingleSelectedRenamableItem()
		{
			if (listView.SelectedItems.Count != 1)
			{
				return null;
			}

			var item = listView.SelectedItems[0];
			return item.Tag is Favorite or FolderRow ? item : null;
		}


		private bool CanMove(ListViewItem item, int direction)
		{
			var neighborIndex = item.Index + direction;
			if (neighborIndex < 0 || neighborIndex >= listView.Items.Count)
			{
				return false;
			}

			var neighbor = listView.Items[neighborIndex];
			return neighbor.Tag is Favorite neighborFavorite &&
				neighborFavorite.FolderID == ((Favorite)item.Tag).FolderID;
		}


		private void RefreshToolbarState(object sender, EventArgs e)
		{
			var item = GetSingleSelectedFavoriteItem();

			upButton.Enabled = item != null && CanMove(item, -1);
			downButton.Enabled = item != null && CanMove(item, 1);
			renameButton.Enabled = GetSingleSelectedRenamableItem() != null;
			deleteButton.Enabled = listView.SelectedItems.Count > 0;
		}


		private void MoveUp(object sender, EventArgs e)
		{
			var item = GetSingleSelectedFavoriteItem();
			if (item == null || !CanMove(item, -1))
			{
				return;
			}

			var index = item.Index;

			listView.BeginUpdate();
			listView.Items.RemoveAt(index);
			listView.Items.Insert(index - 1, item);
			listView.EndUpdate();

			item.Selected = true;
			item.EnsureVisible();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		private void MoveDown(object sender, EventArgs e)
		{
			var item = GetSingleSelectedFavoriteItem();
			if (item == null || !CanMove(item, 1))
			{
				return;
			}

			var index = item.Index;

			listView.BeginUpdate();
			listView.Items.RemoveAt(index);
			listView.Items.Insert(index + 1, item);
			listView.EndUpdate();

			item.Selected = true;
			item.EnsureVisible();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		private void SortCurrentGroup(object sender, EventArgs e)
		{
			var targetFolderID = listView.SelectedItems.Count == 1
				? listView.SelectedItems[0].Tag switch
				{
					Favorite favorite => favorite.FolderID,
					FolderRow row => row.FolderID,
					_ => 0
				}
				: 0;

			var matches = listView.Items.Cast<ListViewItem>()
				.Select((item, index) => (item, index))
				.Where(x => x.item.Tag is Favorite favorite && favorite.FolderID == targetFolderID)
				.ToList();

			if (matches.Count < 2)
			{
				return;
			}

			var sorted = matches
				.Select(x => x.item)
				.OrderBy(i => (((Favorite)i.Tag).Alias ?? ((Favorite)i.Tag).Name).TrimLeadingIcons(),
					StringComparer.CurrentCultureIgnoreCase)
				.ToList();

			var startIndex = matches[0].index;

			listView.BeginUpdate();
			foreach (var (item, index) in matches.AsEnumerable().Reverse())
			{
				listView.Items.RemoveAt(index);
			}

			for (var i = 0; i < sorted.Count; i++)
			{
				listView.Items.Insert(startIndex + i, sorted[i]);
			}

			listView.EndUpdate();
		}


		private void CreateFolder(object sender, EventArgs e)
		{
			var existingNames = listView.Items.Cast<ListViewItem>()
				.Where(i => i.Tag is FolderRow)
				.Select(i => ((FolderRow)i.Tag).Name);

			using var dialog = new RenameDialog(existingNames, string.Empty,
				createTitle: Resx.ManageFavoritesControl_newFolder,
				label: Resx.word_Name);

			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			var name = dialog.Value;
			var item = AddFolderRowAt(FindFolderInsertIndex(name), nextSentinelFolderID--, name);

			item.Selected = true;
			item.EnsureVisible();

			RefreshFolderHints();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		private void DeleteSelected(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0)
			{
				return;
			}

			var folderRows = listView.SelectedItems.Cast<ListViewItem>()
				.Where(i => i.Tag is FolderRow)
				.ToList();

			var favoriteRows = listView.SelectedItems.Cast<ListViewItem>()
				.Where(i => i.Tag is Favorite)
				.ToList();

			var message = folderRows.Count == 0 && favoriteRows.Count == 1
				? string.Format(Resx.ManageFavorites_DeleteMessage,
					((Favorite)favoriteRows[0].Tag).Alias ?? ((Favorite)favoriteRows[0].Tag).Name)
				: string.Format(Resx.ManageFavoritesrControl_deleteConfirmMultiple,
					folderRows.Count + favoriteRows.Count);

			if (MoreMessageBox.Show(this, message, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				!= DialogResult.Yes)
			{
				return;
			}

			listView.BeginUpdate();

			foreach (var row in folderRows)
			{
				var folder = (FolderRow)row.Tag;
				if (folder.FolderID > 0)
				{
					deletedFolderIDs.Add(folder.FolderID);
				}

				var children = listView.Items.Cast<ListViewItem>()
					.Where(i => i.Tag is Favorite favorite && favorite.FolderID == folder.FolderID)
					.ToList();

				foreach (var child in children)
				{
					deletedFavoriteIDs.Add(((Favorite)child.Tag).ID);
					listView.Items.Remove(child);
				}

				listView.Items.Remove(row);
			}

			foreach (var row in favoriteRows)
			{
				if (!listView.Items.Contains(row))
				{
					// already removed as part of a deleted folder above
					continue;
				}

				deletedFavoriteIDs.Add(((Favorite)row.Tag).ID);
				listView.Items.Remove(row);
			}

			listView.EndUpdate();

			RefreshFolderHints();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		private void RenameOnDoubleClick(object sender, EventArgs e)
		{
			RenameSelected(sender, e);
		}


		private void RenameSelected(object sender, EventArgs e)
		{
			var item = GetSingleSelectedRenamableItem();
			if (item == null)
			{
				return;
			}

			if (item.Tag is FolderRow folder)
			{
				RenameSelectedFolder(item, folder);
				return;
			}

			var favorite = (Favorite)item.Tag;

			var siblingNames = listView.Items.Cast<ListViewItem>()
				.Where(i => i.Tag is Favorite other && other != favorite &&
					other.FolderID == favorite.FolderID)
				.Select(i => ((Favorite)i.Tag).Alias ?? ((Favorite)i.Tag).Name);

			using var dialog = new RenameFavoriteDialog(
				siblingNames, favorite.Name, favorite.Alias ?? favorite.Name);

			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			favorite.Alias = dialog.Value;
			item.Text = dialog.Value ?? favorite.Name;
		}


		private void RenameSelectedFolder(ListViewItem item, FolderRow folder)
		{
			var siblingNames = listView.Items.Cast<ListViewItem>()
				.Where(i => i.Tag is FolderRow other && other != folder)
				.Select(i => ((FolderRow)i.Tag).Name);

			using var dialog = new RenameDialog(siblingNames, folder.Name,
				createTitle: Resx.ManageFavoritesControl_renameFolder,
				renameTitle: Resx.ManageFavoritesControl_renameFolder,
				label: Resx.word_Name)
			{
				Rename = true
			};

			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			folder.Name = dialog.Value;
			item.Text = dialog.Value;
		}


		private void ShowContextMenu(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
			{
				return;
			}

			var item = listView.GetItemAt(e.X, e.Y);
			if (item == null || item.Tag is not Favorite)
			{
				return;
			}

			if (!item.Selected)
			{
				foreach (ListViewItem selected in listView.SelectedItems.Cast<ListViewItem>().ToList())
				{
					selected.Selected = false;
				}

				item.Selected = true;
			}

			itemContextMenu.Show(listView, e.Location);
		}
	}
}
