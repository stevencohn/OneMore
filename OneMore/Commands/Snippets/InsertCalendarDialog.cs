//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class InsertCalendarDialog : UI.LocalizableForm
	{
		public InsertCalendarDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.InsertCalendarDialog_Text;

				Localize(new string[]
				{
					"yearLabel",
					"monthLabel",
					"formatLabel",
					"smallRadio",
					"largeRadio",
					"indentBox",
					"okButton",
					"cancelButton"
				});
			}

			yearBox.Value = DateTime.Now.Year;

			for (int i = 1; i <= 12; i++)
			{
				monthBox.Items.Add(AddIn.Culture.DateTimeFormat.GetMonthName(i));
			}

			monthBox.SelectedIndex = DateTime.Now.Month - 1;
		}


		public int Year => (int)yearBox.Value;


		public int Month => monthBox.SelectedIndex + 1;


		public bool Large => largeRadio.Checked;


		public bool Indent => indentBox.Checked;
	}
}
