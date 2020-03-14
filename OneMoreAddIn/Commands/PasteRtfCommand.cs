//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#define diag

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;
	using System.Text;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Documents;
	using System.Xml;
	using System.Xml.Linq;
	using Forms = System.Windows.Forms;


	internal class PasteRtfCommand : Command
	{

		public PasteRtfCommand() : base()
		{
		}

		public void Execute()
		{
			try
			{
				logger.WriteLine("PasteRtfCommand()");

				// transform RTF and Xaml data on clipboard to HTML

				PrepareClipboard();

				// paste what's remaining from clipboard, letting OneNote do the
				// heavy lifting of converting the HTML into one:xml schema

				Keyboard.Press(new Keyboard.KeyCode[]
				{
					Keyboard.KeyCode.CONTROL, Keyboard.KeyCode.KEY_V
				});

#if diag
				DumpClipboard(true);
#endif
			}
			catch (Exception exc)
			{
				logger.WriteLine("PasteRtfCommand!!", exc);
			}
		}


#if diag
		private void DumpClipboard(bool fancy = false)
		{
			// create STA context
			var thread = new Thread(() =>
			{
				if (Clipboard.ContainsText(TextDataFormat.Html))
					DumpContent(Clipboard.GetText(TextDataFormat.Html),
						TextDataFormat.Html, "CLIPBOARD", fancy);

				if (Clipboard.ContainsText(TextDataFormat.Rtf))
					DumpContent(Clipboard.GetText(TextDataFormat.Rtf),
						TextDataFormat.Rtf, "CLIPBOARD");

				if (Clipboard.ContainsText(TextDataFormat.Xaml))
					DumpContent(Clipboard.GetText(TextDataFormat.Xaml),
						TextDataFormat.Xaml, "CLIPBOARD", fancy);

				if (Clipboard.ContainsText(TextDataFormat.CommaSeparatedValue))
					DumpContent(Clipboard.GetText(TextDataFormat.CommaSeparatedValue),
						TextDataFormat.CommaSeparatedValue, "CLIPBOARD");

				if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
					DumpContent(Clipboard.GetText(TextDataFormat.UnicodeText),
						TextDataFormat.UnicodeText, "CLIPBOARD");

				if (Clipboard.ContainsText(TextDataFormat.Text))
					DumpContent(Clipboard.GetText(TextDataFormat.Text),
						TextDataFormat.Text, "CLIPBOARD");
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}

		private void DumpContent(
			string content, TextDataFormat format, string title = "CONTENT", bool fancy = false)
		{
			if (content.StartsWith("Version:"))
			{
				// trim html preamble if it's present
				content = content.Substring(content.IndexOf('<'));
			}

			if (fancy)
			{
				content = XElement.Parse(content).ToString(SaveOptions.None);
			}

			logger.Write($"{title}: ({format.ToString()}) [");
			logger.Write(content);
			logger.WriteLine("]");
		}
#endif

		private void PrepareClipboard()
		{
			// OneNote runs in MTA context but Clipboard required STA context
			var thread = new Thread(() =>
			{
				if (Clipboard.ContainsText(TextDataFormat.Rtf))
				{
					var text = AddHtmlPreamble(
						ConvertXamlToHtml(
							ConvertRtfToXaml(Clipboard.GetText(TextDataFormat.Rtf))));

					RebuildClipboard(text);
				}
				else if (Clipboard.ContainsText(TextDataFormat.Xaml))
				{
					var text = AddHtmlPreamble(
						ConvertXamlToHtml(
							Clipboard.GetText(TextDataFormat.Xaml)));

					RebuildClipboard(text);
				}
				else
				{
					var formats = string.Join(",", Clipboard.GetDataObject().GetFormats(false));
					logger.WriteLine($"... saving {formats} content");
				}
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}


		private void RebuildClipboard(string text)
		{
			var dob = new DataObject();
			dob.SetText(text, TextDataFormat.Html);
			if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
			{
				dob.SetText(Clipboard.GetText(TextDataFormat.UnicodeText), TextDataFormat.UnicodeText);
			}
			if (Clipboard.ContainsText(TextDataFormat.Text))
			{
				dob.SetText(Clipboard.GetText(TextDataFormat.Text), TextDataFormat.Text);
			}
			Clipboard.SetDataObject(dob, true);
		}


		private string ConvertRtfToXaml(string rtf)
		{
			if (string.IsNullOrEmpty(rtf))
			{
				return string.Empty;
			}

			// use the old RichTextBox trick to convert RTF to Xaml
			// we'll convert the Xaml to HTML later...

			var box = new RichTextBox();
			var range = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);

			// store RTF in memory stream and load into rich text box
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(rtf);
					writer.Flush();
					stream.Seek(0, SeekOrigin.Begin);
					range.Load(stream, DataFormats.Rtf);
				}
			}

			// read Xaml from rich text box
			using (var stream = new MemoryStream())
			{
				range = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
				range.Save(stream, DataFormats.Xaml);
				stream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}


		private string ConvertXamlToHtml(string xaml)
		{
			if (string.IsNullOrEmpty(xaml))
			{
				return string.Empty;
			}

			var builder = new StringBuilder();
			builder.AppendLine("<html>");
			builder.AppendLine("<body>");

			using (var outer = new XmlTextReader(new StringReader(xaml)))
			{
				while (outer.Read() && outer.NodeType != XmlNodeType.Element) ;
				if (!outer.EOF)
				{
					using (var writer = new XmlTextWriter(new StringWriter(builder)))
					{
						// prepare proper HTML clipboard skeleton

						writer.WriteComment("StartFragment");

						using (var reader = outer.ReadSubtree())
						{
							ConvertXaml(reader, writer);
						}

						writer.WriteComment("EndFragment");
					}
				}
			}
			builder.AppendLine();
			builder.AppendLine("</body>");
			builder.AppendLine("</html>");

			return builder.ToString();
		}


		private void ConvertXaml(XmlReader reader, XmlTextWriter writer)
		{
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						if (!reader.IsEmptyElement)
						{
							writer.WriteStartElement(TranslateElementName(reader.Name, reader));

							if (reader.HasAttributes)
							{
								TranslateAttributes(reader, writer);
							}
						}
						break;

					case XmlNodeType.EndElement:
						//var n = TranslateElementName(reader.Name);
						writer.WriteEndElement();
						break;

					case XmlNodeType.CDATA:
						writer.WriteCData(Untabify(reader.Value));
						break;

					case XmlNodeType.Text:
						writer.WriteValue(Untabify(reader.Value));
						break;

					case XmlNodeType.SignificantWhitespace:
						//writer.WriteWhitespace(reader.Value);
						writer.WriteValue(Untabify(reader.Value));
						break;

					case XmlNodeType.Whitespace:
						if (reader.XmlSpace == XmlSpace.Preserve)
						{
							//writer.WriteWhitespace(reader.Value);
							writer.WriteValue(Untabify(reader.Value));
						}
						break;

					default:
						// ignore
						break;
				}
			}
		}


		private string Untabify(string text)
		{

			if (text == null)
				return string.Empty;

			if (text.Length == 0 || !char.IsWhiteSpace(text[0]))
				return text;

			var builder = new StringBuilder();

			int i = 0;

			while ((i < text.Length) && (text[i] == ' ' || text[i] == '\t'))
			{
				if (text[i] == ' ')
				{
					builder.Append('\u00a0');
				}
				else if (text[i] == '\t')
				{
					do
					{
						builder.Append('\u00a0');
					}
					while (builder.Length % 4 != 0);
				}

				i++;
			}

			while (i < text.Length)
			{
				builder.Append(text[i]);
				i++;
			}

			var t1 = text.Replace(' ', '.').Replace('\t', '_');
			var t2 = builder.ToString().Replace('\u00a0', '·');
			logger.WriteLine($"... untabified [{t1}] to [{t2}]");

			return builder.ToString();
		}


		private string TranslateElementName(string xname, XmlReader reader = null)
		{
			string name;

			switch (xname)
			{
				case "InlineUIContainer":
				case "Span":
					name = "span";
					break;

				case "Run":
					name = "span";
					break;

				case "Bold":
					name = "b";
					break;

				case "Italic":
					name = "i";
					break;

				case "Paragraph":
					name = "p";
					break;

				case "BlockUIContainer":
				case "Section":
					name = "div";
					break;

				case "Table":
					name = "table";
					break;

				case "TableColumn":
					name = "col";
					break;

				case "TableRowGroup":
					name = "tbody";
					break;

				case "TableRow":
					name = "tr";
					break;

				case "TableCell":
					name = "td";
					break;

				case "List":
					switch (reader.GetAttribute("MarkerStyle"))
					{
						case null:
						case "None":
						case "Disc":
						case "Circle":
						case "Square":
						case "Box":
							name = "ul";
							break;

						default:
							name = "ol";
							break;
					}
					break;

				case "ListItem":
					name = "li";
					break;

				case "Hyperlink":
					name = "a";
					break;

				default:
					// ignore
					name = null;
					break;
			}

			return name;
		}


		private void TranslateAttributes(XmlReader reader, XmlTextWriter writer)
		{
			var styles = new StringBuilder();

			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					// character fomatting

					case "Background":
						styles.Append($"background-color:{ConvertColor(reader.Value)};");
						break;

					case "FontFamily":
						styles.Append($"font-family:'{reader.Value}';");
						break;

					case "FontStyle":
						styles.Append($"font-style:{reader.Value.ToLower()};");
						break;

					case "FontWeight":
						styles.Append($"font-weight:{reader.Value.ToLower()};");
						break;

					case "FontSize":
						styles.Append($"font-size:{ConvertSize(reader.Value, "pt")};");
						break;

					case "Foreground":
						styles.Append($"color:{ConvertColor(reader.Value)};");
						break;

					case "TextDecorations":
						if (reader.Value.ToLower() == "strikethrough")
							styles.Append("text-decoration:line-through;");
						else
							styles.Append("text-decoration:underline;");
						break;

					// paragraph formatting

					case "Padding":
						styles.Append($"padding:{ConvertSize(reader.Value, "px")};");
						break;

					case "Margin":
						styles.Append($"margin:{ConvertSize(reader.Value, "px")};");
						break;

					case "BorderThickness":
						styles.Append($"border-width:{ConvertSize(reader.Value, "px")};");
						break;

					case "BorderBrush":
						styles.Append($"border-color:{ConvertColor(reader.Value)};");
						break;

					case "TextIndent":
						styles.Append($"text-indent:{reader.Value};");
						break;

					case "TextAlignment":
						styles.Append($"text-align:{reader.Value.ToLower()};");
						break;

					// hyperlink Attributes

					case "NavigateUri":
						writer.WriteAttributeString("href", reader.Value);
						break;

					case "TargetName":
						writer.WriteAttributeString("target", reader.Value);
						break;

					// table attributes

					case "Width":
						styles.Append($"width:{reader.Value};");
						break;

					case "ColumnSpan":
						writer.WriteAttributeString("colspan", reader.Value);
						break;

					case "RowSpan":
						writer.WriteAttributeString("rowspan", reader.Value);
						break;
				}
			}

			if (styles.Length > 0)
			{
				writer.WriteAttributeString("style", styles.ToString());
			}

			// move back to element
			reader.MoveToElement();
		}


		private string ConvertColor(string color)
		{
			// Xaml colors are /#[A-F0-9]{8}/
			if (color.Length == 9 && color.StartsWith("#"))
			{
				return "#" + color.Substring(3);
			}

			return color;
		}


		private string ConvertSize(string size, string units = null)
		{
			var parts = size.Split(',');

			for (int i = 0; i < parts.Length; i++)
			{
				if (double.TryParse(parts[i], out var value))
				{
					parts[i] = Math.Ceiling(value).ToString();
				}
				else
				{
					parts[i] = "0";
				}
			}

			var builder = new StringBuilder();
			for (int i = 0; i < parts.Length; i++)
			{
				builder.Append(parts[i]);
				builder.Append(units);

				if (i < parts.Length - 1)
				{
					builder.Append(" ");
				}
			}

			return builder.ToString();
		}


		public string AddHtmlPreamble(string html)
		{
			/*
			 * https://docs.microsoft.com/en-us/windows/win32/dataxchg/html-clipboard-format
			 * 
			 * StartHTML:00071
			 * EndHTML:00170
			 * StartFragment:00140
			 * EndFragment:00160
			 * <html>
			 * <body>
			 * <!--StartFragment--> ... <!--EndFragment-->
			 * </body>
			 * </html>
			 */

			var builder = new StringBuilder();
			builder.AppendLine("Version:0.9");
			builder.AppendLine("StartHTML:0000000000");
			builder.AppendLine("EndHTML:1111111111");
			builder.AppendLine("StartFragment:2222222222");
			builder.AppendLine("EndFragment:3333333333");

			// calculate offsets

			builder.Replace("0000000000", $"{builder.Length:0000000000}");
			builder.Replace("1111111111", $"{(builder.Length + html.Length):0000000000}");

			int sf = html.IndexOf("<!--StartFragment-->") + 20;
			builder.Replace("2222222222", $"{(builder.Length + sf):0000000000}");

			int ef = html.IndexOf("<!--EndFragment-->");
			builder.Replace("3333333333", $"{(builder.Length + ef):0000000000}");

			return builder.ToString() + html;
		}
	}
}
