//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig.Renderers;
	using Markdig.Renderers.Html;
	using Markdig.Syntax;


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


		public OneMoreDigRenderer()
		{
		}


		/// <summary>
		/// Rewrites headings as stylized paragraphs that we can then recognize and
		/// transform into OneNote headings
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="obj"></param>
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
