//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class InsertCalendarDialog : UI.MoreForm
	{
		// the shading is applied to page content so, as in MoreColorPicker, the swatch shows how
		// OneNote renders it on a dark page while the applied color is kept in its Tag
		private readonly Color pageColor;
		private readonly bool darkPage;


		/// <param name="pageColor">
		/// The background color of the current page, which determines dark-mode rendering of the
		/// shading color; empty means the page follows the Office theme
		/// </param>
		public InsertCalendarDialog(Color pageColor = default)
		{
			InitializeComponent();

			this.pageColor = pageColor;
			darkPage = UI.MoreColorPicker.IsDarkPage(pageColor);

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

				sundayButton.Text = AddIn.Culture.DateTimeFormat.GetDayName(DayOfWeek.Sunday);
				mondayButton.Text = AddIn.Culture.DateTimeFormat.GetDayName(DayOfWeek.Monday);
			}

			yearBox.Value = DateTime.Now.Year;

			for (int i = 1; i <= 12; i++)
			{
				monthBox.Items.Add(AddIn.Locale.DateTimeFormat.GetMonthName(i));
			}

			monthBox.SelectedIndex = DateTime.Now.Month - 1;

			if (AddIn.Locale.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday)
			{
				mondayButton.Checked = true;
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetShading(Color.AliceBlue);
		}


		public int Year => (int)yearBox.Value;


		public int Month => monthBox.SelectedIndex + 1;


		public DayOfWeek FirstDay => sundayButton.Checked ? DayOfWeek.Sunday : DayOfWeek.Monday;


		public string HeaderShading => GetShading().ToRGBHtml();


		public bool Large => largeRadio.Checked;


		public bool Indent => indentBox.Checked;


		private void ChangeHeadingColor(object sender, EventArgs e)
		{
			var location = PointToScreen(shadingBox.Location);

			using var picker = new UI.MoreColorPicker(pageColor,
				location.X + shadingBox.Bounds.Location.X + (shadingBox.Width / 2),
				location.Y - 50,
				Resx.PageColorDialog_Text)
			{
				Color = GetShading(),
				AutomaticVisible = false
			};

			if (picker.ShowDialog(this) == DialogResult.OK)
			{
				SetShading(picker.Color);
			}
		}


		private void SetShading(Color color)
		{
			shadingBox.Tag = color;
			shadingBox.BackColor = darkPage ? UI.ColorPalette.ToDark(color) : color;
		}


		private Color GetShading()
		{
			return shadingBox.Tag is Color color ? color : shadingBox.BackColor;
		}
	}
}
