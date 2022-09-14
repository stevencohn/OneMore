//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class FavoritesDialog : UI.LocalizableForm
	{
		private sealed class Favorite
		{
			public int Index { get; set; }
			public string Name { get; set; }
			public string Location { get; set; }
			public string Uri { get; set; }
		}


		public FavoritesDialog()
		{
			InitializeComponent();

			nameColumn.HeaderText = Resx.word_Name;
			locationColumn.HeaderText = Resx.FavoritesSheet_locationColumn_HeaderText;

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			gridView.Columns[1].DataPropertyName = "Location";
			gridView.DataSource = new BindingList<Favorite>(LoadFavorites());
		}


		public string Uri { get; private set; }


		private List<Favorite> LoadFavorites()
		{
			var list = new List<Favorite>();
			var root = new FavoritesProvider(null).LoadFavoritesMenu();
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
					Name = element.Attribute("label").Value,
					Location = element.Attribute("screentip").Value,
					Uri = element.Attribute("tag").Value
				});
			}

			return list;
		}


		private void ChooseFavorite(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			var favorites = (BindingList<Favorite>)gridView.DataSource;
			if (rowIndex > favorites.Count)
				return;

			Uri = favorites[rowIndex].Uri;
		}


		private void gridView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				ChooseFavorite(null, null);
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
