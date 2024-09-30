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
				var label = obj.Label.ToString();
				label = Path.Combine(
					Path.GetDirectoryName(label),
					Path.GetFileNameWithoutExtension(label));

				if (obj.IsImage)
				{
					// convert to <img> ...

					// special case for Obsidian pasted images; presume this is correct!
					var link = obj.Link.ToString();
					var filename = link;
					//var filename = link.StartsWith("Pasted image")
					//	? link.Replace("Pasted image ", string.Empty)
					//	: link;

					var uri = new Uri(Path.Combine(renderer.BaseUrl.AbsolutePath, filename));
					var src = uri.AbsoluteUri;

					renderer.Write($"<img src=\"{src}\" />");
					Logger.Current.WriteLine($"wrender<img> src=[{src}] alt=[{label}]");
				}
				else
				{
					// convert to <a> ...

					var href = new Uri(
						Path.Combine(renderer.BaseUrl.AbsolutePath, obj.Link.Text))
						.AbsoluteUri;

					renderer.Write($"<a href=\"{href}\">{label}</a>");
					Logger.Current.WriteLine($"wrender<a> href=[{href}] alt=[{label}]");
				}
			}
		}
	}
}

