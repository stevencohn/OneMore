//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Snippets
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Snippets.Toc;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol - PageTocGenerator.CheckUpToDate
	 * Used by the Refresh link warning and the RefreshToc CLI command to avoid a slow write-back
	 *
	 *   1. Build a TOC on a page with two headings
	 *   2. Refresh without changing anything; confirm the TOC is reported as up to date
	 *   3. Edit a heading, or delete a heading; confirm the TOC is not reported as up to date
	 */

	[TestClass]
	public class PageTocUpToDateTests : TestBase
	{
		private const string PageId = "page-1";
		private const string TocTime = "2026-05-01T10:00:00.000Z";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static XElement Style(int index, string name, string size) =>
			new(Ns + "QuickStyleDef",
				new XAttribute("index", index), new XAttribute("name", name),
				new XAttribute("font", "Calibri"), new XAttribute("fontSize", size),
				new XAttribute("fontColor", "automatic"));


		private static XElement Heading(string id, int style, string text, string time) =>
			new(Ns + "OE",
				new XAttribute("quickStyleIndex", style),
				new XAttribute("objectID", id),
				new XAttribute("lastModifiedTime", time),
				new XElement(Ns + "T", new XCData(text)));


		private static XElement Entry(string key, string text) =>
			new(Ns + "OE",
				new XElement(Ns + "T", new XCData(
					$"<a href=\"onenote:#p&amp;object-id={{{key}}}&amp;1A\">{text}</a>")));


		/// <summary>
		/// Builds a page with a TOC having one entry per given key, followed by headings
		/// </summary>
		private static string BuildPage(string[] entryKeys, params XElement[] headings)
		{
			var tocChildren = new XElement(Ns + "OEChildren");
			for (var i = 0; i < entryKeys.Length; i++)
			{
				tocChildren.Add(Entry(entryKeys[i], $"Entry {i}"));
			}

			var toc = new XElement(Ns + "OE",
				new XAttribute("lastModifiedTime", TocTime),
				new XElement(Ns + "Meta",
					new XAttribute("name", "omToc"),
					new XAttribute("content", "page")),
				tocChildren);

			var body = new XElement(Ns + "OEChildren", toc);
			foreach (var heading in headings)
			{
				body.Add(heading);
			}

			var page = new XElement(Ns + "Page",
				new XAttribute("ID", PageId),
				new XAttribute("pageColor", "automatic"),
				new XAttribute("lang", "en-US"),
				new XAttribute(XNamespace.Xmlns + "one", Ns.NamespaceName),
				Style(0, "p", "11.0"), Style(1, "h1", "20.0"), Style(2, "h2", "14.0"),
				new XElement(Ns + "PageSettings", new XAttribute("color", "automatic")),
				new XElement(Ns + "Title",
					new XElement(Ns + "OE",
						new XAttribute("objectID", "title-oe"),
						new XElement(Ns + "T", new XCData("TOC Test")))),
				new XElement(Ns + "Outline", body));

			return page.ToString(SaveOptions.OmitDuplicateNamespaces);
		}


		private static PageTocGenerator MakeGenerator(params string[] parameters) =>
			new(new TocParameters(parameters), PageId);


		[TestMethod]
		public async Task CheckUpToDate_NoChanges_IsUpToDate()
		{
			SetupPage(PageId, BuildPage(
				new[] { "AAAA-1", "AAAA-2" },
				Heading("h1", 1, "First", "2026-05-01T09:59:59.000Z"),
				Heading("h2", 2, "Second", TocTime)));

			var (upToDate, _) = await MakeGenerator("page").CheckUpToDate();

			Assert.IsTrue(upToDate);
		}


		[TestMethod]
		public async Task CheckUpToDate_HeadingModifiedAfterToc_IsNotUpToDate()
		{
			SetupPage(PageId, BuildPage(
				new[] { "AAAA-1", "AAAA-2" },
				Heading("h1", 1, "First", "2026-05-01T10:30:00.000Z"),
				Heading("h2", 2, "Second", TocTime)));

			var (upToDate, _) = await MakeGenerator("page").CheckUpToDate();

			Assert.IsFalse(upToDate);
		}


		[TestMethod]
		public async Task CheckUpToDate_HeadingDeleted_IsNotUpToDate()
		{
			// TOC lists two entries but only one heading remains
			SetupPage(PageId, BuildPage(
				new[] { "AAAA-1", "AAAA-2" },
				Heading("h1", 1, "First", TocTime)));

			var (upToDate, _) = await MakeGenerator("page").CheckUpToDate();

			Assert.IsFalse(upToDate);
		}


		[TestMethod]
		public async Task CheckUpToDate_HeadingBeyondLevelRange_IsIgnored()
		{
			// the h2 changed after the TOC but the TOC only covers level 1
			SetupPage(PageId, BuildPage(
				new[] { "AAAA-1" },
				Heading("h1", 1, "First", TocTime),
				Heading("h2", 2, "Second", "2026-05-01T10:30:00.000Z")));

			var (upToDate, _) = await MakeGenerator("page", "level1").CheckUpToDate();

			Assert.IsTrue(upToDate);
		}
	}
}
