//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	internal static class XElementExtensions
	{

		/// <summary>
		/// Extract the style properties of this element by looking at its "style" attribute
		/// if one exists and possibly its immediate CDATA child and any span/style attributes.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static Dictionary<string, string> CollectStyleProperties(
			this XElement element, bool inward = true)
		{
			var props = new Dictionary<string, string>();

			if (inward)
			{
				// gather from CDATA child if one exists
				var cdata = element.Nodes().OfType<XCData>().FirstOrDefault();
				if (cdata != null)
				{
					var span = cdata.GetWrapper().Elements("span")
						.FirstOrDefault(e => e.Attributes("style").Any());

					if (span != null)
					{
						var sprops = span.CollectStyleProperties();
						sprops.ToList().ForEach(c => props.Add(c.Key, c.Value));
					}

					/*
					var wrapper = cdata.GetWrapper();

					// collect inner span@style properties only if CDATA is entirely wrapped
					// by a span element with style attributes

					if (wrapper.Nodes().Count() == 1 &&
						wrapper.FirstNode.NodeType == XmlNodeType.Element &&
						((XElement)wrapper.FirstNode).Name.LocalName == "span")
					{
						var span = (XElement)wrapper.FirstNode;
						if (span.Attributes("style").Any())
						{
							var sprops = span.CollectStyleProperties();
							sprops.ToList().ForEach(c => props.Add(c.Key, c.Value));
						}
					}
					*/
				}
			}

			// gather from style attribute if one exists
			var sheet = element.Attributes("style").Select(a => a.Value);

			if ((sheet == null) || !sheet.Any()) return props;

			foreach (var css in sheet.ToList())
			{
				var parts = css.Split(';');
				if (parts.Length == 0) continue;

				foreach (var part in parts)
				{
					var pair = part.Split(':');
					if (pair.Length < 2) continue;

					var key = pair[0].Trim();
					if (!props.ContainsKey(key))
					{
						props.Add(key, pair[1].Replace("'", string.Empty).Trim());
					}
				}
			}

			// an OE might have spacing attributes

			var attr = element.Attribute("spaceBefore");
			if (attr != null)
			{
				props.Add("spaceBefore", attr.Value);
			}

			attr = element.Attribute("spaceAfter");
			if (attr != null)
			{
				props.Add("spaceAfter", attr.Value);
			}

			return props;
		}


		/// <summary>
		/// Returns the CData node from the current element
		/// </summary>
		/// <param name="element">A one:T element</param>
		/// <returns>An XCData node or null if none</returns>
		public static XCData GetCData(this XElement element)
		{
			return element.DescendantNodes()
				.FirstOrDefault(e => e.NodeType == XmlNodeType.CDATA) as XCData;
		}


		/// <summary>
		/// Returns the InnerXml of the given element
		/// </summary>
		/// <param name="element">The element to interogate</param>
		/// <returns>A string specifying the inner XML of the element.</returns>
		public static string GetInnerXml(this XElement element)
		{
			string xml = null;

			// fastest way to get XElement inner XML
			using (var reader = element.CreateReader())
			{
				reader.MoveToContent();
				xml = reader.ReadInnerXml();
			}

			// undo what XCData.GetWrapper did to <br/> elements
			xml = Regex.Replace(xml, @"<\s*br\s*/>", "<br>");

			return xml;
		}


		/// <summary>
		/// Remove and return the first textual word from the element content
		/// </summary>
		/// <param name="element">The element to modify</param>
		/// <returns>A string that can be appended to a CData's raw content</returns>
		public static string ExtractFirstWord(this XElement element)
		{
			if (element.FirstNode == null)
			{
				return null;
			}

			if (element.FirstNode.NodeType == XmlNodeType.Text)
			{
				var pair = element.Value.ExtractFirstWord();
				element.Value = pair.Item2;
				return pair.Item1;
			}

			var cdata = element.GetCData();

			// could be null if element only contains a <br>
			if (cdata == null || cdata.IsEmpty())
			{
				return string.Empty;
			}

			// copy the first word and remove it from the element...

			var wrapper = cdata.GetWrapper();

			// OneNote is very conservative with styles and excludes prior and following
			// whitespace when applying css to words; so we don't need to worry about whitespace

			// get text node or span element but not others like <br/>
			var node = wrapper.Nodes().FirstOrDefault(n =>
				// text nodes that have at least one word character
				(n.NodeType == XmlNodeType.Text && Regex.IsMatch((n as XText).Value, @"\w")) ||
				// span elements that have at least one word character
				(n.NodeType == XmlNodeType.Element && (n as XElement).Name.LocalName.Equals("span")
					&& Regex.IsMatch((n as XElement).Value, @"\w")));

			if (node == null)
			{
				return null;
			}

			string word = null;

			if (node.NodeType == XmlNodeType.Text)
			{
				// extract first word from raw text in CData
				var result = (node as XText).Value.ExtractFirstWord();
				word = result.Item1;
				(node as XText).Value = result.Item2;
			}
			else
			{
				// extract first word from text in span
				var result = (node as XElement).Value.ExtractFirstWord();
				word = result.Item1;
				(node as XElement).Value = result.Item2;

				if (result.Item2.Length == 0)
				{
					// left the span empty so remove it
					node.Remove();
				}
			}

			// update the CData (our local wrapped copy)
			cdata = new XCData(wrapper.GetInnerXml());

			// update the element's content
			element.DescendantNodes()
				.First(e => e.NodeType == XmlNodeType.CDATA)
				.ReplaceWith(cdata);

			return word;
		}


		/// <summary>
		/// Remove and return the last textual word from the element content
		/// </summary>
		/// <param name="element">The element to modify</param>
		/// <returns>A string that can be appended to a CData's raw content</returns>
		public static string ExtractLastWord(this XElement element)
		{
			if (element.FirstNode == null)
			{
				return null;
			}

			if (element.FirstNode.NodeType == XmlNodeType.Text)
			{
				var pair = element.Value.ExtractLastWord();
				element.Value = pair.Item2;
				return pair.Item1;
			}

			var cdata = element.GetCData();

			// could be null if element only contains a <br>
			if (cdata == null || cdata.IsEmpty())
			{
				return string.Empty;
			}

			// copy the last word and remove it from the element...

			var wrapper = cdata.GetWrapper();

			// OneNote is very conservative with styles and excludes prior and following
			// whitespace when applying css to words; so we don't need to worry about whitespace

			// get text node or span element but not others like <br/>
			// Note the use of Reverse() here so we get the last node with content
			var node = wrapper.Nodes().LastOrDefault(n =>
				// text nodes that have at least one word character
				(n.NodeType == XmlNodeType.Text && Regex.IsMatch((n as XText).Value, @"\w")) ||
				// span elements that have at least one word character
				(n.NodeType == XmlNodeType.Element && (n as XElement).Name.LocalName.Equals("span")
					&& Regex.IsMatch((n as XElement).Value, @"\w")));

			if (node == null)
			{
				return null;
			}

			string word = null;

			if (node.NodeType == XmlNodeType.Text)
			{
				// extract last word from raw text in CData
				var result = (node as XText).Value.ExtractLastWord();
				word = result.Item1;
				(node as XText).Value = result.Item2;
			}
			else
			{
				// extract last word from text in span
				var result = (node as XElement).Value.ExtractLastWord();
				word = result.Item1;
				(node as XElement).Value = result.Item2;

				if (result.Item2.Length == 0)
				{
					// left the span empty so remove it
					node.Remove();
				}
			}

			// update the CData (our local wrapped copy)
			cdata = new XCData(wrapper.GetInnerXml());

			// update the element's content
			element.DescendantNodes()
				.First(e => e.NodeType == XmlNodeType.CDATA)
				.ReplaceWith(cdata);

			return word;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="name"></param>
		/// <param name="breakout"></param>
		/// <returns></returns>
		public static XElement FirstAncestor(this XElement element, XName name, XName breakout = null)
		{
			var found = false;
			var ancestor = element.Parent;
			while (ancestor != null && !found)
			{
				if (ancestor.Name == name)
				{
					found = true;
				}
				else if (breakout != null && ancestor.Name == breakout)
				{
					break;
				}
				else
				{
					ancestor = ancestor.Parent;
				}
			}

			return found ? ancestor : null;
		}


		public static string TextValue(this XElement element)
		{
			return element.Value.ToXmlWrapper().Value;
		}


		public static bool GetAttributeValue(
			this XElement element, string name, out string value, string defaultV = null)
		{
			var attr = element.Attribute(name)?.Value;
			if (attr != null)
			{
				value = attr;
				return true;
			}

			value = defaultV;
			return false;
		}

		public static bool GetAttributeValue<T>(
			this XElement element, string name, out T value, T defaultV = default)
		{
			var attr = element.Attribute(name);
			if (attr != null)
			{
				try
				{
					value = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(
						null, CultureInfo.InvariantCulture,
						attr.Value);
				}
				catch
				{
					Logger.Current.WriteLine($"error translating {name}:{typeof(T).Name} '{attr.Value}'");
					value = defaultV;
					return false;
				}

				return true;
			}

			value = defaultV;
			return false;
		}


		/// <summary>
		/// Set or add the specified attribute with the given value.
		/// </summary>
		/// <param name="element">The element to affect</param>
		/// <param name="name">The name of the attribute</param>
		/// <param name="value">The value to apply</param>
		public static void SetAttributeValue(this XElement element, string name, string value)
		{
			var attr = element.Attribute(name);
			if (attr != null)
			{
				attr.Value = value;
			}
			else
			{
				element.Add(new XAttribute(name, value));
			}
		}
	}
}
