//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	internal abstract class StyleInfoBase : IStyleInfo
	{
		public string Name { get; set; }
		public string Index { get; set; }

		public string FontFamily { get; set; }
		public virtual string FontSize { get; set; }
		public bool? IsBold { get; set; }
		public bool? IsItalic { get; set; }
		public bool? IsUnderline { get; set; }
		public bool? IsStrikethrough { get; set; }
		public bool? IsSuperscript { get; set; }
		public bool? IsSubscript { get; set; }
		public string Color { get; set; }
		public string Highlight { get; set; }
		public string SpaceBefore { get; set; }
		public string SpaceAfter { get; set; }
		public bool? IsHeading { get; set; }

		public int Level { get; set; }


		public void Collect (IStyleInfo other)
		{
			if (Name == null) Name = other.Name;
			if (Index == null) Index = other.Index;
			if (FontFamily == null) FontFamily = other.FontFamily;
			if (FontSize == null) FontSize = other.FontSize;
			if (IsBold == null) IsBold = other.IsBold;
			if (IsItalic == null) IsItalic = other.IsItalic;
			if (IsUnderline == null) IsUnderline = other.IsUnderline;
			if (IsStrikethrough == null) IsStrikethrough = other.IsStrikethrough;
			if (IsSubscript == null) IsSubscript = other.IsSubscript;
			if (IsSuperscript == null) IsSuperscript = other.IsSuperscript;
			if (Color == null) Color = other.Color;
			if (Highlight == null) Highlight = other.Highlight;
			if (SpaceAfter == null) SpaceAfter = other.SpaceAfter;
			if (SpaceBefore == null) SpaceBefore = other.SpaceBefore;
			if (IsHeading == null) IsHeading = other.IsHeading;
		}


		public bool Matches (object obj)
		{
			var other = obj as StyleInfoBase;
			if (other == null) return false;

			if (FontFamily != other.FontFamily) return false;
			if (FontSize != other.FontSize) return false;
			if (IsBold != other.IsBold) return false;
			if (IsItalic != other.IsItalic) return false;
			if (IsUnderline != other.IsUnderline) return false;
			if (Color != other.Color) return false;
			if (Highlight != other.Highlight) return false;
			if (SpaceAfter != other.SpaceAfter) return false;
			if (SpaceBefore != other.SpaceBefore) return false;
			if (IsStrikethrough != other.IsStrikethrough) return false;
			if (IsSubscript != other.IsSubscript) return false;
			if (IsSuperscript != other.IsSuperscript) return false;

			return true;
		}


		protected string FormatColor (string color)
		{
			if ((color == null) || (color == "automatic"))
			{
				return null;
			}

			if (color.Length == 9)
			{
				return color;
			}

			if (color[0] == '#')
			{
				color = color.Substring(1);
			}

			while (color.Length < 8)
			{
				color = "f" + color;
			}

			return "#" + color.ToLower();
		}


		protected string FormatFontFamily (string family)
		{
			if (family == null) return null;
			return family.Replace("'", string.Empty);
		}


		protected string FormatSize (string size)
		{
			if (size == null)
			{
				return null;
			}

			if ((size.IndexOf('.') > 0) && size.EndsWith("pt"))
			{
				return size;
			}

			if (size.EndsWith("pt"))
			{
				size = size.Substring(0, size.Length - 2);
			}

			if (double.TryParse(size, out var points))
			{
				size = points.ToString("#.0") + "pt";
			}

			return size;
		}


		protected string FormatSpace (string space)
		{
			if (space == null)
			{
				return null;
			}

			var i = space.IndexOf('.');
			if (i < 0)
			{
				return int.Parse("0" + space).ToString();
			}

			if (double.TryParse(space, out var s))
			{
				return ((int)s).ToString();
			}

			return space;
		}
	}
}
