//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	internal interface IThemedControl
	{

		/// <summary>
		/// Gets or sets the preferred theme background color.
		/// This is required for special theme-only named colors, like ErrorText
		/// </summary>
		string ThemedBack { get; set; }


		/// <summary>
		/// Gets or sets the preferred theme foreground (text) color.
		/// This is required for special theme-only named colors, like ErrorText
		/// </summary>
		string ThemedFore { get; set; }


		void ApplyTheme(ThemeManager manager);
	}
}
