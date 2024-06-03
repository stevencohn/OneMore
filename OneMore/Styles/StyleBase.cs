//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3005 // Identifiers differ only in case
#pragma warning disable CA1012 // Abstract types should not have constructors

namespace River.OneMoreAddIn.Styles
{
	using Microsoft.Win32;
	using System;
	using System.Drawing;
	using System.Globalization;


	/// <summary>
	/// Base properties of a style to be inherited by all other style classes.
	/// </summary>
	public abstract class StyleBase
	{
		public static readonly string Automatic = "automatic";
		public static readonly string Transparent = "Transparent";
		public static readonly string DefaultCodeFamily = "Lucida Console";
		public static readonly double DefaultCodeSize = 10.0;

		private const string EditingKey = @"Software\Microsoft\Office\16.0\OneNote\Options\Editing";

		protected string color;
		protected string highlight;
		protected double fontSize;
		protected double spaceBefore;
		protected double spaceAfter;
		protected double spacing;

		protected readonly ILogger logger;


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell",
			"S3963:\"static\" fields should be initialized inline",
			Justification = "<Pending>")]
		static StyleBase()
		{
			DefaultFontFamily = "Calibri";
			DefaultFontSize = 11.0;

			// fetch default font attributes from Registyr
			var key = Registry.CurrentUser.OpenSubKey(EditingKey, false);
			if (key is not null)
			{
				if (key.GetValue("DefaultFontFace") is string family &&
					!string.IsNullOrWhiteSpace(family))
				{
					DefaultFontFamily = family;
				}

				if (key.GetValue("DefaultFontSize") is string size &&
					double.TryParse(size,
						NumberStyles.AllowDecimalPoint,
						CultureInfo.InvariantCulture, out var result))
				{
					DefaultFontSize = result;
				}
			}
		}


		/// <summary>
		/// Initializes a new instance with defaults.
		/// </summary>
		protected StyleBase(bool setDefaults = true)
		{
			if (setDefaults)
			{
				Color = Automatic;
				Highlight = Automatic;
				FontFamily = DefaultFontFamily;
				fontSize = DefaultFontSize;
				ApplyColors = true;
			}

			logger = Logger.Current;
		}


		/// <summary>
		/// Base copy constructor, to be invoked by inheritor's copy constructors.
		/// </summary>
		/// <param name="other">
		/// Inheritors may extend their own constructor to manage additional properties
		/// as appropriate.
		/// </param>
		protected StyleBase(StyleBase other)
			: this()
		{
			Name = other.Name;
			StyleType = other.StyleType;
			Index = other.Index;
			Color = other.Color;
			Highlight = other.Highlight;
			FontFamily = other.FontFamily;
			fontSize = other.fontSize;
			IsBold = other.IsBold;
			IsItalic = other.IsItalic;
			IsUnderline = other.IsUnderline;
			IsStrikethrough = other.IsStrikethrough;
			IsSuperscript = other.IsSuperscript;
			IsSubscript = other.IsSubscript;
			spaceBefore = other.spaceBefore;
			spaceAfter = other.spaceAfter;
			spacing = other.spacing;
			Ignored = other.Ignored;

			ApplyColors = other.ApplyColors;
		}


		public static string DefaultFontFamily { get; private set; }

		public static double DefaultFontSize { get; private set; }


		/// <summary>
		/// Gets or sets the name of the style, mainly for user-defined styles.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the type of this style and the scope to which it should be applied.
		/// </summary>
		public StyleType StyleType { get; set; }

		/// <summary>
		/// Gets or sets an index that can be used as a QuickStyleDef index or an ordering
		/// of custom styles.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// Gets or sets the font foreground color.
		/// </summary>
		public string Color
		{
			get { return color; }

			set
			{
				if (value.Equals(Automatic))
				{
					color = value;
				}
				else
				{
					// normalize as #RGB to avoid case-sensitive comparison problems
					color = ColorTranslator.FromHtml(value).ToRGBHtml();
				}
			}
		}

		/// <summary>
		/// Gets or sets the font background color.
		/// </summary>
		public string Highlight
		{
			get { return highlight; }

			set
			{
				if (value.Equals(Automatic) || value.Equals(Transparent))
				{
					highlight = value;
				}
				else
				{
					// normalize as #RGB to avoid case-sensitive comparison problems
					highlight = ColorTranslator.FromHtml(value).ToRGBHtml();
				}
			}
		}

		/// <summary>
		/// Gets or sets the font family name; can be a comma-separated list of names.
		/// </summary>
		public string FontFamily { get; set; }

		/// <summary>
		/// Gets or sets the font size as a string.
		/// </summary>
		public virtual string FontSize
		{
			get { return fontSize.ToString("0.0#", CultureInfo.InvariantCulture); }
			set { fontSize = Convert.ToDouble(value, CultureInfo.InvariantCulture); }
		}

		/// <summary>
		/// Gets or sets a Boolean indicating whether spelling should be ignored in affect text
		/// </summary>
		public bool Ignored { get; set; }

		/// <summary>
		/// Gets or sets a Boolean indicating whether the font style is bold.
		/// </summary>
		public bool IsBold { get; set; }

		/// <summary>
		/// Gets or sets a Boolean indicating whether the font style is italicized.
		/// </summary>
		public bool IsItalic { get; set; }

		/// <summary>
		/// Gets or sets a Boolean indicating whether the font style is underlined.
		/// </summary>
		public bool IsUnderline { get; set; }

		/// <summary>
		/// Gets or sets a Boolean indicating whether the font style is strikethrough.
		/// </summary>
		public bool IsStrikethrough { get; set; }

		/// <summary>
		/// Gets or sets a Boolean indicating whether the font style is superscript.
		/// </summary>
		public bool IsSuperscript { get; set; }

		/// <summary>
		/// Gets or sets a Boolean indicating whether the font style is subscript.
		/// </summary>
		public bool IsSubscript { get; set; }

		/// <summary>
		/// Gets or sets the space added before the paragraph.
		/// </summary>
		public virtual string SpaceBefore
		{
			get { return spaceBefore.ToString("0.0#", CultureInfo.InvariantCulture); }
			set { spaceBefore = Convert.ToDouble(value, CultureInfo.InvariantCulture); }
		}

		/// <summary>
		/// Gets or sets the space added after the paragraph.
		/// </summary>
		public virtual string SpaceAfter
		{
			get { return spaceAfter.ToString("0.0#", CultureInfo.InvariantCulture); }
			set { spaceAfter = Convert.ToDouble(value, CultureInfo.InvariantCulture); }
		}

		/// <summary>
		/// Gets or sets the line spacing between lines of a paragraph.
		/// </summary>
		public virtual string Spacing
		{
			get { return spacing.ToString("0.0#", CultureInfo.InvariantCulture); }
			set { spacing = Convert.ToDouble(value, CultureInfo.InvariantCulture); }
		}


		//----------------------------------------------------------------------------------------
		// extended

		/// <summary>
		/// Gets or sets a Boolean indicating whether colors should be applied along
		/// with the rest of the style.
		/// </summary>
		/// <remarks>
		/// Stored by StyleRecord and used by Style but ignored by QuickStyleDef
		/// </remarks>
		public bool ApplyColors { get; set; }


		/// <summary>
		/// Compares this style with the given style
		/// </summary>
		/// <param name="obj">A StyleBase or a type that derives from StyleBase</param>
		/// <returns>true if the objects match; false otherwise</returns>
		public override bool Equals(object obj)
		{
			if (obj is not StyleBase style)
			{
				return false;
			}

			// does not check StyleType and ApplyColors so that it only checks the
			// visual effects of the style, not its behavior

			return
				//StyleType == style.StyleType &&
				// font-family value may be inconsistent when pasting HTML
				FontFamily.ToLower() == style.FontFamily.ToLower() &&
				FontSize == style.FontSize &&
				//ApplyColors == style.ApplyColors &&
				Color == style.Color &&
				Highlight == style.Highlight &&
				IsBold == style.IsBold &&
				IsItalic == style.IsItalic &&
				IsUnderline == style.IsUnderline &&
				IsStrikethrough == style.IsStrikethrough &&
				IsSubscript == style.IsSubscript &&
				IsSuperscript == style.IsSuperscript &&
				SpaceBefore == style.SpaceBefore &&
				SpaceAfter == style.SpaceAfter &&
				Spacing == style.Spacing &&
				Ignored == style.Ignored;
		}


		public override int GetHashCode()
		{
			return new { Name, StyleType, Index, FontFamily, FontSize }.GetHashCode();
		}
	}
}