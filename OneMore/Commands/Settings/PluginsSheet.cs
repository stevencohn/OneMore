//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class PluginsSheet : SheetBase
	{

		private readonly IRibbonUI ribbon;
		private readonly PluginsProvider pinProvider;
		private BindingList<Plugin> plugins;
		private bool updated = false;


		private PluginsSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = "PluginsSheet";
			Title = Resx.word_Plugins;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"renameButton=word_Rename",
					"deleteButton=word_Delete"
				});

				nameColumn.HeaderText = Resx.word_Name;
				cmdColumn.HeaderText = Resx.word_Command;
			}

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			(_, float scaleY) = UI.Scaling.GetScalingFactors();
			gridView.RowTemplate.Height = (int)(16 * scaleY);

			this.ribbon = ribbon;
			pinProvider = new PluginsProvider();
		}


		// use async factory pattern to instantiate a new instance...

		public static async Task<PluginsSheet> Create(SettingsProvider provider, IRibbonUI ribbon)
		{
			var sheet = new PluginsSheet(provider, ribbon);
			await sheet.Initialize();
			return sheet;
		}


		private async Task Initialize()
		{
			plugins = new BindingList<Plugin>(await LoadPlugins());
			gridView.DataSource = plugins;
		}


		private async Task<List<Plugin>> LoadPlugins()
		{
			var paths = pinProvider.GetPaths();
			var list = new List<Plugin>();

			foreach (var path in paths)
			{
				var plugin = await pinProvider.Load(path);
				list.Add(plugin);
			}

			return list;
		}


		private void EditSelection(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				Edit(gridView.SelectedCells[0].RowIndex);
			}
		}


		private void EditOnDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			// ignore double-click on col header
			if (e.RowIndex >= 0)
			{
				Edit(e.RowIndex);
			}
		}


		private void EditOnDoubleClickRow(object sender, DataGridViewCellMouseEventArgs e)
		{
			// ignore double-click on col header
			if (e.RowIndex >= 0)
			{
				Edit(e.RowIndex);
			}
		}


		private void Edit(int rowIndex)
		{
			var plugin = plugins[rowIndex];

			using var dialog = new PluginDialog(plugin);

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				var edited = dialog.Plugin;
				plugin.Name = edited.Name;
				plugin.OriginalName = edited.OriginalName;
				plugin.Command = edited.Command;
				plugin.Arguments = edited.Arguments;
				plugin.CreateNewPage = edited.CreateNewPage;
				plugin.AsChildPage = edited.AsChildPage;
				plugin.PageName = edited.PageName;
				plugin.Target = edited.Target;

				plugins.ResetItem(rowIndex);
			}
		}


		private void NameValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			var rowIndex = e.RowIndex;
			var name = gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;

			// validate format of name... 

			if (string.IsNullOrWhiteSpace(name))
			{
				gridView.Rows[rowIndex].ErrorText = "Name cannot be empty";
				e.Cancel = true;
				return;
			}

			if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				gridView.Rows[rowIndex].ErrorText = "Name cannot contain invalid characters";
				e.Cancel = true;
				return;
			}

			// validate uniqueness...

			var names = pinProvider.GetNames();
			if (names.Contains(name))
			{
				gridView.Rows[rowIndex].ErrorText = "Name must be unique";
				e.Cancel = true;
			}
		}


		private async void Rename(object sender, DataGridViewCellEventArgs e)
		{
			var rowIndex = e.RowIndex;
			var plugin = plugins[rowIndex];

			gridView.Rows[rowIndex].ErrorText = string.Empty;

			if (await pinProvider.Rename(plugin, plugin.Name))
			{
				updated = true;
			}
		}


		private void DeleteItem(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= plugins.Count)
				return;

			var plugin = plugins[rowIndex];

			var result = UI.MoreMessageBox.Show(this,
				string.Format(Resx.PluginsSheet_ConfirmDelete, plugin.Name),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
				return;

			if (pinProvider.Delete(plugin.Path))
			{
				logger.WriteLine($"Deleted {plugin.Name} plugin");

				plugins.RemoveAt(rowIndex);
				updated = true;

				rowIndex--;
				if (rowIndex >= 0)
				{
					gridView.Rows[rowIndex].Cells[0].Selected = true;
				}
			}
			else
			{
				logger.WriteLine($"Could not delete {plugin.Name} plugin");
			}
		}


		public override bool CollectSettings()
		{
			if (updated)
			{
				ribbon.InvalidateControl("ribFavoritesMenu");
			}

			return false;
		}
	}
}
