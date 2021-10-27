//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class TextToTableDialog : UI.LocalizableForm
	{
		public enum Delimeter
		{
			Paragraphs,
			Tabs,
			// variant of tabs; OneNote sometimes converts tabs to 8x "&nbsp;"
			Nbsp,
			Commas,
			Other
		}

		private int userCols;
		private bool nbsp;


		public TextToTableDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.TextToTableDialog_Text;

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
					"headerBox",
					"unquoteBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public Delimeter DelimetedBy
		{
			get
			{
				if (paragraphsRadio.Checked) return Delimeter.Paragraphs;
				if (tabsRadio.Checked) return nbsp ? Delimeter.Nbsp : Delimeter.Tabs;
				if (commasRadio.Checked) return Delimeter.Commas;
				return Delimeter.Other;
			}

			set
			{
				switch (value)
				{
					case Delimeter.Paragraphs: paragraphsRadio.Checked = true; break;
					case Delimeter.Tabs: tabsRadio.Checked = true; break;
					case Delimeter.Nbsp: tabsRadio.Checked = nbsp = true; break;
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
				rowsBox.Value = value;
				rowsBox.Minimum = value;
			}
		}


		public bool HasHeader => headerBox.Checked;


		public bool Unquote => unquoteBox.Checked;


		private void ChangeDelimetedBy(object sender, System.EventArgs e)
		{
			if (paragraphsRadio.Checked)
			{
				columnsBox.Minimum = 1;
				columnsBox.Value = 1;
				columnsBox.Enabled = false;
				otherBox.Text = string.Empty;
				otherBox.Enabled = false;
			}
			else
			{
				if (!columnsBox.Enabled)
				{
					// coming from paragraphsRadio so reset
					columnsBox.Minimum = userCols;
					columnsBox.Value = userCols;
					columnsBox.Enabled = true;
				}

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
