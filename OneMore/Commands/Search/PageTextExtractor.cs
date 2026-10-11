//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// The text of one paragraph of a page, as the search sees it.
	/// </summary>
	internal sealed class ParagraphText
	{
		/// <summary>Gets the OneNote object ID of the paragraph, valid until the notebook is reopened.</summary>
		public string ObjectID { get; set; }

		/// <summary>Gets the position of the paragraph among those of the page that have text.</summary>
		public int Ordinal { get; set; }

		/// <summary>Gets the text with its markup removed but not yet HTML decoded, which is what
		/// the search patterns are matched against.</summary>
		public string Text { get; set; }
	}


	/// <summary>
	/// Decides which paragraphs of a page are searched and what text each one has, so that the live
	/// search and the text index agree exactly.
	/// </summary>
	internal static class PageTextExtractor
	{
		private static readonly Regex cleaner = new Regex(
			@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
			RegexOptions.Compiled);


		/// <summary>
		/// Selects the paragraphs of the page body that can contain searchable text.
		/// </summary>
		/// <param name="page">The page to read</param>
		/// <param name="includeToc">True to include paragraphs of a generated table of contents</param>
		public static List<XElement> SelectParagraphs(Page page, bool includeToc)
		{
			var ns = page.Namespace;

			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.ToList();

			if (!includeToc)
			{
				paragraphs = paragraphs.Where(e =>
					(e.Elements(ns + "T").Any()
					&& !e.AncestorsAndSelf(ns + "OE")
						.Any(a => a.Elements(ns + "Meta")
							.Any(m => m.Attribute("name")?.Value == "omToc"))
					) ||
					e.Elements(ns + "InsertedFile").Any()
				)
				.ToList();
			}

			return paragraphs;
		}


		/// <summary>
		/// Gets the text of a paragraph without its markup, or for a paragraph that is only an
		/// inserted file, the name and path of the file.
		/// </summary>
		public static string GetRawText(XElement paragraph, XNamespace ns)
		{
			var text = string.Empty;
			foreach (var e in paragraph.Elements(ns + "T"))
			{
				var line = cleaner.Replace(e.Value, string.Empty).Trim();
				if (line.Length > 0)
				{
					text = $"{text}{line} ";
				}
			}

			if (text.Length == 0)
			{
				foreach (var e in paragraph.Elements(ns + "InsertedFile"))
				{
					if (e.Attribute("pathSource")?.Value is string pathSource)
					{
						text = $"{text} {pathSource}";
					}

					if (e.Attribute("preferredName")?.Value is string preferredName)
					{
						text = $"{text} {preferredName}";
					}
				}
			}

			return text.Trim();
		}


		/// <summary>
		/// Gets every searchable paragraph of the page that has text, in document order.
		/// </summary>
		public static List<ParagraphText> Extract(Page page, bool includeToc = false)
		{
			var result = new List<ParagraphText>();
			var ns = page.Namespace;

			foreach (var paragraph in SelectParagraphs(page, includeToc))
			{
				var text = GetRawText(paragraph, ns);
				var objectID = paragraph.Attribute("objectID")?.Value;

				if (text.Length > 0 && objectID is not null)
				{
					result.Add(new ParagraphText
					{
						ObjectID = objectID,
						Ordinal = result.Count,
						Text = text
					});
				}
			}

			return result;
		}
	}
}
