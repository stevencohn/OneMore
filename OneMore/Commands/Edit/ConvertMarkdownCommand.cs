//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Convert the selected markdown text to OneNote native content.
	/// </summary>
	internal class ConvertMarkdownCommand : Command
	{
		public ConvertMarkdownCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);
			var range = new SelectionRange(page);
			var selectedRuns = range.GetSelections(true);

			var allContent =
				range.Scope == SelectionScope.TextCursor ||
				range.Scope == SelectionScope.SpecialCursor;

			var editor = new PageEditor(page) { AllContent = allContent };

			var outlines = allContent
				? page.BodyOutlines
				: selectedRuns.Select(e => e.FirstAncestor(ns + "Outline")).Distinct();

			// cache all OE objectIDs, compare against later, to only space out new OEs
			var paragraphIDs = outlines.Descendants(ns + "OE")
				.Select(e => e.Attribute("objectID").Value).ToList();

			// process each outline in sequence. By scoping to an outline, PageReader/Editor
			// can maintain positional context and scope updates to the outline

			foreach (var outline in outlines.ToList())
			{
				var content = editor.ExtractSelectedContent(outline);
				logger.Debug("outline - - - - - - - - - - - - - - - - - - - - - -");
				logger.Debug(content);
				logger.Debug("/outline");

				var paragraphs = content.Elements(ns + "OE").ToList();

				var reader = new PageReader(page)
				{
					// configure to read for markdown
					IndentationPrefix = "\n",
					Indentation = ">",
					ColumnDivider = "|",
					ParagraphDivider = "<br>",
					TableSides = "|"
				};

				var filepath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

				var text = reader.ReadTextFrom(paragraphs, allContent);
				text = Regex.Replace(text, @"<br>([\n\r]+)", "$1");
				text = Regex.Replace(text, @"\<*input\s+type*=*\""checkbox\""\s+unchecked\s+[a-zA-Z *]*\/\>", "[ ]");
				text = Regex.Replace(text, @"\<*input\s+type*=*\""checkbox\""\s+checked\s+[a-zA-Z *]*\/\>", "[x]");

				var body = OneMoreDig.ConvertMarkdownToHtml(filepath, text);

				editor.InsertAtAnchor(new XElement(ns + "HTMLBlock",
					new XElement(ns + "Data",
						new XCData($"<html><body>{body}</body></html>")
						)
					));
			}

			// update will remove unmodified omHash outlines from the in-memory Page
			await one.Update(page);

			// Pass 2, cleanup...

			// find and convert headers based on styles
			page = await one.GetPage(page.PageId, OneNote.PageDetail.Basic);

			// re-reference paragraphs by ID from newly loaded Page instance
			var touched = page.Root.Descendants(ns + "OE")
				.Where(e => !paragraphIDs.Contains(e.Attribute("objectID").Value))
				.ToList();

			if (touched.Any())
			{
				var converter = new MarkdownConverter(page);

				converter
					.RewriteHeadings(touched)
					.RewriteTodo(touched)
					.SpaceOutParagraphs(touched, 12);

				await one.Update(page);
			}
		}
	}
}
