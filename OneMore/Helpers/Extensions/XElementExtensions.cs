//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
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
		/// OneMore Extension >> Properly deep clones the given element, possibly restoring the
		/// "one:" namespace prefix if it is not present, e.g. from a snipet of a larger document.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static XElement Clone(this XElement element)
		{
			var ns = element.GetNamespaceOfPrefix(OneNote.Prefix);
			if (ns == null)
			{
				ns = element.GetDefaultNamespace();
			}

			if (ns == string.Empty)
			{
				// no namespace, might be a CDATA Wrapper
				return new XElement(
					element.Name.LocalName,
					element.Attributes(),
					element.Nodes()
					);
			}

			// reconstruct the "one" namespace by removing all namespace declaration attributes
			// and adding a new one
			return new XElement(
				ns + element.Name.LocalName,
				element.Attributes().Where(a => !a.IsNamespaceDeclaration),
				new XAttribute(XNamespace.Xmlns + "one" /*OneNote.Prefix*/, ns),
				element.Nodes()
				);
		}


		/// <summary>
		/// OneMore Extension >> Extract the style properties of this element by looking at its
		/// "style" attribute if one exists and possibly its immediate CDATA child and any
		/// span/style attributes.
		/// </summary>
		/// <param name="element">A one:T text run that may have a style attribute</param>
		/// <param name="nested">
		/// If true then collect properties from the embedded CDATA styles as well as the 
		/// style attribute of the T run itself; only the first SPAN is examined
		/// </param>
		/// <returns>
		/// The style properties collected from the inner CDATA and the style attribute of the run
		/// </returns>
		public static Dictionary<string, string> CollectStyleProperties(
			this XElement element, bool nested = true)
		{
			var props = new Dictionary<string, string>();

			if (nested)
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

			attr = element.Attribute("spaceBetween");
			if (attr != null)
			{
				props.Add("spaceBetween", attr.Value);
			}

			return props;
		}


		/// <summary>
		/// OneMore Extension >> Returns the CData node from the current element
		/// </summary>
		/// <param name="element">A one:T element</param>
		/// <returns>An XCData node or null if none</returns>
		public static XCData GetCData(this XElement element)
		{
			return element.DescendantNodes()
				.FirstOrDefault(e => e.NodeType == XmlNodeType.CDATA) as XCData;
		}


		/// <summary>
		/// OneMore Extension >> Returns the InnerXml of the given element
		/// </summary>
		/// <param name="element">The element to interogate</param>
		/// <param name="singleQuote">
		/// If true, replace attribute double quotes with single quotes; this is necessary for
		/// inner CDATA span style attributes within the XML
		/// </param>
		/// <returns>A string specifying the inner XML of the element.</returns>
		public static string GetInnerXml(this XElement element, bool singleQuote = false)
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

			if (singleQuote)
			{
				// match $1 is always the opening quote, match $2 is always the closing quote
				// and Groups[1] will contains all $1s and Groups[2] will contain all $2s
				var matches = Regex.Matches(xml, @"<.+?(?:\s\w+=(\"")[^\""]+(\""))+.*>");
				if (matches.Count > 0)
				{
					var buffer = xml.ToCharArray();
					foreach (Match match in matches)
					{
						for (int i = 1; i < match.Groups.Count; i++)
						{
							foreach (Capture capture in match.Groups[i].Captures)
							{
								buffer[capture.Index] = '\'';
							}
						}
					}

					xml = new string(buffer);
				}
			}

			return xml;
		}


		/// <summary>
		/// OneMore Extension >> Remove and return the first textual word from the element content
		/// </summary>
		/// <param name="element">The element to modify</param>
		/// <param name="styled">Return the word preserving its styled span parent</param>
		/// <returns>A string that can be appended to a CData's raw content</returns>
		public static string ExtractFirstWord(this XElement element, bool styled = false)
		{
			if (element.FirstNode == null)
			{
				return null;
			}

			if (element.FirstNode.NodeType == XmlNodeType.Text)
			{
				var pair = element.Value.SplitAtFirstWord();
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

			if (node is XText text)
			{
				// extract first word from raw text in CData
				var pair = text.Value.SplitAtFirstWord();
				word = pair.Item1;
				text.Value = pair.Item2;
			}
			else if (node is XElement span)
			{
				if (styled)
				{
					// clone the <span> retaining its style= attribute
					var clone = span.Clone();
					// edit the span, removing its first word
					var result = span.ExtractFirstWord();

					// reset our clone to just the first word
					clone.Value = result;
					word = clone.ToString(SaveOptions.DisableFormatting);

					if (span.Value.Length == 0)
					{
						// resultant span is empty so remove it
						span.Remove();
					}
				}
				else
				{
					// extract first word from text in span
					word = span.ExtractFirstWord();

					if (span.Value.Length == 0)
					{
						// left the span empty so remove it
						span.Remove();
					}
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
		/// OneMore Extension >> Remove and return the last textual word from the element content
		/// </summary>
		/// <param name="element">The element to modify</param>
		/// <param name="styled">Return the word preserving its styled span parent</param>
		/// <returns>A string that can be appended to a CData's raw content</returns>
		public static string ExtractLastWord(this XElement element, bool styled = false)
		{
			if (element.FirstNode == null)
			{
				return null;
			}

			if (element.FirstNode.NodeType == XmlNodeType.Text)
			{
				var pair = element.Value.SplitAtLastWord();
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

			if (node is XText text)
			{
				// extract last word from raw text in CData
				var pair = text.Value.SplitAtLastWord();
				word = pair.Item1;
				text.Value = pair.Item2;
			}
			else if (node is XElement span)
			{
				if (styled)
				{
					// clone the <span> retaining its style= attribute
					var clone = span.Clone();
					// edit the span, removing its last word
					var result = span.ExtractLastWord();

					// reset our clone to just the last word
					clone.Value = result;
					word = clone.ToString(SaveOptions.DisableFormatting);

					if (span.Value.Length == 0)
					{
						// resultant span is empty so remove it
						span.Remove();
					}
				}
				else
				{
					// extract last word from text in span
					word = span.ExtractLastWord();

					if (span.Value.Length == 0)
					{
						// left the span empty so remove it
						span.Remove();
					}
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
		/// OneMore Extension >> Find the closest named ancestor element,
		/// aborting if the breakout element is discovered first.
		/// </summary>
		/// <param name="element">The current element</param>
		/// <param name="name">The element name to find</param>
		/// <param name="breakout">The element name to signal abort</param>
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


		/// <summary>
		/// OneMore Extension >> Extract the sanitized text value of the given element
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static string TextValue(this XElement element)
		{
			return element.Value.ToXmlWrapper().Value;
		}


		/// <summary>
		/// OneMore Extension >> Get the value of the named attribute or a default if not present
		/// </summary>
		/// <param name="element"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="defaultV"></param>
		/// <returns></returns>
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


		/// <summary>
		/// OneMore Extension >> Get a typed named attribute value or a default value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="defaultV"></param>
		/// <returns></returns>
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
		/// OneMore Extension >> Set or add the specified attribute with the given value.
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
