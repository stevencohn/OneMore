//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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

			page.GetTextCursor();
			var all = page.SelectionScope != SelectionScope.Region;

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

			var success = await new ClipboardProvider().SetText(builder.ToString());
			if (!success)
			{
				UIHelper.ShowInfo(Resx.Clipboard_locked);
				return;
			}

			logger.WriteTime("copied text");
		}


		private void BuildText(bool all, XNamespace ns, XElement paragraph, StringBuilder builder)
		{
			var runs = paragraph.Elements(ns + "T")?
				.Where(e => all || e.Attribute("selected")?.Value == "all")
				.DescendantNodes().OfType<XCData>()
				.Where(c =>
					c.Value != string.Empty ||
					c.Parent.Parent.Elements(ns + "T").Count() == 1);

			if (runs.Any())
			{
				var text = runs
					.Select(c => c.Value.PlainText())
					.Aggregate(string.Empty, (x, y) =>
					{
						if (string.IsNullOrEmpty(y)) return x;
						else if (string.IsNullOrEmpty(x)) return y;
						else return $"{x}{y}";
					});


				var first = paragraph.Elements().First();
				if (first.Name.LocalName == "List")
				{
					var item = first.Elements().First();
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
					builder.AppendLine(text);
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
							var rot = cells
								.Select(e => e.Value.PlainText())
								.Aggregate(string.Empty, (x, y) =>
								{
									if (string.IsNullOrEmpty(y)) return x;
									else if (string.IsNullOrEmpty(x)) return y;
									else return $"{x}\t{y}";
								});

							builder.AppendLine(rot);
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
