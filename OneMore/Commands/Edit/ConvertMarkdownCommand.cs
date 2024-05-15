//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Convert the selected markdown text to OneNote native content.
	/// </summary>
	internal class ConvertMarkdownCommand : Command
	{
		public ConvertMarkdownCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);
			page.GetTextCursor();

			if (page.SelectionScope != SelectionScope.Region)
			{
				MoreMessageBox.ShowError(owner, "select something");
				return;
			}

			var editor = new PageEditor(page);
			var content = await editor.ExtractSelectedContent();
			var builder = new StringBuilder();
			var paragraphs = content.Elements(ns + "OE").ToList();
			foreach (var paragraph in paragraphs)
			{
				BuildText(ns, paragraph, builder);
			}

			var filepath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var body = OneMoreDig.ConvertMarkdownToHtml(filepath, builder.ToString());

			editor.InsertAtAnchor(new XElement(ns + "HTMLBlock",
				new XElement(ns + "Data",
					new XCData($"<html><body>{body}</body></html>")
					)
				));

			await one.Update(page);
		}


		private void BuildText(XNamespace ns, XElement paragraph, StringBuilder builder)
		{
			var runs = paragraph.Elements(ns + "T")
				.DescendantNodes().OfType<XCData>()
				.ToList();

			if (runs.Any())
			{
				var text = runs
					.Select(c => c.Value.PlainText())
					.Aggregate(string.Empty, (x, y) => y is null ? x : x is null ? y : $"{x}{y}");

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

			var children = paragraph
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE");

			if (children.Any())
			{
				foreach (var child in children)
				{
					BuildText(ns, child, builder);
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
							.Elements(ns + "OE");

						if (cells.Any())
						{
							var rot = cells
								.Select(e => e.Value.PlainText())
								.Aggregate("|", (x, y) =>
									y is null ? x : x is null ? y : $"{x}|{y}");

							builder.AppendLine($"{rot}|");
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
