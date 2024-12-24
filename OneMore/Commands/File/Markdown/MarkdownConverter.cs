//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Utility class for post-processing a page converted from markdown.
	/// </summary>
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

			PageNamespace.Set(ns);

			analyzer = new StyleAnalyzer(page.Root);
		}


		/// <summary>
		/// Applies standard OneNote styling to all recognizable headings in all Outlines
		/// on the page
		/// </summary>
		public void RewriteHeadings()
		{
			foreach (var outline in page.BodyOutlines)
			{
				RewriteHeadings(outline.Descendants(ns + "OE"));
			}

			// added header spacing specific to markdown
			var quickstyles = page.Root.Elements(ns + "QuickStyleDef");
			foreach (var quickstyle in quickstyles)
			{
				var name = quickstyle.Attribute("name").Value;
				if (name.Equals("h1") || name.Equals("h2"))
				{
					replaceAtributes(quickstyle, spaceBefore: 0.8, spaceAfter: 0.5);
				}
				else
				if (name.Equals("h3") || name.Equals("h4"))
				{
					replaceAtributes(quickstyle, spaceBefore: 0.3, spaceAfter: 0.3);
				}
			}
			void replaceAtributes(XElement quickstyle, double spaceBefore, double spaceAfter)
			{
				quickstyle.SetAttributeValue("spaceBefore", spaceBefore.ToString("####0.00", CultureInfo.InvariantCulture));
				quickstyle.SetAttributeValue("spaceAfter", spaceAfter.ToString("####0.00", CultureInfo.InvariantCulture));
			}

		}


		/// <summary>
		/// Applies standard OneNote styling all recognizable headings in the given Outline
		/// </summary>
		public MarkdownConverter RewriteHeadings(IEnumerable<XElement> paragraphs)
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

			return this;
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
		/// Applies standard OneNote styling to all recognizable headings in all Outlines
		/// on the page
		/// </summary>
		public void RewriteTodo()
		{
			foreach (var outline in page.BodyOutlines)
			{
				RewriteTodo(outline.Descendants(ns + "OE"));
			}
		}


		/// <summary>
		/// Tag current line with To Do tag if beginning with [ ] or [x]
		/// Also :TAGS: will be handled here
		/// All other :emojis: should be translated inline by Markdig
		/// </summary>
		/// <param name="paragraphs"></param>
		public MarkdownConverter RewriteTodo(IEnumerable<XElement> paragraphs)
		{
			var boxpattern = new Regex(@"^\\?\[(?<x>x|\s)\]");

			foreach (var paragraph in paragraphs)
			{
				var run = paragraph.Elements(ns + "T").FirstOrDefault();

				if (run is not null)
				{
					var cdata = run.GetCData();
					var wrapper = cdata.GetWrapper();
					if (wrapper.FirstNode is XText)
					{
						cdata.Value = wrapper.GetInnerXml();
					}
					while (wrapper.FirstNode is not XText && wrapper.FirstNode is not null)
					{
						wrapper = (XElement)wrapper.FirstNode;
					}
					if (wrapper.FirstNode is XText text)
					{
						var match = boxpattern.Match(text.Value);
						// special treatment of todo tag 
						if (match.Success)
						{
							var org = text.Value;
							var completed = match.Groups["x"].Value == "x";
							text.Value = text.Value.Replace((completed ? "[x]" : "[ ]"), "");
							cdata.Value = cdata.Value.Replace(org, text.Value);
							page.SetTag(paragraph, tagSymbol: "3", tagStatus:completed,tagName:"todo");
						}
						else
						{
							// look for all other tags
							foreach (var t in MarkdownEmojis.taglist)
							{
								// check for other tags
								if (text.Value.Contains(t.name))
								{
									var org = text.Value;
									text.Value = text.Value.Replace(t.name, "");
									cdata.Value = cdata.Value.Replace(org, text.Value);
									// ensure TagDef exists
									page.SetTag(paragraph, tagSymbol: t.id, tagStatus: false, tagName: t.topic, tagType: t.type);
									break;
								}
							}

						}
					}
				}
			}

			return this;
		}


		/// <summary>
		/// Adds OneNote paragraph spacing in all Outlines on the page
		/// </summary>
		/// <param name="spaceAfter"></param>
		public MarkdownConverter SpaceOutParagraphs(float spaceAfter)
		{
			foreach (var outline in page.BodyOutlines)
			{
				SpaceOutParagraphs(outline.Descendants(ns + "OE"), spaceAfter);
			}

			return this;
		}


		/// <summary>
		/// Adds OneNote paragraph spacing in the given Outline
		/// </summary>
		/// <param name="spaceAfter"></param>
		public MarkdownConverter SpaceOutParagraphs(
			IEnumerable<XElement> paragraphs, float spaceAfter)
		{
			var after = spaceAfter.ToString("####0.00", CultureInfo.InvariantCulture);

			var last = paragraphs.Last();

			var list = paragraphs
				.Where(e =>
					// not the last paragraph in the Outline
					e != last &&
					// any paragraph that is not a List
					((e.NextNode is not null && !e.Elements(ns + "List").Any()) ||
					// any last item in a List
					(e.NextNode is null && e.Elements(ns + "List").Any())
					))
				.ToList();

			foreach (var item in list)
			{
				item.SetAttributeValue("spaceAfter", after);
			}

			return this;
		}
	}
}
