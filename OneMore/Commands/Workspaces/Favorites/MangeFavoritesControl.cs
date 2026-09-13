//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Hosted by ManageWorkspaceDialog. Lets the user organize favorites into folders,
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


		/// <summary>
		/// Raised after CheckFavorites completes a check, whether or not any favorites
		/// were found to be invalid.
		/// </summary>
		public event EventHandler FavoritesChecked;


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
			listView.AllowMultiItemDrag = true;
			listView.GetCellStyle = GetCellStyle;
			listView.GetCellImage = (item, col) =>
				col == 0 && item.Tag is Favorite f ? FavoriteKindGlyphs.GetGlyph(f) : null;
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
					favorite.Status is TargetStatus.Suspect or TargetStatus.Unknown
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
			if (IsDirty() && MoreMessageBox.Show(this, Resx.ManageWorkspaceDialog_discard,
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

			await using var checker = new TargetChecker(Logger.Current);
			if (await checker.InvalidFavorites(collection))
			{
				LoadFavorites(collection);
				isDirtyFromCheck = true;
			}

			FavoritesChecked?.Invoke(this, EventArgs.Empty);
		}


		/// <summary>
		/// Reconstructs a FavoritesCollection from the favorites/folders currently shown in
		/// the listview, reusing the same Favorite/FolderRow instances so in-place edits made
		/// by the caller (e.g. TargetChecker) are reflected immediately in the listview.
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
			var newFolderID = e.MovedToEnd
				? 0
				: e.PrecedingItem?.Tag switch
				{
					FolderRow row => row.FolderID,
					Favorite precedingFavorite => precedingFavorite.FolderID,
					_ => 0
				};

			foreach (var item in e.Items)
			{
				if (item.Tag is Favorite favorite)
				{
					favorite.FolderID = newFolderID;
				}
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

			// sentinel: no real FolderID equals this, so a Favorite encountered
			// before any FolderRow (i.e. a root-level favorite, or the list has no
			// folders yet) immediately stops the scan - folders always sort before
			// root-level favorites.
			var currentFolderID = int.MinValue;

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is FolderRow row)
				{
					if (string.Compare(name, row.Name, StringComparison.CurrentCultureIgnoreCase) < 0)
					{
						return index;
					}

					currentFolderID = row.FolderID;
				}
				else if (item.Tag is Favorite favorite && favorite.FolderID != currentFolderID)
				{
					// this favorite isn't a child of the folder we just passed
					// (either a root-level favorite, or we haven't seen a folder
					// yet); nothing more to scan
					return index;
				}

				index++;
			}

			return index;
		}


		/// <summary>
		/// Returns the current selection as an ordered list, but only when every selected
		/// row is a Favorite (not a folder header), the rows are index-contiguous, and they
		/// all belong to the same folder. That last check matters at exactly one boundary:
		/// the last folder's last child and the first root favorite are index-adjacent with
		/// no folder header between them, but belong to different folders, so such a
		/// selection is not a valid block for the up/down buttons.
		/// </summary>
		private List<ListViewItem> GetSelectedFavoriteBlock()
		{
			if (listView.SelectedItems.Count == 0)
			{
				return null;
			}

			var items = listView.SelectedItems.Cast<ListViewItem>()
				.OrderBy(i => i.Index)
				.ToList();

			if (items.Any(i => i.Tag is not Favorite))
			{
				return null;
			}

			for (var i = 1; i < items.Count; i++)
			{
				if (items[i].Index != items[i - 1].Index + 1)
				{
					return null;
				}
			}

			var folderID = ((Favorite)items[0].Tag).FolderID;
			return items.All(i => ((Favorite)i.Tag).FolderID == folderID) ? items : null;
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


		private bool CanMoveBlock(List<ListViewItem> block, int direction)
		{
			return direction < 0
				? block[0].Index > 0
				: block[block.Count - 1].Index < listView.Items.Count - 1;
		}


		private void RefreshToolbarState(object sender, EventArgs e)
		{
			var block = GetSelectedFavoriteBlock();

			upButton.Enabled = block != null && CanMoveBlock(block, -1);
			downButton.Enabled = block != null && CanMoveBlock(block, 1);
			renameButton.Enabled = GetSingleSelectedRenamableItem() != null;
			deleteButton.Enabled = listView.SelectedItems.Count > 0;
		}


		private void MoveUp(object sender, EventArgs e)
		{
			MoveBlock(-1);
		}


		private void MoveDown(object sender, EventArgs e)
		{
			MoveBlock(1);
		}


		/// <summary>
		/// Moves the current contiguous, same-folder selection one step in the given
		/// direction (-1 up, +1 down), inspecting the single row immediately outside the
		/// block's boundary to decide whether this is a plain reorder, a reparent (crossing
		/// the folder/root boundary in place), or a swap that also crosses into or out of a
		/// folder (entering or ejecting).
		/// </summary>
		private void MoveBlock(int direction)
		{
			var block = GetSelectedFavoriteBlock();
			if (block == null || !CanMoveBlock(block, direction))
			{
				return;
			}

			var neighborIndex = direction < 0 ? block[0].Index - 1 : block[block.Count - 1].Index + 1;
			var neighbor = listView.Items[neighborIndex];
			var folderID = ((Favorite)block[0].Tag).FolderID;

			bool swap;
			int? newFolderID;

			switch (neighbor.Tag)
			{
				case Favorite neighborFavorite when neighborFavorite.FolderID == folderID:
					// plain reorder within the current folder (or root)
					swap = true;
					newFolderID = null;
					break;

				case Favorite neighborFavorite:
					// folder/root boundary: already correctly positioned, just join it
					swap = false;
					newFolderID = neighborFavorite.FolderID;
					break;

				case FolderRow neighborRow when direction < 0 && neighborRow.FolderID == folderID:
					// eject: this is the block's own folder header, only reachable going up
					// since a header always precedes its own children
					swap = true;
					newFolderID = 0;
					break;

				case FolderRow neighborRow when direction < 0:
					// entering an empty folder from below: already correctly positioned
					// right after its header, so no swap needed
					swap = false;
					newFolderID = neighborRow.FolderID;
					break;

				case FolderRow neighborRow:
					// entering the next folder from above: must swap so the block ends up
					// after its new header, or Save()/RebuildCollection() would misfile it
					swap = true;
					newFolderID = neighborRow.FolderID;
					break;

				default:
					return;
			}

			listView.BeginUpdate();

			if (swap)
			{
				SwapBlockWithSingleRow(block, neighbor, direction);
			}

			if (newFolderID.HasValue)
			{
				foreach (var item in block)
				{
					((Favorite)item.Tag).FolderID = newFolderID.Value;
				}
			}

			listView.EndUpdate();

			foreach (var item in block)
			{
				item.Selected = true;
			}

			(direction < 0 ? block[0] : block[block.Count - 1]).EnsureVisible();

			RefreshFolderHints();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		/// <summary>
		/// Swaps the block's position with the single bordering row, moving the row to the
		/// far side of the block (after it when moving up, before it when moving down). The
		/// block's own ListViewItems keep their identity and relative order, shifting
		/// automatically as a side effect of the neighbor's removal/insertion.
		/// </summary>
		private void SwapBlockWithSingleRow(List<ListViewItem> block, ListViewItem neighbor, int direction)
		{
			if (direction < 0)
			{
				var neighborIndex = neighbor.Index;
				listView.Items.RemoveAt(neighborIndex);
				listView.Items.Insert(neighborIndex + block.Count, neighbor);
			}
			else
			{
				var neighborIndex = neighbor.Index;
				var blockStart = block[0].Index;
				listView.Items.RemoveAt(neighborIndex);
				listView.Items.Insert(blockStart, neighbor);
			}
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

			using var dialog = new PageAliasDialog(siblingNames, favorite.Name,
				favorite.Alias ?? favorite.Name, Resx.ManageFavoritesControl_renameFavorite);

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
