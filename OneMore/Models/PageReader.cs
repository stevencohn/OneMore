//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;


	/// <summary>
	/// Reads, extracts, or edits content on a Page.
	/// </summary>
	internal class PageReader : Loggable
	{
		private readonly Page page;
		private readonly XNamespace ns;


		public PageReader(Page page)
		{
			this.page = page;
			ns = page.Namespace;
		}


		/// <summary>
		/// Gets or sets the table column divider. Generally, this is either a tab
		/// for normal text or a vertical bar for markdown.
		/// </summary>
		public string ColumnDivider { get; set; } = "\t";


		/// <summary>
		/// Gets or sets the table left and right borders. Generally, this is either empty
		/// for normal text or a vertical bar for markdown.
		/// </summary>
		public string TableSides { get; set; } = string.Empty;


		/// <summary>
		/// Reads the selected content or the entire page as raw text, without any HTML styling
		/// whatsoever. However, it will indicate list items and attempt to maintain table rows
		/// and columns.
		/// </summary>
		/// <param name="withTitle">True to include the page title as part of the text</param>
		/// <returns>A string of raw text content in document-order.</returns>
		public string GetSelectedText(bool withTitle = true)
		{
			page.GetTextCursor(allowPageTitle: withTitle);
			var allText = page.SelectionScope != SelectionScope.Region;

			// Allow Title selection as well as body selections.
			// Only grab the top level objects; we'll recurse in BuildText
			List<XElement> paragraphs;

			if (withTitle)
			{
				paragraphs = page.Root
					.Elements(ns + "Title")
					.Elements(ns + "OE")
					.Union(page.Root
						.Elements(ns + "Outline")
						.Elements(ns + "OEChildren")
						.Elements(ns + "OE"))
					.ToList();
			}
			else
			{
				paragraphs = page.Root
					.Elements(ns + "Outline")
					.Elements(ns + "OEChildren")
					.Elements(ns + "OE")
					.ToList();
			}

			return ReadTextFrom(paragraphs, allText);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="paragraphs"></param>
		/// <param name="allText"></param>
		/// <returns></returns>
		public string ReadTextFrom(IEnumerable<XElement> paragraphs, bool allText)
		{
			var builder = new StringBuilder();

			foreach (var paragraph in paragraphs)
			{
				BuildText(paragraph, builder, allText);

				if (paragraph.Parent.Name.LocalName == "Title" && builder.Length > 0)
				{
					builder.AppendLine();
				}
			}

			// OneMore can't tell the difference between selecting an entire line and selecting
			// an entire line plus its EOL character. The selected attribute on the one:T is set
			// to "all" while that attribute on the parent one:OE is always set to "partial"!
			// Many use cases may not want an EOL such as when pasting into a shell console to
			// avoid invoking the text as a command. If the user wants a newline, they must select
			// the entire line, plus the beginning of the next line. This is better than always
			// adding a newline if the whole line is selected because it's the lesser of two evils
			// when pasting into Excel, for example.
			var match = true;
			var newline = Environment.NewLine;
			for (var i = 0; i < newline.Length; i++)
			{
				if (builder[builder.Length - (newline.Length - i)] != newline[i])
				{
					match = false;
					break;
				}
			}

			var length = match ? builder.Length - newline.Length : builder.Length;
			return builder.ToString(0, length);
		}


		private void BuildText(XElement paragraph, StringBuilder builder, bool allText)
		{
			var runs = paragraph.Elements(ns + "T")?
				.Where(e => allText || e.Attribute("selected")?.Value == "all")
				.DescendantNodes().OfType<XCData>()
				.ToList();

			if (runs is not null && runs.Any())
			{
				var text = runs
					.Select(c => c.Value.PlainText())
					.Aggregate(string.Empty, (x, y) => $"{x ?? string.Empty}{y ?? string.Empty}");

				if (runs[0].Parent.PreviousNode is XElement prev &&
					prev.Name.LocalName == "List")
				{
					var item = prev.Elements().First();
					if (item.Name.LocalName == "Number")
					{
						builder.AppendLine($"{item.Attribute("text").Value} {text}");
					}
					else
					{
						builder.AppendLine($"* {text}");
					}
				}
				else
				{
					if (runs[0].Parent.PreviousNode is null &&
						runs[runs.Count - 1].Parent.NextNode is null)
					{
						// whole paragraph selected so treat as a paragrah with EOL
						builder.AppendLine(text);
					}
					else
					{
						// partial paragraph selected so only grab selected runs
						builder.Append(text);
					}
				}
			}

			var children = paragraph
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE");

			if (children.Any())
			{
				foreach (var child in children)
				{
					BuildText(child, builder, allText);
				}
			}

			var tables = paragraph.Elements(ns + "Table");
			if (tables.Any())
			{
				var content = false;
				foreach (var table in tables)
				{
					var rows = table.Elements(ns + "Row");
					foreach (var row in rows)
					{
						var cells = row.Elements(ns + "Cell")
							.Elements(ns + "OEChildren")
							.Elements(ns + "OE")
							.Where(e => allText || e.Attribute("selected") != null);

						if (cells.Any())
						{
							if (cells.Count() == 1)
							{
								BuildText(cells.First(), builder, allText);
							}
							else
							{
								var rowText = cells
									.Select(e => e.Value.PlainText())
									.Aggregate(TableSides, (x, y) =>
										$"{x ?? string.Empty}{ColumnDivider}{y ?? string.Empty}");

								builder.AppendLine($"{rowText}{TableSides}");
							}

							content = true;
						}
					}
				}

				if (content)
				{
					builder.AppendLine();
				}
			}
		}
	}
}
