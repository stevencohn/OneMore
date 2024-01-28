//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;
	using System.Windows.Forms;


	internal class MoreLabel : Label, IThemedControl
	{
		public Color PreferredBack { get; set; }

		public Color PreferredFore { get; set; }
	}
}
