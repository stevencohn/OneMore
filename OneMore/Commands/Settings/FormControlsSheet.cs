//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Manages the date format and the user-defined lists used by form controls. Changes are
	/// held in memory and are written to OneMore.db only when the user presses OK.
	/// </summary>
	internal class FormControlsSheet : SheetBase
	{
		private static readonly string[] DateFormats =
		{
			"dddd, MMMM d, yyyy", "ddd, MMM d, yyyy", "MMMM d, yyyy", "MMM d, yyyy",
			"d MMMM yyyy", "d MMM yyyy", "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy",
			"d/M/yyyy", "dd/MM/yyyy", "dd.MM.yyyy"
		};

		private readonly List<FormList> lists;
		private readonly List<string> formats;

		private readonly MoreComboBox formatBox;
		private readonly MoreDataGridView listGrid;
		private readonly MoreDataGridView itemGrid;
		private MoreLabel itemsLabel;
		private readonly MoreMenuItem editListButton;
		private readonly MoreMenuItem deleteListButton;
		private readonly MoreMenuItem addItemButton;
		private readonly MoreMenuItem editItemButton;
		private readonly MoreMenuItem deleteItemButton;
		private readonly MoreMenuItem moveUpButton;
		private readonly MoreMenuItem moveDownButton;

		private readonly string originalFormat;
		private bool changed;
		private bool loading;
		private bool syncing;
		private string editOriginal = string.Empty;


		public FormControlsSheet(SettingsProvider provider)
			: base(provider)
		{
			SuspendLayout();

			Name = "FormControlsSheet";
			Title = Resx.FormControlsSheet_Title;

			// sizes below are authored at 150% like the designer-built sheets so that
			// AutoScaleMode.Font scales every control to the actual monitor DPI
			AutoScaleDimensions = new SizeF(9F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.ControlLightLight;
			ForeColor = Color.Black;
			Margin = new Padding(4, 5, 4, 5);
			MinimumSize = new Size(750, 400);
			Padding = new Padding(10, 9, 10, 9);

			using (var forms = new FormControlsProvider())
			{
				lists = forms.ReadLists();
				originalFormat = forms.ReadSetting(
					FormControlsProvider.DateFormatKey, FormControlsProvider.DefaultDateFormat);
			}

			formats = new List<string>(DateFormats);
			if (!formats.Contains(originalFormat))
			{
				formats.Insert(0, originalFormat);
			}

			// layout, built bottom-up so docking stacks correctly...

			var introBox = new MoreMultilineLabel
			{
				Dock = DockStyle.Top,
				Name = "introBox",
				Padding = new Padding(0, 0, 0, 10),
				Text = Resx.FormControlsSheet_introBox_Text,
				TabIndex = 0,
				ThemedBack = "ControlLightLight",
				ThemedFore = null
			};

			var formatLabel = new MoreLabel
			{
				AutoSize = true,
				Location = new Point(0, 8),
				Name = "dateFormatLabel",
				Text = Resx.FormControlsSheet_dateFormatLabel_Text
			};

			formatBox = new MoreComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Location = new Point(160, 4),
				Name = "dateFormatBox",
				Size = new Size(320, 28),
				TabIndex = 1
			};

			foreach (var format in formats)
			{
				formatBox.Items.Add(DateTime.Now.ToString(format, AddIn.Locale));
			}

			formatBox.SelectedIndex = formats.IndexOf(originalFormat);
			formatBox.SelectedIndexChanged += (s, e) => changed = true;

			var formatPanel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 54,
				Name = "formatPanel",
				TabIndex = 1
			};

			formatPanel.Controls.Add(formatBox);
			formatPanel.Controls.Add(formatLabel);

			var layout = new TableLayoutPanel
			{
				ColumnCount = 2,
				Dock = DockStyle.Fill,
				Name = "layoutPanel",
				RowCount = 1,
				TabIndex = 2
			};

			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			var listPane = MakePane(
				Resx.FormControlsSheet_listsLabel_Text, "list", out listGrid, out var listBar);

			var addListButton = MakeButton("addListButton", "+", Resx.FormControlsSheet_addTip, AddList);
			editListButton = MakeButton("editListButton", "\u270E", Resx.word_Edit, EditList);
			deleteListButton = MakeButton(
				"deleteListButton", "\U0001F5D1", Resx.word_Delete, DeleteList);

			listBar.Items.AddRange(new ToolStripItem[]
			{
				addListButton, editListButton, deleteListButton
			});

			var itemPane = MakePane(
				Resx.FormControlsSheet_itemsLabel_Text, "item", out itemGrid, out var itemBar);

			addItemButton = MakeButton("addItemButton", "+", Resx.FormControlsSheet_addTip, AddItem);
			editItemButton = MakeButton("editItemButton", "\u2336", Resx.word_Rename, EditItem);
			deleteItemButton = MakeButton(
				"deleteItemButton", "\U0001F5D1", Resx.word_Delete, DeleteItem);

			moveUpButton = MakeButton(
				"moveUpButton", "▲", Resx.FormControlsSheet_moveUp, MoveItemUp);
			moveDownButton = MakeButton(
				"moveDownButton", "▼", Resx.FormControlsSheet_moveDown, MoveItemDown);

			itemBar.Items.AddRange(new ToolStripItem[]
			{
				addItemButton, editItemButton, deleteItemButton,
				new ToolStripSeparator(), moveUpButton, moveDownButton
			});

			layout.Controls.Add(listPane, 0, 0);
			layout.Controls.Add(itemPane, 1, 0);

			Controls.Add(layout);
			Controls.Add(formatPanel);
			Controls.Add(introBox);

			listGrid.AllowUserToDeleteRows = false;
			listGrid.SelectionChanged += (s, e) => ShowItems();

			// items are edited in place; the trailing blank row adds a new item
			itemGrid.AllowUserToAddRows = true;
			itemGrid.AllowUserToDeleteRows = true;
			itemGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
			itemGrid.ReadOnly = false;
			itemGrid.CurrentCellChanged += (s, e) => EnableButtons();
			itemGrid.CellBeginEdit += BeginItemEdit;
			itemGrid.CellValidating += ValidateItem;
			itemGrid.CellEndEdit += TrimItem;
			itemGrid.UserDeletingRow += ConfirmDeletingRow;
			itemGrid.CellValueChanged += (s, e) => SyncItems();
			itemGrid.UserAddedRow += (s, e) => SyncItems();
			itemGrid.UserDeletedRow += (s, e) => SyncItems();

			ResumeLayout(false);
			PerformLayout();

			LoadLists(0);
		}


		private Panel MakePane(
			string title, string prefix, out MoreDataGridView grid, out MoreToolStrip bar)
		{
			var pane = new Panel
			{
				Dock = DockStyle.Fill,
				Margin = new Padding(0, 0, 12, 0),
				Name = $"{prefix}Pane"
			};

			grid = new MoreDataGridView
			{
				AllowUserToAddRows = false,
				AllowUserToResizeRows = false,
				BackgroundColor = SystemColors.ControlLightLight,
				ColumnHeadersVisible = false,
				Dock = DockStyle.Fill,
				EditMode = DataGridViewEditMode.EditProgrammatically,
				MultiSelect = false,
				Name = $"{prefix}Grid",
				ReadOnly = true,
				RowHeadersVisible = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				ShowEditingIcon = false,
				TabIndex = 2,
				ThemedBack = null,
				ThemedFore = null
			};

			grid.RowTemplate.Height = 28;

			var column = new DataGridViewTextBoxColumn
			{
				AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
				MinimumWidth = 100,
				Name = $"{prefix}Column"
			};

			column.DefaultCellStyle.ForeColor = Color.Black;
			grid.Columns.Add(column);

			bar = new MoreToolStrip
			{
				BackColor = Color.FromArgb(240, 240, 240),
				Dock = DockStyle.Top,
				GripStyle = ToolStripGripStyle.Hidden,
				ImageScalingSize = new Size(18, 18),
				Name = $"{prefix}Bar",
				Padding = new Padding(0, 0, 3, 0),
				Stretch = true,
				TabIndex = 1
			};

			var label = new MoreLabel
			{
				AutoSize = false,
				Dock = DockStyle.Top,
				Height = 30,
				Name = $"{prefix}Label",
				Padding = new Padding(0, 0, 0, 2),
				Text = title,
				TextAlign = ContentAlignment.MiddleLeft
			};

			if (prefix == "item")
			{
				itemsLabel = label;
			}

			// docking order: last added is placed first (top)
			pane.Controls.Add(grid);
			pane.Controls.Add(bar);
			pane.Controls.Add(label);

			return pane;
		}


		private static MoreMenuItem MakeButton(
			string name, string glyph, string tip, EventHandler handler)
		{
			var button = new MoreMenuItem
			{
				DisplayStyle = ToolStripItemDisplayStyle.Text,
				Font = new Font("Segoe UI Symbol", 11F),
				ForeColor = Color.Black,
				Name = name,
				Size = new Size(29, 29),
				Text = glyph,
				ToolTipText = tip
			};

			button.Click += handler;
			return button;
		}


		#region Lists
		private void LoadLists(int select, FormList focus = null)
		{
			// lists stay alphabetic; items keep the order the user gave them
			lists.Sort((a, b) => string.Compare(
				a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));

			if (focus is not null)
			{
				select = lists.IndexOf(focus);
			}

			loading = true;
			try
			{
				listGrid.Rows.Clear();
				foreach (var list in lists)
				{
					listGrid.Rows.Add(list.Name);
				}

				if (listGrid.Rows.Count > 0)
				{
					select = Math.Max(0, Math.Min(select, listGrid.Rows.Count - 1));
					listGrid.ClearSelection();
					listGrid.Rows[select].Selected = true;
					listGrid.CurrentCell = listGrid.Rows[select].Cells[0];
				}
			}
			finally
			{
				loading = false;
			}

			ShowItems();
		}


		private int SelectedListIndex =>
			listGrid.SelectedRows.Count > 0 ? listGrid.SelectedRows[0].Index : -1;


		private FormList SelectedList
		{
			get
			{
				var index = SelectedListIndex;
				return index >= 0 && index < lists.Count ? lists[index] : null;
			}
		}


		private void AddList(object sender, EventArgs e)
		{
			using var dialog = new RenameDialog(
				lists.Select(l => l.Name), string.Empty,
				Resx.FormControlsSheet_addList, null, Resx.word_Name);

			if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
			{
				var added = new FormList(dialog.Value.Trim());
				lists.Add(added);
				changed = true;
				LoadLists(0, added);
			}
		}


		private void EditList(object sender, EventArgs e)
		{
			var list = SelectedList;
			if (list is null)
			{
				return;
			}

			var others = lists.Where(l => l != list).Select(l => l.Name);
			using var dialog = new RenameDialog(
				others, list.Name, Resx.FormControlsSheet_addList,
				Resx.FormControlsSheet_renameList, Resx.word_Name)
			{
				Rename = true
			};

			if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
			{
				list.Name = dialog.Value.Trim();
				changed = true;
				LoadLists(0, list);
			}
		}


		private void DeleteList(object sender, EventArgs e)
		{
			var list = SelectedList;
			if (list is null)
			{
				return;
			}

			var result = MoreMessageBox.ShowQuestion(
				FindForm(), string.Format(Resx.FormControlsSheet_deleteList, list.Name));

			if (result == DialogResult.Yes)
			{
				var index = SelectedListIndex;
				lists.Remove(list);
				changed = true;
				LoadLists(index);
			}
		}
		#endregion Lists


		#region Items
		private void ShowItems()
		{
			if (loading)
			{
				return;
			}

			syncing = true;
			try
			{
				itemGrid.Rows.Clear();

				var list = SelectedList;
				itemGrid.AllowUserToAddRows = list is not null;

				itemsLabel.Text = list is null
					? Resx.FormControlsSheet_itemsLabel_Text
					: string.Format(Resx.FormControlsSheet_listItemsLabel_Text, list.Name);

				if (list is not null)
				{
					foreach (var item in list.Items.Where(i => !string.IsNullOrWhiteSpace(i)))
					{
						itemGrid.Rows.Add(item);
					}
				}

				if (itemGrid.Rows.Count > 0)
				{
					itemGrid.CurrentCell = itemGrid.Rows[0].Cells[0];
				}
			}
			finally
			{
				syncing = false;
			}

			EnableButtons();
		}


		// the current row if it is an existing item, not the blank insert row
		private int CurrentItemIndex
		{
			get
			{
				var cell = itemGrid.CurrentCell;
				return cell is not null && !itemGrid.Rows[cell.RowIndex].IsNewRow
					? cell.RowIndex
					: -1;
			}
		}


		private void EnableButtons()
		{
			var hasList = SelectedList is not null;
			var hasItem = hasList && CurrentItemIndex >= 0;

			editListButton.Enabled = hasList;
			deleteListButton.Enabled = hasList;
			addItemButton.Enabled = hasList;
			editItemButton.Enabled = hasItem;
			deleteItemButton.Enabled = hasItem;

			var index = hasItem ? CurrentItemIndex : -1;
			var count = itemGrid.Rows.Count - (itemGrid.AllowUserToAddRows ? 1 : 0);
			moveUpButton.Enabled = hasItem && index > 0;
			moveDownButton.Enabled = hasItem && index < count - 1;
		}


		private void BeginItemEdit(object sender, DataGridViewCellCancelEventArgs e)
		{
			editOriginal = itemGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string
				?? string.Empty;
		}


		// a blank or duplicate entry reverts to what the cell held before the edit
		private void ValidateItem(object sender, DataGridViewCellValidatingEventArgs e)
		{
			var text = (e.FormattedValue as string ?? string.Empty).Trim();
			var isNew = itemGrid.Rows[e.RowIndex].IsNewRow;

			var duplicate = false;
			if (text.Length > 0)
			{
				for (var i = 0; i < itemGrid.Rows.Count && !duplicate; i++)
				{
					duplicate = i != e.RowIndex && !itemGrid.Rows[i].IsNewRow &&
						string.Equals(
							(itemGrid.Rows[i].Cells[0].Value as string)?.Trim(), text,
							StringComparison.OrdinalIgnoreCase);
				}
			}

			if (((text.Length == 0 && !isNew) || duplicate) &&
				itemGrid.EditingControl is TextBox box)
			{
				box.Text = isNew ? string.Empty : editOriginal;
			}
		}


		// rebuilds the selected list's items from the grid after any edit, add or delete
		private void SyncItems()
		{
			var list = SelectedList;
			if (syncing || loading || list is null)
			{
				return;
			}

			list.Items.Clear();
			foreach (DataGridViewRow row in itemGrid.Rows)
			{
				var text = (row.Cells[0].Value as string)?.Trim();
				if (!row.IsNewRow && !string.IsNullOrEmpty(text))
				{
					list.Items.Add(text);
				}
			}

			changed = true;
			EnableButtons();
		}


		// no item keeps leading or trailing whitespace
		private void TrimItem(object sender, DataGridViewCellEventArgs e)
		{
			var cell = itemGrid[e.ColumnIndex, e.RowIndex];
			if (cell.Value is string value && value != value.Trim())
			{
				cell.Value = value.Trim();
			}
		}


		// covers deleting with the Delete key; the Delete button confirms in DeleteItem
		private void ConfirmDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
		{
			if (!ConfirmDelete(e.Row.Index))
			{
				e.Cancel = true;
			}
		}


		private bool ConfirmDelete(int index)
		{
			var text = itemGrid.Rows[index].Cells[0].Value as string ?? string.Empty;
			return MoreMessageBox.ShowQuestion(
				FindForm(), string.Format(Resx.FormControlsSheet_deleteItem, text))
				== DialogResult.Yes;
		}


		private void MoveItemUp(object sender, EventArgs e)
		{
			MoveItem(-1);
		}


		private void MoveItemDown(object sender, EventArgs e)
		{
			MoveItem(1);
		}


		private void MoveItem(int direction)
		{
			var index = CurrentItemIndex;
			var other = index + direction;
			if (index < 0 || other < 0 || other >= itemGrid.Rows.Count - 1)
			{
				return;
			}

			// commit any pending edit so the swap moves what the user sees
			itemGrid.EndEdit();

			var source = itemGrid.Rows[index].Cells[0];
			var target = itemGrid.Rows[other].Cells[0];

			syncing = true;
			try
			{
				(source.Value, target.Value) = (target.Value, source.Value);
			}
			finally
			{
				syncing = false;
			}

			itemGrid.CurrentCell = target;
			SyncItems();
		}


		// moves to the blank insert row, ready to type a new item
		private void AddItem(object sender, EventArgs e)
		{
			if (SelectedList is null)
			{
				return;
			}

			itemGrid.Focus();
			itemGrid.CurrentCell = itemGrid.Rows[itemGrid.Rows.Count - 1].Cells[0];
			itemGrid.BeginEdit(true);
		}


		// makes the current item editable in place
		private void EditItem(object sender, EventArgs e)
		{
			var index = CurrentItemIndex;
			if (index < 0)
			{
				return;
			}

			itemGrid.Focus();
			itemGrid.CurrentCell = itemGrid.Rows[index].Cells[0];
			itemGrid.BeginEdit(true);
		}


		private void DeleteItem(object sender, EventArgs e)
		{
			var index = CurrentItemIndex;
			if (index < 0)
			{
				return;
			}

			itemGrid.CancelEdit();
			if (!ConfirmDelete(index))
			{
				return;
			}

			itemGrid.Rows.RemoveAt(index);
			SyncItems();

			// stay on the row that took its place, or the last item
			var count = itemGrid.Rows.Count - 1;
			if (count > 0)
			{
				itemGrid.CurrentCell = itemGrid.Rows[Math.Min(index, count - 1)].Cells[0];
			}
		}
		#endregion Items


		/// <summary>
		/// Writes the lists and date format to the database; called only when OK is pressed.
		/// </summary>
		public override bool CollectSettings()
		{
			if (!changed)
			{
				return false;
			}

			foreach (var list in lists)
			{
				var items = list.Items
					.Select(i => i?.Trim())
					.Where(i => !string.IsNullOrEmpty(i))
					.ToList();

				list.Items.Clear();
				list.Items.AddRange(items);
			}

			using var forms = new FormControlsProvider();
			forms.WriteLists(lists);

			if (formatBox.SelectedIndex >= 0)
			{
				forms.WriteSetting(FormControlsProvider.DateFormatKey, formats[formatBox.SelectedIndex]);
			}

			return false;
		}
	}
}
