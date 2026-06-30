//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig;
	using Markdig.Renderers;
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;


	internal static class OneMoreDig
	{
		public static string ConvertMarkdownToHtml(
			string path, string markdown, bool gfmLineBreaks = false)
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

			renderer.Render(doc);
			writer.Flush();

			return writer.ToString();
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
