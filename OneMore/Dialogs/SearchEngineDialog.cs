//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Dialogs
{
	using River.OneMoreAddIn.Helpers.Settings;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Net;
	using System.Windows.Forms;


	public partial class SearchEngineDialog : Form
	{
		private readonly List<SearchEngine> engines;
		private readonly BindingList<SearchEngine> source;


		public SearchEngineDialog()
		{
			InitializeComponent();

			// prevent VS designer from overriding
			toolStrip.ImageScalingSize = new Size(16, 16);

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Image";
			gridView.Columns[1].DataPropertyName = "Name";
			gridView.Columns[2].DataPropertyName = "Uri";

			engines = new SearchEngineProvider().Load();

			source = new BindingList<SearchEngine>();
			foreach (var engine in engines)
			{
				source.Add(engine);
			}

			gridView.DataSource = source;

			RefreshImages();
		}

		private void RefreshImages()
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (source[i].Image == null)
				{
					RefreshImage(source[i]);
				}
			}
		}


		private void RefreshImage(SearchEngine engine)
		{
			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

			try
			{
				var uri = new Uri(engine.Uri);
				var url = string.Format("https://{0}/favicon.ico", uri.Host);

				var request = WebRequest.Create(url);
				using (var response = request.GetResponse())
				{
					using (var stream = response.GetResponseStream())
					{
						engine.Image = new Bitmap(Image.FromStream(stream), 16, 16);
					}
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"Error getting favicon for {engine.Uri}", exc);
			}
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new Point(Location.X, Location.Y - (Height / 2));
			base.OnShown(e);
		}


		private void gridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 2)
			{
				if (source[e.RowIndex] is SearchEngine engine)
				{
					RefreshImage(engine);
					gridView.Refresh();
				}
			}
		}


		private void gridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			// overrides the "red x" icon in the new-item row
			gridView.Rows[gridView.Rows.Count - 1].Cells[0].Value =
				((DataGridViewImageColumn)gridView.Columns[0]).Image;

			//if (!loaded)
			//{
			//	gridView.Rows[gridView.Rows.Count - 1].Cells[1].Selected = true;
			//	loaded = true;
			//}
		}


		private void refreshButton_Click(object sender, EventArgs e)
		{
			int rowIndex = -1;
			if (gridView.SelectedCells.Count > 0)
			{
				rowIndex = gridView.SelectedCells[0].RowIndex;
			}
			else if (gridView.SelectedRows.Count > 0)
			{
				rowIndex = gridView.SelectedRows[0].Index;
			}

			if (rowIndex >= 0)
			{
				if (source[rowIndex] is SearchEngine engine)
				{
					RefreshImage(engine);
				}
			}
		}


		private void deleteButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex < source.Count)
				{
					source.RemoveAt(rowIndex);

					if (rowIndex > 0)
					{
						rowIndex--;
					}

					gridView.Rows[rowIndex].Cells[colIndex].Selected = true;
				}
			}
		}


		private void upButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex > 0 && rowIndex < source.Count)
				{
					var item = source[rowIndex];
					source.RemoveAt(rowIndex);
					source.Insert(rowIndex - 1, item);

					gridView.Rows[rowIndex - 1].Cells[colIndex].Selected = true;
				}
			}
		}


		private void downButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex < source.Count - 1)
				{
					var item = source[rowIndex];
					source.RemoveAt(rowIndex);
					source.Insert(rowIndex + 1, item);

					gridView.Rows[rowIndex + 1].Cells[colIndex].Selected = true;
				}
			}
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			var list = new List<SearchEngine>();
			foreach (var item in source)
			{
				list.Add(item);
			}

			new SearchEngineProvider().Save(list);
		}
	}
}
