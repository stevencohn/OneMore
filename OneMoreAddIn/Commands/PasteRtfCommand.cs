//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

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


	internal class PasteRtfCommand : Command
	{
		private class TextWithFormat
		{
			public string Text { get; set; }
			public TextDataFormat Format { get; set; }
		}


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
			}
			catch (Exception exc)
			{
				logger.WriteLine("PasteRtfCommand!!", exc);
			}
		}

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

					// clear all formats from clipboard's IDataObject and replace it
					// with our HTML-only format so OneNote will accept only that
					// otherwise it sees RTF and stops dead in its tracks

					Clipboard.Clear();
					Clipboard.SetText(text, TextDataFormat.Html);
					logger.WriteLine("... Rtf -> HTML ready");
				}
				else if (Clipboard.ContainsText(TextDataFormat.Xaml))
				{
					var text = AddHtmlPreamble(
						ConvertXamlToHtml(
							Clipboard.GetText(TextDataFormat.Xaml)));

					Clipboard.Clear();
					Clipboard.SetText(text, TextDataFormat.Html);
					logger.WriteLine("... Xaml -> HTML ready");
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

			using (var reader = new XmlTextReader(new StringReader(xaml)))
			{
				using (var writer = new XmlTextWriter(new StringWriter(builder)))
				{
					// prepare proper HTML clipboard skeleton

					writer.WriteStartElement("html");
					writer.WriteStartElement("body");
					writer.WriteComment("StartFragment");

					ConvertXaml(reader, writer);

					writer.WriteComment("EndFragment");
					writer.WriteEndElement();
					writer.WriteEndElement();
				}
			}

			return builder.ToString();
		}


		private void ConvertXaml(XmlTextReader reader, XmlTextWriter writer)
		{
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						{
							var n = TranslateElementName(reader.Name, reader);
							writer.WriteStartElement(n);

							if (reader.HasAttributes)
							{
								TranslateAttributes(reader, writer);
							}
						}
						break;

					case XmlNodeType.EndElement:
						{
							var n = TranslateElementName(reader.Name);
							writer.WriteEndElement();
						}
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
			logger.WriteLine($"Untabify [{text}]");

			if (text == null)
				return string.Empty;

			if (text.Length == 0 || !char.IsWhiteSpace(text[0]))
				return text;

			var builder = new StringBuilder();

			try
			{
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
				var t2 = builder.ToString().Replace(' ', '.').Replace('\t', '_');
				logger.WriteLine($"... untabified [{t1}] to [{t2}]");
			}
			catch (Exception exc)
			{
				logger.WriteLine("Untabify!!", exc);
			}

			return builder.ToString();
		}


		private string TranslateElementName(string xname, XmlTextReader reader = null)
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


		private void TranslateAttributes(XmlTextReader reader, XmlTextWriter writer)
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
			 * StartHTML: 00071
			 * EndHTML: 00170
			 * StartFragment: 00140
			 * EndFragment: 00160
			 * <html>
			 * <body>
			 * <!--StartFragment--> ... <!--EndFragment-->
			 * </body>
			 * </html>
			 */

			var builder = new StringBuilder();
			builder.Append("StartHTML: 000000000" + Environment.NewLine);
			builder.Append("EndHTML: 111111111" + Environment.NewLine);
			builder.Append("StartFragment: 222222222" + Environment.NewLine);
			builder.Append("EndFragment: 333333333" + Environment.NewLine);

			// calculate offsets

			builder.Replace("000000000", $"{builder.Length:000000000}");
			builder.Replace("111111111", $"{(builder.Length + html.Length):000000000}");

			int sf = html.IndexOf("<!--StartFragment-->") + 20;
			builder.Replace("222222222", $"{(builder.Length + sf):000000000}");

			int ef = html.IndexOf("<!--EndFragment-->");
			builder.Replace("333333333", $"{(builder.Length + ef):000000000}");

			return builder.ToString() + html;
		}
	}
}
