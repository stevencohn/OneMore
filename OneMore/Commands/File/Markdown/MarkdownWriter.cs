//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

#define WriteToDisk

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class MarkdownWriter : Loggable
	{
		private sealed class Context
		{
			//// the container element
			//public string Container;
			//// true if at start of line
			//public bool StartOfLine;
			// index of quick style from container
			public int QuickStyleIndex;
			// accent enclosure char, asterisk* or backquote`
			public string Accent;
		}

		// Note that if pasting md text directly into OneNote, there is no goodway to indent text
		// and force OneNote from auto-formatting. Closest alternative is to use a string of nbsp
		// but that conflicts with other directives like headings and list numbering. On way is to
		// substitute indentations (e.g., OEChildren) with the blockquote directive instead.
		private const string Indent = "  "; //">"; //&nbsp;&nbsp;&nbsp;&nbsp;";
		private const string Quote = ">";

		private readonly Page page;
		private readonly XNamespace ns;
		private readonly List<Style> quickStyles;
		private readonly Stack<Context> contexts;
		private bool saveAttachments;
		private int imageCounter;
#if WriteToDisk
		private StreamWriter writer;
		private string attachmentPath;
		private string attachmentFolder;
#else
		private readonly ILogger writer = Logger.Current;
#endif


		public MarkdownWriter(Page page, bool saveAttachments)
		{
			this.page = page;
			ns = page.Namespace;

			quickStyles = page.GetQuickStyles();
			contexts = new Stack<Context>();

			this.saveAttachments = saveAttachments;
		}


		/// <summary>
		/// Copy the given content as markdown to the clipboard using the current
		/// page as a template for tag and style references.
		/// </summary>
		/// <param name="content"></param>
		public async Task Copy(XElement content)
		{
			using var stream = new MemoryStream();
			using (writer = new StreamWriter(stream))
			{
				await writer.WriteLineAsync($"# {page.Title}");

				if (content.Name.LocalName == "Page")
				{
					content.Elements(ns + "Outline")
						.Elements(ns + "OEChildren")
						.ForEach(e => Write(e));
				}
				else
				{
					Write(content);
				}

				await writer.WriteLineAsync();
				await writer.FlushAsync();

				stream.Position = 0;
				using var reader = new StreamReader(stream);
				var text = await reader.ReadToEndAsync();

				logger.Verbose("markdown - - - - - - - -");
				logger.Verbose(text);
				logger.Verbose("end markdown - - - - - -");

				var clippy = new ClipboardProvider();
				var success = await clippy.SetText(text, true);
				if (!success)
				{
					MoreMessageBox.ShowError(null, Resx.Clipboard_locked);
				}
			}
		}


		/// <summary>
		/// Save the page as markdown to the specified file.
		/// </summary>
		/// <param name="filename"></param>
		public void Save(string filename)
		{
#if WriteToDisk
			var path = Path.GetDirectoryName(filename);
			attachmentFolder = Path.GetFileNameWithoutExtension(filename);
			attachmentPath = Path.Combine(path, attachmentFolder);

			using (writer = File.CreateText(filename))
#endif
			{
				saveAttachments = true;

				writer.WriteLine($"# {page.Title}");

				page.Root.Elements(ns + "Outline")
					.Elements(ns + "OEChildren")
					.Elements()
					.ForEach(e => Write(e));

				// page level Images outside of any Outline
				page.Root.Elements(ns + "Image")
					.ForEach(e =>
					{
						Write(e);
						writer.WriteLine();
					});

				writer.WriteLine();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="container">typically an OEChildren with elements and OEChildren</param>
		/// <param name="prefix">prefix used to indent markdown lines</param>
		/// <param name="contained"></param>
		private void Write(XElement container,
			string prefix = "",
			int depth = 0,
			bool startOfLine = true,
			bool contained = false)
		{
			logger.Verbose($"Write({container.Name.LocalName})");

			foreach (var element in container.Elements())
			{
				var n = element.Name.LocalName;
				if (n == "T")
					logger.Verbose($"- [depth:{depth}, start:{startOfLine}] {prefix}element {n} [{element.Value}]");
				else
					logger.Verbose($"- [depth:{depth}, start:{startOfLine}] {prefix}element {n}");

				switch (element.Name.LocalName)
				{
					case "OEChildren":
						DetectQuickStyle(element);
						writer.WriteLine("  ");
						Write(element, $"{Indent}{prefix}", depth + 1, startOfLine);
						break;

					case "OE":
						{
							var pushed = DetectQuickStyle(element);
							Write(element, prefix, depth, startOfLine);

							var context = pushed ? contexts.Pop() : null;
							if (context is not null && !string.IsNullOrEmpty(context.Accent))
							{
								writer.Write(context.Accent);
							}

							// if not in a table cell
							// or in a cell and this OE is followed by another OE
							if (!contained || (element.NextNode is not null))
							{
								writer.WriteLine("  ");
							}
						}
						break;

					case "Tag":
						writer.Write(prefix);
						WriteTag(element);
						startOfLine = false;
						break;

					case "T":
						DetectQuickStyle(element);
						if (startOfLine && depth > 0)
						{
							Stylize(new String(Quote[0], depth));
							startOfLine = false;
						}
						WriteText(element.GetCData(), startOfLine);
						startOfLine = false;
						break;

					case "List":
						Write(element, prefix, depth, startOfLine);
						startOfLine = false;
						break;

					case "Bullet":
						writer.Write($"{prefix}- ");
						break;

					case "Number":
						writer.Write($"{prefix}1. ");
						break;

					case "Image":
						if (depth > 0)
						{
							writer.Write(new String(Quote[0], depth));
						}
						WriteImage(element);
						break;

					case "InkDrawing":
					case "InsertedFile":
					case "MediaFile":
						WriteFile(element);
						break;

					case "Table":
						if (depth > 0)
						{
							writer.Write(new String(Quote[0], depth));
						}
						WriteTable(element);
						break;
				}
			}

			logger.Verbose("out");
		}


		private bool DetectQuickStyle(XElement element)
		{
			if (element.GetAttributeValue("quickStyleIndex", out int index))
			{
				var context = new Context
				{
					QuickStyleIndex = index
				};
				var quick = quickStyles.First(q => q.Index == index);
				if (quick != null)
				{
					// cite becomes italic
					if (quick.Name == "cite") context.Accent = "*";
					else if (quick.Name == "code") context.Accent = "`";
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
				case "PageTitle":
				case "h1":
					writer.Write("# ");
					break;

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
				case 12:    // schedule/callback
				case 28:    // to do prio 1
				case 71:    // to do prio 2
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
				case 51: writer.Write(":two: "); break;             // two
				case 70: writer.Write(":one: "); break;             // one
				case 118: writer.Write(":mailbox: "); break;        // contact
				case 121: writer.Write(":musical_note: "); break;   // music to listen to
				case 131: writer.Write(":secret: "); break;         // password
				case 133: writer.Write(":movie_camera: "); break;   // movie to see
				case 132: writer.Write(":book: "); break;           // book to read
				case 140: writer.Write(":zap: "); break;            // lightning bolt
				default: writer.Write(":o: "); break;
			}
		}


		private void WriteText(XCData cdata, bool startOfLine)
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

			if (startOfLine && raw.Length > 0 && raw.StartsWith("#"))
			{
				writer.Write("\\");
			}

			logger.Verbose($"text [{raw}]");
			writer.Write(raw);
		}


		private void WriteImage(XElement element)
		{
			if (saveAttachments)
			{
				var data = element.Element(ns + "Data");
				var binhex = Convert.FromBase64String(data.Value);

				using var stream = new MemoryStream(binhex, 0, binhex.Length);
				using var image = Image.FromStream(stream);

				var name = $"{attachmentFolder}_{++imageCounter}.png";
				var filename = Path.Combine(attachmentPath, name);
#if WriteToDisk
				if (!Directory.Exists(attachmentPath))
				{
					Directory.CreateDirectory(attachmentPath);
				}

				image.Save(filename, ImageFormat.Png);
#endif
				var imgPath = Path.Combine(attachmentFolder, name);
				writer.Write($"![Image-{imageCounter}]({imgPath})");
			}
			else
			{
				writer.Write($"(*Image:{++imageCounter}.png*)");
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

			if (saveAttachments)
			{
				var target = Path.Combine(attachmentPath, name);

				try
				{
#if WriteToDisk
					if (!Directory.Exists(attachmentPath))
					{
						Directory.CreateDirectory(attachmentPath);
					}

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
