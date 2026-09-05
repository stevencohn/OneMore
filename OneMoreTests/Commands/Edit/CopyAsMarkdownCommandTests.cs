//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Edit
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol
	 * Commands/Edit/CopyAsMarkdownCommand
	 * Copy the page or selected content as markdown into the system clipboard
	 *
	 *  1. Edit/Copy as Markdown
	 *  2. Paste into an external editor or append to the bottom of this page to see the markdown text
	 *
	 * Sample page content:
	 *   Heading One
	 *   Lorem ipsum dolor sit amet, ...
	 *   This paragraph contains an inline code run.
	 *   /// <summary>
	 *   /// Execute the command
	 *   /// </summary>
	 *   /// <param name="args"></param>
	 *   /// <returns></returns>
	 *   public override void Execute(params object[] args)
	 *   {
	 *       /*
	 *        * multi line using
	 *        * comments
	 *        *\/
	 *       using (var one = new OneNote())  // comment
	 *       {
	 *           new FootnoteEditor(one).AddFootnote("stri//ng");
	 *       }
	 *   }
	 *   Bottom
	 */

	// CopyAsMarkdownCommand.Execute() unconditionally writes its result to the real
	// Windows clipboard via ClipboardProvider, which has no test seam and would mutate
	// shared machine state on every test run. MarkdownWriter was refactored to split the
	// clipboard write (Copy) from markdown generation (Render), so these tests exercise
	// Render directly with hand-built page content rather than going through Execute().

	[TestClass]
	public class CopyAsMarkdownCommandTests : TestBase
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		private const int H1Index = 1;
		private const int CodeIndex = 2;


		private static XElement BuildQuickStyleDef(int index, string name)
		{
			return new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", index.ToString()),
				new XAttribute("name", name),
				new XAttribute("fontSize", "11.0"));
		}


		private static XElement BuildCodeLine(string text)
		{
			return new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", CodeIndex.ToString()),
				new XElement(Ns + "T", new XCData(text)));
		}


		// Builds the full sample page from the manual test protocol: an h1 heading, a plain
		// paragraph, a paragraph with an inline monospace (code) span, a run of consecutive
		// code-styled paragraphs (merged into one fenced block), and a trailing paragraph.
		private static Page BuildSamplePage()
		{
			var pageElement = new PageBuilder("page-1", "Copy As Markdown Test")
				.WithElement(new XElement(Ns + "OE",
					new XAttribute("quickStyleIndex", H1Index.ToString()),
					new XElement(Ns + "T", new XCData("Heading One"))))
				.WithParagraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit.")
				.WithElement(new XElement(Ns + "OE",
					new XElement(Ns + "T", new XCData(
						"This paragraph contains an inline " +
						"<span style=\"font-family:Consolas\">code</span> run."))))
				.WithElement(BuildCodeLine("/// &lt;summary&gt;"))
				.WithElement(BuildCodeLine("/// Execute the command"))
				.WithElement(BuildCodeLine("/// &lt;/summary&gt;"))
				.WithElement(BuildCodeLine("public override void Execute(params object[] args)"))
				.WithElement(BuildCodeLine("{"))
				.WithElement(BuildCodeLine("    /*"))
				.WithElement(BuildCodeLine("     * multi line using"))
				.WithElement(BuildCodeLine("     * comments"))
				.WithElement(BuildCodeLine("     */"))
				.WithElement(BuildCodeLine(
					"    using (var one = new OneNote())  // comment"))
				.WithElement(BuildCodeLine("    {"))
				.WithElement(BuildCodeLine(
					"        new FootnoteEditor(one).AddFootnote(\"stri//ng\");"))
				.WithElement(BuildCodeLine("    }"))
				.WithElement(BuildCodeLine("}"))
				.WithParagraph("Bottom")
				.BuildElement();

			pageElement.AddFirst(BuildQuickStyleDef(H1Index, "h1"));
			pageElement.AddFirst(BuildQuickStyleDef(CodeIndex, "code"));

			// round-trip through ToString()/Parse() so the page matches how a live
			// OneNote page is actually loaded, same as other MarkdownWriter-adjacent tests
			return new Page(XElement.Parse(
				pageElement.ToString(SaveOptions.OmitDuplicateNamespaces)));
		}


		private static XElement GetBodyContent(Page page)
		{
			return page.Root.Element(Ns + "Outline").Element(Ns + "OEChildren");
		}


		[TestMethod]
		public async Task Render_FullPage_IncludesTitleHeadingParagraphsAndFencedCodeBlock()
		{
			var page = BuildSamplePage();
			var content = GetBodyContent(page);

			var markdown = await new MarkdownWriter(page, false).Render(content, includeTitle: true);

			// title is prepended as an H1 heading
			StringAssert.StartsWith(markdown, "# Copy As Markdown Test");

			// h1-styled paragraph is rendered as a markdown heading
			StringAssert.Contains(markdown, "# Heading One");

			// plain paragraph text passes through unchanged
			StringAssert.Contains(markdown, "Lorem ipsum dolor sit amet");

			// inline monospace span becomes backtick-wrapped inline code
			StringAssert.Contains(markdown, "`code`");

			// consecutive code-styled paragraphs are merged into a single fenced block
			StringAssert.Contains(markdown, "```");
			StringAssert.Contains(markdown, "public override void Execute(params object[] args)");

			// doc-comment angle brackets are restored as literal text inside the fence,
			// not swallowed as if they were real XML/HTML tags
			StringAssert.Contains(markdown, "/// <summary>");
			StringAssert.Contains(markdown, "/// </summary>");

			// block-comment asterisks and the embedded "//" inside a string literal are
			// preserved verbatim -- the fenced code path must not apply WriteText's
			// markdown escaping (which would otherwise turn "*" into "\*")
			StringAssert.Contains(markdown, "* multi line using");
			Assert.IsFalse(markdown.Contains("\\* multi line using"),
				"Asterisks inside a fenced code block must not be markdown-escaped");
			StringAssert.Contains(markdown, "\"stri//ng\"");

			// trailing paragraph appears after the code block
			StringAssert.Contains(markdown, "Bottom");

			// rough ordering check: heading, then lorem, then inline-code paragraph,
			// then the fenced block, then Bottom
			var headingPos = markdown.IndexOf("Heading One");
			var loremPos = markdown.IndexOf("Lorem ipsum");
			var inlinePos = markdown.IndexOf("`code`");
			var fencePos = markdown.IndexOf("```");
			var bottomPos = markdown.LastIndexOf("Bottom");

			Assert.IsTrue(headingPos < loremPos, "Heading should precede the lorem paragraph");
			Assert.IsTrue(loremPos < inlinePos, "Lorem paragraph should precede the inline-code paragraph");
			Assert.IsTrue(inlinePos < fencePos, "Inline-code paragraph should precede the fenced block");
			Assert.IsTrue(fencePos < bottomPos, "Fenced block should precede the Bottom paragraph");
		}


		[TestMethod]
		public async Task Render_ConsecutiveCodeStyleParagraphs_MergeIntoSingleFencedBlock()
		{
			var pageElement = new PageBuilder("page-2", "Fence Merge Test")
				.WithElement(BuildCodeLine("line one"))
				.WithElement(BuildCodeLine("line two"))
				.WithElement(BuildCodeLine("line three"))
				.BuildElement();

			pageElement.AddFirst(BuildQuickStyleDef(CodeIndex, "code"));

			var page = new Page(XElement.Parse(
				pageElement.ToString(SaveOptions.OmitDuplicateNamespaces)));

			var markdown = await new MarkdownWriter(page, false)
				.Render(GetBodyContent(page), includeTitle: false);

			var fenceCount = markdown.Split(new[] { "```" }, System.StringSplitOptions.None).Length - 1;
			Assert.AreEqual(2, fenceCount,
				"Three consecutive code-styled paragraphs should produce one opening and one " +
				"closing fence, not three separate fenced blocks");

			StringAssert.Contains(markdown, "line one");
			StringAssert.Contains(markdown, "line two");
			StringAssert.Contains(markdown, "line three");
		}


		[TestMethod]
		public async Task Render_IncludeTitleFalse_OmitsPageTitleHeading()
		{
			var pageElement = new PageBuilder("page-3", "Should Not Appear").BuildElement();
			var page = new Page(XElement.Parse(
				pageElement.ToString(SaveOptions.OmitDuplicateNamespaces)));

			var oe = new XElement(Ns + "OE", new XElement(Ns + "T", new XCData("Selected run only")));
			var content = new XElement(Ns + "OEChildren", oe);

			var markdown = await new MarkdownWriter(page, false).Render(content, includeTitle: false);

			Assert.IsFalse(markdown.Contains("Should Not Appear"),
				"Page title must not appear when includeTitle is false");
			StringAssert.Contains(markdown, "Selected run only");
		}


		[TestMethod]
		public async Task Render_PlainParagraphWithMarkdownSignificantCharacters_EscapesThem()
		{
			var pageElement = new PageBuilder("page-4", "Escaping Test")
				.WithParagraph("Use * or _ for emphasis and | for a table cell")
				.BuildElement();

			var page = new Page(XElement.Parse(
				pageElement.ToString(SaveOptions.OmitDuplicateNamespaces)));

			var markdown = await new MarkdownWriter(page, false)
				.Render(GetBodyContent(page), includeTitle: false);

			// unlike text inside a fenced code block, plain paragraph text must have
			// markdown-significant characters escaped
			StringAssert.Contains(markdown, "\\* or \\_ for emphasis and \\| for a table cell");
		}
	}
}
