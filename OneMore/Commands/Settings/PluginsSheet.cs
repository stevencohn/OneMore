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
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class PluginsSheet : SheetBase
	{

		private readonly IRibbonUI ribbon;
		private readonly PluginsProvider plugsProvider;
		private BindingList<Plugin> plugins;
		private bool updated = false;


		private PluginsSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = "PluginsSheet";
			Title = Resx.PluginsSheet_Text;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introLabel",
					"deleteLabel",
					"deleteButton",
					"okButton",
					"cancelButton"
				});

				nameColumn.HeaderText = Resx.PluginsSheet_nameColumn_HeaderText;
				cmdColumn.HeaderText = Resx.PluginsSheet_cmdColumn_HeaderText;
			}

			toolStrip.Rescale();

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";

			this.ribbon = ribbon;
			plugsProvider = new PluginsProvider();
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
			var paths = plugsProvider.GetPaths();
			var list = new List<Plugin>();

			foreach (var path in paths)
			{
				var plugin = await plugsProvider.Load(path);
				list.Add(plugin);
			}

			return list;
		}


		private void EditSelection(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			var rowIndex = gridView.SelectedCells[0].RowIndex;
			var plugin = plugins[rowIndex];

			using (var dialog = new PluginDialog(plugin))
			{
				if (dialog.ShowDialog() == DialogResult.OK)
				{

				}
			}
		}

		private void RenameSelection(object sender, EventArgs e)
		{
			//
		}


		private void DeleteItem(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= plugins.Count)
				return;

			var plugin = plugins[rowIndex];

			var result = MessageBox.Show(
				string.Format(Resx.PluginsSheet_ConfirmDelete, plugin.Name),
				"OneMore",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2,
				MessageBoxOptions.DefaultDesktopOnly);

			if (result != DialogResult.Yes)
				return;

			plugins.RemoveAt(rowIndex);
			plugsProvider.Delete(plugin.Path);
			updated = true;

			rowIndex--;
			if (rowIndex >= 0)
			{
				gridView.Rows[rowIndex].Cells[0].Selected = true;
			}
		}


		public override void CollectSettings()
		{
			if (updated)
			{
				ribbon.InvalidateControl("ribFavoritesMenu");
			}
		}
	}
}
