//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Resx = Properties.Resources;


	internal partial class CustomSortDialog : UI.MoreForm
	{
		private readonly IReadOnlyList<string> names;
		private readonly IReadOnlyList<SortDataType> detected;


		public CustomSortDialog()
		{
			InitializeComponent();
		}


		public CustomSortDialog(
			IReadOnlyList<string> names, IReadOnlyList<SortDataType> detected)
			: this()
		{
			this.names = names;
			this.detected = detected;

			if (NeedsLocalizing())
			{
				Text = Resx.CustomSortDialog_Text;

				Localize(new string[]
				{
					"columnLabel",
					"orderLabel",
					"typeLabel",
					"addButton",
					"rulesLabel",
					"noteLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			ruleList.Columns[0].Text = "#";
			ruleList.Columns[1].Text = Resx.CustomSortDialog_columnHeader;
			ruleList.Columns[2].Text = Resx.CustomSortDialog_orderHeader;
			ruleList.Columns[3].Text = Resx.CustomSortDialog_typeHeader;
			ruleList.SetColumnProportions(0.08f, 0.40f, 0.24f, 0.28f);

			orderBox.Items.AddRange(new object[]
			{
				Resx.CustomSortDialog_ascending,
				Resx.CustomSortDialog_descending
			});

			typeBox.Items.AddRange(new object[]
			{
				Resx.CustomSortDialog_auto,
				Resx.CustomSortDialog_textType,
				Resx.CustomSortDialog_dateTimeType,
				Resx.CustomSortDialog_numberType
			});

			orderBox.SelectedIndex = 0;
			typeBox.SelectedIndex = 0;

			RefreshColumns();
			UpdateButtons();
		}


		// drop-down entry that remembers the table column index it represents
		private sealed class ColumnItem
		{
			public ColumnItem(int index, string text)
			{
				Index = index;
				Text = text;
			}

			public int Index { get; }
			private string Text { get; }
			public override string ToString() => Text;
		}


		/// <summary>
		/// Repopulates the column drop-down with only those columns not yet used by a rule.
		/// </summary>
		private void RefreshColumns()
		{
			var used = new HashSet<int>(
				ruleList.Items.Cast<System.Windows.Forms.ListViewItem>()
					.Select(i => ((SortRule)i.Tag).ColumnIndex));

			columnBox.BeginUpdate();
			columnBox.Items.Clear();

			for (var i = 0; i < names.Count; i++)
			{
				if (!used.Contains(i))
				{
					columnBox.Items.Add(new ColumnItem(i, GetColumnDisplay(i)));
				}
			}

			columnBox.EndUpdate();
			columnBox.SelectedIndex = columnBox.Items.Count > 0 ? 0 : -1;
		}


		/// <summary>
		/// Gets the sort rules in priority order, first is primary.
		/// </summary>
		public IReadOnlyList<SortRule> Rules =>
			ruleList.Items.Cast<System.Windows.Forms.ListViewItem>()
				.Select(i => (SortRule)i.Tag)
				.ToList();


		protected override void OnDpiScaleChanged(int oldDpi, int newDpi)
		{
			ruleList.SetColumnProportions(0.08f, 0.40f, 0.24f, 0.28f);
		}


		private string GetColumnDisplay(int index)
		{
			var name = names[index];

			// don't repeat the number for default "Column N" names
			return name == string.Format(Resx.CustomSortDialog_ColumnName, index + 1)
				? name
				: string.Format(Resx.CustomSortDialog_ColumnDisplay, name, index + 1);
		}


		private static string GetOrderText(bool descending)
		{
			return descending
				? Resx.CustomSortDialog_descending
				: Resx.CustomSortDialog_ascending;
		}


		private static string GetTypeText(SortDataType type)
		{
			switch (type)
			{
				case SortDataType.DateTime: return Resx.CustomSortDialog_dateTimeType;
				case SortDataType.Number: return Resx.CustomSortDialog_numberType;
				case SortDataType.Text: return Resx.CustomSortDialog_textType;
				default: return Resx.CustomSortDialog_auto;
			}
		}


		private void AddRule(object sender, EventArgs e)
		{
			if (columnBox.SelectedItem is not ColumnItem column)
			{
				return;
			}

			var rule = new SortRule
			{
				ColumnIndex = column.Index,
				Descending = orderBox.SelectedIndex == 1,
				DataType = (SortDataType)typeBox.SelectedIndex
			};

			if (rule.DataType == SortDataType.Auto)
			{
				rule.DataType = detected[rule.ColumnIndex];
			}

			var item = new System.Windows.Forms.ListViewItem(string.Empty) { Tag = rule };
			item.SubItems.Add(GetColumnDisplay(rule.ColumnIndex));
			item.SubItems.Add(GetOrderText(rule.Descending));
			item.SubItems.Add(GetTypeText(rule.DataType));
			ruleList.Items.Add(item);

			// a used column is no longer offered until its rule is deleted
			RefreshColumns();
			Renumber();
			item.Selected = true;
			item.EnsureVisible();
			UpdateButtons();
		}


		private void Renumber()
		{
			for (var i = 0; i < ruleList.Items.Count; i++)
			{
				ruleList.Items[i].Text = (i + 1).ToString();
			}

			ruleList.Invalidate();
		}


		private void ChangeSelection(object sender, EventArgs e)
		{
			UpdateButtons();
		}


		private void UpdateButtons()
		{
			var item = ruleList.SelectedItems.Count > 0 ? ruleList.SelectedItems[0] : null;
			upButton.Enabled = item is not null && item.Index > 0;
			downButton.Enabled = item is not null && item.Index < ruleList.Items.Count - 1;
			deleteButton.Enabled = item is not null;
			okButton.Enabled = ruleList.Items.Count > 0;
			addButton.Enabled = columnBox.Items.Count > 0;
		}


		private void MoveUp(object sender, EventArgs e)
		{
			MoveItem(-1);
		}


		private void MoveDown(object sender, EventArgs e)
		{
			MoveItem(1);
		}


		private void MoveItem(int direction)
		{
			if (ruleList.SelectedItems.Count == 0)
			{
				return;
			}

			var item = ruleList.SelectedItems[0];
			var index = item.Index + direction;
			if (index < 0 || index >= ruleList.Items.Count)
			{
				return;
			}

			ruleList.BeginUpdate();
			ruleList.Items.RemoveAt(item.Index);
			ruleList.Items.Insert(index, item);
			ruleList.EndUpdate();

			Renumber();
			item.Selected = true;
			item.EnsureVisible();
			UpdateButtons();
		}


		private void DeleteRule(object sender, EventArgs e)
		{
			if (ruleList.SelectedItems.Count == 0)
			{
				return;
			}

			var index = ruleList.SelectedItems[0].Index;
			ruleList.Items.RemoveAt(index);
			Renumber();
			RefreshColumns();

			if (ruleList.Items.Count > 0)
			{
				ruleList.Items[Math.Min(index, ruleList.Items.Count - 1)].Selected = true;
			}

			UpdateButtons();
		}
	}
}
