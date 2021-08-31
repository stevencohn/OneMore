//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Discovers the style of a contextual element with respect to its hierarchy
	/// by walking up the hierarchy from T, to OE, to page.QuickStyle.
	/// Or discover local style properties of a span.
	/// </summary>
	internal class StyleAnalyzer
	{
		private readonly bool nested;
		private readonly XElement root;
		private readonly Dictionary<string, string> properties;
		private readonly List<XElement> range;


		/// <summary>
		/// Initialize a new analyzer specifically for the given page.
		/// </summary>
		/// <param name="root">The root of the page to analyze</param>
		/// <param name="nested">
		/// If true then collect properties from the embedded CDATA styles as well as the 
		/// style attribute of the T run itself
		/// </param>
		/// <remarks>
		/// Each analyzer will maintain its own collection of style properties. This is why the
		/// constructor accepts a Page, to isolate its analysis on a per-page basis. If more than
		/// one page needs to be analyzed at the same time, create an analyzer for each page.
		/// </remarks>
		public StyleAnalyzer(XElement root, bool nested = true)
		{
			this.nested = nested;
			this.root = root;
			properties = new Dictionary<string, string>();
			range = new List<XElement>();
		}


		/// <summary>
		/// Gets the elements that comprise the selection range from which styles were
		/// inferred using the CollectFromSelection method.
		/// </summary>
		public IEnumerable<XElement> SelectionRange => range;


		/// <summary>
		/// Clear the collected properties to ready the analyzer for the next element
		/// </summary>nwar
		public void Clear()
		{
			properties.Clear();
			range.Clear();
		}


		/// <summary>
		/// Builds a dictionary of style properties from the element, its parent,
		/// and any referenced quick style.
		/// </summary>
		/// <param name="element">A one:T, one:OE, or SPAN element</param>
		/// <returns>
		/// A Dictionary of property names and values.
		/// </returns>
		public Dictionary<string, string> CollectStyleProperties(XElement element)
		{
			// starting with T, OE, or SPAN
			CollectElementStyleProperties(element);

			if (element.Name.LocalName == "T")
			{
				// OE of T
				CollectElementStyleProperties(element.Parent);
				CollectQuickStyleProperties(element.Parent);
			}
			else if (element.Name.LocalName == "OE")
			{
				CollectQuickStyleProperties(element);
			}

			return properties;
		}


		private void CollectElementStyleProperties(XElement element)
		{
			var props = element.CollectStyleProperties(nested);

			if (props?.Any() == true)
			{
				var e = props.GetEnumerator();
				while (e.MoveNext())
				{
					if (!properties.ContainsKey(e.Current.Key))
					{
						properties.Add(e.Current.Key, e.Current.Value);
					}
				}
			}
		}


		// one:QuickStyleDef possibles:
		// fontColor="automatic" highlightColor="automatic" font="Calibri" fontSize="11.0" spaceBefore="0.0" spaceAfter="0.0" />
		private void CollectQuickStyleProperties(XElement element)
		{
			var index = element.Attribute("quickStyleIndex")?.Value;
			if (index != null)
			{
				var ns = root.GetNamespaceOfPrefix(OneNote.Prefix);

				var quick = root.Elements(ns + "QuickStyleDef")
					.FirstOrDefault(e => e.Attribute("index").Value.Equals(index));

				if (quick != null)
				{
					QuickStyleDef.CollectStyleProperties(quick, properties);
				}
			}
		}


		/*
<one:OE >
  <one:T><![CDATA[<span
    style='font-family:Calibri'>This is the third </span><span style='font-weight:
    bold;font-style:italic;text-decoration:underline line-through;font-family:Consolas;
    color:#70AD47'>li</span>]]></one:T>
  <one:T selected="all" style="font-family:Consolas;font-size:12.0pt;color:#70AD47"><![CDATA[]]></one:T>
  <one:T style="font-family:Consolas;font-size:12.0pt;color:#70AD47"><![CDATA[<span
    style='font-weight:bold;font-style:italic;text-decoration:underline line-through'>ne </span>]]></one:T>
</one:OE>
		 */
		/// <summary>
		/// Infer the style of the selected text on a page. If more than one style is 
		/// included in the selected region then only the first style is returned
		/// </summary>
		public Style CollectFromSelection()
		{
			var ns = root.GetNamespaceOfPrefix(OneNote.Prefix);

			var selection = root.Descendants(ns + "T")
				.FirstOrDefault(e => e.Attributes("selected").Any(a => a.Value == "all"));

			if (selection == null)
			{
				// nothing selected
				return null;
			}

			var cdata = selection.GetCData();
			if (cdata.IsEmpty())
			{
				// inside a word, adjacent to a word, or somewhere in whitespace?

				if ((selection.PreviousNode is XElement prev) && !prev.GetCData().EndsWithWhitespace())
				{
					selection = prev;

					if ((selection.DescendantNodes()?
						.OfType<XCData>()
						.LastOrDefault() is XCData data) && !string.IsNullOrEmpty(data?.Value))
					{
						var wrapper = data.GetWrapper();

						// if last node is text then skip the cdata and examine the parent T
						// otherwise if last node is a span then start with its style

						var last = wrapper.Nodes().Reverse().First(n =>
							n.NodeType == XmlNodeType.Text ||
							((n is XElement ne) && ne.Name.LocalName == "span"));

						if (last?.NodeType == XmlNodeType.Element)
						{
							var wspan = last as XElement;
							if (wspan.Attribute("style") != null)
							{
								CollectStyleProperties(wspan);
							}
						}
					}
				}
				else
				{
					if ((selection.NextNode is XElement next) && !next.GetCData().StartsWithWhitespace())
					{
						selection = next;

						if ((selection.DescendantNodes()?
							.OfType<XCData>()
							.FirstOrDefault() is XCData data) && !string.IsNullOrEmpty(data?.Value))
						{
							var wrapper = data.GetWrapper();

							// if first node is text then skip the cdata and examine the parent T
							// otherwise if first node is a span then start with its style

							var last = wrapper.Nodes().First(n =>
								n.NodeType == XmlNodeType.Text ||
								((n is XElement ne) && ne.Name.LocalName == "span"));

							if (last?.NodeType == XmlNodeType.Element)
							{
								var wspan = last as XElement;
								if (wspan.Attribute("style") != null)
								{
									CollectStyleProperties(wspan);
								}
							}
						}
					}
				}
			}

			CollectStyleProperties(selection);

			range.Add(selection);

			var style = new Style(properties)
			{
				Name = $"Style-{new Random().Next(1000, 9999)}"
			};

			return style;
		}
	}
}
