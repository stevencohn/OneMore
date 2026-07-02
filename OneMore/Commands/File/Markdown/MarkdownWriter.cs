//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

// mask this definition to debug raw markdown processing to ILogger instead of a file/folder
#define WriteToDisk

// unmask this definition to allow debug logging
//#define DBGLOG

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

		// Note that if pasting md text directly into OneNote, there's no good way to indent text
		// and prevent OneNote from auto-formatting. Closest alt is to use a string of nbsp's
		// but that conflicts with other directives like headings and list numbering. One way is
		// to substitute indentations (e.g., OEChildren) with the blockquote directive instead.
		private const string Indent = "    "; //">"; //&nbsp;&nbsp;&nbsp;&nbsp;";
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
		/// <param name="includeTitle">True to prepend the page title as an H1 heading</param>
		public async Task Copy(XElement content, bool includeTitle = true)
		{
			using var stream = new MemoryStream();
			using (writer = new StreamWriter(stream))
			{
				if (includeTitle)
				{
					await writer.WriteLineAsync($"# {page.Title}");
				}

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

#if DBGLOG
				logger.Debug("markdown - - - - - - - -");
				logger.Debug(text);
				logger.Debug("end markdown - - - - - -");
#endif

				var clippy = new ClipboardProvider();
				var success = await clippy.SetText(text, true);
				if (!success)
				{
					MoreMessageBox.ShowError(null, Resx.Clipboard_locked);
				}

#if DBGLOG
				logger.Debug("copied");
#endif
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

				if (page.Title?.Length > 0)
				{
					writer.WriteLine($"# {page.Title}");
				}

				page.Root.Elements(ns + "Outline")
					.Elements(ns + "OEChildren")
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
			bool contained = false)
		{
			// Lines start at the beginning of each paragraph/OE which contains a flat list of
			// Tag, List, and T, so startOfLine can be handled locally rather than recursively.
			var startOfLine = true;

#if DBGLOG
			logger.Debug($"Write({container.Name.LocalName}, prefix:[{prefix}], depth:{depth}, contained:{contained})");
#endif
			// For OE containers: ensure the List element (bullet/number marker) is processed
			// before any Tag elements so output order is "- [x] text" not "[x] - text"
			IEnumerable<XElement> children = container.Elements();
			if (container.Name.LocalName == "OE"
				&& container.Elements(ns + "List").Any()
				&& container.Elements(ns + "Tag").Any())
			{
				var listElem = container.Element(ns + "List");
				children = children
					.Where(e => !ReferenceEquals(e, listElem))
					.Prepend(listElem);
			}

			var elements = children.ToList();
			for (int ei = 0; ei < elements.Count; ei++)
			{
				var element = elements[ei];
				var n = element.Name.LocalName;
#if DBGLOG
				var m = $"- [prefix:[{prefix}] depth:{depth} start:{startOfLine} contained:{contained} element {n}";
				logger.Debug(n == "T" ? $"{m} [{element.Value}]" : m);
#endif
				switch (n)
				{
					case "OEChildren":
						Write(element, $"{Indent}{prefix}", depth + 1, contained);
						break;

					case "OE":
						{
							// Detect a run of consecutive code-block OEs and emit as a fenced block
							if (IsCodeBlockOE(element))
							{
								var codeLines = new List<XElement> { element };
								while (ei + 1 < elements.Count
									&& elements[ei + 1].Name.LocalName == "OE"
									&& IsCodeBlockOE(elements[ei + 1]))
								{
									ei++;
									codeLines.Add(elements[ei]);
								}

								if (!contained) writer.WriteLine();
								writer.WriteLine("```");
								foreach (var codeLine in codeLines)
								{
									writer.WriteLine(GetCodeText(codeLine));
								}
								writer.WriteLine("```");
								break;
							}

							if (!contained) // not in table cell
							{
								writer.WriteLine("  ");
							}

							var context = DetectQuickStyle(element);
							Write(element, prefix, depth, contained);

							if (context is not null)
							{
								if (!string.IsNullOrEmpty(context.Accent))
								{
									// close the accent
									writer.Write(context.Accent);
								}

								contexts.Pop();
							}
						}
						break;

					case "Tag":
						{
							var context = DetectQuickStyle(element);
							// Only write line prefix at start of line; when a List element has already
							// been processed first (Tag+List reordering above), the list marker wrote
							// the prefix so we must not write it again.
							if (startOfLine)
							{
								if (context is not null)
								{
									Stylize(depth > 0 ? prefix : string.Empty);
								}
								else
								{
									writer.Write(prefix);
								}
							}

							WriteTag(element);

							if (context is not null)
							{
								contexts.Pop();
							}

							startOfLine = false;
						}
						break;

					case "T":
						{
							var context = DetectQuickStyle(element);
							if (context is not null)
							{
								Stylize(depth > 0 && startOfLine
									? prefix
									: string.Empty);

								startOfLine = false;
							}

							WriteText(element.GetCData(), startOfLine);

							if (context is not null)
							{
								if (!string.IsNullOrEmpty(context.Accent))
								{
									// close the accent
									writer.Write(context.Accent);
								}

								contexts.Pop();
							}

							startOfLine = false;
						}
						break;

					case "List":
						Write(element, prefix, depth, contained);
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

#if DBGLOG
			logger.Debug("out");
#endif
		}


		private bool IsCodeBlockOE(XElement oe)
		{
			// Check the OE itself then its parent (OEChildren) for a code quickstyle.
			// Only OE-level code style indicates a standalone code block; a code style
			// on an inner T is inline code handled by DetectQuickStyle in the T case.
			if (!oe.GetAttributeValue("quickStyleIndex", out int index, -1))
			{
				if (oe.Parent is null
					|| !oe.Parent.GetAttributeValue("quickStyleIndex", out index, -1))
				{
					return false;
				}
			}

			var quick = quickStyles.FirstOrDefault(q => q.Index == index);
			return quick?.Name?.ToLower().Contains("code") == true;
		}


		private string GetCodeText(XElement oe)
		{
			// concatenate all T runs — a code OE can have multiple runs when
			// syntax-highlighting or other formatting splits the CData across elements.
			var runs = oe.Elements(ns + "T").ToList();
			if (!runs.Any()) return string.Empty;
			return string.Concat(runs.Select(t =>
			{
				var cdata = t.GetCData();
				return cdata?.GetWrapper().Value ?? string.Empty;
			})).TrimEnd();
		}


		private Context DetectQuickStyle(XElement element)
		{
			// quickStyleIndex could be on T, OE, or OEChildren, Outline, Page
			// so ascend until we find one...

			int index = -1;
			while (element is not null &&
				!element.GetAttributeValue("quickStyleIndex", out index, -1))
			{
				element = element.Parent;
			}

			if (index >= 0)
			{
				var context = new Context
				{
					QuickStyleIndex = index
				};

				// FirstOrDefault so missing index returns null instead of throwing
				var quick = quickStyles.FirstOrDefault(q => q.Index == index);
				if (quick is not null)
				{
					var name = quick.Name.ToLower();

					// cite becomes italic
					if (name.In("cite", "citation")) context.Accent = "*";
					else if (name.Contains("code")) context.Accent = "`";

				}

				contexts.Push(context);
				return context;
			}

			return null;
		}


		private void Stylize(string prefix)
		{
			writer.Write(prefix);
			if (contexts.Count == 0) return;
			var context = contexts.Peek();
			var quick = quickStyles.FirstOrDefault(q => q.Index == context.QuickStyleIndex);
			if (quick is null) return;
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
					// guard against missing completed attribute
					var check = element.Attribute("completed")?.Value == "true" ? "x" : " ";
					writer.Write($"[{check}] ");
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
				default: writer.Write(":o: "); break;               // big red circle
			}
		}


		private void WriteText(XCData cdata, bool startOfLine)
		{
			cdata.Value = cdata.Value
				.Replace("<br>", "  ") // usually followed by NL so leave it there
				.TrimEnd();

			var wrapper = cdata.GetWrapper();

			// Escape markdown-significant characters in text nodes only, before span and
			// anchor processing so href attribute values are never inadvertently escaped.
			// New XText nodes created by anchor replacement below are not revisited.
			foreach (var textNode in wrapper.DescendantNodes().OfType<XText>().ToList())
			{
				textNode.Value = textNode.Value
					.Replace("[", "\\[")
					.Replace("|", "\\|")
					.Replace("*", "\\*")
					.Replace("_", "\\_")
					.Replace("~", "\\~")
					.Replace("`", "\\`");
			}

			foreach (var span in wrapper.Descendants("span").ToList())
			{
				var text = span.Value;
				var att = span.Attribute("style");
				// span might only have a lang attribute
				if (att is not null)
				{
					var style = new Style(att.Value);
					if (style.FontFamily?.IndexOf("Consolas", StringComparison.OrdinalIgnoreCase) >= 0)
						text = $"`{text}`";
					if (style.IsStrikethrough) text = $"~~{text}~~";
					if (style.IsItalic) text = $"*{text}*";
					if (style.IsBold) text = $"**{text}**";
					if (style.IsUnderline) text = $"<u>{text}</u>";
					if (style.IsSuperscript) text = $"<sup>{text}</sup>";
					if (style.IsSubscript) text = $"<sub>{text}</sub>";
				}
				span.ReplaceWith(new XText(text));
			}

			foreach (var anchor in wrapper.Elements("a").ToList())
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

			// escape remaining directives (| and [ already escaped in text nodes above)
			var raw = wrapper.GetInnerXml()
				.Replace("&lt;", "\\<");

			if (startOfLine && raw.Length > 0 && raw.StartsWith("#"))
			{
				writer.Write("\\");
			}

#if DBGLOG
			logger.Debug($"text [{raw}]");
#endif
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

			name = PathHelper.CleanFileName(name);

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

				// use relative path (portable) to match image link behavior
				var relPath = Path.Combine(attachmentFolder, name);
				writer.WriteLine($"[{name}]({relPath})");
			}
			else
			{
				writer.WriteLine($"(*File:{name}*)");
			}
		}


		private void WriteTable(XElement element)
		{
			#region WriteRow(TableRow row)
			void WriteRow(TableRow row)
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
			#endregion WriteRow

			var table = new Table(element);

			// table needs a blank line before it
			writer.WriteLine();

			var rows = table.Rows;

			// header - - - - - - - - - - - - - - - - - - -

			if (table.HasHeaderRow && rows.Any())
			{
				// use first row data as header
				WriteRow(rows.First());
				// skip the header row, leaving data rows
				rows = rows.Skip(1);
			}
			else
			{
				// write generic column headers: A, B, C, ...
				writer.Write("| ");
				for (var i = 0; i < table.ColumnCount; i++)
				{
					writer.Write($" {TableCell.IndexToLetters(i + 1)} |");
				}
				writer.WriteLine();
			}

			// separator - - - - - - - - - - - - - - - - -

			writer.Write("|");
			for (int i = 0; i < table.ColumnCount; i++)
			{
				writer.Write(" :--- |");
			}
			writer.WriteLine();

			// data - - - - - - - - - - - - - - - - - - - -

			foreach (var row in rows)
			{
				WriteRow(row);
			}
		}
	}
}
