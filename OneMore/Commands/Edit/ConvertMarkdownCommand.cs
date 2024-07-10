//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.IO;
	using System.Linq;
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
			page.GetTextCursor();

			var editor = new PageEditor(page)
			{
				AllContent = (page.SelectionScope != SelectionScope.Region)
			};

			var content = await editor.ExtractSelectedContent();
			var paragraphs = content.Elements(ns + "OE").ToList();

			var reader = new PageReader(page)
			{
				ColumnDivider = "|",
				TableSides = "|"
			};

			var filepath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			var text = reader.ReadTextFrom(paragraphs, page.SelectionScope != SelectionScope.Region);
			var body = OneMoreDig.ConvertMarkdownToHtml(filepath, text);

			editor.InsertAtAnchor(new XElement(ns + "HTMLBlock",
				new XElement(ns + "Data",
					new XCData($"<html><body>{body}</body></html>")
					)
				));


			System.Diagnostics.Debugger.Launch();

			// temporarily collect all outlines
			var outlineIDs = page.Root.Elements(ns + "Outline")
				.Select(e => e.Attribute("objectID").Value)
				.ToList();

			// update will remove unmodified omHash outlines
			await one.Update(page);

			// identify only remaining untouched outlines by exclusion
			var untouchedIDs = outlineIDs.Except(
				page.Root.Elements(ns + "Outline").Select(e => e.Attribute("objectID").Value))
				.ToList();

			if (untouchedIDs.Count < outlineIDs.Count)
			{
				// Pass 2, cleanup...

				// find and convert headers based on styles
				page = await one.GetPage(page.PageId, OneNote.PageDetail.Basic);

				var converter = new MarkdownConverter(page);

				// only clean up new (modified) outlines
				var touched = page.Root.Elements(ns + "Outline")
					.Where(e => !untouchedIDs.Contains(e.Attribute("objectID").Value));

				foreach (var outline in touched)
				{
					converter.RewriteHeadings(outline);
					converter.SpaceOutParagraphs(outline, 12);
				}

				await one.Update(page);
			}
		}
	}
}
