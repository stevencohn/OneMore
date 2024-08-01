//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml.Linq;


	/// <summary>
	/// Reads, extracts, or edits content on a Page.
	/// </summary>
	internal class PageReader : Loggable
	{
		public sealed class CountedWord
		{
			public CountedWord(string word, int count)
			{
				Word = word;
				Count = count;
			}
			public string Word { get; private set; }
			public int Count { get; private set; }
		}


		private static readonly string[] Blacklist = new[]
		{
			#region Blacklist
			"about",
			"after",
			"again",
			"air",
			"all",
			"along",
			"also",
			"an",
			"and",
			"another",
			"any",
			"are",
			"around",
			"as",
			"at",
			"away",
			"back",
			"be",
			"because",
			"been",
			"before",
			"below",
			"between",
			"both",
			"but",
			"by",
			"came",
			"can",
			"come",
			"could",
			"day",
			"did",
			"different",
			"do",
			"does",
			"don't",
			"down",
			"each",
			"end",
			"even",
			"every",
			"few",
			"find",
			"first",
			"for",
			"found",
			"from",
			"get",
			"give",
			"go",
			"good",
			"great",
			"had",
			"has",
			"have",
			"he",
			"help",
			"her",
			"here",
			"him",
			"his",
			"home",
			"house",
			"how",
			"if",
			"in",
			"into",
			"is",
			"it",
			"its",
			"just",
			"know",
			"large",
			"last",
			"left",
			"like",
			"line",
			"little",
			"long",
			"look",
			"made",
			"make",
			"man",
			"many",
			"may",
			"me",
			"men",
			"might",
			"more",
			"most",
			"Mr.",
			"must",
			"my",
			"name",
			"never",
			"new",
			"next",
			"no",
			"not",
			"now",
			"number",
			"of",
			"off",
			"old",
			"on",
			"one",
			"only",
			"or",
			"other",
			"our",
			"out",
			"over",
			"own",
			"part",
			"people",
			"place",
			"put",
			"read",
			"right",
			"said",
			"same",
			"saw",
			"say",
			"see",
			"she",
			"should",
			"show",
			"small",
			"so",
			"some",
			"something",
			"sound",
			"still",
			"such",
			"take",
			"tell",
			"than",
			"that",
			"the",
			"them",
			"then",
			"there",
			"these",
			"they",
			"thing",
			"think",
			"this",
			"those",
			"thought",
			"three",
			"through",
			"time",
			"to",
			"together",
			"too",
			"two",
			"under",
			"up",
			"us",
			"use",
			"very",
			"want",
			"water",
			"way",
			"we",
			"well",
			"went",
			"were",
			"what",
			"when",
			"where",
			"which",
			"while",
			"who",
			"why",
			"will",
			"with",
			"word",
			"work",
			"world",
			"would",
			"write",
			"year",
			"you",
			"your",
			"was",
			"able",
			"above",
			"across",
			"add",
			"against",
			"ago",
			"almost",
			"among",
			"animal",
			"answer",
			"became",
			"become",
			"began",
			"behind",
			"being",
			"better",
			"black",
			"best",
			"body",
			"book",
			"boy",
			"brought",
			"call",
			"cannot",
			"car",
			"certain",
			"change",
			"children",
			"city",
			"close",
			"cold",
			"country",
			"course",
			"cut",
			"didn't",
			"dog",
			"done",
			"door",
			"draw",
			"during",
			"early",
			"earth",
			"eat",
			"enough",
			"ever",
			"example",
			"eye",
			"face",
			"family",
			"far",
			"father",
			"feel",
			"feet",
			"fire",
			"fish",
			"five",
			"food",
			"form",
			"four",
			"front",
			"gave",
			"given",
			"got",
			"green",
			"ground",
			"group",
			"grow",
			"half",
			"hand",
			"hard",
			"heard",
			"high",
			"himself",
			"however",
			"I'll",
			"I'm",
			"idea",
			"important",
			"inside",
			"John",
			"keep",
			"kind",
			"knew",
			"known",
			"land",
			"later",
			"learn",
			"let",
			"letter",
			"life",
			"light",
			"live",
			"living",
			"making",
			"mean",
			"means",
			"money",
			"morning",
			"mother",
			"move",
			"Mrs.",
			"near",
			"night",
			"nothing",
			"once",
			"open",
			"order",
			"page",
			"paper",
			"parts",
			"perhaps",
			"picture",
			"play",
			"point",
			"ready",
			"red",
			"remember",
			"rest",
			"room",
			"run",
			"school",
			"sea",
			"second",
			"seen",
			"sentence",
			"several",
			"short",
			"shown",
			"since",
			"six",
			"slide",
			"sometime",
			"soon",
			"space",
			"States",
			"story",
			"sun",
			"sure",
			"table",
			"though",
			"today",
			"told",
			"took",
			"top",
			"toward",
			"tree",
			"try",
			"turn",
			"United",
			"until",
			"upon",
			"using",
			"usually",
			"white",
			"whole",
			"wind",
			"without",
			"yes",
			"yet",
			"young"
			#endregion Blacklist
		};

		private const int PoolSize = 20;

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
		/// Gets or sets an string used to indent content. Normally null, can be set to ">"
		/// for example to indent markdown content.
		/// </summary>
		public string Indentation { get; set; }


		/// <summary>
		/// Gets or sets the prefix char used to start indents. Normally null, can be set
		/// to "\n" for example to prepare markdown as a new line of content.
		/// </summary>
		public string IndentationPrefix { get; set; }


		/// <summary>
		/// Gets or sets the paragraph divider. This can be used my PreviewMarkdown to force
		/// a newline "\" to emulate a line break in between consecutive paragraphs.
		/// </summary>
		public string ParagraphDivider { get; set; }


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
			var range = new SelectionRange(page);
			range.GetSelections(true);

			var allText =
				range.Scope == SelectionScope.TextCursor ||
				range.Scope == SelectionScope.SpecialCursor;

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
		/// Finds the most commonly used words on the page, limited by PoolSize and excluding
		/// BlackList words. Each word is annotated with the count of that word on the page.
		/// </summary>
		/// <returns>A collection of CountedWord</returns>
		public IEnumerable<CountedWord> ReadCommonWords()
		{
			var builder = new StringBuilder();

			// collect all visible text into one StringBuilder

			var runs = page.BodyOutlines.Descendants(ns + "T");

			foreach (var run in runs)
			{
				var cdata = run.GetCData();
				if (cdata.Value.Contains("<"))
				{
					var wrapper = cdata.GetWrapper();
					var text = wrapper.Value.Trim();
					if (text.Length > 0)
					{
						builder.Append(" ");
						builder.Append(HttpUtility.HtmlDecode(text));
					}
				}
				else
				{
					var text = cdata.Value.Trim();
					if (text.Length > 0)
					{
						builder.Append(" ");
						builder.Append(HttpUtility.HtmlDecode(text));
					}
				}
			}

			// collect OCR text, e.g. <one:OCRText><![CDATA[...

			var data = page.Root.Elements(ns + "Outline").Descendants(ns + "OCRText")
				.Select(e => e.GetCData());

			foreach (var cdata in data)
			{
				builder.Append(" ");
				builder.Append(cdata.Value);
			}

			// split text into individual words, discarding all non-word chars and numbers

			var alltext = builder.Replace("\n", string.Empty).ToString();

			var words = Regex.Split(alltext, @"\W")
				.Select(w => w.Trim().ToLower()).Where(w =>
					w.Length > 1 &&
					!Blacklist.Contains(w) &&
					!Regex.Match(w, @"^\s*\d+\s*$").Success)
				.GroupBy(w => w)
				.Select(g => new CountedWord(g.Key, g.Count()))
				.OrderByDescending(g => g.Count)
				.Take(PoolSize);

			return words;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="paragraphs"></param>
		/// <param name="allText"></param>
		/// <returns></returns>
		public string ReadTextFrom(IEnumerable<XElement> paragraphs, bool allText)
		{
			if (!paragraphs.Any())
			{
				return string.Empty;
			}

			var builder = new StringBuilder();

			foreach (var paragraph in paragraphs)
			{
				BuildText(paragraph, builder, allText, string.Empty);

				// null check here is needed if caller has manually injected a fabricated
				// paragraph; it won't have a doc parent and that's OK; see PreviewMarkdownCommand
				if (paragraph.Parent?.Name.LocalName == "Title" && builder.Length > 0)
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


		private void BuildText(
			XElement paragraph, StringBuilder builder, bool allText, string indent)
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

				// "---" is a special case for markdown rules
				if (ParagraphDivider is not null && text.Length > 0 && text != "---")
				{
					text = $"{text}{ParagraphDivider}";
				}

				if (runs[0].Parent.PreviousNode is XElement prev &&
					prev.Name.LocalName == "List")
				{
					var item = prev.Elements().First();
					if (item.Name.LocalName == "Number")
					{
						builder.AppendLine($"{indent}{item.Attribute("text").Value} {text}");
					}
					else
					{
						builder.AppendLine($"{indent}* {text}");
					}
				}
				else
				{
					if (runs[0].Parent.PreviousNode is null &&
						runs[runs.Count - 1].Parent.NextNode is null)
					{
						// whole paragraph selected so treat as a paragrah with EOL
						builder.AppendLine($"{indent}{text}");
					}
					else
					{
						// partial paragraph selected so only grab selected runs
						builder.Append($"{indent}{text}");
					}
				}
			}

			var children = paragraph
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE");

			if (children.Any())
			{
				if (Indentation is not null)
				{
					indent = indent == string.Empty
						? $"{IndentationPrefix}{Indentation}"
						: $"{indent}{Indentation}";
				}

				foreach (var child in children)
				{
					BuildText(child, builder, allText, indent);
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
								BuildText(cells.First(), builder, allText, indent);
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
