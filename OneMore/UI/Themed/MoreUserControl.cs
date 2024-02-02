//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// Changes the theme of the current UserControl instance to Dark mode if the Windows
	/// system default theme is also Dark mode.
	/// </summary>
	internal class MoreUserControl : UserControl
	{
		protected readonly ThemeManager manager;


		protected MoreUserControl()
		{
			manager = ThemeManager.Instance;
		}


		protected override void OnLoad(EventArgs e)
		{
			SuspendLayout();
			base.OnLoad(e);
			manager.InitializeTheme(this);
			ResumeLayout();
		}


		public virtual void OnThemeChange()
		{
		}
	}
}
