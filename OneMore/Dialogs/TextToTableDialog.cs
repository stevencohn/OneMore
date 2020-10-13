//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class TextToTableDialog : LocalizableForm
	{
		public enum Delimeter
		{
			Paragraphs,
			Tabs,
			Commas,
			Other
		}

		private int userCols;
		private int userRows;


		public TextToTableDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				//Text = Resx.TextToTableDialog_Text;

				Localize(new string[]
				{
					"group1",
					"paragraphsRadio",
					"tabsRadio",
					"commasRadio",
					"otherRadio",
					"otherBox",
					"group2",
					"columnsLabel",
					"columnsBox",
					"rowsLabel",
					"rowsBox",
					"okButton",
					"cancelButton"
				});
			}
		}


		public Delimeter DelimetedBy
		{
			get
			{
				if (paragraphsRadio.Checked) return Delimeter.Paragraphs;
				if (tabsRadio.Checked) return Delimeter.Tabs;
				if (commasRadio.Checked) return Delimeter.Commas;
				return Delimeter.Other;
			}

			set
			{
				switch (value)
				{
					case Delimeter.Paragraphs: paragraphsRadio.Checked = true; break;
					case Delimeter.Tabs: tabsRadio.Checked = true; break;
					case Delimeter.Commas: commasRadio.Checked = true; break;
					case Delimeter.Other: otherRadio.Checked = true; break;
				}
			}
		}


		public string CustomDelimeter
		{
			get => otherBox.Text;
			set => otherBox.Text = value;
		}


		public int Columns
		{
			get => (int)columnsBox.Value;

			set
			{
				columnsBox.Value = userCols = value;
				columnsBox.Minimum = value;
			}
		}


		public int Rows
		{
			get => (int)rowsBox.Value;

			set
			{
				rowsBox.Value = userRows = value;
				rowsBox.Minimum = value;
			}
		}


		private void ChangeDelimetedBy(object sender, System.EventArgs e)
		{
			if (paragraphsRadio.Checked)
			{
				columnsBox.Value = 1;
				otherBox.Text = string.Empty;
				otherBox.Enabled = false;
			}
			else
			{
				columnsBox.Value = userCols;
				otherBox.Enabled = otherRadio.Checked;

				if (otherRadio.Checked && (otherBox.Text == string.Empty))
				{
					otherBox.Text = "~";
				}
			}

			okButton.Enabled = true;
		}


		private void ChangeCustomDelimeter(object sender, System.EventArgs e)
		{
			if (otherBox.Enabled)
			{
				okButton.Enabled = otherBox.Text.Length > 0;
			}
		}
	}
}
