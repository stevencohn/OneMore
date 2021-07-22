//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	internal partial class Page
	{
		private List<Style> quickStyles;
		private List<Style> customStyles;
		private string pageColor;


		/*
		 * <one:OE quickStyleIndex="1">
		 *   <one:T><![CDATA[. .Heading 2]]></one:T>
		 * </one:OE>
		 */

		public List<Heading> GetHeadings(OneNote one)
		{
			quickStyles = GetQuickStyles();

			customStyles = new StyleProvider().GetStyles()
				.Where(e => e.StyleType == StyleType.Heading)
				.OrderBy(e => e.Index)
				.ToList();

			var headings = new List<Heading>();

			// Find all OE blocks with text, e.g., containing at least one T

			var blocks =
				from e in Root.Elements(Namespace + "Outline").Descendants(Namespace + "OE")
				let c = e.Elements(Namespace + "T").DescendantNodes()
					.FirstOrDefault(p => p.NodeType == XmlNodeType.CDATA) as XCData
				where c?.Value.Length > 0 && !Regex.IsMatch(c.Value, @"[\s\b]+<span[\s\b]+style=") &&
					(e.Attribute("quickStyleIndex") != null || e.Attribute("style") != null)
				select e;

			if (blocks?.Any() == true)
			{
				GetPageColor();

				foreach (var block in blocks)
				{
					// concat text from each T in block; handles when cursor bisects heading
					var text = block.Elements(Namespace + "T")
						.Select(e => e.Value).Aggregate((x, y) => $"{x}{y}");

					if (text == null)
					{
						continue;
					}

					Heading heading = null;

					if (block.GetAttributeValue("quickStyleIndex", out var quickStyleIndex, -1))
					{
						var style = quickStyles
							.Find(s => s.StyleType == StyleType.Heading && s.Index == quickStyleIndex);

						if (style != null)
						{
							// found standard heading
							heading = new Heading
							{
								Root = block,
								// text might include <style...
								Text = text,
								Link = GetHyperlink(block, one),
								Style = style
							};

							headings.Add(heading);
						}
					}

					if (heading == null)
					{
						// custom heading?
						var style = FindCustomStyle(block);

						if (style != null)
						{
							// found standard heading
							heading = new Heading
							{
								Root = block,
								// text might include <style...
								Text = text,
								Link = GetHyperlink(block, one),
								Style = style
							};

							headings.Add(heading);
						}
					}
				}
			}

			// style.Index is used for TOC indenting, regardless of heading level
			ResetHeadingStyleIndexes();

			// Level is used as the intended Heading level, e.g. Heading4 level is 4
			foreach (var heading in headings)
			{
				heading.Level = heading.Style.Index;
			}

			return headings;
		}


		private void GetPageColor()
		{
			pageColor = Root.Elements(Namespace + "PageSettings")
				.Where(e => e.Attribute("color") != null)
				.Select(e => e.Attribute("color").Value)
				.FirstOrDefault();

			if (pageColor == null)
			{
				pageColor = "automatic";
			}
		}


		private string GetHyperlink(XElement element, OneNote one)
		{
			var attr = element.Attribute("objectID");
			if (!string.IsNullOrEmpty(attr?.Value))
			{
				try
				{
					var link = one.GetHyperlink(PageId, attr.Value);
					return link;
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error getting hyperlink", exc);
				}
			}

			return null;
		}


		private Style FindCustomStyle(XElement block)
		{
			var child = block.Descendants(Namespace + "T").FirstOrDefault();
			if (child != null)
			{
				var analyzer = new StyleAnalyzer(Root);
				var style = new Style(analyzer.CollectStyleProperties(child));

				// normalize style background to page background
				if (style.Highlight != pageColor && pageColor == "automatic")
				{
					if (style.Highlight == "#FFFFFF")
					{
						style.Highlight = "automatic";
					}
				}

				// compare and find
				var find = customStyles.Find(s => s.Equals(style));
				return find;
			}

			return null;
		}


		/// <summary>
		/// Repurpose the style Index property to represent the heading indent level,
		/// so reset each heading index according to its logical hierarhcy
		/// </summary>
		private void ResetHeadingStyleIndexes()
		{
			quickStyles = quickStyles.Where(s => Regex.IsMatch(s.Name, @"h(\d+)")).ToList();

			foreach (var style in quickStyles)
			{
				var match = Regex.Match(style.Name, @"h(\d+)");
				if (match.Success && match.Captures.Count > 0)
				{
					if (int.TryParse(match.Captures[0].Value, out var index))
					{
						style.Index = index;
					}
				}
			}

			quickStyles = quickStyles.OrderBy(s => s.Index).ToList();

			for (int i = 0; i < quickStyles.Count; i++)
			{
				quickStyles[i].Index = i;
			}

			for (int i = 0; i < customStyles.Count; i++)
			{
				customStyles[i].Index = i;
			}
		}
	}
}
