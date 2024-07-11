﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.IO;
	using System.Net;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal partial class SearchEngineSheet : SheetBase
	{
		private sealed class SearchEngine
		{
			public Image Image { get; set; }
			public string Name { get; set; }
			public string Uri { get; set; }
		}


		private readonly BindingList<SearchEngine> engines;


		public SearchEngineSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "SearchEngineSheet";
			Title = Resx.SearchEngineDialog_Text;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"upButton",
					"downButton",
					"refreshButton=word_Refresh",
					"deleteButton=word_Delete"
				});

				iconColumn.HeaderText = Resx.SearchEngineDialog_iconColumn_HeaderText;
				nameColumn.HeaderText = Resx.word_Name;
				urlColumn.HeaderText = Resx.SearchEngineDialog_urlColumn_HeaderText;
			}

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Image";
			gridView.Columns[1].DataPropertyName = "Name";
			gridView.Columns[2].DataPropertyName = "Uri";

			(_, float scaleY) = UI.Scaling.GetScalingFactors();
			gridView.RowTemplate.Height = (int)(16 * scaleY);

			engines = new BindingList<SearchEngine>(LoadSettings());

			gridView.DataSource = engines;

			RefreshImages();
		}


		private List<SearchEngine> LoadSettings()
		{
			var list = new List<SearchEngine>();
			var settings = provider.GetCollection(Name).Get<XElement>("engines");

			if (settings != null)
			{
				foreach (var element in settings.Elements("engine"))
				{
					var bytes = Convert.FromBase64String(element.Element("image").Value);
					using var stream = new MemoryStream(bytes, 0, bytes.Length);
					list.Add(new SearchEngine
					{
						Image = Image.FromStream(stream),
						Name = element.Element("name").Value,
						Uri = element.Element("uri").Value
					});
				}
			}

			return list;
		}


		private void RefreshImages()
		{
			foreach (var engine in engines)
			{
				if (engine.Image == null)
				{
					RefreshImage(engine);
				}
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			if (manager.DarkMode)
			{
				using var img = iconColumn.Image;

				iconColumn.Image = new ImageEditor
				{
					Size = new Size(16, 16),
					Style = ImageEditor.Stylization.Invert
				}
				.Apply(img);
			}

			base.OnLoad(e);
		}


		private static void RefreshImage(SearchEngine engine)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			try
			{
				var uri = new Uri(engine.Uri);
				var url = string.Format("https://{0}/favicon.ico", uri.Host);

				var request = WebRequest.Create(url);
				using var response = request.GetResponse();
				using var stream = response.GetResponseStream();
				engine.Image = new Bitmap(Image.FromStream(stream), 16, 16);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error getting favicon for {engine.Uri}", exc);
			}
		}


		private void gridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 2)
			{
				if (engines[e.RowIndex] is SearchEngine engine)
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
				if (engines[rowIndex] is SearchEngine engine)
				{
					RefreshImage(engine);
				}
			}
		}


		private void deleteButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= engines.Count)
				return;

			var engine = engines[rowIndex];

			var result = UI.MoreMessageBox.Show(this,
				string.Format(Resx.SearchEngineDialog_DeleteMessage, engine.Name),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
				return;

			engines.RemoveAt(rowIndex);

			rowIndex--;
			if (rowIndex >= 0)
			{
				gridView.Rows[rowIndex].Cells[0].Selected = true;
			}
		}


		private void downButton_Click(object sender, EventArgs e)
		{
			gridView.MoveSelectedItemDown(engines);
		}


		private void upButton_Click(object sender, EventArgs e)
		{
			gridView.MoveSelectedItemUp(engines);
		}


		public override bool CollectSettings()
		{
			var element = new XElement("engines");

			foreach (var engine in engines)
			{
				element.Add(new XElement("engine",
					new XElement("image", engine.Image.ToBase64String()),
					new XElement("name", engine.Name),
					new XElement("uri", engine.Uri)
					));
			}

			if (element.HasElements)
			{
				var settings = provider.GetCollection(Name);
				settings.Add("engines", element);
				provider.SetCollection(settings);
			}
			else
			{
				provider.RemoveCollection(Name);
			}

			return false;
		}
	}
}
