//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig.Renderers;
	using Markdig.Renderers.Html;
	using System;
	using System.IO;


	/// <summary>
	/// From https://github.com/Temetra/MarkdigExtensions/tree/main under MIT license
	/// </summary>
	internal class WikilinkRenderer : HtmlObjectRenderer<WikilinkInline>
	{

		protected override void Write(HtmlRenderer renderer, WikilinkInline obj)
		{
			if (renderer.EnableHtmlForInline)
			{
				// Label.Text and Link.Text are null when CheckClosing never ran (unclosed [[)
				if (obj.Label.Text is null || obj.Link.Text is null || renderer.BaseUrl is null)
				{
					return;
				}

				var label = obj.Label.Text.Substring(obj.Label.Start, obj.Label.Length);
				// only strip known markdown file extensions, not all dots in the label
				var ext = Path.GetExtension(label);
				if (ext.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
					ext.Equals(".markdown", StringComparison.OrdinalIgnoreCase))
				{
					label = Path.Combine(
						Path.GetDirectoryName(label),
						Path.GetFileNameWithoutExtension(label));
				}

				if (obj.IsImage)
				{
					// convert to <img> ...

					// special case for Obsidian pasted images; presume this is correct!
					var link = obj.Link.Text.Substring(obj.Link.Start, obj.Link.Length);
					var filename = link;
					//var filename = link.StartsWith("Pasted image")
					//	? link.Replace("Pasted image ", string.Empty)
					//	: link;

					var uri = new Uri(Path.Combine(renderer.BaseUrl.AbsolutePath, filename));

					var src = uri.AbsoluteUri;

					if (src.Contains("%25"))
					{
						// double-encoding might encode "%20" to "%2520" (%25 is "%")
						src = Uri.UnescapeDataString(src);
					}

					renderer.Write($"<img src=\"{src}\" alt=\"{label}\" />");
				}
				else
				{
					// convert to <a> ...

					var link = obj.Link.Text.Substring(obj.Link.Start, obj.Link.Length);
					if (string.IsNullOrEmpty(link)) link = label;

					var href = new Uri(
						Path.Combine(renderer.BaseUrl.AbsolutePath, link))
						.AbsoluteUri;

					renderer.Write($"<a href=\"{href}\">{label}</a>"); // fix #7
				}
			}
		}
	}
}

