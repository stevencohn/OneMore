//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// Changes the theme of the current Form instance to Dark mode if the Windows
	/// system default theme is also Dark mode.
	/// </summary>
	internal class ThemedForm : Form
	{

		protected bool StaticColors { get; set; }


		protected ThemeProvider Theme => ThemeProvider.Instance;


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!StaticColors)
			{
				Theme.InitializeTheme(this);
			}
		}


		public virtual void OnThemeChange()
		{
		}
	}
}
