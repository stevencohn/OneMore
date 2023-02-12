//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;
	using Favorite = FavoritesProvider.Favorite;
	using FavoriteStatus = FavoritesProvider.FavoriteStatus;
	using Resx = Properties.Resources;


	internal partial class FavoritesDialog : UI.LocalizableForm
	{

		public FavoritesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Favorites;

				Localize(new string[]
				{
					"goButton=word_Go",
					"cancelButton=word_Cancel"
				});

				nameColumn.HeaderText = Resx.word_Name;
				locationColumn.HeaderText = Resx.FavoritesSheet_locationColumn_HeaderText;
			}
		}


		private async void BindOnLoad(object sender, EventArgs e)
		{
			using var provider = new FavoritesProvider(null);
			var favorites = await provider.LoadFavorites();

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			gridView.Columns[1].DataPropertyName = "Location";
			gridView.DataSource = new BindingList<Favorite>(favorites);
			gridView.ClearSelection();
		}


		private void FocusOnActivated(object sender, EventArgs e)
		{
			searchBox.Focus();
		}


		public string Uri { get; private set; }


		private void ValidateOnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (gridView.Rows[e.RowIndex].DataBoundItem is Favorite favorite)
			{
				if (favorite.Status == FavoriteStatus.Unknown)
				{
					gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText =
						Resx.Favorites_unknown;

					e.CellStyle.BackColor = Color.Pink;
					e.FormattingApplied = true;
				}
				else if (favorite.Status == FavoriteStatus.Suspect)
				{
					gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText =
						Resx.Favorites_suspect;

					e.CellStyle.BackColor = Color.LightGoldenrodYellow;
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

			if (e.KeyCode == Keys.Down)
			{
				e.Handled = SelectNextRow();
			}
			else if (e.KeyCode == Keys.Up)
			{
				e.Handled = SelectPreviousRow();
			}
			else
			{
				var text = searchBox.Text.Trim();

				var index = gridView.SelectedCells.Count > 0
					? gridView.SelectedCells[0].RowIndex
					: 0;

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

				// ensure selection...

				if (index >= 0)
				{

				}
				else
				{
					gridView.Rows[0].Cells[0].Selected = true;
				}
			}
		}


		private bool ShowText()
		{
			searchBox.Text = (string)gridView.SelectedCells[0].Value;
			searchBox.Select(searchBox.Text.Length, 0);
			return true;
		}


		private bool SelectNextRow()
		{
			if (gridView.SelectedCells.Count == 0)
			{
				gridView.Rows[0].Cells[0].Selected = true;
				return ShowText();
			}
			else
			{
				var index = gridView.SelectedCells[0].RowIndex;
				if (index < gridView.Rows.Count - 1)
				{
					gridView.Rows[index + 1].Cells[0].Selected = true;
					return ShowText();
				}
			}

			return false;
		}


		private bool SelectPreviousRow()
		{
			if (gridView.SelectedCells.Count == 0)
			{
				gridView.Rows[0].Cells[0].Selected = true;
				return ShowText();
			}
			else
			{
				var index = gridView.SelectedCells[0].RowIndex;
				if (index > 0)
				{
					gridView.Rows[index - 1].Cells[0].Selected = true;
					return ShowText();
				}
			}

			return false;
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


		private void ChooseByKeyboard(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				ChooseByClick(null, null);
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
