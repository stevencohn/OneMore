//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;


	[Flags]
	public enum SelectorScope
	{
		Page = 1,
		Section = 2,
		Notebook = 4,
		Notebooks = 8,
		SelectedNotebooks = 16
	}
}
