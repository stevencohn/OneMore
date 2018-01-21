//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Text;
	using System.Xml.Linq;


	internal class CustomStyle : IDisposable
	{
		public CustomStyle (string name, Font font,
			Color color, Color background,
			int spaceBefore = 0, int spaceAfter = 0,
			bool isHeading = false)
		{
			this.Name = name;
			this.Font = font;
			this.Color = color;
			this.Background = background;
			this.SpaceAfter = spaceAfter;
			this.SpaceBefore = spaceBefore;
			this.IsHeading = isHeading;
		}

		public string Name { get; set; }

		public Font Font { get; set; }

		public Color Color { get; set; }

		public Color Background { get; set; }

		public int SpaceBefore { get; set; }

		public int SpaceAfter { get; set; }

		public bool IsHeading { get; set; }


		private bool disposedValue = false;
		protected virtual void Dispose (bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Font?.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose ()
		{
			Dispose(true);
		}


		public override string ToString ()
		{
			return Name;
		}


		/// <summary>
		/// Build a CSS style string suitable for adding to a SPAN element in Page content
		/// </summary>
		/// <returns></returns>

		public string ToCss (bool extended = false)
		{
			var builder = new StringBuilder();

			if (extended)
			{
				builder.Append($"font-family:{Font.FontFamily.Name};");
				builder.Append($"font-size:{(int)Font.Size}pt;");
			}

			if ((Font.Style & FontStyle.Bold) > 0) builder.Append("font-weight:bold;");
			if ((Font.Style & FontStyle.Italic) > 0) builder.Append("font-style:italic;");
			if ((Font.Style & FontStyle.Underline) > 0) builder.Append("text-decoration:underline;");

			if (!Color.IsEmpty && !Color.Equals(Color.Transparent))
			{
				var hex = Color.ToArgb().ToString("X6");
				if (hex.Length > 6) hex = hex.Substring(hex.Length - 6);
				builder.Append($"color:#{hex};");
			}

			if (!Background.IsEmpty && !Background.Equals(Color.Transparent) && !Background.Equals(Color))
			{
				var hex = Background.ToArgb().ToString("X6");
				if (hex.Length > 6) hex = hex.Substring(hex.Length - 6);
				builder.Append($"background:#{hex};");
			}

			return builder.ToString();
		}


		public bool Matches (object obj)
		{
			var other = obj as CustomStyle;
			if (other == null)
			{
				return false;
			}

			if (!Name.Equals(other.Name)) return false;
			if (!Font.Equals(other.Font)) return false;
			if (!Color.Equals(other.Color)) return false;
			if (!Background.Equals(other.Background)) return false;
			if (!SpaceBefore.Equals(other.SpaceBefore)) return false;
			if (!SpaceAfter.Equals(other.SpaceAfter)) return false;

			return true;
		}
	}
}
 