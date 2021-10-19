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
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;


	internal class MarkdownWriter
	{
		private class Context
		{
			public string Owner;
			public int QuickStyleIndex;
			public string Enclosure;
		}

		// no good way to indent text; closest alternative is to use a string of nbsp but that
		// conflicts with other directives like headings and list numbering. so substitute
		// indentations (OEChildren) with the blockquote directive instead
		private const string Indent = ">"; //&nbsp;&nbsp;&nbsp;&nbsp;";

		private readonly Page page;
		private readonly XNamespace ns;
		private readonly bool withAttachments;
		private readonly List<Style> quickStyles;
		private readonly Stack<Context> contexts;
		private int imageCounter;
#if LOG
		private readonly ILogger writer = Logger.Current;
#else
		private StreamWriter writer;
		private string path;
#endif


		public MarkdownWriter(Page page, bool withAttachments)
		{
			this.page = page;
			ns = page.Namespace;
			quickStyles = page.GetQuickStyles();
			contexts = new Stack<Context>();
			this.withAttachments = withAttachments;
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


		private void Write(XElement element,
			string prefix = "",
			bool startpara = false,
			bool contained = false)
		{
			bool pushed = false;
			bool dive = true;

			switch (element.Name.LocalName)
			{
				case "OEChildren":
					pushed = DetectQuickStyle(element);
					writer.WriteLine("  ");
					prefix = $"{Indent}{prefix}";
					break;

				case "OE":
					pushed = DetectQuickStyle(element);
					startpara = true;
					break;

				case "Tag":
					WriteTag(element);
					break;

				case "T":
					pushed = DetectQuickStyle(element);
					if (startpara) Stylize(prefix);
					WriteText(element.GetCData(), startpara);
					break;

				case "Bullet":
					writer.Write($"{prefix}- ");
					break;

				case "Number":
					writer.Write($"{prefix}1. ");
					break;

				case "Image":
					WriteImage(element);
					dive = false;
					break;

				case "InsertedFile":
					WriteFile(element);
					dive = false;
					break;

				case "Table":
					WriteTable(element);
					dive = false;
					break;
			}

			if (dive && element.HasElements)
			{
				foreach (var child in element.Elements())
				{
					Write(child, prefix, startpara);
					startpara = false;
				}
			}

			var context = pushed ? contexts.Pop() : null;
			if (element.Name.LocalName == "OE")
			{
				if (context != null && !string.IsNullOrEmpty(context.Enclosure))
				{
					writer.Write(context.Enclosure);
				}

				// if not in a table cell
				// or in a cell and this OE is followed by another OE
				if (!contained ||(element.NextNode != null))
				{
					writer.WriteLine("  ");
				}
			}
		}


		private bool DetectQuickStyle(XElement element)
		{
			if (element.GetAttributeValue("quickStyleIndex", out int index))
			{
				var context = new Context
				{
					Owner = element.Name.LocalName,
					QuickStyleIndex = index
				};
				var quick = quickStyles.First(q => q.Index == index);
				if (quick != null)
				{
					// cite becomes italic
					if (quick.Name == "cite") context.Enclosure = "*";
					else if (quick.Name == "code") context.Enclosure = "`";
				}
				contexts.Push(context);
				return true;
			}

			return false;
		}


		private void Stylize(string prefix)
		{
			writer.Write(prefix);
			if (contexts.Count == 0) return;
			var context = contexts.Peek();
			var quick = quickStyles.First(q => q.Index == context.QuickStyleIndex);
			switch (quick.Name)
			{
				case "PageTitle": writer.Write("# "); break;
				case "h1": writer.Write("# "); break;
				case "h2": writer.Write("## "); break;
				case "h3": writer.Write("### "); break;
				case "h4": writer.Write("#### "); break;
				case "h5": writer.Write("##### "); break;
				case "h6": writer.Write("###### "); break;
				case "blockquote": writer.Write("> "); break;
				// cite and code are both block-scope style, on the OE
				case "cite": writer.Write("*"); break;
				case "code": writer.Write("`"); break;
				//case "p": logger.Write(Environment.NewLine); break;
			}
		}


		private void WriteTag(XElement element)
		{
			var symbol = page.Root.Elements(ns + "TagDef")
				.Where(e => e.Attribute("index").Value == element.Attribute("index").Value)
				.Select(e => int.Parse(e.Attribute("symbol").Value))
				.FirstOrDefault();

			switch (symbol)
			{
				case 3:     // to do
				case 8:     // client request
				case 12:	// schedule/callback
				case 28:	// todo prio 1
				case 71:    // todo prio 2
				case 94:    // discuss person a/b
				case 95:    // discuss manager
					var check = element.Attribute("completed").Value == "true" ? "x" : " ";
					writer.Write($"- [{check}] ");
					break;

				case 6: writer.Write(":question: "); break;         // question
				case 13: writer.Write(":star: "); break;            // important
				case 17: writer.Write(":exclamation: "); break;     // critical
				case 18: writer.Write(":phone: "); break;           // phone
				case 21: writer.Write(":bulb: "); break;            // idea
				case 23: writer.Write(":house: "); break;           // address
				case 33: writer.Write(":three: "); break;           // three
				case 39: writer.Write(":zero: "); break;            // zero
				case 51: writer.Write(":two: "); break;				// two
				case 70: writer.Write(":one: "); break;				// one
				case 118: writer.Write(":mailbox: "); break;        // contact
				case 121: writer.Write(":musical_note: "); break;   // music to listen to
				case 131: writer.Write(":secret: "); break;			// password
				case 133: writer.Write(":movie_camera: "); break;   // movie to see
				case 132: writer.Write(":book: "); break;           // book to read
				case 140: writer.Write(":zap: "); break;			// lightning bolt
				default: writer.Write(":o: "); break;
			}
		}


		private void WriteText(XCData cdata, bool startParagraph)
		{
			cdata.Value = cdata.Value
				.Replace("<br>", "  ") // usually followed by NL so leave it there
				.Replace("[", "\\[")   // escape to prevent confusion with md links
				.TrimEnd();

			var wrapper = cdata.GetWrapper();
			foreach (var span in wrapper.Descendants("span").ToList())
			{
				var text = span.Value;
				var att = span.Attribute("style");
				// span might only have a lang attribute
				if (att != null)
				{
					var style = new Style(span.Attribute("style").Value);
					if (style.IsStrikethrough) text = $"~~{text}~~";
					if (style.IsItalic) text = $"*{text}*";
					if (style.IsBold) text = $"**{text}**";
				}
				span.ReplaceWith(new XText(text));
			}

			foreach (var anchor in wrapper.Elements("a"))
			{
				var href = anchor.Attribute("href")?.Value;
				if (!string.IsNullOrEmpty(href))
				{
					if (href.StartsWith("onenote:") || href.StartsWith("onemore:"))
					{
						// removes the hyperlink but preserves the text
						anchor.ReplaceWith(anchor.Value);
					}
					else
					{
						anchor.ReplaceWith(new XText($"[{anchor.Value}]({href})"));
					}
				}
			}

			// escape directives
			var raw = wrapper.GetInnerXml()
				.Replace("&lt;", "\\<")
				.Replace("|", "\\|");

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
					var prefix = page.Title.Replace(" ", string.Empty);
					var name = $"{prefix}_{++imageCounter}.png";
					var filename = Path.Combine(path, name);
#if !LOG
					image.Save(filename, ImageFormat.Png);
#endif

					writer.Write($"![Image-{imageCounter}]({name})");
				}
			}
		}


		private void WriteFile(XElement element)
		{
			// get and validate source
			var source = element.Attribute("pathSource")?.Value;
			if (string.IsNullOrEmpty(source) || !File.Exists(source))
			{
				source = element.Attribute("pathCache")?.Value;
				if (string.IsNullOrEmpty(source) || !File.Exists(source))
				{
					// broken link, remove marker
					return;
				}
			}

			// get preferredName; this will become the output file name
			var name = element.Attribute("preferredName")?.Value;
			if (string.IsNullOrEmpty(name))
			{
				// broken link, remove marker
				return;
			}

			if (withAttachments)
			{
				var target = Path.Combine(path, name);

				try
				{
#if !LOG
					// copy cached/source file to md output directory
					File.Copy(source, target, true);
#endif
				}
				catch
				{
					// error copying, drop marker
					return;
				}

				// this is a relative path that allows us to move the folder around
				var uri = new Uri(target).AbsoluteUri;
				writer.WriteLine($"[{name}]({uri})");
			}
			else
			{
				writer.WriteLine($"(*File:{name}*)");
			}
		}


		private void WriteTable(XElement element)
		{
			var table = new Table(element);

			// table needs a blank line before it
			writer.WriteLine();

			// header
			writer.Write("|");
			for (int i = 0; i < table.ColumnCount; i++)
			{
				writer.Write($" {TableCell.IndexToLetters(i + 1)} |");
			}
			writer.WriteLine();

			// separator
			writer.Write("|");
			for (int i = 0; i < table.ColumnCount; i++)
			{
				writer.Write(" :--- |");
			}
			writer.WriteLine();

			// data
			foreach (var row in table.Rows)
			{
				writer.Write("| ");
				foreach (var cell in row.Cells)
				{
					cell.Root
						.Element(ns + "OEChildren")
						.Elements(ns + "OE")
						.ForEach(e => Write(e, contained: true));

					writer.Write(" | ");
				}
				writer.WriteLine();
			}
		}
	}
}
