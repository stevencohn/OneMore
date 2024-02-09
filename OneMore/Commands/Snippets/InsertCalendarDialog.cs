//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class InsertCalendarDialog : UI.MoreForm
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
					"firstLabel",
					"colorLabel",
					"clickLabel",
					"formatLabel=word_Format",
					"smallRadio",
					"largeRadio",
					"indentBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});

				sundayButton.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Sunday);
				mondayButton.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Monday);
			}

			yearBox.Value = DateTime.Now.Year;

			for (int i = 1; i <= 12; i++)
			{
				monthBox.Items.Add(AddIn.Culture.DateTimeFormat.GetMonthName(i));
			}

			monthBox.SelectedIndex = DateTime.Now.Month - 1;

			if (CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday)
			{
				mondayButton.Checked = true;
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			shadingBox.BackColor = Color.AliceBlue;
		}


		public int Year => (int)yearBox.Value;


		public int Month => monthBox.SelectedIndex + 1;


		public DayOfWeek FirstDay => sundayButton.Checked ? DayOfWeek.Sunday : DayOfWeek.Monday;


		public string HeaderShading => shadingBox.BackColor.ToRGBHtml();


		public bool Large => largeRadio.Checked;


		public bool Indent => indentBox.Checked;


		private void ChangeHeadingColor(object sender, EventArgs e)
		{
			var location = PointToScreen(shadingBox.Location);

			using var dialog = new UI.MoreColorDialog(Resx.PageColorDialog_Text,
				location.X + shadingBox.Bounds.Location.X + (shadingBox.Width / 2),
				location.Y - 50);

			dialog.Color = shadingBox.BackColor;

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				shadingBox.BackColor = dialog.Color;
			}
		}
	}
}
