//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig;
	using Markdig.Renderers;
	using Markdig.Renderers.Html;
	using Markdig.Renderers.Html.Inlines;
	using System;


	internal class OneMoreDigExtension : IMarkdownExtension
	{
		public OneMoreDigExtension()
		{
		}


		public void Setup(MarkdownPipelineBuilder pipeline)
		{
			// Intentially empty
		}


		public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

			if (renderer is TextRendererBase<HtmlRenderer> htmlRenderer)
			{
				var defaultHR = htmlRenderer.ObjectRenderers.FindExact<HeadingRenderer>();
				if (defaultHR != null)
				{
					htmlRenderer.ObjectRenderers.Remove(defaultHR);
					htmlRenderer.ObjectRenderers.AddIfNotAlready(new OneMoreDigRenderer());
				}

				var defaultCR = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
				if (defaultCR != null)
				{
					htmlRenderer.ObjectRenderers.Remove(defaultCR);
					htmlRenderer.ObjectRenderers.AddIfNotAlready(new OneMoreDigCodeRenderer());
				}

				var defaultICR = htmlRenderer.ObjectRenderers.FindExact<CodeInlineRenderer>();
				if (defaultICR != null)
				{
					htmlRenderer.ObjectRenderers.Remove(defaultICR);
					htmlRenderer.ObjectRenderers.AddIfNotAlready(new OneMoreDigInlineCodeRenderer());
				}
			}
		}
	}
}
