﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using System;
	using System.Text.RegularExpressions;
	using Resx = Properties.Resources;


	internal partial class FormulaDialog : UI.MoreForm
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

			calculator = new Calculator(null);
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
					calculator.Execute(formula, 0, 0);
					validStatusLabel.ForeColor = manager.GetColor("ControlText");
					validStatusLabel.Text = Resx.word_OK;
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


		private void ResolveSymbol(object sender, SymbolEventArgs e)
		{
			if (Regex.Match(e.Name, Processor.AddressPattern).Success)
			{
				logger.Verbose($"ResolveSymbol({e.Name}) OK");
				e.SetResult(1.0);
				e.Status = SymbolStatus.OK;
			}
			else
			{
				logger.Verbose($"ResolveSymbol({e.Name}) undefined");
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
