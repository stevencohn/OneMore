//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System.Drawing;
	using System.Linq;
	using System.Text;
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
		public enum Clearing
		{
			None,       // don't remove color styling
			All,        // remove all color styling
			Gray        // only gray color styling
		}

		private readonly Style style;
		private readonly string css;


		/// <summary>
		/// Initializes a new instance with the given style info.
		/// </summary>
		/// <param name="style">Style information</param>
		public Stylizer(Style style)
		{
			this.style = style;

			// generate css string here to optimize usage
			css = style.ToCss();
		}


		/// <summary>
		/// Applies styles to the given CDATA (and its parent OE) node with
		/// one or more text and spans.
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
		/// Apply the given CSS to the content, presuming the container is a one:T element.
		/// </summary>
		/// <param name="info">Style info (instantiated from CustomStyle)</param>
		public void ApplyStyle(XElement container)
		{
			XElement parent = null;

			// find all CData (should only be one but...)
			foreach (var cdata in container.DescendantNodes()
				.Where(e => e.NodeType == XmlNodeType.CDATA).Cast<XCData>())
			{
				bool hasPlainText;

				if (cdata.Value.Length > 0)
				{
					// wrap the CData as an XElement so we can parse it
					var wrapper = cdata.GetWrapper();

					// true if there are at least one Text nodes
					hasPlainText = false;

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
				}
				else
				{
					// on an empty one:T/CData so blast styles into one:T
					hasPlainText = true;
				}

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
				var given = element.Attribute("style")?.Value;
				if (given != null)
				{
					// merge style into element's style
					var estyle = new Style(given);
					estyle.Merge(style);
					span.Value = estyle.ToCss();
				}
			}
		}


		/// <summary>
		/// Clear all styles from the given element and its descendant nodes
		/// </summary>
		/// <param name="element">An OE or T node</param>
		/// <param name="clearing">Exactly which color stylings to remove</param>
		public bool Clear(XElement element, Clearing clearing)
		{
			var cleared = false;
			var attr = element.Attribute("style");
			if (attr != null)
			{
				if (clearing == Clearing.All)
				{
					// discard all styling
					attr.Remove();
					cleared = true;
				}
				else if (clearing == Clearing.Gray)
				{
					var colorfulCss = ClearGrays(new Style(attr.Value));
					if (!string.IsNullOrEmpty(colorfulCss))
					{
						// found explicit colors
						attr.Value = colorfulCss;
					}
					else
					{
						// no explicit colors so discard everything else
						attr.Remove();
					}

					cleared = true;
				}
			}

			if (element.HasElements)
			{
				foreach (var child in element.Elements())
				{
					cleared |= Clear(child, clearing);
				}
			}

			// CData...

			var data = element.Nodes().OfType<XCData>()
				.Where(c => c.Value.Contains("span"));

			if (data == null || !data.Any())
			{
				return cleared;
			}

			foreach (var cdata in data)
			{
				var wrapper = cdata.GetWrapper();
				if (Clear(wrapper, clearing))
				{
					cdata.Value = wrapper.ToString(SaveOptions.DisableFormatting);
					cleared = true;
				}
			}

			return cleared;
		}


		private static string ClearGrays(Style style)
		{
			var builder = new StringBuilder();

			if (!string.IsNullOrEmpty(style.Color) && !style.Color.Equals("automatic"))
			{
				if (!ColorTranslator.FromHtml(style.Color).IsGray())
				{
					builder.Append("color:" + style.Color + ";");
				}
			}

			if (!string.IsNullOrEmpty(style.Highlight) && !style.Highlight.Equals("automatic"))
			{
				if (!ColorTranslator.FromHtml(style.Highlight).IsGray())
				{
					builder.Append("background:" + style.Highlight + ";");
				}
			}

			return builder.ToString();
		}
	}
}
