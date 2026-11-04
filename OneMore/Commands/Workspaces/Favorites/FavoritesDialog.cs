//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Microsoft.Office.Core;
	using Resx = Properties.Resources;


	internal partial class FavoritesDialog : UI.MoreForm
	{
		/// <summary>
		/// Marks a row that represents a folder rather than a favorite.
		/// </summary>
		private sealed class FolderRow
		{
			public int FolderID { get; set; }
			public string Name { get; set; }
		}


		private const int FavoriteIndent = 20;

		private readonly IRibbonUI ribbon;
		private FavoritesCollection collection;
		private string _programmaticText = string.Empty;

		// favorite ID -> handle of the OneNote window that is already showing it
		private Dictionary<int, IntPtr> openWindows = new();

		// the row where a Shift+arrow range starts and the row it currently reaches
		private int anchorIndex = -1;
		private int caretIndex = -1;

		private bool adjustingSelection;
		private bool goIsWide;
		private int goNormalWidth;
		private int goGap;


		public FavoritesDialog()
		{
			InitializeComponent();
			RememberSize = true;

			if (NeedsLocalizing())
			{
				Text = Resx.word_Favorites;

				Localize(new string[]
				{
					"addButton",
					"manageButton",
					"deleteButton=word_Delete",
					"searchLabel=word_Search",
					"goButton=word_Go",
					"cancelButton=word_Cancel",
					"goItem=word_Go",
					"newWindowItem=NavigatorWindow_menuOpenInNew",
					"closeBox=FavoritesDialog_closeExisting"
				});

				nameColumn.Text = Resx.word_Name;
				locationColumn.Text = Resx.MangeFavoritesControl_locationColumn_HeaderText;
			}

			listView.SetColumnProportions(0.4f, 0.6f);
			listView.GetCellStyle = GetCellStyle;
			listView.GetCellImage = (item, col) =>
				col == 0 && item.Tag is Favorite f ? FavoriteKindGlyphs.GetGlyph(f) : null;

			DefaultControl = searchBox;
		}


		public FavoritesDialog(IRibbonUI ribbon) : this()
		{
			this.ribbon = ribbon;
		}


		private void BindOnLoad(object sender, EventArgs e)
		{
			using var provider = new FavoritesProvider();
			collection = provider.ReadFavorites();

			Populate(string.Empty);

			var index = FirstFavoriteIndex();
			if (index >= 0)
			{
				listView.Items[index].Selected = true;
				listView.Items[index].EnsureVisible();
			}
		}


		protected override async void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// highlight what is already open; done after the dialog is on screen, because asking
			// OneNote about its windows takes longer the more windows are open
			await Task.Delay(50);

			if (IsDisposed || collection is null)
			{
				return;
			}

			var favorites = collection.Folders.SelectMany(f => f.Items).Concat(collection.Items);
			await OpenFavoriteWindows.Find(favorites, found =>
			{
				if (!IsDisposed)
				{
					openWindows = found;
					listView.Invalidate();
				}
			});
		}


		private void FocusOnActivated(object sender, EventArgs e)
		{
			searchBox.Focus();
		}


		public bool Manage { get; private set; }


		public string Uri { get; private set; }


		/// <summary>
		/// Gets the favorite the user chose, so it can be resolved if its link no longer opens.
		/// </summary>
		public Favorite Selected { get; private set; }


		/// <summary>
		/// Gets whether the favorites in <see cref="Targets"/> are to be opened in new windows
		/// rather than navigating the current window to <see cref="Selected"/>.
		/// </summary>
		public bool NewWindows { get; private set; }


		/// <summary>
		/// Gets the favorites to open in new windows: the selected favorites or, if only a folder
		/// was selected, the favorites in the folder.
		/// </summary>
		public IReadOnlyList<Favorite> Targets { get; private set; }


		/// <summary>
		/// Gets whether a single favorite chosen from the menu is to get a new window even if
		/// another window already shows it.
		/// </summary>
		public bool AllowDuplicate { get; private set; }


		/// <summary>
		/// Gets whether to close the OneNote windows that do not show a target.
		/// </summary>
		public bool CloseExisting { get; private set; }


		/// <summary>
		/// Gets the window that already shows <see cref="Selected"/>, to be raised instead of
		/// navigating, or IntPtr.Zero if there is none.
		/// </summary>
		public IntPtr RaiseHandle { get; private set; }


		private MoreListView.CellStyle GetCellStyle(ListViewItem item, int columnIndex)
		{
			if (item.Tag is Favorite favorite)
			{
				var indent = columnIndex == 0 && favorite.FolderID != 0 ? FavoriteIndent : 0;

				// a favorite that is already open in a window is drawn in the link color
				return openWindows.ContainsKey(favorite.ID)
					? new MoreListView.CellStyle(indent, false, "LinkColor")
					: new MoreListView.CellStyle(indent, false);
			}

			return MoreListView.CellStyle.Default;
		}


		/// <summary>
		/// Rebuilds the list from the in-memory collection, showing only favorites matching
		/// the given filter text (or all favorites when the filter is too short) and only the
		/// folders that still have at least one matching favorite.
		/// </summary>
		private void Populate(string filterText)
		{
			if (collection == null)
			{
				return;
			}

			var text = filterText.Trim();
			var filtering = text.Length > 1;

			listView.BeginUpdate();
			listView.Items.Clear();

			foreach (var folder in collection.Folders)
			{
				var matches = filtering
					? folder.Items.Where(f => Matches(f, text)).ToList()
					: folder.Items;

				if (matches.Count == 0)
				{
					continue;
				}

				AddFolderRow(folder.FolderID, folder.Name);

				foreach (var favorite in matches)
				{
					AddFavoriteRow(favorite);
				}
			}

			var rootMatches = filtering
				? collection.Items.Where(f => Matches(f, text)).ToList()
				: collection.Items;

			foreach (var favorite in rootMatches)
			{
				AddFavoriteRow(favorite);
			}

			listView.EndUpdate();
		}


		private static bool Matches(Favorite favorite, string text)
		{
			return (favorite.Alias ?? favorite.Name).ContainsICIC(text) ||
				favorite.Location.ContainsICIC(text);
		}


		private void AddFolderRow(int folderID, string name)
		{
			var item = new ListViewItem(name)
			{
				Tag = new FolderRow { FolderID = folderID, Name = name },
				Font = new Font(listView.Font, FontStyle.Bold)
			};

			item.SubItems.Add(string.Empty);
			listView.Items.Add(item);
		}


		private void AddFavoriteRow(Favorite favorite)
		{
			var item = new ListViewItem(favorite.Alias ?? favorite.Name) { Tag = favorite };
			item.SubItems.Add(favorite.Location);
			listView.Items.Add(item);
		}


		private void FilterRowOnKeyUp(object sender, KeyEventArgs e)
		{
			if (listView.Items.Count > 0)
			{
				switch (e.KeyCode)
				{
					case Keys.Down:
						e.Handled = e.Shift ? ExtendSelection(1) : SelectNextRow();
						break;

					case Keys.Up:
						e.Handled = e.Shift ? ExtendSelection(-1) : SelectPreviousRow();
						break;

					case Keys.PageDown:
						e.Handled = MovePageDown();
						break;

					case Keys.PageUp:
						e.Handled = MovePageUp();
						break;

					case Keys.Home:
						if (e.Modifiers == 0)
						{
							e.Handled = MoveTop();
						}
						break;

					case Keys.End:
						if (e.Modifiers == 0)
						{
							e.Handled = MoveBottom();
						}
						break;

					case Keys.Left:
					case Keys.Right:
						if (e.Modifiers == 0)
						{
							e.Handled = true;
						}
						break;
				}

				if (e.Handled)
				{
					return;
				}
			}

			if (Char.IsControl((char)e.KeyValue) &&
				e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back)
			{
				e.Handled = true;
				return;
			}

			// A stale WM_KEYUP (no preceding WM_CHAR) leaves the text unchanged from what
			// ShowText set — skip filtering. Once the user actually types, WM_CHAR changes
			// the text first so this check passes and _programmaticText is cleared.
			if (_programmaticText.Length > 0 && searchBox.Text == _programmaticText)
			{
				e.Handled = true;
				return;
			}

			_programmaticText = string.Empty;

			// filter list based on search text; preserve the selected favorite by ID since
			// rebuilding the list invalidates row indices

			var selectedID = listView.SelectedItems.Count > 0 &&
				listView.SelectedItems[0].Tag is Favorite selected
					? selected.ID
					: (int?)null;

			Populate(searchBox.Text);

			var index = selectedID.HasValue ? IndexOfFavorite(selectedID.Value) : -1;
			if (index < 0)
			{
				index = FirstFavoriteIndex();
			}

			if (index >= 0)
			{
				listView.Items[index].Selected = true;
				listView.Items[index].EnsureVisible();
			}

			e.Handled = true;
		}


		private int IndexOfFavorite(int id)
		{
			for (var i = 0; i < listView.Items.Count; i++)
			{
				if (listView.Items[i].Tag is Favorite favorite && favorite.ID == id)
				{
					return i;
				}
			}

			return -1;
		}


		/// <summary>
		/// Scans from start in the given direction (1 or -1), skipping folder rows, and
		/// returns the index of the next favorite row, or -1 if there isn't one.
		/// </summary>
		private int AdjacentFavoriteIndex(int start, int direction)
		{
			var i = start;
			do
			{
				i += direction;
			}
			while (i >= 0 && i < listView.Items.Count && listView.Items[i].Tag is not Favorite);

			return i >= 0 && i < listView.Items.Count ? i : -1;
		}


		private int FirstFavoriteIndex()
		{
			return AdjacentFavoriteIndex(-1, 1);
		}


		private int LastFavoriteIndex()
		{
			return AdjacentFavoriteIndex(listView.Items.Count, -1);
		}


		private int PageSize()
		{
			if (listView.Items.Count == 0)
			{
				return 1;
			}

			var rowHeight = listView.Items[0].Bounds.Height;
			return rowHeight > 0 ? Math.Max(1, listView.ClientSize.Height / rowHeight) : 1;
		}


		private void SelectRow(int index)
		{
			// the list allows many rows to be selected, so selecting one does not unselect the rest
			adjustingSelection = true;
			listView.SelectedItems.Clear();
			listView.Items[index].Selected = true;
			adjustingSelection = false;

			anchorIndex = caretIndex = index;

			listView.Items[index].EnsureVisible();
			OnSelectionChanged(null, null);
			ShowText();
		}


		/// <summary>
		/// Shift+Down and Shift+Up: moves the end of the selection one favorite and selects every
		/// favorite between there and the row where the range started.
		/// </summary>
		private bool ExtendSelection(int direction)
		{
			if (listView.SelectedItems.Count == 0)
			{
				return MoveTop();
			}

			if (anchorIndex < 0 || anchorIndex >= listView.Items.Count ||
				caretIndex < 0 || caretIndex >= listView.Items.Count)
			{
				anchorIndex = caretIndex = listView.SelectedItems[0].Index;
			}

			var next = AdjacentFavoriteIndex(caretIndex, direction);
			if (next < 0)
			{
				return true;
			}

			caretIndex = next;

			var first = Math.Min(anchorIndex, caretIndex);
			var last = Math.Max(anchorIndex, caretIndex);

			adjustingSelection = true;
			listView.BeginUpdate();

			for (var i = 0; i < listView.Items.Count; i++)
			{
				var item = listView.Items[i];
				var selected = i >= first && i <= last && item.Tag is Favorite;
				if (item.Selected != selected)
				{
					item.Selected = selected;
				}
			}

			listView.EndUpdate();
			adjustingSelection = false;

			listView.Items[caretIndex].EnsureVisible();
			OnSelectionChanged(null, null);
			return true;
		}


		private int CountSelectedFavorites()
		{
			var count = 0;
			foreach (ListViewItem item in listView.SelectedItems)
			{
				if (item.Tag is Favorite)
				{
					count++;
				}
			}

			return count;
		}


		// gets the folder if it is the only thing selected
		private FolderRow SelectedFolder()
		{
			return listView.SelectedItems.Count == 1 &&
				listView.SelectedItems[0].Tag is FolderRow folder
					? folder
					: null;
		}


		// gets whether choosing the selection opens favorites in new windows
		private bool IsOpeningMany()
		{
			return SelectedFolder() is not null || CountSelectedFavorites() > 1;
		}


		private void OnSelectionChanged(object sender, EventArgs e)
		{
			if (adjustingSelection)
			{
				return;
			}

			// a folder can be chosen only by itself
			if (listView.SelectedItems.Count > 1)
			{
				var folders = new List<ListViewItem>();
				foreach (ListViewItem item in listView.SelectedItems)
				{
					if (item.Tag is FolderRow)
					{
						folders.Add(item);
					}
				}

				if (folders.Count > 0)
				{
					adjustingSelection = true;
					foreach (var item in folders)
					{
						item.Selected = false;
					}
					adjustingSelection = false;
				}
			}

			var many = IsOpeningMany();

			goButton.Text = many ? Resx.FavoritesDialog_openInNewWindows : Resx.word_Go;
			closeBox.Visible = many;

			if (many != goIsWide)
			{
				if (many)
				{
					goNormalWidth = goButton.Width;
					goGap = cancelButton.Left - goButton.Right;
				}

				var width = many ? goNormalWidth * 8 / 5 : goNormalWidth;
				goButton.SetBounds(
					cancelButton.Left - goGap - width, goButton.Top, width, goButton.Height);

				goIsWide = many;
			}
		}


		private void ShowText()
		{
			if (listView.SelectedItems.Count != 1)
			{
				return;
			}

			_programmaticText = listView.SelectedItems[0].Text;
			searchBox.Text = _programmaticText;
			searchBox.Select(searchBox.Text.Length, 0);
		}


		private bool MoveBottom()
		{
			var index = LastFavoriteIndex();
			if (index < 0)
			{
				return false;
			}

			SelectRow(index);
			return true;
		}


		private bool MoveTop()
		{
			var index = FirstFavoriteIndex();
			if (index < 0)
			{
				return false;
			}

			SelectRow(index);
			return true;
		}


		private bool MovePageDown()
		{
			if (listView.SelectedItems.Count == 0)
			{
				return MoveTop();
			}

			var displayed = PageSize();
			var index = listView.SelectedItems[0].Index;

			while (displayed > 0)
			{
				var next = AdjacentFavoriteIndex(index, 1);
				if (next < 0)
				{
					break;
				}

				index = next;
				displayed--;
			}

			SelectRow(index);
			return true;
		}


		private bool MovePageUp()
		{
			if (listView.SelectedItems.Count == 0)
			{
				return MoveTop();
			}

			var displayed = PageSize();
			var index = listView.SelectedItems[0].Index;

			while (displayed > 0)
			{
				var previous = AdjacentFavoriteIndex(index, -1);
				if (previous < 0)
				{
					break;
				}

				index = previous;
				displayed--;
			}

			SelectRow(index);
			return true;
		}


		private bool SelectNextRow()
		{
			if (listView.SelectedItems.Count == 0)
			{
				return MoveTop();
			}

			var index = AdjacentFavoriteIndex(listView.SelectedItems[0].Index, 1);
			if (index >= 0)
			{
				SelectRow(index);
			}

			return true;
		}


		private bool SelectPreviousRow()
		{
			if (listView.SelectedItems.Count == 0)
			{
				return MoveTop();
			}

			var index = AdjacentFavoriteIndex(listView.SelectedItems[0].Index, -1);
			if (index >= 0)
			{
				SelectRow(index);
			}

			return true;
		}


		private void RefocusOnGotFocus(object sender, EventArgs e)
		{
			searchBox.Focus();
		}


		private async void AddCurrentPage(object sender, EventArgs e)
		{
			await using var one = new OneNote();
			var info = await one.GetPageInfo();
			if (info == null)
			{
				return;
			}

			var favorite = collection.Folders.SelectMany(f => f.Items)
				.Concat(collection.Items)
				.FirstOrDefault(f => f.PageID == info.PageId);

			if (favorite == null)
			{
				var cmd = new AddFavoriteCommand();
				await cmd.Execute(one.CurrentNotebookId, one.CurrentSectionId, one.CurrentPageId);

				using var provider = new FavoritesProvider();
				collection = provider.ReadFavorites();

				favorite = collection.Folders.SelectMany(f => f.Items)
					.Concat(collection.Items)
					.FirstOrDefault(f => f.PageID == info.PageId);
			}

			searchBox.Clear();
			Populate(string.Empty);

			if (favorite != null)
			{
				var index = IndexOfFavorite(favorite.ID);
				if (index >= 0)
				{
					SelectRow(index);
				}
			}
		}


		private void ClickShowText(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 1 && listView.SelectedItems[0].Tag is Favorite)
			{
				anchorIndex = caretIndex = listView.SelectedItems[0].Index;
				ShowText();
			}
		}


		// the Go button: navigates the current window to a single favorite, or opens many
		private void ChooseByClick(object sender, EventArgs e)
		{
			Choose(false);
		}


		/// <summary>
		/// Records what the user chose in the result properties.
		/// </summary>
		/// <param name="newWindow">True to open a single favorite in a new window; many favorites
		/// and a folder always open in new windows</param>
		/// <returns>True if there is something to open</returns>
		private bool Choose(bool newWindow)
		{
			Selected = null;
			Uri = null;
			Targets = null;
			NewWindows = false;
			CloseExisting = false;
			AllowDuplicate = false;
			RaiseHandle = IntPtr.Zero;

			List<Favorite> favorites;

			var folder = SelectedFolder();
			if (folder is not null)
			{
				favorites = new List<Favorite>();
				foreach (ListViewItem item in listView.Items)
				{
					if (item.Tag is Favorite f && f.FolderID == folder.FolderID)
					{
						favorites.Add(f);
					}
				}

				newWindow = true;
			}
			else
			{
				favorites = new List<Favorite>();
				foreach (ListViewItem item in listView.SelectedItems)
				{
					if (item.Tag is Favorite f)
					{
						favorites.Add(f);
					}
				}
			}

			if (favorites.Count == 0)
			{
				return false;
			}

			if (newWindow || favorites.Count > 1)
			{
				AllowDuplicate = newWindow && folder is null && favorites.Count == 1;
				Targets = favorites;
				NewWindows = true;
				CloseExisting = closeBox.Visible && closeBox.Checked;
				return true;
			}

			Selected = favorites[0];
			Uri = Selected.GetNavigationTarget();

			if (openWindows.TryGetValue(Selected.ID, out var handle))
			{
				RaiseHandle = handle;
			}

			return true;
		}


		private void ChooseByDoubleClick(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count != 1 ||
				listView.SelectedItems[0].Tag is not Favorite)
			{
				return;
			}

			if (Choose(false))
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}


		private void ChooseByKeyboard(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
			{
				return;
			}

			if (Choose(false))
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}


		private void ShowRowMenu(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
			{
				return;
			}

			// right-clicking a row that is not selected selects just that row; right-clicking one
			// that is selected keeps a multiple selection
			var hit = listView.GetItemAt(e.X, e.Y);
			if (hit is null)
			{
				return;
			}

			if (!hit.Selected)
			{
				if (hit.Tag is Favorite)
				{
					SelectRow(hit.Index);
				}
				else
				{
					listView.SelectedItems.Clear();
					hit.Selected = true;
				}
			}

			var many = IsOpeningMany();
			var single = !many && CountSelectedFavorites() == 1;
			if (!many && !single)
			{
				return;
			}

			goItem.Visible = single;
			newWindowItem.Visible = true;
			rowMenu.Show(listView, e.Location);
		}


		private void GoFromMenu(object sender, EventArgs e)
		{
			if (Choose(false))
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}


		private void OpenInNewWindowFromMenu(object sender, EventArgs e)
		{
			if (Choose(true))
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}


		private void ShowMenu(object sender, EventArgs e)
		{
			deleteButton.Enabled =
				listView.SelectedItems.Count == 1 &&
				listView.SelectedItems[0].Tag is Favorite;

			contextMenu.Show(menuButton, new Point(
				-(contextMenu.Width - menuButton.Width),
				menuButton.Height));
		}


		private void ManageFavorites(object sender, EventArgs e)
		{
			Manage = true;
			DialogResult = DialogResult.OK;
			Close();
		}


		private void DeleteFavorite(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count != 1 ||
				listView.SelectedItems[0].Tag is not Favorite favorite)
			{
				return;
			}

			var message =string.Format(
				Resx.ManageFavorites_DeleteMessage, favorite.Alias ?? favorite.Name);

			if (MoreMessageBox.Show(this, message, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				!= DialogResult.Yes)
			{
				return;
			}

			using var provider = new FavoritesProvider();
			if (!provider.DeleteFavorite(favorite.ID))
			{
				return;
			}

			var folder = collection.Folders.FirstOrDefault(f => f.FolderID == favorite.FolderID);
			if (folder != null)
			{
				folder.Items.Remove(favorite);
			}
			else
			{
				collection.Items.Remove(favorite);
			}

			searchBox.Clear();
			Populate(string.Empty);

			var index = FirstFavoriteIndex();
			if (index >= 0)
			{
				listView.Items[index].Selected = true;
				listView.Items[index].EnsureVisible();
			}
		}
	}
}
