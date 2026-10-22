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
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class SnippetsSheet : SheetBase
	{
		private sealed class Snippet
		{
			public string Name { get; set; }
			public string Path { get; set; }
			public DateTime Modified { get; set; }

			// legacy .snp snippets are suffixed so they're distinguishable from .snpx
			public bool IsLegacy => !SnippetsProvider.IsXml(Path);

			public string DisplayName => IsLegacy
				? $"{Name}{SnippetsProvider.HtmlExtension}"
				: Name;
		}


		private readonly IRibbonUI ribbon;
		private readonly SnippetsProvider snipsProvider;
		private readonly BoxTypesProvider boxTypesProvider;
		private readonly BoxTypesPanel boxTypesPanel;

		// built by EnsureSnippetsLoaded(); "My Snippets" is the first, default tab
		private BindingList<Snippet> snippets;

		private bool updated = false;


		public SnippetsSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = "SnippetsSheet";
			Title = Resx.word_Snippets;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"codeStyleBox",
					"boxTypesTab.Text=BoxTypesPanel_Title",
					"mySnippetsTab",
					"boxWidthLabel"
				});

				nameColumn.HeaderText = Resx.word_Name;
				modifiedColumn.HeaderText = Resx.SnippetsSheet_modifiedColumn;
				editButton.ToolTipText = Resx.word_Edit;
				renameButton.ToolTipText = Resx.word_Rename;
				deleteButton.ToolTipText = Resx.word_Delete;
			}

			// options

			var settings = provider.GetCollection(Name);
			codeStyleBox.Checked = settings.Get("applyStyle", false);

			// update my styles gridview

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = nameof(Snippet.DisplayName);
			gridView.Columns[1].DataPropertyName = nameof(Snippet.Modified);
			gridView.Columns[1].DefaultCellStyle.Format = "g";
			gridView.CellFormatting += FormatLegacyRow;

			(_, float scaleY) = UI.Scaling.GetScalingFactors();
			gridView.RowTemplate.Height = (int)(16 * scaleY);

			this.ribbon = ribbon;
			snipsProvider = new SnippetsProvider();

			// snippets (My Snippets tab) is the default tab so load it now
			tabs.SelectedIndexChanged += (s, e) => EnsureSnippetsLoaded();
			EnsureSnippetsLoaded();

			// box types

			boxTypesProvider = new BoxTypesProvider(provider);
			boxWidthBox.Value = (decimal)boxTypesProvider.GetBoxWidth();
			boxWidthBox.ValueChanged += (s, e) => updated = true;

			boxTypesPanel = new BoxTypesPanel(boxTypesProvider.LoadAll())
			{
				Dock = DockStyle.Fill
			};
			boxTypesPanel.Changed += (s, e) => updated = true;
			boxTypesTab.Controls.Add(boxTypesPanel);

			// WinForms docks siblings in REVERSE Controls-collection index order (highest
			// index processed first - this is also why the last-ADDED of several same-Dock
			// siblings ends up topmost elsewhere in this file). optionsPanel/boxWidthPanel
			// were added first (lower indices, in InitializeComponent) and boxTypesPanel
			// (Dock=Fill) was added after (higher index) - meaning Fill got evaluated BEFORE
			// its Top siblings reserved their space, so it saw the full tab and overlapped
			// them instead of sitting below them. Moving it to index 0 makes it the LAST
			// thing evaluated in that reverse sequence, after Top has already been reserved.
			boxTypesTab.Controls.SetChildIndex(boxTypesPanel, 0);
		}


		/// <summary>
		/// Builds the file-based "My Snippets" grid. Safe to call repeatedly; only the
		/// first call, while that tab is selected, does anything.
		/// </summary>
		private void EnsureSnippetsLoaded()
		{
			if (snippets is not null || tabs.SelectedTab != mySnippetsTab)
			{
				return;
			}

			snippets = new BindingList<Snippet>(LoadSnippets());
			gridView.DataSource = snippets;
		}


		private List<Snippet> LoadSnippets()
		{
			var paths = snipsProvider.GetPaths();
			var list = new List<Snippet>();

			foreach (var path in paths)
			{
				list.Add(new Snippet
				{
					Name = Path.GetFileNameWithoutExtension(path),
					Path = path,
					Modified = File.GetLastWriteTime(path)
				});
			}

			return list;
		}


		private void FormatLegacyRow(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (snippets is null || e.RowIndex < 0 || e.RowIndex >= snippets.Count)
			{
				return;
			}

			if (snippets[e.RowIndex].IsLegacy)
			{
				// selection keeps its normal highlight colors so it stays readable
				e.CellStyle.ForeColor = UI.ThemeManager.Instance.GetColor("GrayText");
			}
		}


		private void DeleteItem(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= snippets.Count)
				return;

			var snippet = snippets[rowIndex];

			var result = UI.MoreMessageBox.Show(this,
				string.Format(Resx.SnippetsSheet_ConfirmDelete, snippet.Name),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
				return;

			snippets.RemoveAt(rowIndex);
			snipsProvider.Delete(snippet.Path);
			updated = true;

			rowIndex--;
			if (rowIndex >= 0)
			{
				gridView.Rows[rowIndex].Cells[0].Selected = true;
			}
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);

			updated = codeStyleBox.Checked
				? settings.Add("applyStyle", true) || updated
				: settings.Remove("applyStyle") || updated;

			if (updated)
			{
				provider.SetCollection(settings);
				boxTypesProvider.SaveAll(boxTypesPanel.GetBoxTypes(), (float)boxWidthBox.Value);
				ribbon.SafeInvalidateControl(SnippetsProvider.MenuID);
			}

			return false;
		}

		private void EditItem(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
			{
				return;
			}

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= snippets.Count)
			{
				return;
			}

			var path = snippets[rowIndex].Path;

			if (!SnippetsProvider.IsXml(path))
			{
				UI.MoreMessageBox.Show(this, Resx.SnippetsSheet_LegacyNotEditable);
				return;
			}

			// do not create the page while this modal dialog is open; hand the path to the
			// dialog's owner command, which opens the page after the dialog has closed
			if (FindForm() is SettingsDialog dialog)
			{
				dialog.SnippetToEdit = path;
				dialog.AcceptAndClose();
			}
		}


		private void RenameItem(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= snippets.Count)
				return;

			var snippet = snippets[rowIndex];

			using var dialog = new SaveSnippetDialog();
			dialog.SnippetName = snippet.Name;

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				var path = snipsProvider.Rename(snippet.Path, dialog.SnippetName);
				if (!string.IsNullOrEmpty(path))
				{
					snippet.Name = dialog.SnippetName;
					snippet.Path = path;
					updated = true;
				}
			}
		}
	}
}
