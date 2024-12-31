//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	#region Wrappers
	internal class InsertCodeBoxCommand : InsertBoxCommand
	{
		public InsertCodeBoxCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(true);
		}
	}
	internal class InsertTextBoxCommand : InsertBoxCommand
	{
		public InsertTextBoxCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(false);
		}
	}
	#endregion Wrappers


	/// <summary>
	/// Inserts a single-cell table with starter text or a snippet that resembes the Confluence
	/// Code macro.
	/// </summary>
	internal class InsertBoxCommand : Command
	{
		private const string Shading = "#D0CECE";
		private const string TitleColor = "#000000";
		private const string TextColor = "#000000";

		private const float DefaultWidth = 600f;

		private XNamespace ns;


		public InsertBoxCommand()
		{
		}


		/// <summary>
		/// Insert a new Code table with starter content
		/// </summary>
		public override async Task Execute(params object[] args)
		{
			var addTitle = args.Length == 0 || (bool)args[0];

			await using var one = new OneNote(out var page, out ns);
			if (!page.ConfirmBodyContext())
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			PageNamespace.Set(ns);

			var table = new Table(ns)
			{
				BordersVisible = true
			};

			// remember selection cursor

			var range = new SelectionRange(page);
			var cursor = range.GetSelection(true);

			// determine if cursor is inside a table or outline with a user set width
			table.AddColumn(CalculateWidth(cursor, page.Root), true);

			TableRow row;
			TableCell cell;

			// title row...

			if (addTitle)
			{
				row = table.AddRow();
				cell = row.Cells.First();

				var normalStyle = page.GetQuickStyle(Styles.StandardStyles.Normal);
				normalStyle.Color = TitleColor;

				cell.SetContent(
					new Paragraph($"<span style='font-weight:bold'>{Resx.word_Code}</span>")
						.SetStyle(normalStyle.ToCss()));

				cell.ShadingColor = Shading;
			}

			// body row...

			row = table.AddRow();
			cell = row.Cells.First();

			if (range.Scope == SelectionScope.TextCursor)
			{
				// empty text cursor found, add default content
				cell.SetContent(MakeDefaultContent(addTitle));

				var editor = new PageEditor(page);
				editor.ExtractSelectedContent();

				var box = new XElement(ns + "OE", table.Root);
				if (editor.Anchor.Name.LocalName.In("OE", "HTMLBlock"))
				{
					editor.Anchor.AddAfterSelf(box);
				}
				else // if (localName.In("OEChildren", "Outline"))
				{
					editor.Anchor.AddFirst(box);
				}
			}
			else
			{
				// selection range found so move it into snippet
				var editor = new PageEditor(page)
				{
					// the extracted content will be selected=all, keep it that way
					KeepSelected = true
				};

				var content = editor.ExtractSelectedContent();

				if (!content.HasElements)
				{
					ShowError(Resx.Error_BodyContext);
					logger.WriteLine("error reading page content!");
					return;
				}

				editor.Deselect();
				editor.FollowWithCurosr(content);

				cell.SetContent(content);

				var shading = DetermineShading(page, content);
				if (shading is not null)
				{
					cell.ShadingColor = shading;
				}

				var localName = editor.Anchor.Name.LocalName;
				var box = new XElement(ns + "OE", table.Root);

				if (localName.In("OE", "HTMLBlock"))
				{
					editor.Anchor.AddAfterSelf(box);
				}
				else // if (localName.In("OEChildren", "Outline"))
				{
					editor.Anchor.AddFirst(box);
				}
			}

			await one.Update(page);
		}


		private float CalculateWidth(XElement cursor, XElement root)
		{
			// if selected range
			if (cursor is null)
			{
				cursor = root.Elements(ns + "Outline")
					.Descendants(ns + "T")
					.FirstOrDefault(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

				if (cursor is null)
				{
					// shouldn't happen
					return DefaultWidth;
				}
			}

			// calculate delta for width based on indent of outline
			var delta = 0;
			var outline = cursor.Ancestors(ns + "Outline").FirstOrDefault();
			if (outline is not null)
			{
				var x = outline.Elements(ns + "Position").Attributes("x").FirstOrDefault();
				if (x is not null)
				{
					delta = (int)double.Parse(x.Value, CultureInfo.InvariantCulture) - 36;
				}
			}

			// if insertion point is within a table cell then assume the width of that cell

			var cell = cursor.Ancestors(ns + "Cell").FirstOrDefault();
			if (cell is not null)
			{
				var index = cell.ElementsBeforeSelf(ns + "Cell").Count().ToString();
				var column = cell.Ancestors(ns + "Table")
					.Elements(ns + "Columns").Elements(ns + "Column")
					.FirstOrDefault(e => e.Attribute("index")?.Value == index);

				if (column is not null)
				{
					return (float)Math.Floor(double.Parse(
						column.Attribute("width").Value, CultureInfo.InvariantCulture)) - delta;
				}
			}

			// if insertion point is within an Outline with a width set by the users
			// then assume the width of the Outline

			if (outline is not null)
			{
				var size = outline.Elements(ns + "Size")
					.FirstOrDefault(e => e.Attribute("isSetByUser")?.Value == "true");

				if (size is not null)
				{
					return (float)Math.Floor(double.Parse(
						size.Attribute("width").Value, CultureInfo.InvariantCulture)) - delta;
				}
			}

			return DefaultWidth - delta;
		}


		private XElement MakeDefaultContent(bool withTitle)
		{
			if (withTitle)
			{
				var css = $"font-family:Consolas;font-size:10.0pt;color:{TextColor}";

				return new XElement(ns + "OEChildren",
					new Paragraph(string.Empty).SetStyle(css),
					new Paragraph(Resx.InsertBoxCommand_Code).SetStyle(css),
					new Paragraph(string.Empty).SetStyle(css)
					);
			}

			return new XElement(ns + "OEChildren",
				new Paragraph(string.Empty),
				new Paragraph(
					new XElement(ns + "T",
						new XAttribute("selected", "all"),
						new XCData(Resx.phrase_YourContentHere))
					),
				new Paragraph(string.Empty)
				);
		}


		private string DetermineShading(Page page, XElement content)
		{
			string background = null;

			var runs = content.Descendants(ns + "T");

			// collect all CDATA style properties from all runs
			var styles = runs
				.Where(r => r.Value.Length > 0)
				.Select(r => r.GetCData().GetWrapper())
				.SelectMany(w => w.Elements("span"))
				.Where(s => s.Attribute("style") != null)
				.Select(s => s.Attribute("style").Value);

			// extract the background property from all style elements
			var grounds = new List<string>();
			var regex = new Regex(@"background:([^;]+);?");
			foreach (var style in styles)
			{
				var match = regex.Match(style);
				if (match.Success)
				{
					grounds.Add(match.Groups[1].Value);
				}
			}

			var dark = page.GetPageColor(out _, out var black).GetBrightness() < 0.5;

			// if all runs have background styles (this is an estimate since it is possible
			// for there to be more backgrounds than runs if there are multiples in each run)
			if (grounds.Any() && grounds.Count >= runs.Count())
			{
				// find the most frequently occurring color
				var mostFrequent = grounds.Select(v => v)
					.GroupBy(v => v)
					.OrderByDescending(v => v.Count())
					.Select(v => v.Key)
					.First();

				var ground = ColorTranslator.FromHtml(mostFrequent);
				var bright = ground.GetBrightness() >= 0.5;

				if ((dark && bright) || (!dark && !bright))
				{
					// page is dark and text background is all light then return
					// light background color
					background = mostFrequent;
				}
			}

			return background == null ? null : ColorTranslator.FromHtml(background).ToRGBHtml();
		}
	}
}
