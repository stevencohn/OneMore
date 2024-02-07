//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	internal class MoreLabel : Label, ILoadControl
	{

		public string ThemedBack { get; set; }

		public string ThemedFore { get; set; }


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;

			BackColor = Parent != null
				? Parent.BackColor
				: manager.GetColor("Control", ThemedBack);

			ForeColor = Enabled
				? manager.GetColor("ControlText", ThemedFore)
				: manager.GetColor("GrayText");
		}
	}
}
