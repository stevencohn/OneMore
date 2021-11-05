//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


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
	internal class StyleAnalyzer
	{
		private class Catalog : Dictionary<string, StyleProperties> { }

		private readonly Catalog catalog;
		private readonly XElement root;
		private readonly XNamespace ns;


		/// <summary>
		/// Initialze a new analyzer for the given page.
		/// </summary>
		/// <param name="root">The root of the page</param>
		public StyleAnalyzer(XElement root)
		{
			this.root = root;
			ns = root.GetNamespaceOfPrefix(OneNote.Prefix);
			catalog = new Catalog();
		}


		/// <summary>
		/// Debugging/diagnostics, get the number of items in the catalog
		/// </summary>
		public int Depth => catalog.Count;


		/// <summary>
		/// Debugging/diagnostics, get the number of times the catalog had a hit
		/// </summary>
		public int Hits { get; private set; }


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
				Hits++;
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

			if (id != null && !catalog.ContainsKey(id))
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
				Hits++;
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
					Hits++;
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

			if (runs == null || !runs.Any())
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

			// ensure we also capture the quick style of the paragraph
			properties.Add(CollectFromParagraph(selection.Parent));

			var style = new Style(properties)
			{
				Name = $"Style-{new Random().Next(1000, 9999)}"
			};

			return style;
		}
	}
}
