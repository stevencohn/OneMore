//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig;
	using Markdig.Renderers;
	using Markdig.Syntax;
	using System;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;


	internal static class OneMoreDig
	{
		// zero-width space; marks a paragraph that should collapse to a genuinely
		// empty OneNote OE in MarkdownConverter.RewriteBlankLines
		internal static readonly string BlankLineMarker = ((char)0x200B).ToString();


		public static string ConvertMarkdownToHtml(
			string path, string markdown, bool gfmLineBreaks = false,
			bool preserveBlankLines = false, bool blankBeforeHeadings = false)
		{
			markdown = IsolateCheckboxLines(markdown);

			using var writer = new StringWriter();
			var renderer = new HtmlRenderer(writer)
			{
				BaseUrl = new Uri($"{Path.GetDirectoryName(path)}/")
			};

			var builder = new MarkdownPipelineBuilder()
				.UseAdvancedExtensions()
				.UseOneMoreExtensions()
				.UseWikilinks();

			// Remove the TaskList extension so "- [ ]" and "- [x]" are rendered as plain
			// list items with literal "[ ]"/"[x]" text. If TaskList is active it converts
			// these to <input type="checkbox"> which OneNote strips, losing the checkbox
			// information before MarkdownConverter.RewriteTodo() can act on it.
			builder.Extensions.RemoveAll(
				e => e.GetType().Name == "TaskListExtension");

			if (gfmLineBreaks)
			{
				// GitHub-Flavored-Markdown style: a single newline is a hard line break
				// rather than CommonMark's default soft break (which OneNote collapses)
				builder.UseSoftlineBreakAsHardlineBreak();
			}

			var pipeline = builder.Build();

			pipeline.Setup(renderer);

			var doc = Markdown.Parse(markdown, pipeline);

			if (preserveBlankLines)
			{
				RenderPreservingBlankLines(doc, markdown, renderer, writer, blankBeforeHeadings);
			}
			else
			{
				renderer.Render(doc);
			}

			writer.Flush();

			return writer.ToString();
		}


		/// <summary>
		/// Renders each top-level block individually, inserting an invisible marker
		/// paragraph between adjacent Paragraph/Heading blocks whenever the source
		/// markdown had a real blank line between them, so that a genuine blank OneNote
		/// paragraph can be reconstructed later by MarkdownConverter.RewriteBlankLines.
		/// </summary>
		private static void RenderPreservingBlankLines(
			MarkdownDocument doc, string markdown, HtmlRenderer renderer,
			StringWriter writer, bool blankBeforeHeadings)
		{
			Block previous = null;

			foreach (var block in doc)
			{
				if (previous is not null &&
					IsSimpleBlock(previous) && IsSimpleBlock(block) &&
					!(blankBeforeHeadings && block is HeadingBlock) &&
					HasBlankLineBetween(markdown, previous, block))
				{
					writer.Write($"<p>{BlankLineMarker}</p>");
				}

				renderer.Render(block);
				previous = block;
			}
		}


		private static bool IsSimpleBlock(Block block)
		{
			return block is ParagraphBlock || block is HeadingBlock;
		}


		/// <summary>
		/// Determines whether the source markdown had at least one blank line between
		/// the given adjacent top-level blocks, based on their parsed source spans.
		/// </summary>
		private static bool HasBlankLineBetween(string markdown, Block previous, Block next)
		{
			var start = previous.Span.End + 1;
			var end = next.Span.Start;

			if (end <= start)
			{
				return false;
			}

			var gap = markdown.Substring(start, end - start);
			return gap.Count(c => c == '\n') >= 2;
		}


		/// <summary>
		/// Forces every "[ ]"/"[x]" checkbox-marker line into its own markdown paragraph
		/// block, even when adjacent lines have no blank line between them. Without this,
		/// CommonMark merges contiguous non-blank lines into a single paragraph, and
		/// MarkdownConverter.RewriteTodo can only ever recognize the first checkbox in
		/// that merged block.
		/// </summary>
		private static string IsolateCheckboxLines(string markdown)
		{
			var boxPattern = new Regex(@"^\\?\[(?:x|\s)\]");
			var lines = markdown.Replace("\r\n", "\n").Split('\n');
			var builder = new StringBuilder();

			for (var i = 0; i < lines.Length; i++)
			{
				if (i > 0 &&
					lines[i - 1].Length > 0 && lines[i].Length > 0 &&
					(boxPattern.IsMatch(lines[i - 1]) || boxPattern.IsMatch(lines[i])))
				{
					builder.AppendLine();
				}

				builder.AppendLine(lines[i]);
			}

			return builder.ToString();
		}
	}
}
