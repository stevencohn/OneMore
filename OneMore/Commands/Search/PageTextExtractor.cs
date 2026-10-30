//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// What a searchable piece of a page is.
	/// </summary>
	internal enum ParagraphKind
	{
		/// <summary>Text typed on the page. The value is what the text index stores in its kind column.</summary>
		Text = 0,

		/// <summary>Text OneNote recognized in an image. The value is what the text index stores in
		/// its kind column.</summary>
		Image = 1
	}


	/// <summary>
	/// The text of one paragraph of a page, or of one image, as the search sees it.
	/// </summary>
	internal sealed class ParagraphText
	{
		/// <summary>Gets whether this is typed text or the text recognized in an image.</summary>
		public ParagraphKind Kind { get; set; }

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

		private static readonly Regex whitespace = new Regex(@"\s+", RegexOptions.Compiled);


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
		/// Selects the images of the page body that OneNote recognized text in, plus the file
		/// printout images that sit directly on the page, outside any outline.
		/// </summary>
		/// <remarks>
		/// Other page-level images are background images, which are design and not content.
		/// </remarks>
		public static List<XElement> SelectImages(Page page)
		{
			var ns = page.Namespace;

			var printouts = page.Root
				.Elements(ns + "Image")
				.Where(e => string.Equals(
					(string)e.Attribute("isPrintOut"), "true", StringComparison.OrdinalIgnoreCase));

			return page.BodyOutlines
				.Descendants(ns + "Image")
				.Concat(printouts)
				.Where(e => e.Elements(ns + "OCRData").Elements(ns + "OCRText").Any())
				.ToList();
		}


		/// <summary>
		/// Gets the text OneNote recognized in an image, on one line.
		/// </summary>
		/// <remarks>
		/// This is plain text, often of poor quality. It is deliberately not cleaned like the markup
		/// of typed text, as a screenshot of markup should be found by the markup it shows, and it is
		/// never parsed as XML because it may hold anything.
		/// </remarks>
		public static string GetImageText(XElement image, XNamespace ns)
		{
			var builder = new StringBuilder();

			foreach (var e in image.Elements(ns + "OCRData").Elements(ns + "OCRText"))
			{
				// element value, which is the CDATA content, as a string
				var line = whitespace.Replace(e.Value, " ").Trim();
				if (line.Length > 0)
				{
					builder.Append(builder.Length > 0 ? " " : string.Empty).Append(line);
				}
			}

			return builder.ToString();
		}


		/// <summary>
		/// Gets every searchable paragraph of the page that has text, in document order, and when
		/// asked, the text recognized in its images.
		/// </summary>
		/// <param name="page">The page to read</param>
		/// <param name="includeToc">True to include paragraphs of a generated table of contents</param>
		/// <param name="includeOcr">True to include the text OneNote recognized in images, each image
		/// being one item of kind <see cref="ParagraphKind.Image"/></param>
		public static List<ParagraphText> Extract(
			Page page, bool includeToc = false, bool includeOcr = false)
		{
			var ns = page.Namespace;

			var items = SelectParagraphs(page, includeToc)
				.Select(e => (Element: e, Kind: ParagraphKind.Text))
				.ToList();

			if (includeOcr)
			{
				items.AddRange(SelectImages(page).Select(e => (Element: e, Kind: ParagraphKind.Image)));

				// OrderBy is stable, and an image never contains a paragraph, so this only places
				// each image among the paragraphs around it
				items = items
					.OrderBy(i => i.Element, XNode.DocumentOrderComparer)
					.ToList();
			}

			var result = new List<ParagraphText>();

			foreach (var (element, kind) in items)
			{
				var text = kind == ParagraphKind.Image
					? GetImageText(element, ns)
					: GetRawText(element, ns);

				var objectID = element.Attribute("objectID")?.Value;

				// an image without an object ID of its own is reached by the paragraph holding it
				if (objectID is null && kind == ParagraphKind.Image)
				{
					objectID = element.Parent?.Attribute("objectID")?.Value;
				}

				if (text.Length > 0 && objectID is not null)
				{
					result.Add(new ParagraphText
					{
						Kind = kind,
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
