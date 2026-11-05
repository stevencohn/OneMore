//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Globalization;
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

			// refresh the preview when format or decimal places change
			formatBox.SelectedIndexChanged += ChangedFormula;
			decimalBox.ValueChanged += ChangedFormula;
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

			LoadVariables(calculator);
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


		private static void LoadVariables(Calculator calculator)
		{
			using var provider = new VariableProvider();
			var variables = provider.ReadVariables();
			foreach (var variable in variables)
			{
				calculator.SetVariable(variable.Name, variable.Value);
			}
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

					var text = FormatResult(result);

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


		private string FormatResult(double result)
		{
			var dplaces = DecimalPlaces;

			switch (Format)
			{
				case FormulaFormat.Currency:
					return result.ToString($"C{dplaces}", AddIn.Locale);

				case FormulaFormat.Number:
					return result.ToString($"N{dplaces}", AddIn.Locale);

				case FormulaFormat.Percentage:
					return (result / 100).ToString($"P{dplaces}", AddIn.Locale);

				case FormulaFormat.Time:
					return TimeSpan.FromMilliseconds(result).ToString();

				default:
					return $"{result}";
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

			var text = cell.GetTextLines()
				.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim()
				?? string.Empty;

			if (Processor.TryParseNumber(text, out var dvalue))
			{
				e.Value = dvalue.ToString(CultureInfo.InvariantCulture);
			}
			else if (TimeSpan.TryParse(text, AddIn.Locale, out var tvalue))
			{
				e.Value = tvalue.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				e.Value = text;
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
