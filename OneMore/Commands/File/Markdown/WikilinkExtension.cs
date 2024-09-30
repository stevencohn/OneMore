//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig;
	using Markdig.Parsers.Inlines;
	using Markdig.Renderers;


	/// <summary>
	/// From https://github.com/Temetra/MarkdigExtensions/tree/main under MIT license
	/// </summary>
	internal class WikilinkExtension : IMarkdownExtension
	{
		public void Setup(MarkdownPipelineBuilder pipeline)
		{
			pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new WikilinkParser());
		}

		public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
		{
			renderer.ObjectRenderers.Add(new WikilinkRenderer());
		}
	}


	internal static class WikilinkPipelineBuilder
	{
		public static MarkdownPipelineBuilder UseWikilinks(this MarkdownPipelineBuilder builder)
		{
			builder.Extensions.Add(new WikilinkExtension());
			return builder;
		}
	}
}


