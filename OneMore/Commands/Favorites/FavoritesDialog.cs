//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Favorite = FavoritesProvider.Favorite;
	using FavoriteStatus = FavoritesProvider.FavoriteStatus;
	using Resx = Properties.Resources;


	internal partial class FavoritesDialog : UI.MoreForm
	{

		public FavoritesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Favorites;

				Localize(new string[]
				{
					"addButton",
					"checkButton",
					"manageButton",
					"searchLabel=word_Search",
					"goButton=word_Go",
					"cancelButton=word_Cancel"
				});

				nameColumn.HeaderText = Resx.word_Name;
				locationColumn.HeaderText = Resx.FavoritesSheet_locationColumn_HeaderText;
			}

			DefaultControl = searchBox;
		}


		private async void BindOnLoad(object sender, EventArgs e)
		{
			//Native.SwitchToThisWindow(Handle, false);

			await using var provider = new FavoritesProvider(null);
			var favorites = provider.LoadFavorites();

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			gridView.Columns[1].DataPropertyName = "Location";
			gridView.DataSource = new BindingList<Favorite>(favorites);
		}


		private void FocusOnActivated(object sender, EventArgs e)
		{
			searchBox.Focus();
		}


		public bool Manage { get; private set; }


		public string Uri { get; private set; }


		private void ValidateOnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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


		private void FilterRowOnKeyUp(object sender, KeyEventArgs e)
		{
			if (gridView.Rows.Count == 0)
			{
				return;
			}

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

			if (Char.IsControl((char)e.KeyValue) &&
				e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back)
			{
				e.Handled = true;
				return;
			}

			// filter list based on search text...

			var text = searchBox.Text.Trim();

			var selected = gridView.SelectedCells.Count > 0
				? gridView.SelectedCells[0].RowIndex
				: -1;

			// must suspend currency manager in order to hide selected or remaining rows
			var mgr = (CurrencyManager)BindingContext[gridView.DataSource];
			mgr.SuspendBinding();

			if (text.Length > 2)
			{
				foreach (DataGridViewRow row in gridView.Rows)
				{
					if (row.Cells[0].Value.ToString().ContainsICIC(text) ||
						row.Cells[1].Value.ToString().ContainsICIC(text))
					{
						row.Visible = true;
					}
					else
					{
						row.Cells[0].Selected = row.Cells[1].Selected = false;
						row.Selected = false;
						row.Visible = false;
					}
				}
			}
			else
			{
				foreach (DataGridViewRow row in gridView.Rows)
				{
					row.Visible = true;
				}
			}

			mgr.ResumeBinding();

			// ensure there is a selection...

			if (selected < 0 || text.Length < 3)
			{
				// previously was no selection, this should force first visible
				var first = gridView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
				if (first >= 0)
				{
					gridView.Rows[first].Cells[0].Selected = true;
				}
			}
			else
			{
				// find visible row above starting position
				selected = gridView.Rows.GetPreviousRow(selected, DataGridViewElementStates.Visible);
				if (selected < 0)
				{
					selected = gridView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
				}

				if (selected >= 0)
				{
					gridView.Rows[selected].Cells[0].Selected = true;
				}
			}

			e.Handled = true;
		}


		private void ShowText()
		{
			searchBox.Text = (string)gridView.SelectedCells[0].Value;
			searchBox.Select(searchBox.Text.Length, 0);
		}


		private bool MoveBottom()
		{
			if (gridView.Rows.Count == 0)
			{
				return true;
			}

			var index = gridView.Rows.GetLastRow(DataGridViewElementStates.Visible);
			if (index >= 0)
			{
				gridView.Rows[index].Cells[0].Selected = true;
				ShowText();
				return true;
			}

			return false;
		}


		private bool MoveTop()
		{
			if (gridView.Rows.Count == 0)
			{
				return true;
			}

			var index = gridView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
			if (index >= 0)
			{
				gridView.Rows[index].Cells[0].Selected = true;
				ShowText();
				return true;
			}

			return false;
		}


		private bool MovePageDown()
		{
			if (gridView.Rows.Count == 0)
			{
				return true;
			}

			if (gridView.SelectedCells.Count == 0)
			{
				var first = gridView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
				gridView.Rows[first].Cells[0].Selected = true;
				ShowText();
			}
			else
			{
				var displayed = gridView.DisplayedRowCount(true);
				var index = gridView.SelectedCells[0].RowIndex;

				while (displayed >= 0 && index > 0 && index < gridView.Rows.Count)
				{
					index = gridView.Rows.GetNextRow(index, DataGridViewElementStates.Visible);
					displayed--;
				}

				if (index < 0 || index > gridView.Rows.Count)
				{
					index = gridView.Rows.GetLastRow(DataGridViewElementStates.Visible);
				}

				gridView.Rows[index].Cells[0].Selected = true;
				ShowText();
			}

			return true;
		}


		private bool MovePageUp()
		{
			if (gridView.Rows.Count == 0)
			{
				return true;
			}

			if (gridView.SelectedCells.Count == 0)
			{
				var first = gridView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
				gridView.Rows[first].Cells[0].Selected = true;
				ShowText();
			}
			else
			{
				var displayed = gridView.DisplayedRowCount(true);
				var index = gridView.SelectedCells[0].RowIndex;

				while (displayed >= 0 && index >= 0)
				{
					index = gridView.Rows.GetPreviousRow(index, DataGridViewElementStates.Visible);
					displayed--;
				}

				if (index < 0)
				{
					index = gridView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
				}

				gridView.Rows[index].Cells[0].Selected = true;
				ShowText();
			}

			return true;
		}


		private bool SelectNextRow()
		{
			if (gridView.SelectedCells.Count == 0)
			{
				var first = gridView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
				if (first >= 0)
				{
					gridView.Rows[first].Cells[0].Selected = true;
					ShowText();
				}

				return true;
			}

			var start = gridView.SelectedCells[0].RowIndex;
			var index = gridView.Rows.GetNextRow(start, DataGridViewElementStates.Visible);
			if (index > 0)
			{
				gridView.Rows[index].Cells[0].Selected = true;
				ShowText();
			}
			else
			{
				index = gridView.Rows.GetPreviousRow(start, DataGridViewElementStates.Visible);
				if (index >= 0)
				{
					gridView.Rows[index].Cells[0].Selected = true;
					ShowText();
				}
			}

			return true;
		}


		private bool SelectPreviousRow()
		{
			if (gridView.SelectedCells.Count == 0)
			{
				var first = gridView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
				if (first >= 0)
				{
					gridView.Rows[first].Cells[0].Selected = true;
					ShowText();
				}

				return true;
			}

			var start = gridView.SelectedCells[0].RowIndex;
			var index = gridView.Rows.GetPreviousRow(start, DataGridViewElementStates.Visible);
			if (index >= 0)
			{
				gridView.Rows[index].Cells[0].Selected = true;
				ShowText();
			}
			else
			{
				index = gridView.Rows.GetNextRow(start, DataGridViewElementStates.Visible);
				if (index > 0)
				{
					gridView.Rows[index].Cells[0].Selected = true;
					ShowText();
				}
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

			var source = gridView.DataSource as BindingList<Favorite>;
			var index = 0;
			Favorite favorite = null;
			while (index < source.Count && favorite == null)
			{
				if (source[index].Location.EqualsICIC(info.Path))
				{
					favorite = source[index];
				}
				else
				{
					index++;
				}
			}

			if (favorite == null)
			{
				await AddIn.Self.AddFavoritePageCmd(null);

				await using var provider = new FavoritesProvider(null);
				var favorites = provider.LoadFavorites();
				source.Add(favorites[favorites.Count - 1]);
				MoveBottom();
			}
			else
			{
				gridView.Rows[index].Cells[0].Selected = true;
				ShowText();
			}
		}


		private void ChooseByClick(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			var favorites = (BindingList<Favorite>)gridView.DataSource;
			if (rowIndex > favorites.Count)
				return;

			Uri = favorites[rowIndex].Uri;
		}


		private void ChooseByDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			ChooseByClick(null, null);
			DialogResult = DialogResult.OK;
			Close();
		}


		private void ChooseByKeyboard(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				ChooseByClick(null, null);
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private void ShowMenu(object sender, EventArgs e)
		{
			contextMenu.Show(menuButton, new Point(
				-(contextMenu.Width - menuButton.Width),
				menuButton.Height));
		}


		private async void CheckFavorites(object sender, EventArgs e)
		{
			await using var provider = new FavoritesProvider(null);

			var list = ((BindingList<Favorite>)gridView.DataSource).ToList();
			await provider.ValidateFavorites(list);
			gridView.DataSource = new BindingList<Favorite>(list);
		}


		private void ManageFavorites(object sender, EventArgs e)
		{
			Manage = true;
			DialogResult = DialogResult.OK;
			Close();
		}


		private async void SortFavorites(object sender, EventArgs e)
		{
			await using var provider = new FavoritesProvider(null);
			var list = provider.SortFavorites();
			gridView.DataSource = new BindingList<Favorite>(list);
		}
	}
}
