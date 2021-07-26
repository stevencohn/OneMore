//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Discovers the style of a contextual element with respect to its hierarchy
	/// by walking up the hierarchy from T, to OE, to page.QuickStyle.
	/// Or discover local style properties of a span.
	/// </summary>
	internal class StyleAnalyzer
	{
		private readonly bool inward;
		private readonly XElement page;
		private readonly Dictionary<string, string> properties;


		public StyleAnalyzer(XElement page, bool inward = true)
		{
			this.inward = inward;
			this.page = page;
			properties = new Dictionary<string, string>();
		}


		/// <summary>
		/// Clear the collected properties to ready the analyzer for the next element
		/// </summary>
		public void Clear()
		{
			properties.Clear();
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


		private void CollectElementStyleProperties (XElement element)
		{
			var props = element.CollectStyleProperties(inward);

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
		private void CollectQuickStyleProperties (XElement element)
		{
			var index = element.Attribute("quickStyleIndex")?.Value;
			if (index != null)
			{
				var ns = page.GetNamespaceOfPrefix(OneNote.Prefix);

				var quick = page.Elements(ns + "QuickStyleDef")
					.FirstOrDefault(e => e.Attribute("index").Value.Equals(index));

				if (quick != null)
				{
					QuickStyleDef.CollectStyleProperties(quick, properties);
				}
			}
		}
	}
}
