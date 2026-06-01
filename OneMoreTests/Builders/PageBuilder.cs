//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Builders
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml.Linq;


	/// <summary>
	/// Builds valid OneNote 2013 page XML for use in unit tests.
	/// </summary>
	internal class PageBuilder
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		private readonly string pageId;
		private readonly string title;
		private readonly string pageColor;
		private readonly List<XElement> outlineElements = new();
		private readonly List<(string name, string content)> metaItems = new();


		/// <param name="pageId">The page ID (used as the XML ID attribute)</param>
		/// <param name="title">The page title text</param>
		/// <param name="pageColor">Optional page background color (default: automatic)</param>
		public PageBuilder(string pageId = "page-1", string title = "Test Page",
			string pageColor = "automatic")
		{
			this.pageId = pageId;
			this.title = title;
			this.pageColor = pageColor;
		}


		/// <summary>
		/// Adds a plain text paragraph to the page body.
		/// </summary>
		/// <param name="text">The text content</param>
		/// <param name="selected">When true, the T run carries selected="all"</param>
		/// <param name="quickStyleIndex">Optional quick-style index on the OE</param>
		public PageBuilder WithParagraph(string text, bool selected = false,
			int quickStyleIndex = 0)
		{
			var t = new XElement(Ns + "T", new XCData(text));
			if (selected)
			{
				t.SetAttributeValue("selected", "all");
			}

			var oe = new XElement(Ns + "OE");
			if (quickStyleIndex > 0)
			{
				oe.SetAttributeValue("quickStyleIndex", quickStyleIndex.ToString());
			}

			oe.Add(t);
			outlineElements.Add(oe);
			return this;
		}


		/// <summary>
		/// Adds a paragraph whose content is provided as a pre-built XElement.
		/// </summary>
		public PageBuilder WithElement(XElement oe)
		{
			outlineElements.Add(oe);
			return this;
		}


		/// <summary>
		/// Adds a Meta element to the page.
		/// </summary>
		public PageBuilder WithMeta(string name, string content)
		{
			metaItems.Add((name, content));
			return this;
		}


		/// <summary>
		/// Builds and returns the page XML as a string.
		/// </summary>
		public string Build()
		{
			return BuildElement().ToString(SaveOptions.OmitDuplicateNamespaces);
		}


		/// <summary>
		/// Builds and returns the page as an XElement (for callers that need the DOM).
		/// </summary>
		public XElement BuildElement()
		{
			var page = new XElement(Ns + "Page",
				new XAttribute("ID", pageId),
				new XAttribute("pageColor", pageColor),
				new XAttribute("lang", "en-US"),
				new XAttribute(XNamespace.Xmlns + "one",
					"http://schemas.microsoft.com/office/onenote/2013/onenote"));

			// Meta elements
			foreach (var (name, content) in metaItems)
			{
				page.Add(new XElement(Ns + "Meta",
					new XAttribute("name", name),
					new XAttribute("content", content)));
			}

			// PageSettings — required by Page.GetPageColor()
			page.Add(new XElement(Ns + "PageSettings",
				new XAttribute("color", pageColor)));

			// Title
			page.Add(new XElement(Ns + "Title",
				new XElement(Ns + "OE",
					new XElement(Ns + "T", new XCData(title)))));

			// Outline with body paragraphs
			if (outlineElements.Count > 0)
			{
				var oeChildren = new XElement(Ns + "OEChildren");
				foreach (var oe in outlineElements)
				{
					oeChildren.Add(oe);
				}

				page.Add(new XElement(Ns + "Outline",
					new XElement(Ns + "OEChildren", oeChildren.Elements())));
			}

			return page;
		}
	}
}
