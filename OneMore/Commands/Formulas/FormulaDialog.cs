//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Formulas;
	using System;
	using System.Drawing;
	using System.Text.RegularExpressions;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class FormulaDialog : UI.LocalizableForm
	{
		private readonly int helpHeight;
		private readonly Calculator calculator;


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
					"formatLabel",
					"decLabel",
					"helpButton",
					"helpBox",
					"statusLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});

				formatBox.Items.Clear();
				formatBox.Items.AddRange(Resx.FormulaDialog_formatBox_Items.Split(new char[] { '\n' }));

				validStatusLabel.Text = Resx.FormulaDialog_status_Empty;
			}

			formatBox.SelectedIndex = 0;

			calculator = new Calculator();
			calculator.ProcessSymbol += ResolveSymbol;
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
					calculator.Execute(formula);
					validStatusLabel.ForeColor = SystemColors.WindowText;
					validStatusLabel.Text = Resx.FormulaDialog_status_OK;
					okButton.Enabled = true;
				}
				catch
				{
					validStatusLabel.ForeColor = Color.Red;
					validStatusLabel.Text = Resx.FormulaDialog_status_Invalid;
					okButton.Enabled = false;
				}
			}
			else
			{
				validStatusLabel.ForeColor = SystemColors.WindowText;
				validStatusLabel.Text = Resx.FormulaDialog_status_Empty;
				okButton.Enabled = false;
			}
		}


		private void ResolveSymbol(object sender, SymbolEventArgs e)
		{
			if (Regex.Match(e.Name, @"^([a-zA-Z]{1,3})(\d{1,3})$").Success)
			{
				e.Result = 1.0;
				e.Status = SymbolStatus.OK;
			}
			else
			{
				e.Status = SymbolStatus.UndefinedSymbol;
			}
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
