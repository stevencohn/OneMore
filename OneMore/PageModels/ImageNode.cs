//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>Wraps a one:Image element.</summary>
	internal sealed class ImageNode : OneNoteNode
	{
		internal ImageNode(XElement el) : base(el) { }


		/// <summary>Creates a new image element from raw base64-encoded bytes.</summary>
		public static ImageNode Create(string base64Data, string format, double widthPx, double heightPx)
		{
			var image = E("Image",
				new XAttribute("format", format),
				E("Size",
					new XAttribute("width", widthPx.ToString("F2", CultureInfo.InvariantCulture)),
					new XAttribute("height", heightPx.ToString("F2", CultureInfo.InvariantCulture))),
				E("Data", base64Data));
			return new ImageNode(image);
		}


		/// <summary>Image format string, e.g. "png", "jpg".</summary>
		public string Format
		{
			get => Attr("format");
			set => Attr("format", value);
		}


		/// <summary>Base64-encoded image data.</summary>
		public string Data
		{
			get => el.Element(NS + "Data")?.Value;
			set
			{
				var data = el.Element(NS + "Data");
				if (data is null)
					el.Add(E("Data", value));
				else
					data.Value = value ?? string.Empty;
			}
		}


		public double Width
		{
			get => GetSize("width");
			set => SetSize("width", value);
		}


		public double Height
		{
			get => GetSize("height");
			set => SetSize("height", value);
		}


		public string HyperlinkUrl
		{
			get => Attr("hyperlink");
			set => Attr("hyperlink", value);
		}


		public string AltText
		{
			get => el.Element(NS + "AltText")?.Value
				?? Attr("alt");
			set
			{
				var altEl = el.Element(NS + "AltText");
				if (value is null)
				{
					altEl?.Remove();
					Attr("alt", null);
				}
				else if (altEl is not null)
				{
					altEl.Value = value;
				}
				else
				{
					el.Add(E("AltText", value));
				}
			}
		}


		private double GetSize(string dimension)
		{
			var size = el.Element(NS + "Size");
			if (size is null) return 0;
			return double.TryParse(
				size.Attribute(dimension)?.Value,
				NumberStyles.Any,
				CultureInfo.InvariantCulture,
				out var d) ? d : 0;
		}


		private void SetSize(string dimension, double value)
		{
			var size = el.Element(NS + "Size");
			if (size is null)
			{
				size = E("Size");
				el.Add(size);
			}
			size.SetAttributeValue(dimension, value.ToString("F2", CultureInfo.InvariantCulture));
		}
	}
}
