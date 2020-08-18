//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;


	public partial class FormulaDialog : Form
	{
		public FormulaDialog()
		{
			InitializeComponent();

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
