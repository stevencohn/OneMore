//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using River.OneMoreAddIn.Commands.Formula;
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class FormulaDialog : LocalizableForm
	{
		public FormulaDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.FormulaDialog_Text;

				Localize(new string[]
				{
					"currentCellLabel",
					"formulaLabel",
					"formatLabel",
					"helpBox",
					"okButton",
					"cancelButton"
				});

				formatBox.Items.Clear();
				formatBox.Items.AddRange(Resx.FomulaDialog_formatBox_Items.Split(new char[] { '\n' }));
			}

			formatBox.SelectedIndex = 0;
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
		}


		internal FormulaFormat Format
		{
			get { return (FormulaFormat)formatBox.SelectedIndex; }
			set { formatBox.SelectedIndex = (int)value; }
		}


		internal string Formula
		{
			get => formulaBox.Text;
			set => formulaBox.Text = value;
		}


		internal void SetCellNames(string names)
		{
			cellLabel.Text = names;
		}


		private void ChangedFormula(object sender, EventArgs e)
		{
			okButton.Enabled = formulaBox.Text.Trim().Length > 0;
		}
	}
}
