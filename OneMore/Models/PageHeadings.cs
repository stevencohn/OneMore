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


		/*
		 * <one:OE quickStyleIndex="1">
		 *   <one:T><![CDATA[. .Heading 2]]></one:T>
		 * </one:OE>
		 */

		public List<Heading> GetHeadings(ApplicationManager manager)
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
					.Where(p => p.NodeType == XmlNodeType.CDATA).FirstOrDefault() as XCData
				where c?.Value.Length > 0 && !Regex.IsMatch(c.Value, @"[\s\b]+<span[\s\b]+style=") &&
					(e.Attribute("quickStyleIndex") != null || e.Attribute("style") != null)
				select e;

			if (blocks?.Any() == true)
			{
				foreach (var block in blocks)
				{
					// get first T as heading text, filtering out any OEChildren (indented text)
					var text = block.Elements(Namespace + "T").FirstOrDefault()?.Value;
					if (text == null)
					{
						continue;
					}

					Heading heading = null;

					if (block.ReadAttributeValue("quickStyleIndex", out var quickStyleIndex, -1))
					{
						var style = quickStyles
							.Find(s => s.StyleType == StyleType.Heading && s.Index == quickStyleIndex);

						if (style != null)
						{
							// found standard heading
							heading = new Heading
							{
								Root = block,
								Text = text, // text might include <style
								Link = GetHyperlink(block, manager),
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
								Text = text, // text might include <style
								Link = GetHyperlink(block, manager),
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


		private string GetHyperlink(XElement element, ApplicationManager manager)
		{
			var attr = element.Attribute("objectID");
			if (!string.IsNullOrEmpty(attr?.Value))
			{
				try
				{
					manager.Application.GetHyperlinkToObject(PageId, attr.Value, out var link);
					return link;
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("Error getting hyperlink", exc);
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

				var find = customStyles.Find(s =>
					s.FontFamily == style.FontFamily &&
					s.FontSize == style.FontSize &&
					s.Color == style.Color &&
					s.Highlight == style.Highlight &&
					s.IsBold == style.IsBold &&
					s.IsItalic == style.IsItalic &&
					s.IsUnderline == style.IsUnderline &&
					s.SpaceBefore == style.SpaceBefore &&
					s.SpaceAfter == style.SpaceAfter
				// ignoring IsStrikethrough, IsSuperscript, IsSubscript
				);

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
