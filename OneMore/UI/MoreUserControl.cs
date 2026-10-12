//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Changes the theme of the current UserControl instance to Dark mode if the Windows
	/// system default theme is also Dark mode.
	/// </summary>
	internal class MoreUserControl : UserControl, IDpiAware
	{
		protected readonly ThemeManager manager;

		private Font ambientFont;

		// controls and tool strip items whose non-ambient fonts have been DPI scaled
		private readonly System.Collections.Generic.HashSet<object> scaledFonts = new();


		protected MoreUserControl()
		{
			manager = ThemeManager.Instance;

			// keep in sync with MoreForm: pin the design-time ambient font so AutoScaleMode.Font
			// is deterministic even when the control is created before it is parented. The size
			// follows the actual monitor DPI, see Scaling.GetAmbientFontScale
			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
				var (dpi, _) = Scaling.GetDpiValues();
				ambientFont = CreateAmbientFont((int)dpi);
				Font = ambientFont;
			}
		}


		/// <summary>
		/// Scales the explicit, non-ambient fonts of any descendants not yet scaled for the
		/// current DPI. This runs once when the control loads; call it again after adding
		/// controls with explicit fonts later, such as lazily built content.
		/// </summary>
		protected void ScaleExplicitFonts()
		{
			Scaling.ScaleExplicitFonts(
				this, Scaling.GetAmbientFontScale(Scaling.GetDpi(this)), scaledFonts, false);
		}


		/// <summary>
		/// Allows an inheritor to recalculate DPI-dependent values it caches, called after
		/// this control's fonts have been rescaled for the new DPI.
		/// </summary>
		protected virtual void OnDpiScaleChanged(int oldDpi, int newDpi)
		{
		}


		private static Font CreateAmbientFont(int dpi)
		{
			return new Font(
				"Microsoft Sans Serif", 8.25F * Scaling.GetAmbientFontScale(dpi));
		}


		/// <summary>
		/// Called by the hosting MoreForm after its DPI changed; mirrors the ambient font
		/// change of the form because this control pins its own font.
		/// </summary>
		void IDpiAware.OnDpiChanged(int oldDpi, int newDpi)
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}

			// When the parent has already rescaled, which rescaled this control's children
			// along with its own bounds, changing the font here must not autoscale them a
			// second time. A control that is not parented, e.g. a hidden sheet, was never
			// touched by a parent so the font change is what rescales it.
			var mode = AutoScaleMode;
			var guard = Parent is not null && mode != AutoScaleMode.None;
			if (guard)
			{
				AutoScaleMode = AutoScaleMode.None;
			}

			try
			{
				var previous = ambientFont;
				ambientFont = CreateAmbientFont(newDpi);
				Font = ambientFont;
				previous?.Dispose();

				if (oldDpi > 0)
				{
					Scaling.ScaleExplicitFonts(this, newDpi / (float)oldDpi, scaledFonts, true);
				}
			}
			finally
			{
				if (guard)
				{
					AutoScaleMode = mode;
				}
			}

			OnDpiScaleChanged(oldDpi, newDpi);
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ambientFont?.Dispose();
				ambientFont = null;
			}

			base.Dispose(disposing);
		}


		protected override void OnLoad(EventArgs e)
		{
			SuspendLayout();
			base.OnLoad(e);

			// fonts that don't inherit the ambient font, before ILoadControl sizing relies on them
			Scaling.ScaleExplicitFonts(
				this, Scaling.GetAmbientFontScale(Scaling.GetDpi(this)), scaledFonts, false);

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
