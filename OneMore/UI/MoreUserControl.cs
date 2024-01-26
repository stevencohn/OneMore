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
		protected ThemeManager Theme => ThemeManager.Instance;


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Theme.InitializeTheme(this);
		}


		public virtual void OnThemeChange()
		{
		}
	}
}
