//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;


	internal interface IThemedControl
	{

		/// <summary>
		/// Gets or sets the preferred background color
		/// </summary>
		Color PreferredBack { get; set; }


		/// <summary>
		/// Gets or sets the preferred foreground color, the text color
		/// </summary>
		Color PreferredFore { get; set; }
	}
}
