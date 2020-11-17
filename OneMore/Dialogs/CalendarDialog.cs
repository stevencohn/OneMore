//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class CalendarDialog : LocalizableForm
	{
		public CalendarDialog()
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


		protected override void OnShown(EventArgs e)
		{
			Location = new Point(Location.X, Location.Y - (Height / 2));
			base.OnShown(e);
		}


		public int Year => (int)yearBox.Value;


		public int Month => monthBox.SelectedIndex + 1;


		public bool Large => largeRadio.Checked;
	}
}
