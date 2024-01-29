//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	internal interface IThemedControl
	{

		/// <summary>
		/// Gets or sets the preferred background color
		/// </summary>
		string PreferredBack { get; set; }


		/// <summary>
		/// Gets or sets the preferred foreground color, the text color
		/// </summary>
		string PreferredFore { get; set; }
	}
}
