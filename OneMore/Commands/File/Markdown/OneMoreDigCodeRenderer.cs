//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig.Renderers;
	using Markdig.Renderers.Html;
	using Markdig.Syntax;
	using Markdig.Syntax.Inlines;


	/// <summary>
	/// Renders a fenced or indented code block as individual styled paragraphs so that
	/// OneNote's HTML importer preserves the font-family hint and RewriteCode can later
	/// promote each line to the "Code" quickstyle.
	/// </summary>
	internal class OneMoreDigCodeRenderer : HtmlObjectRenderer<CodeBlock>
	{
		internal const string CodeStyle = "font-family:Consolas";


		protected override void Write(HtmlRenderer renderer, CodeBlock obj)
		{
			if (!renderer.EnableHtmlForBlock)
			{
				renderer.WriteLeafRawLines(obj, true, false);
				return;
			}

			var lines = obj.Lines;
			for (int i = 0; i < lines.Count; i++)
			{
				renderer.Write("<p style=\"");
				renderer.Write(CodeStyle);
				renderer.Write("\">");
				renderer.WriteEscape(lines.Lines[i].Slice.ToString());
				renderer.WriteLine("</p>");
			}
		}
	}


	/// <summary>
	/// Renders an inline code span as a Consolas-styled span so that OneNote's HTML
	/// importer preserves the font hint in the text run's CDATA.
	/// </summary>
	internal class OneMoreDigInlineCodeRenderer : HtmlObjectRenderer<CodeInline>
	{
		protected override void Write(HtmlRenderer renderer, CodeInline obj)
		{
			if (renderer.EnableHtmlForInline)
			{
				renderer.Write("<span style=\"");
				renderer.Write(OneMoreDigCodeRenderer.CodeStyle);
				renderer.Write("\">");
			}

			renderer.WriteEscape(obj.Content.ToString());

			if (renderer.EnableHtmlForInline)
			{
				renderer.Write("</span>");
			}
		}
	}
}
