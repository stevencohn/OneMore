//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3002 // Return type is not CLS-compliant
#pragma warning disable CS3009 // Base type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using Markdig;
	using Markdig.Renderers;
	using Markdig.Renderers.Html;
	using Markdig.Syntax;
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


	internal static class OneMoreDigExtensions
	{
		public static MarkdownPipelineBuilder UseOneMoreExtensions(
			this MarkdownPipelineBuilder pipeline)
		{
			pipeline.Extensions.Add(new OneMoreDigExtension());
			return pipeline;
		}
	}


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


	internal class OneMoreDigRenderer : HtmlObjectRenderer<HeadingBlock>
	{
		private static readonly string[] styles = {
			"font-family:Calibri;font-size:16pt;color:#1e4e79",
			"font-family:Calibri;font-size:14pt;color:#2e75b5",
			"font-family:Calibri;font-size:12pt;color:#5b9bd5",
			"font-family:Calibri;font-size:12pt;color:#5b9bd5;font-style:italic",
			"font-family:Calibri;font-size:11pt;color:#2e75b5",
			"font-family:Calibri;font-size:11pt;color:#2e75b5;font-style:italic"
		};


		public OneMoreDigRenderer(HeadingRenderer headingRenderer)
		{
		}


		protected override void Write(HtmlRenderer renderer, HeadingBlock obj)
		{
			var attributes = obj.TryGetAttributes() ?? new HtmlAttributes();

			int index = obj.Level - 1;
			var style = (index >= 0) && (index < styles.Length) ? styles[index] : styles[0];
			attributes.AddProperty("style", style);

			if (renderer.EnableHtmlForBlock)
			{
				renderer.Write("<p");
				renderer.WriteAttributes(obj);
				renderer.Write('>');
			}

			renderer.WriteLeafInline(obj);

			if (renderer.EnableHtmlForBlock)
			{
				renderer.Write("</p>");
			}

			renderer.EnsureLine();

		}
	}
}
