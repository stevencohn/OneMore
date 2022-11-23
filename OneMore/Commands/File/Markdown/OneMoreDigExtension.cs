//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig;
	using Markdig.Renderers;
	using Markdig.Renderers.Html;
	using System;


	internal class OneMoreDigExtension : IMarkdownExtension
	{
		public OneMoreDigExtension()
		{
		}


		public void Setup(MarkdownPipelineBuilder pipeline) { }


		public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

			if (renderer is TextRendererBase<HtmlRenderer> htmlRenderer)
			{
				var headingRenderer = htmlRenderer.ObjectRenderers.FindExact<HeadingRenderer>();
				if (headingRenderer != null)
				{
					htmlRenderer.ObjectRenderers.Remove(headingRenderer);

					htmlRenderer.ObjectRenderers.AddIfNotAlready(
						new OneMoreDigRenderer(headingRenderer));
				}
			}
		}
	}
}
