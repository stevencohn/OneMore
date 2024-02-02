//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Windows.Forms;


	internal class MoreTextBox : TextBox, IThemedControl
	{
		public MoreTextBox()
		{
			BorderStyle = BorderStyle.FixedSingle;
		}


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		public void ApplyTheme(ThemeManager manager)
		{
			ForeColor = manager.GetThemedColor(ThemedFore ?? "WindowText");
			BackColor = manager.GetThemedColor(Enabled ? (ThemedBack ?? "Window") : "InactiveWindow");
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			ApplyTheme(ThemeManager.Instance);
		}
	}
}
