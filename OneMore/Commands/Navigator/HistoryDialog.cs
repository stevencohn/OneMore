//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using HistoryRecord = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	/// <summary>
	/// A filterable, keyboard-navigable picker of recently visited pages, invoked by
	/// HistoryCommand (Shift+Alt+H). Read-only jump list; managing/deleting history
	/// entries remains the job of NavigatorWindow's History panel.
	/// </summary>
	internal partial class HistoryDialog : MoreForm
	{
		private List<HistoryRecord> records;
		private string _programmaticText = string.Empty;


		public HistoryDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_History;

				Localize(new string[]
				{
					"searchLabel=word_Search",
					"goButton=word_Go",
					"cancelButton=word_Cancel"
				});

				nameColumn.Text = Resx.word_Name;
				locationColumn.Text = Resx.MangeFavoritesControl_locationColumn_HeaderText;
			}

			listView.SetColumnProportions(0.4f, 0.6f);

			DefaultControl = searchBox;
		}


		public string Uri { get; private set; }


		private async void BindOnLoad(object sender, EventArgs e)
		{
			using var provider = new NavigationProvider();
			var log = await provider.ReadHistoryLog();
			records = log.History;

			Populate(string.Empty);

			if (listView.Items.Count > 0)
			{
				listView.Items[0].Selected = true;
				listView.Items[0].EnsureVisible();
			}
		}


		private void FocusOnActivated(object sender, EventArgs e)
		{
			searchBox.Focus();
		}


		/// <summary>
		/// Rebuilds the list from the in-memory history, showing only records whose name
		/// or location match the given filter text (or all records when the filter is too
		/// short).
		/// </summary>
		private void Populate(string filterText)
		{
			if (records == null)
			{
				return;
			}

			var text = filterText.Trim();
			var filtering = text.Length > 1;

			var matches = filtering
				? records.Where(r => Matches(r, text)).ToList()
				: records;

			listView.BeginUpdate();
			listView.Items.Clear();

			foreach (var record in matches)
			{
				AddHistoryRow(record);
			}

			listView.EndUpdate();
		}


		private static bool Matches(HistoryRecord record, string text)
		{
			return record.Name.ContainsICIC(text) || record.Path.ContainsICIC(text);
		}


		private void AddHistoryRow(HistoryRecord record)
		{
			var item = new ListViewItem(record.Name) { Tag = record };
			item.SubItems.Add(record.Path);
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

			if (char.IsControl((char)e.KeyValue) &&
				e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back)
			{
				e.Handled = true;
				return;
			}

			// A stale WM_KEYUP (no preceding WM_CHAR) leaves the text unchanged from what
			// ShowText set - skip filtering. Once the user actually types, WM_CHAR changes
			// the text first so this check passes and _programmaticText is cleared.
			if (_programmaticText.Length > 0 && searchBox.Text == _programmaticText)
			{
				e.Handled = true;
				return;
			}

			_programmaticText = string.Empty;

			// filter list based on search text; preserve the selected record by page ID
			// since rebuilding the list invalidates row indices

			var selectedId = listView.SelectedItems.Count > 0 &&
				listView.SelectedItems[0].Tag is HistoryRecord selected
					? selected.PageId
					: null;

			Populate(searchBox.Text);

			var index = selectedId != null ? IndexOfRecord(selectedId) : -1;
			if (index < 0 && listView.Items.Count > 0)
			{
				index = 0;
			}

			if (index >= 0)
			{
				listView.Items[index].Selected = true;
				listView.Items[index].EnsureVisible();
			}

			e.Handled = true;
		}


		private int IndexOfRecord(string pageId)
		{
			for (var i = 0; i < listView.Items.Count; i++)
			{
				if (listView.Items[i].Tag is HistoryRecord record && record.PageId == pageId)
				{
					return i;
				}
			}

			return -1;
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
			if (listView.Items.Count == 0)
			{
				return false;
			}

			SelectRow(listView.Items.Count - 1);
			return true;
		}


		private bool MoveTop()
		{
			if (listView.Items.Count == 0)
			{
				return false;
			}

			SelectRow(0);
			return true;
		}


		private bool MovePageDown()
		{
			if (listView.Items.Count == 0)
			{
				return false;
			}

			var index = listView.SelectedItems.Count == 0
				? 0
				: Math.Min(listView.Items.Count - 1, listView.SelectedItems[0].Index + PageSize());

			SelectRow(index);
			return true;
		}


		private bool MovePageUp()
		{
			if (listView.Items.Count == 0)
			{
				return false;
			}

			var index = listView.SelectedItems.Count == 0
				? 0
				: Math.Max(0, listView.SelectedItems[0].Index - PageSize());

			SelectRow(index);
			return true;
		}


		private bool SelectNextRow()
		{
			if (listView.SelectedItems.Count == 0)
			{
				return MoveTop();
			}

			var index = listView.SelectedItems[0].Index + 1;
			if (index < listView.Items.Count)
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

			var index = listView.SelectedItems[0].Index - 1;
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


		private void ChooseByClick(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0 ||
				listView.SelectedItems[0].Tag is not HistoryRecord record)
			{
				return;
			}

			Uri = record.Link;
		}


		private void ChooseByDoubleClick(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0 ||
				listView.SelectedItems[0].Tag is not HistoryRecord)
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
				listView.SelectedItems[0].Tag is not HistoryRecord)
			{
				return;
			}

			ChooseByClick(null, null);
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
