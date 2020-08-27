//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using System.Windows.Forms;
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
					"rangeLabel",
					"formatLabel",
					"functionLabel",
					"okButton",
					"cancelButton",
					"colButton",
					"rowButton"
				});

				formatBox.Items.Clear();
				formatBox.Items.AddRange(Resx.FomulaDialog_formatBox_Items.Split(new char[] { '\n' }));

				functionBox.Items.Clear();
				functionBox.Items.AddRange(Resx.FormulaDialog_functionBox_Items.Split(new char[] { '\n' }));
			}

			formatBox.SelectedIndex = 0;
			functionBox.SelectedIndex = 0;
		}


		internal FormulaDirection Direction
		{
			get
			{
				return colButton.Checked ? FormulaDirection.Vertical: FormulaDirection.Horizontal;
			}

			set
			{
				if (value == FormulaDirection.Horizontal)
				{
					rowButton.Checked = true;
				}
				else
				{
					colButton.Checked = true;
				}
			}
		}


		internal FormulaFormat Format
		{
			get { return (FormulaFormat)formatBox.SelectedIndex; }
			set { formatBox.SelectedIndex = (int)value; }
		}


		internal FormulaFunction Function
		{
			get { return (FormulaFunction)functionBox.SelectedIndex; }
			set { functionBox.SelectedIndex = (int)value; }
		}


		private void OK(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
