//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig;
	using Markdig.Renderers;
	using System;
	using System.IO;


	internal static class OneMoreDig
	{
		public static string ConvertMarkdownToHtml(string path, string markdown)
		{
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

			var pipeline = builder.Build();

			pipeline.Setup(renderer);

			var doc = Markdown.Parse(markdown, pipeline);

			renderer.Render(doc);
			writer.Flush();

			return writer.ToString();
		}
	}
}
