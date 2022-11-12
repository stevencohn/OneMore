﻿//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	/// <summary>
	/// Changes the theme of the current Form instance to Dark mode if the Windows
	/// system default theme is also Dark mode.
	/// </summary>
	internal abstract class ThemedForm : Form
	{
		public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

		[DllImport("dwmapi.dll", PreserveSig = true)]
		public static extern int DwmSetWindowAttribute(
			IntPtr hwnd, int attr, ref bool attrValue, int attrSize);


		protected bool StaticColors { get; set; }


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!StaticColors && Office.SystemDefaultDarkMode())
			{
				// true=dark, false=normal
				var value = true;

				DwmSetWindowAttribute(
					Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, Marshal.SizeOf(value));

				// controls...

				BackColor = AppColors.BackColor;
				ForeColor = AppColors.ForeColor;

				Colorize(Controls);
			}
		}


		private void Colorize(Control.ControlCollection controls)
		{
			foreach (Control control in controls)
			{
				control.BackColor = AppColors.BackColor;
				control.ForeColor = AppColors.ForeColor;

				if (control.Controls.Count > 0)
				{
					Colorize(control.Controls);
				}

				if (control is ListView view)
				{
					foreach (ListViewItem item in view.Items)
					{
						item.BackColor = AppColors.BackColor;
						item.ForeColor = AppColors.ForeColor;
					}
				}
			}
		}
	}
}
