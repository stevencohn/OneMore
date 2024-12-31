//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Preview the selected markdown text in a separate window.
	/// </summary>
	internal class PreviewMarkdownCommand : Command
	{
		public PreviewMarkdownCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);
			var bounds = one.GetCurrentMainWindowBounds();

			var range = new SelectionRange(page);
			range.GetSelection();

			var editor = new PageEditor(page)
			{
				AllContent = (range.Scope != SelectionScope.Range)
			};

			var paragraphs = new List<XElement>();
			PageNamespace.Set(ns);

			// collect all outlines on page, in document-order
			foreach (var outline in page.BodyOutlines)
			{
				var content = editor.ExtractSelectedContent(outline);
				if (content != null)
				{
					if (paragraphs.Count >= 1)
					{
						// insert horizontal line in between outlines
						paragraphs.Add(new Paragraph(string.Empty));
						paragraphs.Add(new Paragraph("---"));
						paragraphs.Add(new Paragraph(string.Empty));
					}

					paragraphs.AddRange(content.Elements(ns + "OE"));
				}
			}

			var reader = new PageReader(page)
			{
				// configure to read for markdown
				IndentationPrefix = "\n",
				Indentation = ">",
				ColumnDivider = "|",
				ParagraphDivider = "<br>",
				TableSides = "|"
			};

			var text = reader.ReadTextFrom(paragraphs, range.Scope != SelectionScope.Range);
			logger.Verbose("preview raw text:");
			logger.Verbose(text);

			var title = editor.AllContent
				? $"<p style=\"font-family:Calibri;font-size:20pt\">{page.Title}</p>"
				: string.Empty;

			var filepath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var body = title + OneMoreDig.ConvertMarkdownToHtml(filepath, text);

			filepath = Path.Combine(
				Path.GetDirectoryName(filepath),
				Path.GetFileNameWithoutExtension(filepath)) + ".htm";

			logger.WriteLine($"markdown preview saved to {filepath}");
			File.WriteAllText(filepath, body);

			await SingleThreaded.Invoke(() =>
			{
				// WebView2 needs a message pump so host in its own invisible worker dialog
				using var form = new WebViewDialog(new System.Uri(filepath))
				{
					Width = bounds.Width / 2 + 100,
					Height = bounds.Height - 200,
					Left = bounds.Left + (bounds.Width / 2) - 80,
					Top = bounds.Top + 100,
					TopMost = true,

					Text = Resx.PreviewMarkdownCommand_Title
				};

				form.ShowDialog(owner);
			});
		}
	}
}
