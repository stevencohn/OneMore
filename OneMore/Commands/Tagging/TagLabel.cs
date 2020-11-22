//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;


	internal partial class TagLabel : UserControl
	{

		public event EventHandler Deleting;


		public TagLabel()
		{
			InitializeComponent();
		}


		public TagLabel(string name) : this()
		{
			label.Text = name;
			Width = label.Width + xButton.Width + Margin.Left + Margin.Right;
		}


		public string Label => label.Text;


		private void DeleteTag(object sender, EventArgs e)
		{
			if (Deleting != null)
			{
				Deleting(this, e);
			}
		}
	}
}
