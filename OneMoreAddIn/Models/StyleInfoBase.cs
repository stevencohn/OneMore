//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

using System;

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

		/// <summary>
		/// Used by InsertTocCommand to keep track of TOC indent level...
		/// There must be a better way, right?
		/// </summary>
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

			if (!CompareColors(Color, other.Color)) return false;
			if (!CompareColors(Highlight, other.Highlight)) return false;

			if (!CompareNullables(SpaceAfter, other.SpaceAfter, "0")) return false;
			if (!CompareNullables(SpaceBefore, other.SpaceBefore, "0")) return false;
			if (!CompareNullables(IsStrikethrough, other.IsStrikethrough, false)) return false;
			if (!CompareNullables(IsSubscript, other.IsSubscript, false)) return false;
			if (!CompareNullables(IsSuperscript, other.IsSuperscript, false)) return false;

			return true;
		}


		private bool CompareColors (string c1, string c2)
		{
			if (!string.IsNullOrEmpty(c1) && !string.IsNullOrEmpty(c2))
			{
				if (c1[0] == '#') c1 = c1.Substring(1);
				if (c2[0] == '#') c2 = c2.Substring(1);

				while (c1.Length < 6) c1 = '0' + c1;
				while (c2.Length < 6) c2 = '0' + c2;

				if (c1.Length - c2.Length != 0)
				{
					if (c1.Length > 6) c1 = c1.Substring(c1.Length - 6);
					if (c2.Length > 6) c2 = c2.Substring(c2.Length - 6);
				}
			}

			return c1 == c2;
		}


		private bool CompareNullables (string v1, string v2, string defaultv)
		{
			if (v1 == v2) return true;
			if ((v1 == null) && (v2 == defaultv)) return true;
			if ((v2 == null) && (v1 == defaultv)) return true;
			return false;
		}


		private bool CompareNullables (bool? v1, bool? v2, bool defaultv)
		{
			if (v1 == v2) return true;
			if ((v1 == null) && (v2 == defaultv)) return true;
			if ((v2 == null) && (v1 == defaultv)) return true;
			return false;
		}


		protected string FormatColor (string color)
		{
			if (color == null)
			{
				return null;
			}

			// look for "automatic" or "yellow"...
			if (char.IsLetter(color[0]))
			{
				return color;
			}

			// if #rrggbb
			if (color.Length == 7)
			{
				return color;
			}

			if (color[0] == '#')
			{
				color = color.Substring(1);
			}

			while (color.Length < 6)
			{
				color = "0" + color;
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
