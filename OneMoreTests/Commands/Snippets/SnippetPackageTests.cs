//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Snippets
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;

	/*
	 * Test Protocol - SnippetPackage
	 * Snippets are saved as page XML so formatting survives the round trip.
	 *
	 *   1. Select some paragraphs, one with a heading quick style, and build a package;
	 *      the package must contain only the selected paragraphs and the referenced
	 *      QuickStyleDef, and the source page must be unchanged.
	 *   2. Select 2x2 cells of a 3x3 table; the package must contain a table with two
	 *      columns, preserving their widths and the hidden borders.
	 *   3. Merge the package into a page with different quick style indexes; the content
	 *      must be remapped to the target page's indexes.
	 *   4. Insert paragraphs at an empty cursor line; the line is replaced.
	 */

	[TestClass]
	public class SnippetPackageTests : TestBase
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static XElement Para(string text, string style = null, bool selected = false)
		{
			var t = new XElement(Ns + "T", new XCData(text));
			if (selected)
			{
				t.SetAttributeValue("selected", "all");
			}

			var oe = new XElement(Ns + "OE", t);
			if (style is not null)
			{
				oe.SetAttributeValue("quickStyleIndex", style);
			}

			return oe;
		}


		private static Page Make(params object[] outlineContent)
		{
			var root = new XElement(Ns + "Page",
				new XAttribute("ID", "page-1"),
				new XAttribute(XNamespace.Xmlns + "one", Ns.NamespaceName),
				new XElement(Ns + "QuickStyleDef",
					new XAttribute("index", "0"), new XAttribute("name", "p"),
					new XAttribute("fontColor", "automatic"), new XAttribute("highlightColor", "automatic"),
					new XAttribute("font", "Calibri"), new XAttribute("fontSize", "11.0"),
					new XAttribute("spaceBefore", "0.0"), new XAttribute("spaceAfter", "0.0")),
				new XElement(Ns + "QuickStyleDef",
					new XAttribute("index", "1"), new XAttribute("name", "h1"),
					new XAttribute("fontColor", "#2F5496"), new XAttribute("highlightColor", "automatic"),
					new XAttribute("font", "Calibri Light"), new XAttribute("fontSize", "20.0"),
					new XAttribute("spaceBefore", "0.0"), new XAttribute("spaceAfter", "0.0")),
				new XElement(Ns + "Outline",
					new XAttribute("selected", "partial"),
					new XElement(Ns + "OEChildren", outlineContent)));

			return new Page(root);
		}


		[TestMethod]
		public void BuildKeepsOnlySelectedParagraphsAndStyles()
		{
			var page = Make(
				Para("before", "0"),
				Para("heading", "1", selected: true),
				Para("body", "0", selected: true),
				Para("after", "0"));

			var before = page.Root.ToString();

			var package = SnippetPackage.Build(page, SelectionScope.Range);

			Assert.IsNotNull(package);
			Assert.AreEqual(before, page.Root.ToString(), "source page was modified");

			var texts = package.Descendants(Ns + "T").Select(e => e.Value).ToList();
			CollectionAssert.AreEqual(new[] { "heading", "body" }, texts);

			var defs = package.Elements(Ns + "QuickStyleDef").ToList();
			Assert.AreEqual(2, defs.Count);
			Assert.IsFalse(package.Descendants().Attributes("selected").Any());
		}


		[TestMethod]
		public void MergeRemapsQuickStyleIndexes()
		{
			var source = Make(Para("heading", "1", selected: true));
			var package = SnippetPackage.Build(source, SelectionScope.Range);

			// target page has the same h1 but at index 3
			var target = Make(Para("x", "0", selected: true));
			target.Root.Elements(Ns + "QuickStyleDef").Last()
				.SetAttributeValue("index", "3");

			var content = SnippetPackage.Parse(
				package.ToString(), Ns, out var kind, out var root);

			SnippetPackage.MergeDefinitions(target, root, content);

			var oe = content.Descendants(Ns + "OE").First();
			Assert.AreEqual("3", oe.Attribute("quickStyleIndex").Value);
			Assert.AreEqual(2, target.Root.Elements(Ns + "QuickStyleDef").Count());
		}


		[TestMethod]
		public void InsertReplacesEmptyCursorLine()
		{
			var target = Make(Para("keep", "0"), Para(string.Empty, "0", selected: true));
			var content = new XElement(Ns + "OEChildren", Para("one"), Para("two"));

			SnippetPackage.Insert(target, content, SnippetPackage.KindParagraphs);

			var texts = target.Root.Descendants(Ns + "T").Select(e => e.Value).ToList();
			CollectionAssert.AreEqual(new[] { "keep", "one", "two" }, texts);
			Assert.AreEqual("two", target.Root.Descendants(Ns + "T")
				.Single(t => t.Attribute("selected") is not null).Value);
		}
	}
}
