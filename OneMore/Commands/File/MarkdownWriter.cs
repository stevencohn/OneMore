//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

#define LOGx

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;


	internal class MarkdownWriter
	{
		private readonly Page page;
		private readonly XNamespace ns;
		private readonly List<Style> quickStyles;
		private readonly Stack<int> qindexes;
		private int imageCounter;
#if LOG
		private readonly ILogger writer = Logger.Current;
#else
		private StreamWriter writer;
		private string path;
#endif


		public MarkdownWriter(Page page)
		{
			this.page = page;
			ns = page.Namespace;
			quickStyles = page.GetQuickStyles();
			qindexes = new Stack<int>();
		}


		public void Save(string filename)
		{
#if !LOG
			path = Path.GetDirectoryName(filename);
			using (writer = File.CreateText(filename))
#endif
			{
				writer.WriteLine($"# {page.Title}");

				page.Root.Elements(ns + "Outline")
					.Elements(ns + "OEChildren")
					.Elements()
					.ForEach(e => Write(e));

				writer.WriteLine();
			}
		}


		private void Write(XElement element, string prefix = "", bool startParagraph = false)
		{
			bool qpush = false;

			switch (element.Name.LocalName)
			{
				case "OEChildren":
					qpush = DetectQuickStyle(element);
					writer.WriteLine("  ");
					prefix = prefix.Length == 0 ? ">" : $">{prefix}";
					break;

				case "OE":
					qpush = DetectQuickStyle(element);
					startParagraph = true;
					break;

				case "T":
					qpush = DetectQuickStyle(element);
					if (startParagraph) Stylize(prefix);
					WriteText(element.GetCData(), startParagraph);
					break;

				case "Bullet":
					writer.Write($"{prefix}- ");
					break;

				case "Number":
					writer.Write($"{prefix}1. ");
					break;

				case "Image":
					WriteImage(element);
					break;
			}

			if (element.HasElements)
			{
				foreach (var child in element.Elements())
				{
					Write(child, prefix, startParagraph);
					startParagraph = false;
				}

				if (element.Name.LocalName == "OE")
				{
					writer.WriteLine("  ");
				}
			}

			if (qpush)
			{
				qindexes.Pop();
			}
		}

		private bool DetectQuickStyle(XElement element)
		{
			if (element.GetAttributeValue("quickStyleIndex", out int index))
			{
				qindexes.Push(index);
				return true;
			}

			return false;
		}

		private void Stylize(string prefix)
		{
			writer.Write(prefix);
			if (qindexes.Count == 0) return;
			var index = qindexes.Peek();
			var quick = quickStyles.First(q => q.Index == index);
			switch (quick.Name)
			{
				case "PageTitle": writer.Write("# "); break;
				case "h1": writer.Write("# "); break;
				case "h2": writer.Write("## "); break;
				case "h3": writer.Write("### "); break;
				case "h4": writer.Write("#### "); break;
				case "h5": writer.Write("##### "); break;
				case "h6": writer.Write("###### "); break;
				// cite and code are both block-scope style, on the OE
				case "cite": writer.Write("*"); break;
				case "code": writer.Write("`"); break;
				//case "p": logger.Write(Environment.NewLine); break;
			}
		}


		private void WriteText(XCData cdata, bool startParagraph)
		{
			cdata.Value = cdata.Value
				.Replace("<br>", "  ") // usually followed by NL so leave it there
				.Replace("[", "\\[")   // escape to prevent confusion with md links
				.Trim();

			var wrapper = cdata.GetWrapper();
			foreach (var span in wrapper.Descendants("span").ToList())
			{
				var sat = span.Attribute("style");
				var style = new Style(sat.Value);
				if (style.IsBold || style.IsItalic || style.IsStrikethrough)
				{
					var text = span.Value;
					if (style.IsStrikethrough) text = $"~~{text}~~";
					if (style.IsItalic) text = $"*{text}*";
					if (style.IsBold) text = $"**{text}**";
					span.ReplaceWith(new XText(text));
				}
				else
				{
					span.ReplaceWith(new XText(span.Value));
				}
			}

			foreach (var anchor in wrapper.Elements("a"))
			{
				var href = anchor.Attribute("href")?.Value;
				if (!string.IsNullOrEmpty(href))
				{
					anchor.ReplaceWith(new XText($"[{anchor.Value}]({href})"));
				}
			}

			var raw = wrapper.GetInnerXml()
				.Replace("&gt;", "\\>")
				.Replace("&lt;", "\\<");

			if (startParagraph && raw.Length > 0 && raw.StartsWith("#"))
			{
				writer.Write("\\");
			}

			writer.Write(raw);
		}


		private void WriteImage(XElement element)
		{
			var data = element.Element(ns + "Data");
			var binhex = Convert.FromBase64String(data.Value);
			using (var stream = new MemoryStream(binhex, 0, binhex.Length))
			{
				using (var image = Image.FromStream(stream))
				{
					var name = page.Title.Replace(" ", string.Empty);
					var filename = Path.Combine(path, $"{name}_{++imageCounter}.png");
#if !LOG
					image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
#endif

					writer.Write($"![Image-{imageCounter}]({filename})");
				}
			}
		}
	}
}
