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
				BaseUrl = new Uri(Path.GetDirectoryName(path) + "/")
			};

			var pipeline = new MarkdownPipelineBuilder()
				.UseAdvancedExtensions()
				.UseOneMoreExtensions()
				.Build();

			pipeline.Setup(renderer);

			var doc = Markdown.Parse(markdown, pipeline);

			renderer.Render(doc);
			writer.Flush();

			return writer.ToString();
		}
	}
}
