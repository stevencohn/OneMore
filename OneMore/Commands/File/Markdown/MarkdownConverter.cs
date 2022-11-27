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
			public StandardStyles? Standard;
		}


		public static void RewriteHeadings(Page page)
		{
			var analyzer = new StyleAnalyzer(page.Root);
			var ns = page.Namespace;

			var headings = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "OE")
				.Where(e => e.Elements(ns + "T").Count() == 1)
				.Select(e => new Candidate
				{
					Element = e,
					Style = analyzer.CollectStyleFrom(e.Elements(ns + "T").First())
				})
				.Select(c =>
				{
					c.Standard = MatchHeading(c.Style);
					return c;
				})
				.Where(c => c.Standard != null);

			headings
				.Where(heading => heading.Standard != null)
				.ForEach(heading =>
				{
					var quick = page.GetQuickStyle((StandardStyles)heading.Standard);

					heading.Element.Attributes().Where(a => a.Name == "style").Remove();
					heading.Element.SetAttributeValue("quickStyleIndex", quick.Index);

					var stylizer = new Stylizer(quick);

					heading.Element
						.Elements(ns + "T")
						.ForEach(e =>
						{
							stylizer.ApplyStyle(e);
						});
				});
		}


		private static StandardStyles? MatchHeading(Style style)
		{
			if (style.FontFamily == "Calibri")
			{
				if (style.FontSize == "16.0" && style.Color == "#1E4E79")
					return StandardStyles.Heading1;

				if (style.FontSize == "14.0" && style.Color == "#2E75B5")
					return StandardStyles.Heading2;

				if (style.FontSize == "12.0" && style.Color == "#5B9BD5")
					return StandardStyles.Heading3;

				if (style.FontSize == "12.0" && style.Color == "#5B9BD5" && style.IsItalic)
					return StandardStyles.Heading4;

				if (style.FontSize == "11.0" && style.Color == "#2E75B5")
					return StandardStyles.Heading5;

				if (style.FontSize == "11.0" && style.Color == "#2E75B5" && style.IsItalic)
					return StandardStyles.Heading6;
			}

			return null;
		}
	}
}
