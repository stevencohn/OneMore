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
	/// Simple typedef and extension of a Dictionary to manage style properties
	/// </summary>
	internal class StyleProperties : Dictionary<string, string>
	{

		// add without override
		/// <summary>
		/// Add given properties to current collection without overriding.
		/// Used within StyleAnalyzer to aggregate styles of nested elements on the page.
		/// </summary>
		/// <param name="other">Properties to add to this collection</param>
		public StyleProperties Add(Dictionary<string, string> other)
		{
			foreach (var key in other.Keys)
			{
				if (!ContainsKey(key))
				{
					Add(key, other[key]);
				}
			}

			return this;
		}
	}


	/// <summary>
	/// Discovers the resultant style of an element considering its own style properties,
	/// its CDATA embedded properties if the element is a T, its owner OE properties
	/// and any reference to the page QuickStyles.
	/// </summary>
	/// <remarks>
	/// One instance should be used to analyze a single page, one-to-one, since an analyzer will
	/// catalog and optimize each element and quick style so subsequent element cross-references
	/// will perform faster.
	/// </remarks>
	internal class StyleAnalyzer2
	{
		private class Catalog : Dictionary<string, StyleProperties> { }

		private readonly Catalog catalog;
		private readonly XElement root;
		private readonly XNamespace ns;


		/// <summary>
		/// Initialze a new analyzer for the given page.
		/// </summary>
		/// <param name="root">The root of the page</param>
		public StyleAnalyzer2(XElement root)
		{
			this.root = root;
			ns = root.GetNamespaceOfPrefix(OneNote.Prefix);
			catalog = new Catalog();
		}


		/// <summary>
		/// Collect the aggregated style properties of the given element within its context
		/// </summary>
		/// <param name="element">A page element, T or OE, or an embedd span from a CDATA wrapper</param>
		/// <param name="nested">True to look down the hierarchy as well as up</param>
		/// <returns>A StyleProperties collection</returns>
		public StyleProperties CollectFrom(XElement element, bool nested = false)
		{
			var id = element.Attribute("objectID")?.Value;
			if (id != null && catalog.ContainsKey(id))
			{
				return catalog[id];
			}

			var properties = new StyleProperties();

			if (element.Name.LocalName == "span")
			{
				// wrapped CDATA so no connected parent/ancestor to backtrack
				properties.Add(element.CollectStyleProperties(false));
			}
			else if (element.Name.LocalName == "T")
			{
				properties.Add(element.CollectStyleProperties(nested));
				properties.Add(CollectFromParagraph(element.Parent));
			}
			else if (element.Name.LocalName == "OE")
			{
				properties.Add(CollectFromParagraph(element));
			}

			if (id != null)
			{
				catalog.Add(id, properties);
			}

			return properties;
		}


		private StyleProperties CollectFromParagraph(XElement paragraph)
		{
			var id = paragraph.Attribute("objectID")?.Value;
			if (id != null && catalog.ContainsKey(id))
			{
				return catalog[id];
			}

			var properties = new StyleProperties
			{
				paragraph.CollectStyleProperties(nested: false),
				CollectFromQuickStyle(paragraph)
			};

			if (id != null)
			{
				catalog.Add(id, properties);
			}

			return properties;
		}


		private StyleProperties CollectFromQuickStyle(XElement element)
		{
			var index = element.Attribute("quickStyleIndex")?.Value;
			if (index != null)
			{
				var key = $"quick-{index}";
				if (catalog.ContainsKey(key))
				{
					return catalog[key];
				}

				var quick = root.Elements(ns + "QuickStyleDef")
					.FirstOrDefault(e => e.Attribute("index").Value.Equals(index));

				if (quick != null)
				{
					var props = new StyleProperties();
					QuickStyleDef.CollectStyleProperties(quick, props);
					catalog.Add(key, props);
					return props;
				}
			}

			return new StyleProperties();
		}


		/// <summary>
		/// Collect the aggregated style properties of the given element within its context
		/// </summary>
		/// <param name="element">A page element, T or OE, or an embedd span from a CDATA wrapper</param>
		/// <returns>A Style object representing the element's contextual style</returns>
		public Style CollectStyleFrom(XElement element)
		{
			return new Style(CollectFrom(element));
		}


		/// <summary>
		/// Infers the style of the selected text on a page. If more than one style is 
		/// included in the selected region then only the first style is returned
		/// </summary>
		public Style CollectFromSelection()
		{
			var runs = root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (runs == null)
			{
				// nothing selected
				return null;
			}

			var properties = new StyleProperties();
			var selection = runs.First();
			var cdata = selection.GetCData();
			if (!cdata.IsEmpty() || runs.Count() > 1)
			{
				// collect from first if one non-empty or more than one run is selected
				properties.Add(CollectFrom(selection, true));
			}
			else if (cdata.IsEmpty())
			{
				// is cursor adjacent to a previous non-empty run?
				if ((selection.PreviousNode is XElement prev) &&
					(prev.GetCData() is XCData pdata) && !pdata.EndsWithWhitespace())
				{
					// if last node is a SPAN then examine its style
					var wrapper = pdata.GetWrapper();
					if (wrapper.Nodes().Last() is XElement span && span.Attribute("style") != null)
					{
						properties.Add(CollectFrom(span));
					}
				}
				else if ((selection.NextNode is XElement next) &&
					(next.GetCData() is XCData ndata) && !ndata.StartsWithWhitespace())
				{
					// if first node is a SPAN then examine its style
					var wrapper = ndata.GetWrapper();
					if (wrapper.Nodes().First() is XElement span && span.Attribute("style") != null)
					{
						properties.Add(CollectFrom(span));
					}
				}
				else
				{
					// standalone empty run? still might have style
					properties.Add(CollectFrom(selection));
				}
			}

			var style = new Style(properties)
			{
				Name = $"Style-{new Random().Next(1000, 9999)}"
			};

			return style;
		}
	}


	internal class StyleAnalyzer
	{
		private bool nested;
		private readonly XElement root;
		private readonly Dictionary<string, string> properties;
		private readonly List<XElement> range;

		public StyleAnalyzer(XElement root, bool nested = true)
		{
			this.nested = nested;
			this.root = root;
			properties = new Dictionary<string, string>();
			range = new List<XElement>();
		}

		public IEnumerable<XElement> SelectionRange => range;

		public void Clear()
		{
			properties.Clear();
			range.Clear();
		}

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

		public void SetNested(bool nested)
		{
			this.nested = nested;
		}
	}
}
