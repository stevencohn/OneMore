//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	internal partial class YearsForm : Form
	{
		private const int Radius = 8;


		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		private static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect,     // x-coordinate of upper-left corner
			int nTopRect,      // y-coordinate of upper-left corner
			int nRightRect,    // x-coordinate of lower-right corner
			int nBottomRect,   // y-coordinate of lower-right corner
			int nWidthEllipse, // width of ellipse
			int nHeightEllipse // height of ellipse
		);


		public YearsForm()
		{
			InitializeComponent();

			FormBorderStyle = FormBorderStyle.None;
			Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, Radius, Radius));
		}


		protected override async void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignMode)
			{
				var years = await new OneNoteProvider()
					.GetYears(await new SettingsProvider().GetNotebookIDs());

				years.ToList().ForEach(y =>
				{
					listView.Items.Add(y.ToString());
				});
			}
		}


		public int Year => 2022;



		private void YearsForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}


		private void Cancel(object sender, EventArgs e)
		{
			Close();
		}


		private void Apply(object sender, EventArgs e)
		{

			Close();
		}

		private void YearsForm_Leave(object sender, EventArgs e)
		{
			Close();
		}


		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			using (var pen = new Pen(AppColors.PressedBorder))
			{
				var r = new Rectangle(0, 0, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
				e.Graphics.DrawRoundedRectangle(pen, r, Radius);
			}
		}
	}
}
