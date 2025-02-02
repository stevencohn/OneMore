//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Ribbon
{
	internal enum RibbonPosiition
	{
		// NOTE that these values MUST match the RibbonBarSheet combobox value indexes.
		// If these enums are modified, it will likely break user's established settings
		// so may need migration if changed...

		// NOTE that the name of each enum equates to its insertAfterMso name

		GroupClipboard = 0,
		GroupBasicText = 1,
		GroupStyles = 2,
		GroupTagging = 3,
		GroupShareEmail = 4,
		GroupOutlook = 5,
		TabHome = 6,
		TabInsert = 7,
		TabDraw = 8,
		TabHistory = 9,
		TabReview = 10,
		TabView = 11,
		TabHelp = 12,
		End = 99
	}
}
