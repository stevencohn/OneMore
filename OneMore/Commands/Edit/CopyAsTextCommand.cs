//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using NStandard;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Copy the page or selected content as plan text onto the system clipboard
	/// </summary>
	internal class CopyAsTextCommand : Command
	{

		public CopyAsTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);

			var size = page.Root.ToString(SaveOptions.DisableFormatting).Length;
			logger.WriteLine($"page size is {size} byets");

			var all = CopyEntirePage(page);
			var builder = new StringBuilder();

			var paragraphs = page.Root
				.Elements(ns + "Outline")
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE");

			if (paragraphs.Any())
			{
				foreach (var paragraph in paragraphs)
				{
					BuildText(all, ns, paragraph, builder);
				}
			}

			logger.WriteLine(builder.ToString());

			await new ClipboardProvider().SetText(builder.ToString());
		}


		private bool CopyEntirePage(Page page)
		{
			var cursor = page.GetTextCursor();

			// cursor is null or
			// selection range is a single line containing a hyperlink
			if (cursor == null ||
				!(page.SelectionSpecial && page.SelectionScope == SelectionScope.Empty))
			{
				return true;
			}

			// if only images are selected and no text content then copy entire page...

			var other = page.Root.Descendants().Where(e =>
				e.Attribute("selected")?.Value == "all" &&
				e.Name.LocalName != "Image");

			return !other.Any();
		}


		private void BuildText(bool all, XNamespace ns, XElement paragraph, StringBuilder builder)
		{
			var text = paragraph.Elements(ns + "T")?
				.Where(e => all || e.Attribute("selected")?.Value == "all")
				.DescendantNodes().OfType<XCData>()
				.Where(c => c.Value != string.Empty)
				.Select(c => GetPlainText(c.Value))
				.Aggregate(string.Empty, (x, y) =>
				{
					if (string.IsNullOrEmpty(y)) return x;
					else if (string.IsNullOrEmpty(x)) return y;
					else return $"{x}{y}";
				});


			if (text != null)
			{
				var list = false;
				var first = paragraph.Elements().First();
				if (first.Name.LocalName == "List")
				{
					var item = first.Elements().First();
					if (item.Name.LocalName == "Number")
					{
						builder.Append($"{item.Attribute("text").Value} {text}");
					}
					else
					{
						builder.Append($"* {text}");
					}
					list = true;
				}
				else
				{
					builder.AppendLine(text);
				}

				if (list) // || !text.IsNullOrWhiteSpace())
				{
					builder.AppendLine();
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
				foreach (var table in tables)
				{
					var rows = table.Elements(ns + "Row");
					foreach (var row in rows)
					{
						var rot = row.Elements(ns + "Cell")
							.Elements(ns + "OEChildren")
							.Elements(ns + "OE")
							.Where(e => all || e.Attribute("selected") != null)
							.Select(e => GetPlainText(e.Value))
							.Aggregate(string.Empty, (x, y) =>
							{
								if (string.IsNullOrEmpty(y)) return x;
								else if (string.IsNullOrEmpty(x)) return y;
								else return $"{x}\t{y}";
							});

						builder.AppendLine(rot);
					}
				}

				builder.AppendLine();
			}
		}


		private string GetPlainText(string text)
		{
			// normalize the text to be XML compliant...
			var value = text.Replace("&nbsp;", " ");
			value = Regex.Replace(value, @"\<\s*br\s*\>", "\n");
			value = Regex.Replace(value, @"(\s)lang=([\w\-]+)([\s/>])", "$1lang=\"$2\"$3");
			value = Regex.Replace(value, "…", "...");

			// wrap and then extract Text nodes to filter out <spans>
			var result = XElement.Parse($"<cdata>{value}</cdata>")
				.DescendantNodes()
				.OfType<XText>()
				.Select(t => t.Value)
				.Aggregate(string.Empty, (x, y) =>
				{
					if (string.IsNullOrEmpty(y)) return x;
					else if (string.IsNullOrEmpty(x)) return y;
					else return $"{x.TrimEnd()} {y.TrimStart()}";
				});

			return result;
		}
	}
}
