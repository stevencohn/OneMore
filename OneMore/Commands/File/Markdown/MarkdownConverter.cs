//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Xml.Linq;


	internal class MarkdownConverter
	{
		private sealed class Candidate
		{
			public XElement Element;
			public Style Style;
			public StandardStyles? Key;
		}


		public static void RewriteHeadings(Page page)
		{
			var analyzer = new StyleAnalyzer(page.Root);
			var ns = page.Namespace;

			page.Root.Elements(ns + "Outline")
				.Descendants(ns + "OE")
				// candidate headings imported from markdown should have exactly one text run
				.Where(e => e.Elements(ns + "T").Count() == 1)
				.Select(e => new Candidate
				{
					Element = e,
					// deep dive into style of text run
					Style = new Style(analyzer.CollectFrom(e.Elements(ns + "T").First(), true))
				})
				.Select(c =>
				{
					// have to match heading after Style is set in previous Select
					c.Key = MatchHeading(c.Style);
					return c;
				})
				// shouldn't happen but...
				.Where(c => c.Key != null)
				.ForEach(heading =>
				{
					// ensures quick style is declared if not already
					var quick = page.GetQuickStyle((StandardStyles)heading.Key);

					// nix inline style we added during phase 1 import
					heading.Element.Attributes().Where(a => a.Name == "style").Remove();
					// set heading quick style on OE
					heading.Element.SetAttributeValue("quickStyleIndex", quick.Index);

					var stylizer = new Stylizer(quick);

					heading.Element
						.Elements(ns + "T")
						.ForEach(e =>
						{
							// set any additional css on text run such as italics
							stylizer.ApplyStyle(e);
						});
				});
		}


		private static StandardStyles? MatchHeading(Style style)
		{
			if (style.FontFamily == "Calibri")
			{
				if (style.FontSize == "16.0" && style.Color == "#1E4E79")
				{
					return StandardStyles.Heading1;
				}

				if (style.FontSize == "14.0" && style.Color == "#2E75B5")
				{
					return StandardStyles.Heading2;
				}

				if (style.FontSize == "12.0" && style.Color == "#5B9BD5")
				{
					return style.IsItalic ? StandardStyles.Heading4 : StandardStyles.Heading3;
				}

				if (style.FontSize == "11.0" && style.Color == "#2E75B5")
				{
					return style.IsItalic ? StandardStyles.Heading6 : StandardStyles.Heading5;
				}
			}

			return null;
		}
	}
}
