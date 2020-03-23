//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	internal class InsertTocCommand : Command
	{
		private class Heading
		{
			public string Text;
			public string Link;
			public StyleBase Style;
		}


		private XElement page;
		private XNamespace ns;
		private string pageId;
		private List<Style> quickStyles;
		private List<Style> customStyles;


		public InsertTocCommand () : base()
		{
		}


		public void Execute ()
		{
			try
			{
				using (var manager = new ApplicationManager())
				{
					page = manager.CurrentPage();
					if (page != null)
					{
						ns = page.GetNamespaceOfPrefix("one");
						pageId = page.Attribute("ID").Value;

						CollectHeadingDefinitions();

						Evaluate(manager);
						manager.UpdatePageContent(page);
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error executing InsertTocCommand", exc);
			}
		}


		private void Evaluate (ApplicationManager manager)
		{
			var headings = new List<Heading>();

			// Find all OE blocks with text, e.g., containing at least one T

			var blocks =
				from e in page.Elements(ns + "Outline").Descendants(ns + "OE")
				let c = e.Elements(ns + "T").DescendantNodes().Where(p => p.NodeType == XmlNodeType.CDATA).FirstOrDefault() as XCData
				where c?.Value.Length > 0 && !Regex.IsMatch(c.Value, @"[\s\b]+<span[\s\b]+style=") &&
					(e.Attribute("quickStyleIndex") != null || e.Attribute("style") != null)
				select e;

			if (blocks?.Any() == true)
			{
				foreach (var block in blocks)
				{
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
								Text = block.Value,
								Link = GetHyperlink(block, manager),
								Style = style
							};
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
								Text = block.Value,
								Link = GetHyperlink(block, manager),
								Style = style
							};
						}
					}

					if (heading != null)
					{
						headings.Add(heading);
					}
				}
			}

			ReorderKnownStyles();
			GenerateTableOfContents(headings);
		}


		/// <summary>
		/// Construct a list of possible templates from both this page's quick styles
		/// and our own custom style definitions, choosing only Heading styles, all
		/// ordered by the internal Index.
		/// </summary>
		/// <returns>A List of Styles ordered by Index</returns>
		private void CollectHeadingDefinitions()
		{
			// collect all quick style defs
			
			// going to reference both heading and non-headings
			quickStyles = page.Elements(ns + "QuickStyleDef")
				.Select(p => new Style(new QuickStyleDef(p)))
				.ToList();

			// tag the headings (h1, h2, h3, ...)
			foreach (var style in quickStyles)
			{
				if (Regex.IsMatch(style.Name, @"h\d"))
				{
					style.StyleType = StyleType.Heading;
				}
			}

			// collect custom heading styles

			customStyles = new StyleProvider().GetStyles()
				.Where(e => e.StyleType == StyleType.Heading)
				.OrderBy(e => e.Index)
				.ToList();
		}


		private string GetHyperlink(XElement element, ApplicationManager manager)
		{
			var attr = element.Attribute("objectID");
			if (!string.IsNullOrEmpty(attr?.Value))
			{
				try
				{
					manager.Application.GetHyperlinkToObject(pageId, attr.Value, out var link);
					return link;
				}
				catch (Exception exc)
				{
					logger.WriteLine("Error getting hyperlink", exc);
				}
			}

			return null;
		}


		private Style FindCustomStyle(XElement block)
		{
			var child = block.Descendants(ns + "T").FirstOrDefault();
			if (child != null)
			{
				var analyzer = new StyleAnalyzer(page);
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
		private void ReorderKnownStyles()
		{
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

			for (int i = 1; i < customStyles.Count; i++)
			{
				customStyles[i].Index = i;
			}
		}


		/*
		 * <one:OE quickStyleIndex="1">
		 *   <one:T><![CDATA[. .Heading 2]]></one:T>
		 * </one:OE>
		 */

		/// <summary>
		/// Generates a table of contents at the top of the current page
		/// </summary>
		/// <param name="headings"></param>
		private void GenerateTableOfContents (List<Heading> headings)
		{
			var top = page.Element(ns + "Outline")?.Element(ns + "OEChildren");
			if (top == null)
			{
				return;
			}

			var items = new List<XElement>
			{
				// "Table of Contents"
				new XElement(ns + "OE",
				new XAttribute("style", "font-size:16.0pt"),
				new XElement(ns + "T",
					new XCData("<span style='font-weight:bold'>Table of Contents</span>")
					)
				)
			};

			if (headings?.Any() == true)
			{
				// use the minimum intent level
				var minlevel = headings.Min(e => e.Style.Index);

				foreach (var heading in headings)
				{
					var text = new StringBuilder();
					var count = minlevel;
					while (count < heading.Style.Index)
					{
						text.Append(". . ");
						count++;
					}

					if (!string.IsNullOrEmpty(heading.Link))
					{
						text.Append($"<a href=\"{heading.Link}\">{heading.Text}</a>");
					}
					else
					{
						text.Append(heading.Text);
					}

					items.Add(new XElement(ns + "OE",
						new XElement(ns + "T", new XCData(text.ToString()))
						));
				}
			}

			// empty line after the TOC
			items.Add(new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))));

			top.AddFirst(items);
		}
	}
}
