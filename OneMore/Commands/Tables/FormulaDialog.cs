//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using Resx = Properties.Resources;


	internal partial class FormulaDialog : UI.MoreForm
	{
		private readonly int helpHeight;
		private readonly Calculator calculator;
		private readonly Table table;


		public FormulaDialog()
		{
			InitializeComponent();

			helpHeight = helpPanel.Height;
			helpPanel.Visible = false;
			Height -= helpHeight;

			if (NeedsLocalizing())
			{
				Text = Resx.FormulaDialog_Text;

				Localize(new string[]
				{
					"selectedLabel",
					"formulaLabel",
					"formatLabel=word_Format",
					"decLabel",
					"helpBox",
					"statusLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});

				formatBox.Items.Clear();
				formatBox.Items.AddRange(Resx.FormulaDialog_formatBox_Items.Split('\n'));

				validStatusLabel.Text = Resx.FormulaDialog_status_Empty;
			}

			formatBox.SelectedIndex = 0;

			calculator = new Calculator();
			calculator.GetCellValue += GetCellValue;
		}


		public FormulaDialog(Table table)
			: this()
		{
			this.table = table;

			calculator.SetVariable("tablecols", table.ColumnCount);
			calculator.SetVariable("tablerows", table.RowCount);

			var cell = table.GetSelectedCells(out _).First();
			calculator.SetVariable("col", cell.ColNum);
			calculator.SetVariable("row", cell.RowNum);
		}


		public int DecimalPlaces
		{
			get { return (int)decimalBox.Value; }
			set { decimalBox.Value = value; }
		}


		public FormulaFormat Format
		{
			get { return (FormulaFormat)formatBox.SelectedIndex; }
			set { formatBox.SelectedIndex = (int)value; }
		}


		public string Formula
		{
			get => formulaBox.Text;
			set => formulaBox.Text = value;
		}


		public void SetCellNames(string names)
		{
			cellLabel.Text = names;
		}


		public bool Tagged
		{
			get => tagBox.Checked;
			set => tagBox.Checked = value;
		}


		private void ChangedFormula(object sender, EventArgs e)
		{
			var formula = formulaBox.Text.Trim();

			if (formula.Length > 0)
			{
				try
				{
					var result = calculator.Compute(formula);
					validStatusLabel.ForeColor = manager.GetColor("ControlText");

					var text = Format == FormulaFormat.Time
						? TimeSpan.FromMilliseconds(result).ToString()
						: $"{result}";

					validStatusLabel.Text = $"{Resx.word_OK} ({text})";
					tooltip.SetToolTip(validStatusLabel, string.Empty);

					okButton.Enabled = true;
				}
				catch (Exception exc)
				{
					validStatusLabel.ForeColor = manager.GetColor("ErrorText");
					validStatusLabel.Text = Resx.FormulaDialog_status_Invalid;

					tooltip.SetToolTip(validStatusLabel, exc.Message);

					okButton.Enabled = false;
				}
			}
			else
			{
				validStatusLabel.ForeColor = manager.GetColor("ControlText");
				validStatusLabel.Text = Resx.FormulaDialog_status_Empty;
				tooltip.SetToolTip(validStatusLabel, string.Empty);
				okButton.Enabled = false;
			}
		}


		private void GetCellValue(object sender, GetCellValueEventArgs e)
		{
			var cell = table.GetCell(e.Name.ToUpper());
			if (cell is null)
			{
				e.Value = string.Empty;
				return;
			}

			e.Value = cell.GetText().Trim()
				.Replace(AddIn.Culture.NumberFormat.CurrencySymbol, string.Empty)
				.Replace(AddIn.Culture.NumberFormat.PercentSymbol, string.Empty);

			if (TimeSpan.TryParse(e.Value, AddIn.Culture, out var tvalue))
			{
				e.Value = tvalue.TotalMilliseconds.ToString();
			}

			logger.Verbose($"FormulaDialog.GetCellValue({e.Name}) = [{e.Value}]");
		}


		private void ToggleHelp(object sender, EventArgs e)
		{
			if (helpButton.Checked)
			{
				Height += helpHeight;
				helpPanel.Visible = true;
			}
			else
			{
				helpPanel.Visible = false;
				Height -= helpHeight;
			}
		}
	}
}
