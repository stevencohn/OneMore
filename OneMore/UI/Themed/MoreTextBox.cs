//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	internal class MoreTextBox : TextBox, IThemedControl
	{
		public MoreTextBox()
		{
			BorderStyle = BorderStyle.FixedSingle;
		}


		/// <summary>
		/// Gets or sets the preferred background color
		/// </summary>
		public string PreferredBack { get; set; }


		/// <summary>
		/// Gets or sets the preferred foreground color
		/// </summary>
		public string PreferredFore { get; set; }
	}
}
