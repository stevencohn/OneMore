//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	/// <summary>
	/// Represents the Outline element of a OneNote page
	/// </summary>
	/// <remarks>
	/// OneMore page models are typically used in conjunction with the PageNamespace class.
	/// However, there are times when multiple pages and multiple namespaces may be needed
	/// so model classes also provide override constructors that accept explicit namespaces.
	/// </remarks>
	internal class Outline : XElement
	{
		private readonly XNamespace ns;


		/// <summary>
		/// Instantiates a new empty outline with the predefined namespace
		/// </summary>
		/// <remarks>
		/// PageNamespace.Value must be set prior to using this constructor
		/// </remarks>
		public Outline()
			: this(PageNamespace.Value)
		{
		}


		/// <summary>
		/// Initializes a new empty outline with the given namespace
		/// </summary>
		/// <param name="ns">A namespace</param>
		public Outline(XNamespace ns)
			: base(ns + "Outline")
		{
			this.ns = ns;
		}


		/// <summary>
		/// Initialize a new outline, adding the given content
		/// </summary>
		/// <param name="content"></param>
		/// <remarks>
		/// PageNamespace.Value must be set prior to using this constructor
		/// </remarks>
		public Outline(XElement content)
			: this(PageNamespace.Value, content)
		{
		}


		/// <summary>
		/// Initialize a new outline, adding the given content
		/// </summary>
		/// <param name="ns">A namespace</param>
		/// <param name="content">Content to add to the outline</param>
		public Outline(XNamespace ns, XElement content)
			: this(ns)
		{
			if (content.Name.LocalName == "Outline")
			{
				if (content.HasElements)
				{
					Add(content.Elements());
				}
			}
			else
			{
				Add(content);
			}
		}


		/// <summary>
		/// Get the width of the outline
		/// </summary>
		/// <returns>An integer approximated the width</returns>
		public int GetWidth()
		{
			var size = Element(ns + "Size");
			if (size != null)
			{
				if (size.GetAttributeValue("width", out decimal width))
				{
					return (int)width;
				}
			}

			return 0;
		}


		/// <summary>
		/// Sets the size of the outline
		/// </summary>
		/// <param name="width">The width</param>
		/// <param name="height">The height, optional</param>
		public void SetSize(int width, int height = 0)
		{
			var size = Element(ns + "Size");
			if (size == null)
			{
				size = new XElement(ns + "Size", new XAttribute("width", $"{width}.0"));

				if (height > 0)
				{
					size.Add(new XAttribute("height", $"{height}.0"));
				}

				AddFirst(size);
			}
			else
			{
				size.SetAttributeValue("width", $"{width}.0");

				if (height > 0)
				{
					size.SetAttributeValue("height", $"{height}.0");
				}
			}
		}
	}
}
