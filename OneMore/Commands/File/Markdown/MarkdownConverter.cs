//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
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


		private readonly Page page;
		private readonly XNamespace ns;
		private readonly StyleAnalyzer analyzer;


		public MarkdownConverter(Page page)
		{
			this.page = page;
			ns = page.Namespace;

			analyzer = new StyleAnalyzer(page.Root);
		}


		/// <summary>
		/// Applies standard OneNote styling to all recognizable headings in all Outlines
		/// on the page
		/// </summary>
		public void RewriteHeadings()
		{
			foreach (var outline in page.Root.Elements("Outline"))
			{
				RewriteHeadings(outline.Descendants(ns + "OE"));
			}
		}


		/// <summary>
		/// Applies standard OneNote styling all recognizable headings in the given Outline
		/// </summary>
		public void RewriteHeadings(IEnumerable<XElement> paragraphs)
		{
			var headings = paragraphs
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
				.Where(c => c.Key != null);

			foreach (var heading in headings)
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
			}
		}


		private static StandardStyles? MatchHeading(Style style)
		{
			if (style.FontFamily == "Calibri")
			{
				var standard = StandardStyles.PageTitle.GetDefaults();
				if (style.FontSize == standard.FontSize && style.Color == Style.Automatic)
				{
					return StandardStyles.PageTitle;
				}

				standard = StandardStyles.Heading1.GetDefaults();
				if (style.FontSize == standard.FontSize && style.Color == standard.Color)
				{
					return StandardStyles.Heading1;
				}

				standard = StandardStyles.Heading2.GetDefaults();
				if (style.FontSize == standard.FontSize && style.Color == standard.Color)
				{
					return StandardStyles.Heading2;
				}

				standard = StandardStyles.Heading3.GetDefaults();
				if (style.FontSize == standard.FontSize && style.Color == standard.Color)
				{
					return style.IsItalic ? StandardStyles.Heading4 : StandardStyles.Heading3;
				}

				standard = StandardStyles.Heading5.GetDefaults();
				if (style.Color == standard.Color)
				{
					return style.IsItalic ? StandardStyles.Heading6 : StandardStyles.Heading5;
				}
			}

			return null;
		}


		/// <summary>
		/// Adds OneNote paragraph spacing in all Outlines on the page
		/// </summary>
		/// <param name="spaceAfter"></param>
		public void SpaceOutParagraphs(float spaceAfter)
		{
			foreach (var outline in page.Root.Elements("Outline"))
			{
				SpaceOutParagraphs(outline.Descendants(ns + "OE"), spaceAfter);
			}
		}


		/// <summary>
		/// Adds OneNote paragraph spacing in the given Outline
		/// </summary>
		/// <param name="spaceAfter"></param>
		public void SpaceOutParagraphs(IEnumerable<XElement> paragraphs, float spaceAfter)
		{
			var after = $"{spaceAfter:0.0}";

			var last = paragraphs.Last();

			foreach (var item in paragraphs
				.Where(e =>
					// not the last paragraph in the Outline
					e != last &&
					// any paragraph that is not a List
					((e.NextNode is not null && !e.Elements(ns + "List").Any()) ||
					// any last item in a List
					(e.NextNode is null && e.Elements(ns + "List").Any())
					)))
			{
				item.SetAttributeValue("spaceAfter", after);
			}
		}
	}
}
