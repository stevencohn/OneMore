//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Linq;
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


		public FavoritesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Favorites;

				Localize(new string[]
				{
					"addButton",
					"manageButton",
					"searchLabel=word_Search",
					"goButton=word_Go",
					"cancelButton=word_Cancel"
				});

				nameColumn.Text = Resx.word_Name;
				locationColumn.Text = Resx.MangeFavoritesControl_locationColumn_HeaderText;
			}

			listView.SetColumnProportions(0.4f, 0.6f);
			listView.GetCellStyle = GetCellStyle;

			DefaultControl = searchBox;
		}


		public FavoritesDialog(IRibbonUI ribbon) : this()
		{
			this.ribbon = ribbon;
		}


		private async void BindOnLoad(object sender, EventArgs e)
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


		private void FocusOnActivated(object sender, EventArgs e)
		{
			searchBox.Focus();
		}


		public bool Manage { get; private set; }


		public string Uri { get; private set; }


		private static MoreListView.CellStyle GetCellStyle(ListViewItem item, int columnIndex)
		{
			if (item.Tag is Favorite favorite)
			{
				var indent = columnIndex == 0 && favorite.FolderID != 0 ? FavoriteIndent : 0;
				return new MoreListView.CellStyle(indent, false);
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
						e.Handled = SelectNextRow();
						break;

					case Keys.Up:
						e.Handled = SelectPreviousRow();
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
			listView.Items[index].Selected = true;
			listView.Items[index].EnsureVisible();
			ShowText();
		}


		private void ShowText()
		{
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
			if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0].Tag is Favorite)
			{
				ShowText();
			}
		}


		private void ChooseByClick(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0 ||
				listView.SelectedItems[0].Tag is not Favorite favorite)
			{
				return;
			}

			Uri = favorite.Uri;
		}


		private void ChooseByDoubleClick(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0 ||
				listView.SelectedItems[0].Tag is not Favorite)
			{
				return;
			}

			ChooseByClick(null, null);
			DialogResult = DialogResult.OK;
			Close();
		}


		private void ChooseByKeyboard(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
			{
				return;
			}

			if (listView.SelectedItems.Count == 0 ||
				listView.SelectedItems[0].Tag is not Favorite)
			{
				return;
			}

			ChooseByClick(null, null);
			DialogResult = DialogResult.OK;
			Close();
		}

		private void ShowMenu(object sender, EventArgs e)
		{
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
	}
}
