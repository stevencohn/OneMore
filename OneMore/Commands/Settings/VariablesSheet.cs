//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class VariablesSheet : SheetBase
	{

		private readonly BindingList<Variable> variables;
		private bool loaded = false;


		public VariablesSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "VariablesSheet";
			Title = Resx.VariablesSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"sortButton=word_Sort",
					"deleteButton=word_Delete"
				});

				nameColumn.HeaderText = Resx.word_Name;
				valueColumn.HeaderText = Resx.word_Value;
			}

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Name";
			gridView.Columns[1].DataPropertyName = "Value";

			(_, float scaleY) = UI.Scaling.GetScalingFactors();
			gridView.RowTemplate.Height = (int)(16 * scaleY);

			// must be editable; otherwise gridView thinks it's reaonly!
			variables = new BindingList<Variable>(LoadVariables())
			{
				AllowEdit = true,
				AllowNew = true,
				AllowRemove = true
			};

			gridView.DataSource = variables;
		}


		private static List<Variable> LoadVariables()
		{
			using var provider = new VariableProvider();
			return provider.ReadVariables();
		}


		private void gridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.ColumnIndex == 0)
			{
				var name = e.FormattedValue.ToString().ToLower();
				if (name.In("tablecols", "tablerows"))
				{
					e.Cancel = true;
					UI.MoreMessageBox.Show(this,
						Resx.VariablesSheet_reserved,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);

					return;
				}

				for (var i = 0; i < variables.Count; i++)
				{
					if (i != e.RowIndex && variables[i].Name == e.FormattedValue.ToString())
					{
						e.Cancel = true;
						UI.MoreMessageBox.Show(this,
							Resx.VariablesSheet_uniqueError,
							MessageBoxButtons.OK, MessageBoxIcon.Warning);

						return;
					}
				}
			}
			else
			{
				if (!double.TryParse(e.FormattedValue.ToString(), out _))
				{
					e.Cancel = true;
					UI.MoreMessageBox.Show(this,
						Resx.VariablesSheet_doubleWarning,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}


		private void gridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			if (!loaded)
			{
				//gridView.Rows[gridView.Rows.Count - 1].Cells[1].Selected = true;
				//foreach (DataGridViewRow row in gridView.Rows)
				//{
				//	row.ReadOnly = false;
				//}

				loaded = true;
			}
		}


		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				if (gridView.IsCurrentCellDirty)
				{
					gridView.CancelEdit();
				}

				return true;
			}

			if (keyData == Keys.Delete)
			{
				if (gridView.SelectedCells.Count > 0)
				{
					var result = UI.MoreMessageBox.Show(this,
						Resx.VariablesSheet_deleteVariables,
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (result == DialogResult.Yes)
					{
						var rows = gridView.SelectedCells
							.Cast<DataGridViewCell>()
							.Select(c => c.OwningRow)
							.Distinct()
							.ToList();

						foreach (DataGridViewRow row in rows)
						{
							if (!row.IsNewRow)
							{
								gridView.Rows.Remove(row);
							}
						}
					}
				}

				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}


		private void deleteButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count == 0)
				return;

			int rowIndex = gridView.SelectedCells[0].RowIndex;
			if (rowIndex >= variables.Count)
				return;

			var variable = variables[rowIndex];

			var result = UI.MoreMessageBox.Show(this,
				string.Format(Resx.VariablesSheet_deleteVariable, variable.Name),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
				return;

			variables.RemoveAt(rowIndex);

			rowIndex--;
			if (rowIndex >= 0)
			{
				gridView.Rows[rowIndex].Cells[0].Selected = true;
			}
		}


		private void SortItems(object sender, EventArgs e)
		{
			var ordered = variables
				.Where(v => !string.IsNullOrWhiteSpace(v.Name))
				.OrderBy(v => v.Name).ToList();

			variables.Clear();
			foreach (var variable in ordered)
			{
				variables.Add(variable);
			}
		}


		public override bool CollectSettings()
		{
			// there no settings here; variables are stored in the DB...

			using var provider = new VariableProvider();

			if (variables.Any())
			{
				provider.WriteVariables(variables);
			}
			else
			{
				provider.DeleteVariables();
			}

			return false;
		}
	}
}
