//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// Displays a tag label with its own Delete button. Used in the TaggingDialog for adding
	/// and removing tags to a page.
	/// </summary>
	internal partial class TagControl : UserControl
	{

		public TagControl()
		{
			InitializeComponent();
		}


		public TagControl(string name) : this()
		{
			label.Text = name;
			Width = label.Width + xButton.Width + Margin.Left + Margin.Right;
		}


		/// <summary>
		/// Fires when this tag's delete buttin is clicked
		/// </summary>
		public event EventHandler Deleting;


		/// <summary>
		/// Get the text of this tag
		/// </summary>
		public string Label => label.Text;


		private void DeleteTag(object sender, EventArgs e)
		{
			Deleting?.Invoke(this, e);
		}
	}
}
