//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	internal class MoreLabel : Label, IThemedControl
	{

		public string PreferredBack { get; set; }

		public string PreferredFore { get; set; }
	}
}
