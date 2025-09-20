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


		/// <summary>
		/// Localizes each of the named controls by looking up the appropriate resource string
		/// </summary>
		/// <param name="keys">An collection of strings specifying the control names</param>
		protected void Localize(string[] keys)
		{
			Translator.Localize(this, keys);
		}


		/// <summary>
		/// Determines if the sheet needs to be localized
		/// </summary>
		/// <returns>True if the sheets needs to be localized; language is not default 'en'</returns>
		protected static bool NeedsLocalizing()
		{
			return AddIn.Culture.TwoLetterISOLanguageName != "en";
		}
	}
}
