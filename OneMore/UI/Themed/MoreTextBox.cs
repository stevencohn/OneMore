//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;
	using System.Windows.Forms;


	internal class MoreTextBox : TextBox, IThemedControl
	{
		/// <summary>
		/// Gets or sets the preferred background color
		/// </summary>
		public Color PreferredBack { get; set; } = Color.Empty;


		/// <summary>
		/// Gets or sets the preferred foreground color
		/// </summary>
		public Color PreferredFore { get; set; } = Color.Empty;
	}
}
