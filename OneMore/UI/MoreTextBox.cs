//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Windows.Forms;


	internal class MoreTextBox : TextBox, ILoadControl
	{
		public MoreTextBox()
		{
			BorderStyle = BorderStyle.FixedSingle;
		}


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;
			ForeColor = manager.GetColor(ThemedFore ?? "WindowText");
			BackColor = manager.GetColor(Enabled ? (ThemedBack ?? "Window") : "InactiveWindow");
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			((ILoadControl)this).OnLoad();
		}
	}
}
