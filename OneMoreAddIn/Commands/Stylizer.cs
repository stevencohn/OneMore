//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;


	/*
	 * <one:T><![CDATA[One<span style='font-weight:bold'>two</span> thre]]></one:T>
	 */

	/// <summary>
	/// Manages styles across a range of content.
	/// </summary>
	/// <remarks>
	/// OneNote cdata elements never seem to contain anything other than text
	/// and text wrapped in simple spans with style attributes (no other attributes).
	/// So presume this is always the case when managing cdata content.
	/// </remarks>

	internal class Stylizer
	{

		private readonly CssInfo info;
		private readonly string css;


		/// <summary>
		/// Initializes a new instance with the given style info.
		/// </summary>
		/// <param name="info">Style information</param>
		public Stylizer(CssInfo info)
		{
			this.info = info;

			// generate css string here to optimize usage
			css = info.ToCss();
		}


		/// <summary>
		/// Applies styles to the given CDATA node with one or more text and spans.
		/// </summary>
		/// <param name="cdata">The CDATA node</param>
		/// <remarks>
		/// Cannot apply style to an empty CDATA because OneNote will strip the styling off,
		/// so instead apply to the node's parent one:T
		/// </remarks>
		public void ApplyStyle(XCData cdata)
		{
			ApplyStyle(cdata.Parent);
		}


		/// <summary>
		/// Apply the given CSS to the content
		/// </summary>
		/// <param name="info">Style info (instantiated from CustomStyle)</param>
		public void ApplyStyle(XElement container)
		{
			XElement parent = null;

			// find all CData
			foreach (var cdata in container.DescendantNodes()
				.Where(e => e.NodeType == XmlNodeType.CDATA).Cast<XCData>())
			{
				// wrap the CData as an XElement so we can parse it
				var wrapper = cdata.GetWrapper();

				// true if there are at least one Text nodes
				var hasPlainText = false;

				foreach (var child in wrapper.Nodes())
				{
					if (child.NodeType == XmlNodeType.Text)
					{
						// will apply style to parent one:T when at least one Text node exists
						hasPlainText = true;
					}
					else if (child.NodeType == XmlNodeType.Element)
					{
						// focus on spans, skipping br and other elements
						if ((child as XElement).Name.LocalName == "span")
						{
							// blast styles into span
							Apply(child as XElement);
						}
					}
				}

				// unwrap our temporary <cdata>
				cdata.Value = wrapper.GetInnerXml();

				// capture the parent element and if we found Text nodes then set global style
				if (parent != cdata.Parent)
				{
					parent = cdata.Parent;

					if (hasPlainText)
					{
						Apply(parent);
					}
				}
			}
		}


		/// <summary>
		/// Apply info styles to given element, merging with existing properties
		/// </summary>
		/// <param name="info">The css styles to apply</param>
		/// <param name="element">The element to which the styles are applied</param>
		private void Apply(XElement element)
		{
			var span = element.Attribute("style");
			if (span == null)
			{
				// give element new style
				element.Add(new XAttribute("style", css));
			}
			else
			{
				// merge info style into element's style
				var css = new CssInfo(element);
				css.Merge(info);
				span.Value = css.ToCss();
			}
		}
	}
}
