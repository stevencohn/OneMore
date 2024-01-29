//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;
	using System.Windows.Forms;


	internal class MoreLabel : Label, IThemedControl
	{

		public string PreferredBack { get; set; }

		public string PreferredFore { get; set; }


		public void SetMultilineWrapWidth(int width)
		{
			MaximumSize = new Size(width, 0);
			AutoSize = true;
		}
	}
}
