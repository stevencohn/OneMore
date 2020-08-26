//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;


	public partial class TagPickerDialog : Form
	{
		private class Zone
		{
			public string Tag;
			public Rectangle Location;
		}

		private Graphics graphics;
		private List<Zone> zones;


		public TagPickerDialog()
		{
			InitializeComponent();

			graphics = Graphics.FromImage(pictureBox.Image);

			zones = new List<Zone>
			{
				new Zone { Tag = "", Location = new Rectangle { X=0, Y=0, Width=16, Height=16 } }
			};
		}


		private void pictureBox_MouseMove(object sender, MouseEventArgs e)
		{

		}


		private void pictureBox_Click(object sender, EventArgs e)
		{

		}
	}
}
