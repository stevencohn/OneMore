//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.RegularExpressions;
	using Markdig;
	using Markdig.Extensions.Tables;
	using Markdig.Renderers;
	using Markdig.Syntax;


	internal static class OneMoreDig
	{
		// zero-width space; marks a paragraph that should collapse to a genuinely
		// empty OneNote OE in MarkdownConverter.RewriteBlankLines
		internal static readonly string BlankLineMarker = ((char)0x200B).ToString();


		public static string ConvertMarkdownToHtml(
			string path, string markdown, bool gfmLineBreaks = false,
			bool preserveBlankLines = false, bool blankBeforeHeadings = false)
		{
			markdown = IsolateBlockBoundaries(markdown, out var syntheticOffsets);

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
			// list items with literal "[ ]"/"[x]" text, which MarkdownConverter.RewriteTodo
			// detects and converts to a To Do tag. If TaskList is active it converts these
			// to <input type="checkbox"> which OneNote strips, losing the checkbox
			// information before RewriteTodo can act on it.
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
				RenderPreservingBlankLines(
					doc, markdown, renderer, writer, blankBeforeHeadings, syntheticOffsets);
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
		/// paragraph between adjacent Paragraph/Heading/List/Code/Table blocks whenever
		/// the source markdown had a real blank line between them, so that a genuine
		/// blank OneNote paragraph can be reconstructed later by
		/// MarkdownConverter.RewriteBlankLines.
		/// </summary>
		private static void RenderPreservingBlankLines(
			MarkdownDocument doc, string markdown, HtmlRenderer renderer,
			StringWriter writer, bool blankBeforeHeadings, HashSet<int> syntheticOffsets)
		{
			Block previous = null;

			foreach (var block in doc)
			{
				if (previous is not null &&
					IsSimpleBlock(previous) && IsSimpleBlock(block) &&
					!(blankBeforeHeadings && block is HeadingBlock) &&
					HasBlankLineBetween(markdown, previous, block, syntheticOffsets))
				{
					// the trailing newline matters: without it, OneNote's HTML importer
					// treats an immediately-following block (e.g. "<p>...</p><ul>") as
					// nested inside this marker paragraph rather than as its sibling
					writer.Write($"<p>{BlankLineMarker}</p>\n");
				}

				renderer.Render(block);
				previous = block;
			}
		}


		private static bool IsSimpleBlock(Block block)
		{
			return block is ParagraphBlock or HeadingBlock or ListBlock or CodeBlock or Table;
		}


		/// <summary>
		/// Determines whether the source markdown had at least one blank line between
		/// the given adjacent top-level blocks, based on their parsed source spans.
		/// Newlines at offsets recorded in syntheticOffsets were inserted by
		/// IsolateBlockBoundaries purely to keep parsing correct and do not count as a
		/// blank line the user actually typed.
		/// </summary>
		private static bool HasBlankLineBetween(
			string markdown, Block previous, Block next, HashSet<int> syntheticOffsets)
		{
			// most blocks (Paragraph, Heading, CodeBlock, Table) end their Span at
			// their last content character, but ListBlock ends its Span one character
			// later, at its own trailing line break; detect that case so the break
			// isn't skipped and undercounted below
			var end = next.Span.Start;
			var start = previous.Span.End >= 0 && markdown[previous.Span.End] == '\n'
				? previous.Span.End
				: previous.Span.End + 1;

			if (end <= start)
			{
				return false;
			}

			var count = 0;
			for (var i = start; i < end; i++)
			{
				if (markdown[i] == '\n' && !syntheticOffsets.Contains(i))
				{
					count++;
				}
			}

			return count >= 2;
		}


		private static readonly Regex ListItemPattern = new(@"^\s*([-*+]|\d+[.)])\s+\S");
		private static readonly Regex TableRowPattern = new(@"^\s*\|.*\|\s*$");
		private static readonly Regex BlockquotePattern = new(@"^\s*>");
		private static readonly Regex FootnoteDefinitionPattern = new(@"^\s*\[\^[^\]]+\]:");


		/// <summary>
		/// Ensures a bullet/numbered list, a pipe table, a blockquote, or a footnote
		/// definition is properly terminated by a blank line before any following
		/// non-blank line that isn't itself a continuation of that construct. Without
		/// this, CommonMark's lazy-continuation rule absorbs the following line(s) as
		/// literal text of the list's last item, the blockquote, the footnote, or runs
		/// them into the table, silently corrupting unrelated content that follows.
		/// Returns the offset, within the returned markdown, of each inserted blank
		/// line's own newline character, so HasBlankLineBetween can tell these synthetic
		/// separators apart from a blank line the user actually typed.
		/// </summary>
		private static string IsolateBlockBoundaries(string markdown, out HashSet<int> syntheticOffsets)
		{
			var lines = markdown.Replace("\r\n", "\n").Split('\n');
			var outputLines = new List<string>();
			var syntheticIndexes = new HashSet<int>();
			var inBlock = false;

			foreach (var line in lines)
			{
				var isListItem = ListItemPattern.IsMatch(line);
				var isTableRow = TableRowPattern.IsMatch(line);
				var isBlockquote = BlockquotePattern.IsMatch(line);
				var isFootnoteDefinition = FootnoteDefinitionPattern.IsMatch(line);
				var isBlank = line.Length == 0;

				if (inBlock && !isBlank && !isListItem && !isTableRow &&
					!isBlockquote && !isFootnoteDefinition)
				{
					syntheticIndexes.Add(outputLines.Count);
					outputLines.Add(string.Empty);
					inBlock = false;
				}
				else if (isBlank)
				{
					inBlock = false;
				}

				if (isListItem || isTableRow || isBlockquote || isFootnoteDefinition)
				{
					inBlock = true;
				}

				outputLines.Add(line);
			}

			syntheticOffsets = new HashSet<int>();
			var offset = 0;

			for (var i = 0; i < outputLines.Count; i++)
			{
				if (syntheticIndexes.Contains(i))
				{
					syntheticOffsets.Add(offset);
				}

				offset += outputLines[i].Length + 1;
			}

			return string.Join("\n", outputLines);
		}
	}
}
