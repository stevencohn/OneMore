//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Layouts
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
	/// Hosted by ManageWorkspaceDialog. Lets the user organize layouts and the windows within
	/// them, reorder them, rename their displayed alias, and delete layouts and windows. All
	/// edits are staged in memory; nothing is written to the database until Save() is
	/// called by the hosting dialog's OK handler.
	/// </summary>
	internal partial class ManageLayoutsControl : MoreUserControl
	{
		/// <summary>
		/// Marks a row that represents a layout rather than a window. LayoutID is the
		/// real database ID, or a negative sentinel for a layout created during this
		/// session but not yet persisted.
		/// </summary>
		private sealed class LayoutRow
		{
			public int LayoutID { get; set; }
			public string Name { get; set; }
		}


		private const int WindowIndent = 20;

		private readonly HashSet<int> deletedWindowIDs = new();
		private readonly HashSet<int> deletedLayoutIDs = new();
		private List<(int LayoutID, string Name)> originalLayouts = new();
		private List<(int ID, int LayoutID, string Alias, int Position)> originalWindows = new();
		private int nextSentinelLayoutID = -1;
		private bool isDirtyFromCheck;


		/// <summary>
		/// Raised after CheckLayouts completes a check, whether or not any layout windows
		/// were found to be invalid.
		/// </summary>
		public event EventHandler LayoutsChecked;


		/// <summary>
		/// Raised when the user clicks Restore Layout, carrying the selected layout's name.
		/// Restoring can open new OneNote windows, which requires OneNote's main window to
		/// not be disabled by this control's hosting dialog being shown modally - so this
		/// control only requests the restore; the hosting dialog closes itself first (saving
		/// pending edits, like OK) and the restore actually runs only once the dialog (and
		/// its modal hold on OneNote's window) is gone.
		/// </summary>
		public event EventHandler<string> RestoreRequested;


		public ManageLayoutsControl()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				nameColumn.Text = Resx.word_Name;
				locationColumn.Text = Resx.ManageLayoutsControl_locationColumn_HeaderText;
				sortButton.ToolTipText = Resx.ManageLayoutsControl_sort;
				upButton.ToolTipText = Resx.NavigatorWindow_menuMoveUp;
				downButton.ToolTipText = Resx.NavigatorWindow_menuMoveDown;
				captureLayoutButton.ToolTipText = Resx.ManageLayoutsControl_captureLayout;
				restoreButton.ToolTipText = Resx.ManageLayoutsControl_restore;
				deleteButton.ToolTipText = Resx.word_Delete;
				renameButton.ToolTipText = Resx.ManageLayoutsControl_renameWindow;
				renameMenuItem.Text = Resx.ManageLayoutsControl_renameWindow;
				checkButton.ToolTipText = Resx.ManageLayoutsControl_check;
				importButton.ToolTipText = Resx.ManageLayoutsControl_import;
				exportButton.ToolTipText = Resx.ManageLayoutsControl_export;
			}

			listView.SetColumnProportions(0.4f, 0.6f);
			listView.CanDragItem = item => item.Tag is LayoutWindow;
			listView.IsInsertionAnchor = item => item.Tag is LayoutRow;
			listView.GetCellStyle = GetCellStyle;
		}


		/// <summary>
		/// Loads the current layouts and their windows from the database into the working view.
		/// </summary>
		public void LoadLayouts()
		{
			using var provider = new LayoutsProvider();
			var collection = provider.ReadLayouts();

			LoadLayouts(collection);
		}


		private void LoadLayouts(LayoutsCollection collection)
		{
			listView.BeginUpdate();
			listView.Items.Clear();

			foreach (var layout in collection.Layouts)
			{
				AddLayoutRow(layout.LayoutID, layout.Name);
				foreach (var window in layout.Windows)
				{
					AddWindowRow(window);
				}
			}

			listView.EndUpdate();

			RefreshLayoutHints();
			originalLayouts = CurrentLayoutsShape();
			originalWindows = CurrentWindowsShape();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		/// <summary>
		/// Returns true if the user has made any change since LoadLayouts() was called.
		/// </summary>
		public bool IsDirty()
		{
			if (deletedWindowIDs.Count > 0 || deletedLayoutIDs.Count > 0 || isDirtyFromCheck)
			{
				return true;
			}

			if (listView.Items.Cast<ListViewItem>().Any(i => i.Tag is LayoutRow row && row.LayoutID < 0))
			{
				return true;
			}

			return !CurrentLayoutsShape().SequenceEqual(originalLayouts) ||
				!CurrentWindowsShape().SequenceEqual(originalWindows);
		}


		/// <summary>
		/// Commits all staged additions, deletions, and updates to the database.
		/// </summary>
		/// <returns>True if every change was saved successfully</returns>
		public bool Save()
		{
			using var provider = new LayoutsProvider();

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is LayoutRow row && row.LayoutID < 0)
				{
					var newID = provider.CreateLayout(row.Name);
					if (newID == 0)
					{
						return false;
					}

					var oldID = row.LayoutID;
					row.LayoutID = newID;

					foreach (ListViewItem windowItem in listView.Items)
					{
						if (windowItem.Tag is LayoutWindow window && window.LayoutID == oldID)
						{
							window.LayoutID = newID;
						}
					}
				}
				else if (item.Tag is LayoutRow existingRow && existingRow.LayoutID > 0)
				{
					if (!provider.RenameLayout(existingRow.LayoutID, existingRow.Name))
					{
						return false;
					}
				}
			}

			// must persist remaining windows' (possibly changed) LayoutID before deleting any
			// layout below: DeleteLayout cascades by deleting every window row still pointing
			// at that layoutID in the database, and a window the user just dragged out of the
			// layout still has its old, stale layoutID in the database until UpdateWindow runs -
			// deleting the layout first would catch it too.
			var positionWithinLayout = new Dictionary<int, int>();
			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is LayoutWindow window)
				{
					var position = positionWithinLayout.TryGetValue(window.LayoutID, out var p) ? p : 0;
					positionWithinLayout[window.LayoutID] = position + 1;

					window.ZOrder = position;

					if (window.ID == 0)
					{
						if (!provider.WriteWindow(window))
						{
							return false;
						}
					}
					else if (!provider.UpdateWindow(window))
					{
						return false;
					}
				}
			}

			foreach (var layoutID in deletedLayoutIDs)
			{
				if (!provider.DeleteLayout(layoutID))
				{
					return false;
				}
			}

			foreach (var windowID in deletedWindowIDs)
			{
				if (!provider.DeleteWindow(windowID))
				{
					return false;
				}
			}

			return true;
		}


		private static MoreListView.CellStyle GetCellStyle(ListViewItem item, int columnIndex)
		{
			if (item.Tag is LayoutWindow window)
			{
				var indent = columnIndex == 0 ? WindowIndent : 0;
				var foreColorKey =
					window.Status is TargetStatus.Suspect or TargetStatus.Unknown
						? "ErrorText"
						: null;

				return new MoreListView.CellStyle(indent, false, foreColorKey);
			}

			if (columnIndex == 1 && item.SubItems.Count > 1 &&
				item.SubItems[1].Text == Resx.ManageLayoutsControl_emptyLayoutHint)
			{
				return new MoreListView.CellStyle(0, true);
			}

			return MoreListView.CellStyle.Default;
		}


		private async void ImportLayouts(object sender, EventArgs e)
		{
			if (IsDirty() && MoreMessageBox.Show(this, Resx.ManageWorkspaceDialog_discard,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			var command = new ImportLayoutsCommand();
			command.SetLogger(Logger.Current);
			command.SetOwner(this);
			await command.Execute();

			LoadLayouts();
		}


		private async void ExportLayouts(object sender, EventArgs e)
		{
			var command = new ExportLayoutsCommand();
			command.SetLogger(Logger.Current);
			command.SetOwner(this);
			await command.Execute();
		}


		private async void CheckLayouts(object sender, EventArgs e)
		{
			if (!listView.Items.Cast<ListViewItem>().Any(i => i.Tag is LayoutWindow))
			{
				return;
			}

			var collection = RebuildCollection();

			await using var checker = new TargetChecker(Logger.Current);
			if (await checker.InvalidLayoutWindow(collection))
			{
				LoadLayouts(collection);
				isDirtyFromCheck = true;
			}
			else
			{
				listView.Invalidate();
			}

			LayoutsChecked?.Invoke(this, EventArgs.Empty);
		}


		/// <summary>
		/// Reconstructs a LayoutsCollection from the layouts/windows currently shown in the
		/// listview, reusing the same LayoutWindow/LayoutRow instances so in-place edits made
		/// by the caller (e.g. TargetChecker) are reflected immediately in the listview.
		/// </summary>
		private LayoutsCollection RebuildCollection()
		{
			var collection = new LayoutsCollection();
			var layouts = new Dictionary<int, Layout>();

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is LayoutRow row)
				{
					var layout = new Layout { LayoutID = row.LayoutID, Name = row.Name };
					collection.Layouts.Add(layout);
					layouts[row.LayoutID] = layout;
				}
				else if (item.Tag is LayoutWindow window &&
					layouts.TryGetValue(window.LayoutID, out var layout))
				{
					layout.Windows.Add(window);
				}
			}

			return collection;
		}


		private void ListViewItemMoved(object sender, MoreListView.ItemMovedEventArgs e)
		{
			if (e.Item.Tag is LayoutWindow window)
			{
				var contextTag = e.PrecedingItem?.Tag;
				if (contextTag is null)
				{
					// Unlike Favorites, layouts have no "root" to fall back on - every window
					// must belong to some layout. MovedToEnd means the drop landed below the
					// last row; look at what is now immediately above the moved item itself
					// (post-move) rather than leaving the window orphaned from any layout.
					var index = e.Item.Index - 1;
					contextTag = index >= 0 ? listView.Items[index].Tag : null;
				}

				window.LayoutID = contextTag switch
				{
					LayoutRow row => row.LayoutID,
					LayoutWindow precedingWindow => precedingWindow.LayoutID,
					_ => window.LayoutID
				};
			}

			RefreshLayoutHints();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		private ListViewItem AddLayoutRow(int layoutID, string name)
		{
			return AddLayoutRowAt(listView.Items.Count, layoutID, name);
		}


		private ListViewItem AddLayoutRowAt(int index, int layoutID, string name)
		{
			var item = new ListViewItem(name)
			{
				Tag = new LayoutRow { LayoutID = layoutID, Name = name },
				Font = new Font(listView.Font, FontStyle.Bold)
			};

			item.SubItems.Add(string.Empty);
			listView.Items.Insert(index, item);
			return item;
		}


		private ListViewItem AddWindowRow(LayoutWindow window)
		{
			return AddWindowRowAt(listView.Items.Count, window);
		}


		private ListViewItem AddWindowRowAt(int index, LayoutWindow window)
		{
			var item = new ListViewItem(window.Alias ?? window.Name) { Tag = window };
			item.SubItems.Add(window.Location);
			listView.Items.Insert(index, item);
			return item;
		}


		private List<(int LayoutID, string Name)> CurrentLayoutsShape()
		{
			return listView.Items.Cast<ListViewItem>()
				.Where(i => i.Tag is LayoutRow)
				.Select(i => (((LayoutRow)i.Tag).LayoutID, ((LayoutRow)i.Tag).Name))
				.ToList();
		}


		private List<(int ID, int LayoutID, string Alias, int Position)> CurrentWindowsShape()
		{
			var result = new List<(int ID, int LayoutID, string Alias, int Position)>();
			var positionWithinLayout = new Dictionary<int, int>();

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is LayoutWindow window)
				{
					var position = positionWithinLayout.TryGetValue(window.LayoutID, out var p) ? p : 0;
					positionWithinLayout[window.LayoutID] = position + 1;

					result.Add((window.ID, window.LayoutID, window.Alias, position));
				}
			}

			return result;
		}


		private void RefreshLayoutHints()
		{
			var windowCountByLayout = new Dictionary<int, int>();
			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is LayoutWindow window)
				{
					windowCountByLayout[window.LayoutID] =
						windowCountByLayout.TryGetValue(window.LayoutID, out var count) ? count + 1 : 1;
				}
			}

			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is LayoutRow row)
				{
					item.SubItems[1].Text = windowCountByLayout.ContainsKey(row.LayoutID)
						? string.Empty
						: Resx.ManageLayoutsControl_emptyLayoutHint;
				}
			}
		}


		private int FindLayoutInsertIndex(string name)
		{
			foreach (ListViewItem item in listView.Items)
			{
				if (item.Tag is LayoutRow row &&
					string.Compare(name, row.Name, StringComparison.CurrentCultureIgnoreCase) < 0)
				{
					return item.Index;
				}
			}

			return listView.Items.Count;
		}


		private ListViewItem GetSingleSelectedWindowItem()
		{
			if (listView.SelectedItems.Count != 1)
			{
				return null;
			}

			var item = listView.SelectedItems[0];
			return item.Tag is LayoutWindow ? item : null;
		}


		private ListViewItem GetSingleSelectedRenamableItem()
		{
			if (listView.SelectedItems.Count != 1)
			{
				return null;
			}

			var item = listView.SelectedItems[0];
			return item.Tag is LayoutWindow or LayoutRow ? item : null;
		}


		private ListViewItem GetSingleSelectedLayoutItem()
		{
			if (listView.SelectedItems.Count != 1)
			{
				return null;
			}

			var item = listView.SelectedItems[0];
			return item.Tag is LayoutRow ? item : null;
		}


		private bool CanMove(ListViewItem item, int direction)
		{
			var neighborIndex = item.Index + direction;
			if (neighborIndex < 0 || neighborIndex >= listView.Items.Count)
			{
				return false;
			}

			var neighbor = listView.Items[neighborIndex];
			return neighbor.Tag is LayoutWindow neighborWindow &&
				neighborWindow.LayoutID == ((LayoutWindow)item.Tag).LayoutID;
		}


		private void RefreshToolbarState(object sender, EventArgs e)
		{
			var item = GetSingleSelectedWindowItem();

			upButton.Enabled = item != null && CanMove(item, -1);
			downButton.Enabled = item != null && CanMove(item, 1);
			renameButton.Enabled = GetSingleSelectedRenamableItem() != null;
			deleteButton.Enabled = listView.SelectedItems.Count > 0;
			restoreButton.Enabled = GetSingleSelectedLayoutItem() != null;
		}


		private void MoveUp(object sender, EventArgs e)
		{
			var item = GetSingleSelectedWindowItem();
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
			var item = GetSingleSelectedWindowItem();
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
			var targetLayoutID = listView.SelectedItems.Count == 1
				? listView.SelectedItems[0].Tag switch
				{
					LayoutWindow window => window.LayoutID,
					LayoutRow row => row.LayoutID,
					_ => 0
				}
				: 0;

			var matches = listView.Items.Cast<ListViewItem>()
				.Select((item, index) => (item, index))
				.Where(x => x.item.Tag is LayoutWindow window && window.LayoutID == targetLayoutID)
				.ToList();

			if (matches.Count < 2)
			{
				return;
			}

			var sorted = matches
				.Select(x => x.item)
				.OrderBy(i => (((LayoutWindow)i.Tag).Alias ?? ((LayoutWindow)i.Tag).Name).TrimLeadingIcons(),
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


		/// <summary>
		/// Prompts for a layout name, then captures every currently open OneNote window into
		/// it. An existing name's windows are replaced (staged in memory, like every other
		/// edit here) rather than blocked as a duplicate - that's how the user asks to
		/// overwrite a layout.
		/// </summary>
		private async void CaptureLayout(object sender, EventArgs e)
		{
			using var dialog = new RenameDialog(Enumerable.Empty<string>(), string.Empty,
				createTitle: Resx.ManageLayoutsControl_captureLayout,
				label: Resx.word_Name);

			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			var name = dialog.Value;

			await using var one = new OneNote();
			var captured = await SaveLayoutCommand.CaptureWindows(one);

			if (captured.Count == 0)
			{
				MoreMessageBox.ShowError(this, Resx.SaveLayoutCommand_noWindows);
				return;
			}

			var layoutItem = listView.Items.Cast<ListViewItem>()
				.FirstOrDefault(i => i.Tag is LayoutRow row &&
					string.Equals(row.Name, name, StringComparison.CurrentCultureIgnoreCase));

			int layoutID;

			listView.BeginUpdate();

			if (layoutItem is not null)
			{
				// overwrite: replace this layout's current windows with the captured snapshot
				layoutID = ((LayoutRow)layoutItem.Tag).LayoutID;

				var children = listView.Items.Cast<ListViewItem>()
					.Where(i => i.Tag is LayoutWindow window && window.LayoutID == layoutID)
					.ToList();

				foreach (var child in children)
				{
					if (((LayoutWindow)child.Tag).ID > 0)
					{
						deletedWindowIDs.Add(((LayoutWindow)child.Tag).ID);
					}

					listView.Items.Remove(child);
				}
			}
			else
			{
				layoutID = nextSentinelLayoutID--;
				layoutItem = AddLayoutRowAt(FindLayoutInsertIndex(name), layoutID, name);
			}

			var insertIndex = layoutItem.Index + 1;
			foreach (var window in captured.OrderBy(w => w.ZOrder))
			{
				window.LayoutID = layoutID;
				AddWindowRowAt(insertIndex++, window);
			}

			listView.EndUpdate();

			layoutItem.Selected = true;
			layoutItem.EnsureVisible();

			RefreshLayoutHints();
			RefreshToolbarState(this, EventArgs.Empty);
		}


		/// <summary>
		/// Requests that the selected layout be restored. The actual restore (which can open
		/// new OneNote windows and therefore can't run while this control's hosting dialog is
		/// modal) is deferred to whoever is listening for RestoreRequested.
		/// </summary>
		private void RestoreLayout(object sender, EventArgs e)
		{
			var item = GetSingleSelectedLayoutItem();
			if (item == null)
			{
				return;
			}

			RestoreRequested?.Invoke(this, ((LayoutRow)item.Tag).Name);
		}


		private void DeleteSelected(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0)
			{
				return;
			}

			var layoutRows = listView.SelectedItems.Cast<ListViewItem>()
				.Where(i => i.Tag is LayoutRow)
				.ToList();

			var windowRows = listView.SelectedItems.Cast<ListViewItem>()
				.Where(i => i.Tag is LayoutWindow)
				.ToList();

			var message = layoutRows.Count == 0 && windowRows.Count == 1
				? string.Format(Resx.ManageLayouts_DeleteMessage,
					((LayoutWindow)windowRows[0].Tag).Alias ?? ((LayoutWindow)windowRows[0].Tag).Name)
				: string.Format(Resx.ManageLayoutsControl_deleteConfirmMultiple,
					layoutRows.Count + windowRows.Count);

			if (MoreMessageBox.Show(this, message, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				!= DialogResult.Yes)
			{
				return;
			}

			listView.BeginUpdate();

			foreach (var row in layoutRows)
			{
				var layout = (LayoutRow)row.Tag;
				if (layout.LayoutID > 0)
				{
					deletedLayoutIDs.Add(layout.LayoutID);
				}

				var children = listView.Items.Cast<ListViewItem>()
					.Where(i => i.Tag is LayoutWindow window && window.LayoutID == layout.LayoutID)
					.ToList();

				foreach (var child in children)
				{
					deletedWindowIDs.Add(((LayoutWindow)child.Tag).ID);
					listView.Items.Remove(child);
				}

				listView.Items.Remove(row);
			}

			foreach (var row in windowRows)
			{
				if (!listView.Items.Contains(row))
				{
					// already removed as part of a deleted layout above
					continue;
				}

				deletedWindowIDs.Add(((LayoutWindow)row.Tag).ID);
				listView.Items.Remove(row);
			}

			listView.EndUpdate();

			RefreshLayoutHints();
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

			if (item.Tag is LayoutRow layout)
			{
				RenameSelectedLayout(item, layout);
				return;
			}

			var window = (LayoutWindow)item.Tag;

			var siblingNames = listView.Items.Cast<ListViewItem>()
				.Where(i => i.Tag is LayoutWindow other && other != window &&
					other.LayoutID == window.LayoutID)
				.Select(i => ((LayoutWindow)i.Tag).Alias ?? ((LayoutWindow)i.Tag).Name);

			using var dialog = new PageAliasDialog(siblingNames, window.Name,
				window.Alias ?? window.Name, Resx.ManageLayoutsControl_renameWindow);

			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			window.Alias = dialog.Value;
			item.Text = dialog.Value ?? window.Name;
		}


		private void RenameSelectedLayout(ListViewItem item, LayoutRow layout)
		{
			var siblingNames = listView.Items.Cast<ListViewItem>()
				.Where(i => i.Tag is LayoutRow other && other != layout)
				.Select(i => ((LayoutRow)i.Tag).Name);

			using var dialog = new RenameDialog(siblingNames, layout.Name,
				createTitle: Resx.ManageLayoutsControl_renameLayout,
				renameTitle: Resx.ManageLayoutsControl_renameLayout,
				label: Resx.word_Name)
			{
				Rename = true
			};

			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			layout.Name = dialog.Value;
			item.Text = dialog.Value;
		}


		private void ShowContextMenu(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
			{
				return;
			}

			var item = listView.GetItemAt(e.X, e.Y);
			if (item == null || item.Tag is not LayoutWindow)
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
