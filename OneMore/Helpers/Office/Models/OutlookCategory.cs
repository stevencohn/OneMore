//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	internal class OutlookCategory
	{

		/// <summary>
		/// Gets or sets the category name. This is the name that will be displayed in Outlook.
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Gets or sets the category color name. This is the name of the color that will be 
		/// displayed in Outlook. There is only a small preset list of color names.
		/// </summary>
		public string ColorName { get; set; }
	}
}
