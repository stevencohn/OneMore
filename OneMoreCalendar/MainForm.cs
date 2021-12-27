//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Windows.Forms;


	public partial class MainForm : Form
	{
		private MonthView monthView;
		private OneNoteProvider provider;


		public MainForm()
		{
			InitializeComponent();

			Width = 1500;
			Height = 1000;

			provider = new OneNoteProvider();
			var pages = provider.GetPages();

			var now = DateTime.Now.Date;

			monthView = new MonthView(now, pages)
			{
				BackColor = System.Drawing.Color.White,
				Dock = DockStyle.Fill,
				Location = new System.Drawing.Point(0, 0),
				Margin = new Padding(0),
				Name = "monthView",
				Size = new System.Drawing.Size(978, 506),
				TabIndex = 0
			};

			contentPanel.Controls.Add(monthView);
		}
	}
}
