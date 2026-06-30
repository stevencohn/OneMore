//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Edit
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Colorizer;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;

	/*
	 * Test Protocol
	 * Commands/Edit/ColorizeCommand
	 *
	 * This is a test of the Colorize class. It should be instantiated and fed content as
	 * demonstrated by the ColorizeCommand.Colorize method. Apply syntax highlighting to
	 * blocks of code
	 *
	 * 	1. Select each block of code in turn from below: C#, JSON, SQL, and XML.
	 * 	   Run Colorize/<language>
	 * 	2. Confirm all syntax is highlighted appropriately
	 *
	 * C#
	 * /// <summary>
	 * /// Execute the command
	 * /// </summary>
	 * /// <param name="args"></param>
	 * /// <returns></returns>
	 * public override void Execute(params object[] args)
	 * {
	 *     /*
	 *      * multi line using
	 *      * comments
	 *      *\/
	 *     using (var one = new OneNote())  // comment
	 *     {
	 *         // single line comment
	 *         new FootnoteEditor(one).AddFootnote("stri//ng");
	 * +        var a = 1 + 34.34;
	 *     }
	 * }
	 * // end
	 *
	 * JSON
	 * { "EventId": "...", "Timestamp": "...", "EventName": "InsertInfoBox", ...
	 *   "Client": { "OneVersion": "...", "OsMajor": 10, ... }, "Data": { "Message": "", "Info": "" } }
	 *
	 * SQL
	 * SELECT GETDATE() AS [Today], /* same-line comment *\/ (GETDATE() + 1) AS [Tomorrow],
	 * CAST(1 as bit) AS [Boolean], CAST(1 as int) AS [Number]
	 * GO
	 * /* multi-line comment spanning separate lines/paragraphs *\/
	 * SELECT ... CAST(1 as int) AS [Number]
	 * GO
	 *
	 * XML
	 * <one:Page> <!-- comment --> <one:QuickStyleDef .../> <one:PageSettings ...>
	 *   ... <one:T><![CDATA[Lorem ipsum dolor sit amet]]></one:T> ...
	 * </one:Page>
	 */

	[TestClass]
	public class ColorizeCommandTests
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		/// <summary>
		/// Colorize(key, runs) is the testable core of ColorizeCommand; Execute() only adds
		/// the OneNote interop plumbing (selection lookup, AddDepth/RemoveDepth, page update).
		/// Tests build runs directly from a Page and feed them to Colorize(), bypassing Execute().
		/// </summary>
		private static (Page page, ColorizeCommand command) Setup(
			XElement pageElement, bool fontOverride = false)
		{
			var page = new Page(pageElement);
			return (page, new ColorizeCommand(page, fontOverride));
		}


		private static List<XElement> SelectedRuns(Page page)
		{
			return page.Root.Descendants(Ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();
		}


		[TestMethod]
		public void Colorize_SelectedRun_WrapsTextInSpansAndSetsLangAttribute()
		{
			var pageElement = new PageBuilder(PageId, "Colorize Test")
				.WithParagraph("var x = 1;", selected: true)
				.BuildElement();

			var (page, command) = Setup(pageElement);
			var runs = SelectedRuns(page);

			var updated = command.Colorize("csharp", runs);

			Assert.IsTrue(updated, "Expected Colorize to report the page as updated");

			var run = runs.Single();
			Assert.AreEqual("yo", run.Attribute("lang")?.Value);

			var cdata = run.GetCData();
			Assert.IsNotNull(cdata);
			StringAssert.Contains(cdata.Value, "<span", "Expected colorized text to be wrapped in span(s)");
		}


		[TestMethod]
		public void Colorize_TextWithSoftBreak_SplitsIntoRunoffParagraph()
		{
			var pageElement = new PageBuilder(PageId, "Soft Break Test")
				.WithParagraph("Hello<br>\nWorld", selected: true)
				.BuildElement();

			var (page, command) = Setup(pageElement);
			var runs = SelectedRuns(page);

			var updated = command.Colorize("csharp", runs);

			Assert.IsTrue(updated);

			// the soft break should expand into a second OE paragraph alongside the original
			var paragraphs = page.Root
				.Descendants(Ns + "OEChildren")
				.First()
				.Elements(Ns + "OE")
				.ToList();

			Assert.AreEqual(2, paragraphs.Count,
				"Expected the soft break to produce a runoff paragraph");

			var secondLine = paragraphs[1].Descendants(Ns + "T").First()
				.GetCData().Value.PlainText();

			Assert.AreEqual("World", secondLine);
		}


		[TestMethod]
		public void Colorize_DiffAddedLine_AppliesBackgroundStyle()
		{
			// omDepth=0 simulates what AddDepth() would normally stamp onto a top-level run
			// before Colorize() runs; that pass is part of Execute() and is bypassed here.
			var t = new XElement(Ns + "T",
				new XAttribute("selected", "all"),
				new XAttribute("omDepth", "0"),
				new XCData("+ added line"));

			var pageElement = new PageBuilder(PageId, "Diff Test")
				.WithElement(new XElement(Ns + "OE", t))
				.BuildElement();

			var (page, command) = Setup(pageElement);
			var runs = SelectedRuns(page);

			var updated = command.Colorize("csharp", runs);

			Assert.IsTrue(updated);

			var style = runs.Single().Attribute("style");
			Assert.IsNotNull(style, "Expected a diff background style to be applied");
			StringAssert.Contains(style.Value, "background:");
		}


		[TestMethod]
		public void Colorize_UnknownLanguage_Throws()
		{
			var pageElement = new PageBuilder(PageId, "Bad Language Test")
				.WithParagraph("text", selected: true)
				.BuildElement();

			var (page, command) = Setup(pageElement);
			var runs = SelectedRuns(page);

			Assert.ThrowsException<FileNotFoundException>(
				() => command.Colorize("not-a-real-language", runs));
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Language-specific coverage from the manual test protocol above. Each block is built
		// as one top-level, selected OE/T paragraph per source line -- the typical OneNote
		// representation of a pasted multi-line code block -- and run through Colorize(). The
		// expected colorized text per line is computed independently by driving the real
		// Colorizer engine (the same one Colorize() uses internally) over the same lines in the
		// same order, so multi-line comment continuation across paragraphs is exercised exactly
		// as production code exercises it, without re-implementing the tokenizer in the test.

		private static readonly string[] CSharpLines =
		{
			"/// <summary>",
			"/// Execute the command",
			"/// </summary>",
			"/// <param name=\"args\"></param>",
			"/// <returns></returns>",
			"public override void Execute(params object[] args)",
			"{",
			"    /*",
			"     * multi line using",
			"     * comments",
			"     */",
			"    using (var one = new OneNote())  // comment",
			"    {",
			"        // single line comment",
			"        new FootnoteEditor(one).AddFootnote(\"stri//ng\");",
			"+        var a = 1 + 34.34;",
			"    }",
			"}",
			"// end"
		};

		private static readonly string[] JsonLines =
		{
			"{",
			"  \"EventId\": \"02b29c6f2d3d44a4b93a2ce81a65f6b5\",",
			"  \"Timestamp\": \"2026-03-27T10:53:36.1054786Z\",",
			"  \"EventName\": \"InsertInfoBox\",",
			"  \"EventType\": \"event\",",
			"  \"Version\": \"6.8.2\",",
			"  \"SessionId\": \"4c2ee1f9bd65458d9ff42dc330488a20\",",
			"  \"Client\": {",
			"    \"OneVersion\": \"16.0.19725.20190\",",
			"    \"OneArc\": \"x64\",",
			"    \"MoreArc\": \"x64\",",
			"    \"OsMajor\": 10,",
			"    \"OsMinor\": 0,",
			"    \"OsBuild\": 26200,",
			"    \"OsEdition\": \"Professional\",",
			"    \"OsArc\": \"x64\",",
			"    \"Culture\": \"en-US\",",
			"    \"MoreCulture\": \"en-US\"",
			"  },",
			"  \"Data\": {",
			"    \"Message\": \"\",",
			"    \"Info\": \"\"",
			"  }",
			"}"
		};

		private static readonly string[] SqlLines =
		{
			"SELECT",
			"    GETDATE() AS [Today],",
			"    /* This is a multi-line comment that starts and ends in the same line */",
			"    (GETDATE() + 1) AS [Tomorrow],",
			"    CAST(1 as bit) AS [Boolean],",
			"    /*This is another multi-line comment that starts and ends in the same line */",
			"    CAST(1 as int) AS [Number]",
			"GO",
			"/*",
			"** SQL 2",
			"*/",
			"SELECT",
			"    GETDATE() AS [Today],",
			"    /* This is a multi-line comment that starts and ends in the same line */",
			"    (GETDATE() + 1) AS [Tomorrow],",
			"    CAST(1 as bit) AS [Boolean],",
			"    /*",
			"    This is a multi-line comment that starts and ends in different lines",
			"    */",
			"    CAST(1 as int) AS [Number]",
			"GO"
		};

		private static readonly string[] XmlLines =
		{
			"<one:Page>",
			"  <!-- comment -->",
			"  <one:QuickStyleDef index=\"0\" name=\"p\" fontColor=\"automatic\" highlightColor=\"automatic\" font=\"Calibri\" fontSize=\"11.0\" spaceBefore=\"0.0\" spaceAfter=\"0.0\" />",
			"  <one:PageSettings RTL=\"false\" color=\"#FFFFFF\">",
			"    <one:PageSize>",
			"      <one:Automatic />",
			"    </one:PageSize>",
			"    <one:RuleLines visible=\"false\" />",
			"  </one:PageSettings>",
			"  <one:Outline>",
			"    <one:Position x=\"36.0\" y=\"32.39999771118164\" z=\"0\" />",
			"    <one:Size width=\"162.1652679443359\" height=\"13.48873901367187\" />",
			"    <one:OEChildren>",
			"      <one:OE alignment=\"left\" quickStyleIndex=\"0\">",
			"        <one:T><![CDATA[Lorem ipsum dolor sit amet]]></one:T>",
			"      </one:OE>",
			"    </one:OEChildren>",
			"  </one:Outline>",
			"</one:Page>"
		};


		/// <summary>
		/// Builds a page with one top-level, selected OE/T paragraph per source line.
		/// Forces an explicit non-"automatic" white page color so theme selection inside
		/// Colorize() is deterministic (resolves to the "light" theme, automatic=false)
		/// regardless of the test machine's own Office theme setting.
		/// '&lt;' and '&gt;' are escaped the way OneNote stores literal angle brackets inside
		/// CDATA-based rich text -- GetWrapper() re-parses the CDATA as XML, so raw '&lt;'
		/// in source content would be misread as markup instead of literal text.
		/// </summary>
		private static (Page page, List<XElement> runs) BuildCodeBlock(string title, string[] lines)
		{
			var builder = new PageBuilder(PageId, title, pageColor: "#FFFFFF");

			foreach (var line in lines)
			{
				var encoded = line.Replace("<", "&lt;").Replace(">", "&gt;");
				var t = new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XAttribute("omDepth", "0"),
					new XCData(encoded));

				builder.WithElement(new XElement(Ns + "OE", t));
			}

			var page = new Page(builder.BuildElement());
			return (page, SelectedRuns(page));
		}


		/// <summary>
		/// Computes the expected colorized text for each line by driving the same Colorizer
		/// engine that ColorizeCommand.Colorize() uses internally, over the raw (unescaped)
		/// lines in the same order on a single shared instance -- so multi-line comment scope
		/// carries across lines/paragraphs exactly as it does in production.
		/// </summary>
		private static List<string> Highlight(string key, string[] lines)
		{
			var oracle = new Colorizer(key, "light", false);
			return lines.Select(oracle.ColorizeOne).ToList();
		}


		private static string GetThemeBackground(string styleName)
		{
			var path = Path.Combine(
				Colorizer.GetColorizerDirectory(), "Themes", "light-theme.json");

			var theme = Provider.LoadTheme(path, false);
			return theme.GetStyle(styleName).Background;
		}


		[TestMethod]
		public void Colorize_CSharpBlock_MatchesEngineOutputAndAppliesDiffStyle()
		{
			var (page, runs) = BuildCodeBlock("CSharp Test", CSharpLines);
			var command = new ColorizeCommand(page, false);
			var expected = Highlight("csharp", CSharpLines);

			var updated = command.Colorize("csharp", runs);

			Assert.IsTrue(updated);
			Assert.AreEqual(CSharpLines.Length, runs.Count);

			for (var i = 0; i < runs.Count; i++)
			{
				Assert.AreEqual("yo", runs[i].Attribute("lang")?.Value);
				Assert.AreEqual(expected[i], runs[i].GetCData().Value,
					$"Mismatch on C# line {i}: \"{CSharpLines[i]}\"");
			}

			// the "+        var a = 1 + 34.34;" line should pick up the diff-add background
			var diffIndex = Array.FindIndex(CSharpLines, l => l.StartsWith("+", StringComparison.Ordinal));
			Assert.IsTrue(diffIndex >= 0, "Test data should include a diff-marked line");

			var diffStyle = runs[diffIndex].Attribute("style");
			Assert.IsNotNull(diffStyle, "Expected the diff-marked line to receive a background style");
			StringAssert.Contains(diffStyle.Value, $"background:{GetThemeBackground("diffadd")}");
		}


		[TestMethod]
		public void Colorize_JsonBlock_MatchesEngineOutput()
		{
			var (page, runs) = BuildCodeBlock("Json Test", JsonLines);
			var command = new ColorizeCommand(page, false);
			var expected = Highlight("json", JsonLines);

			var updated = command.Colorize("json", runs);

			Assert.IsTrue(updated);
			Assert.AreEqual(JsonLines.Length, runs.Count);

			for (var i = 0; i < runs.Count; i++)
			{
				Assert.AreEqual("yo", runs[i].Attribute("lang")?.Value);
				Assert.AreEqual(expected[i], runs[i].GetCData().Value,
					$"Mismatch on JSON line {i}: \"{JsonLines[i]}\"");
			}
		}


		[TestMethod]
		public void Colorize_SqlBlock_MatchesEngineOutputAcrossMultiLineComments()
		{
			var (page, runs) = BuildCodeBlock("Sql Test", SqlLines);
			var command = new ColorizeCommand(page, false);
			var expected = Highlight("sql", SqlLines);

			var updated = command.Colorize("sql", runs);

			Assert.IsTrue(updated);
			Assert.AreEqual(SqlLines.Length, runs.Count);

			for (var i = 0; i < runs.Count; i++)
			{
				Assert.AreEqual("yo", runs[i].Attribute("lang")?.Value);
				Assert.AreEqual(expected[i], runs[i].GetCData().Value,
					$"Mismatch on SQL line {i}: \"{SqlLines[i]}\"");
			}
		}


		[TestMethod]
		public void Colorize_XmlBlock_MatchesEngineOutput()
		{
			var (page, runs) = BuildCodeBlock("Xml Test", XmlLines);
			var command = new ColorizeCommand(page, false);
			var expected = Highlight("xml", XmlLines);

			var updated = command.Colorize("xml", runs);

			Assert.IsTrue(updated);
			Assert.AreEqual(XmlLines.Length, runs.Count);

			for (var i = 0; i < runs.Count; i++)
			{
				Assert.AreEqual("yo", runs[i].Attribute("lang")?.Value);
				Assert.AreEqual(expected[i], runs[i].GetCData().Value,
					$"Mismatch on XML line {i}: \"{XmlLines[i]}\"");
			}
		}
	}
}
