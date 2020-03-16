//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	/*
	 * <![CDATA[One<span style='font-weight:bold'>two</span> thre]]>
	 */

	/// <summary>
	/// Manages styles across a ranage of content.
	/// </summary>
	/// <remarks>
	/// OneNote cdata elements never seem to contain anything other than text
	/// and text wrapped in simple spans with style attributes (no other attributes).
	/// So presume this is always the case when managing cdata content.
	/// </remarks>

	internal class Stylizer
	{

		private XElement container;


		/// <summary>
		/// Initializes a new instance that manages a CDATA nodes with zero or more
		/// text and spans.
		/// </summary>
		/// <param name="cdata"></param>
		/// <remarks>
		/// Cannot apply style to an empty CDATA because OneNote will
		/// strip the styling off, so instead need to apply to parent one:T which
		/// is why this override is referencing its Parent.
		/// </remarks>
		public Stylizer(XCData cdata) : this(cdata.Parent)
		{
		}


		/// <summary>
		/// Initializes a new instance that manages an element with zero or more CDATA nodes,
		/// each of which may contain zero or more text and spans.
		/// </summary>
		/// <param name="element">
		/// A one:T element
		/// </param>
		public Stylizer(XElement element)
		{
			if (element.Name.LocalName != "T")
			{
				throw new System.Exception("Stylizer argument must be a one:T element");
			}

			container = element;
		}


		/// <summary>
		/// </summary>
		/// <param name="style"></param>
		public void ApplyStyle(CustomStyle style)
		{
			XElement parent = null;

			foreach (var cdata in
				container.DescendantNodes().Where(e => e.NodeType == XmlNodeType.CDATA).Cast<XCData>())
			{
				var wrapper = ConvertCDataToXml(cdata.Value);

				foreach (var child in wrapper.Nodes())
				{
					if (child.NodeType == XmlNodeType.Text)
					{
					}
					else if (child.NodeType == XmlNodeType.Element)
					{
					}
				}



				if (parent != cdata.Parent)
				{
					parent = cdata.Parent;

					// TODO: apply styles to parent...
				}
			}
		}


		/// <summary>
		/// Creates an XElement from the given cdata value.
		/// </summary>
		/// <param name="ctext"></param>
		/// <returns></returns>
		/// <remarks>
		/// Since CData only has a Value, we need a wrapper to convert it to proper XML
		/// that we can then parse and modify.
		/// </remarks>
		private XElement ConvertCDataToXml(string ctext)
		{
			// ensure proper XML

			// OneNote doesn't like &nbsp; but &#160; is ok and is the same as \u00A0 but 1-byte
			var value = ctext.Replace("&nbsp;", "&#160;");

			// XElement doesn't like <br> so replace with <br/>
			value = Regex.Replace(value, @"\<\s*br\s*\>", "<br/>");

			// quote unquote language attribute, e.g., lang=yo to lang="yo" (or two part en-US)
			value = Regex.Replace(value, @"(\s)lang=([\w\-]+)([\s/>])", "$1lang=\"$2\"$3");

			return XElement.Parse("<cdata>" + value + "</cdata>");
		}
	}
}
