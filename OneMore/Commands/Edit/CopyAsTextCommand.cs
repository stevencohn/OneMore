//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S2259 // Null pointers should not be dereferenced

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Copy the page or selected content as plain text onto the system clipboard
	/// </summary>
	internal class CopyAsTextCommand : Command
	{

		public CopyAsTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			logger.StartClock();

			await using var one = new OneNote(out var page, out var ns);

			page.GetTextCursor(allowPageTitle: true);
			var all = page.SelectionScope != SelectionScope.Region;

			var builder = new StringBuilder();

			// Allow Title selection as well as body selections.
			// Only grab the top level objects; we'll recurse in BuildText
			var paragraphs = page.Root
				.Elements(ns + "Title")
				.Elements(ns + "OE")
				.Union(page.Root
					.Elements(ns + "Outline")
					.Elements(ns + "OEChildren")
					.Elements(ns + "OE"))
				.ToList();

			if (paragraphs.Any())
			{
				foreach (var paragraph in paragraphs)
				{
					BuildText(all, ns, paragraph, builder);

					if (paragraph.Parent.Name.LocalName == "Title" && builder.Length > 0)
					{
						builder.AppendLine();
					}
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
			var success = await new ClipboardProvider().SetText(builder.ToString(0, length), true);

			if (!success)
			{
				MoreMessageBox.ShowWarning(owner, Resx.Clipboard_locked);
				return;
			}

			logger.WriteTime("copied text");
		}


		private void BuildText(bool all, XNamespace ns, XElement paragraph, StringBuilder builder)
		{
			var runs = paragraph.Elements(ns + "T")?
				.Where(e => all || e.Attribute("selected")?.Value == "all")
				.DescendantNodes().OfType<XCData>()
				.ToList();

			if (runs.Any())
			{
				var text = runs
					.Select(c => c.Value.PlainText())
					.Aggregate(string.Empty, (x, y) => y is null ? x : x is null ? y : $"{x}{y}");

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
					BuildText(all, ns, child, builder);
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
							.Where(e => all || e.Attribute("selected") != null);

						if (cells.Any())
						{
							if (cells.Count() == 1)
							{
								BuildText(all, ns, cells.First(), builder);
							}
							else
							{
								var rowText = cells
									.Select(e => e.Value.PlainText())
									.Aggregate(string.Empty, (x, y) =>
										y is null ? x : x is null ? y : $"{x}\t{y}");

								builder.AppendLine(rowText);
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
