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
	using System.Xml.Linq;


	internal class MarkdownWriter
	{
		private class Context
		{
			public string Owner;
			public int QuickStyleIndex;
			public string Enclosure;
		}


		private readonly Page page;
		private readonly XNamespace ns;
		private readonly List<Style> quickStyles;
		private readonly Stack<Context> contexts;
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
			contexts = new Stack<Context>();
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
					prefix = prefix.Length == 0 ? ">" : $">{prefix}";
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

				if (!contained)
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
				case 133: writer.Write(":movie_camera: "); break;	// movie to see
				case 132: writer.Write(":book: "); break;			// book to read
				default: writer.Write(":red_circle: "); break;
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
				var style = new Style(span.Attribute("style").Value);
				var text = span.Value;
				if (style.IsStrikethrough) text = $"~~{text}~~";
				if (style.IsItalic) text = $"*{text}*";
				if (style.IsBold) text = $"**{text}**";
				span.ReplaceWith(new XText(text));
			}

			foreach (var anchor in wrapper.Elements("a"))
			{
				var href = anchor.Attribute("href")?.Value;
				if (!string.IsNullOrEmpty(href))
				{
					anchor.ReplaceWith(new XText($"[{anchor.Value}]({href})"));
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
					var name = page.Title.Replace(" ", string.Empty);
					var filename = Path.Combine(path, $"{name}_{++imageCounter}.png");
#if !LOG
					image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
#endif

					writer.Write($"![Image-{imageCounter}]({filename})");
				}
			}
		}


		private void WriteTable(XElement element)
		{
			var table = new Table(element);

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
