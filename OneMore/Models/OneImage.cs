﻿//************************************************************************************************
// Copyright © 2024 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a one:Image element.
	/// </summary>
	internal class OneImage
	{
		private readonly XElement root;
		private readonly XElement size;
		private readonly XNamespace ns;


		/// <summary>
		/// Clone the given element
		/// </summary>
		/// <param name="root"></param>
		public OneImage(XElement root)
		{
			this.root = root;
			ns = root.GetNamespaceOfPrefix(OneNote.Prefix);

			size = root.Element(ns + "Size");
		}


		public static implicit operator XElement(OneImage e) => e.root;
		public static explicit operator OneImage(XElement e) => new(e);


		public string Data
		{
			get => root.Element(ns + "Data").Value;
			set => root.Element(ns + "Data").Value = value;
		}


		public int Height => (int)decimal
			.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);


		public bool IsSetByUser
		{
			get => size.Attribute("isSetByUser")?.Value == "true";
			set => size.SetAttributeValue("isSetByUser", value.ToString().ToLowerInvariant());
		}


		public int Width => (int)decimal
			.Parse(size.Attribute("width").Value, CultureInfo.InvariantCulture);


		public Image ReadImage()
		{
			var data = Convert.FromBase64String(root.Element(ns + "Data").Value);
			using var stream = new MemoryStream(data, 0, data.Length);
			return Image.FromStream(stream);
		}


		public void SetAutoSize()
		{
			size.Attribute("isSetByUser")?.Remove();
			root.Parent.Attribute("objectID")?.Remove();
		}


		public void SetSize(int width, int height)
		{
			size.SetAttributeValue("width", width.ToInvariantString());
			size.SetAttributeValue("height", height.ToInvariantString());
			size.SetAttributeValue("isSetByUser", "true");
		}


		public void WriteImage(Image image)
		{
			var bytes = (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));
			var data = root.Element(ns + "Data");
			data.Value = Convert.ToBase64String(bytes);
		}


		public override string ToString() => root.ToString();


		public string ToString(SaveOptions options) => root.ToString(options);
	}
}
