//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Layouts
{
	using River.OneMoreAddIn.UI;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Lets the user pick a saved layout and restore it. Unlike FavoritesDialog, there's no
	/// search box - layouts are expected to be few, so the grouped listview alone is enough.
	/// Choosing a layout (double-click, Enter, or the Go button) runs RestoreLayoutCommand
	/// directly and closes the dialog.
	/// </summary>
	internal partial class LayoutsDialog : MoreForm
	{
		/// <summary>
		/// Marks a row that represents a layout rather than a window within it.
		/// </summary>
		private sealed class LayoutRow
		{
			public int LayoutID { get; set; }
			public string Name { get; set; }
		}


		private const int WindowIndent = 20;


		public LayoutsDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Layouts;

				Localize(new string[]
				{
					"goButton=word_Go",
					"cancelButton=word_Cancel"
				});

				nameColumn.Text = Resx.word_Name;
				locationColumn.Text = Resx.ManageLayoutsControl_locationColumn_HeaderText;
			}

			listView.SetColumnProportions(0.4f, 0.6f);
			listView.GetCellStyle = GetCellStyle;
		}


		private async void BindOnLoad(object sender, System.EventArgs e)
		{
			using var provider = new LayoutsProvider();
			var collection = provider.ReadLayouts();

			Populate(collection);

			await Task.Yield();
		}


		private void Populate(LayoutsCollection collection)
		{
			listView.BeginUpdate();
			listView.Items.Clear();

			foreach (var layout in collection.Layouts)
			{
				AddLayoutRow(layout.LayoutID, layout.Name);
				foreach (var window in layout.Windows)
				{
					AddWindowRow(window);
				}
			}

			listView.EndUpdate();

			if (listView.Items.Count > 0)
			{
				listView.Items[0].Selected = true;
				listView.Items[0].EnsureVisible();
			}
		}


		private static MoreListView.CellStyle GetCellStyle(ListViewItem item, int columnIndex)
		{
			if (item.Tag is LayoutWindow)
			{
				var indent = columnIndex == 0 ? WindowIndent : 0;
				return new MoreListView.CellStyle(indent, false);
			}

			return MoreListView.CellStyle.Default;
		}


		private void AddLayoutRow(int layoutID, string name)
		{
			var item = new ListViewItem(name)
			{
				Tag = new LayoutRow { LayoutID = layoutID, Name = name },
				Font = new Font(listView.Font, FontStyle.Bold)
			};

			item.SubItems.Add(string.Empty);
			listView.Items.Add(item);
		}


		private void AddWindowRow(LayoutWindow window)
		{
			var item = new ListViewItem(window.Alias ?? window.Name) { Tag = window };
			item.SubItems.Add(window.Location);
			listView.Items.Add(item);
		}


		/// <summary>
		/// Returns the name of the layout represented by the current selection - either the
		/// layout row itself or one of its windows - or null if nothing is selected.
		/// </summary>
		private string GetSelectedLayoutName()
		{
			if (listView.SelectedItems.Count == 0)
			{
				return null;
			}

			return listView.SelectedItems[0].Tag switch
			{
				LayoutRow row => row.Name,
				LayoutWindow window => FindLayoutName(window.LayoutID),
				_ => null
			};
		}


		private string FindLayoutName(int layoutID)
		{
			return listView.Items.Cast<ListViewItem>()
				.Select(i => i.Tag)
				.OfType<LayoutRow>()
				.FirstOrDefault(r => r.LayoutID == layoutID)
				?.Name;
		}


		private async void ChooseByClick(object sender, System.EventArgs e)
		{
			await Choose();
		}


		private async void ChooseByDoubleClick(object sender, System.EventArgs e)
		{
			await Choose();
		}


		private async void ChooseByKeyboard(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
			{
				return;
			}

			e.Handled = true;
			await Choose();
		}


		/// <summary>
		/// Restores the selected layout via RestoreLayoutCommand and closes the dialog. Done
		/// here (rather than deferring to a caller, the way FavoritesDialog defers navigation
		/// to FavoritesCommand) since there's no separate ribbon-level LayoutsCommand yet.
		/// </summary>
		private async Task Choose()
		{
			var name = GetSelectedLayoutName();
			if (string.IsNullOrEmpty(name))
			{
				return;
			}

			var command = new RestoreLayoutCommand();
			command.SetLogger(Logger.Current);
			command.SetOwner(this);
			await command.Execute(name);

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
