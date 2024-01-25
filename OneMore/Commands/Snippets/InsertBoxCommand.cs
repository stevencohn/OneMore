﻿//************************************************************************************************
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

			using var one = new OneNote(out var page, out ns);
			if (!page.ConfirmBodyContext())
			{
				UIHelper.ShowError(Resx.Error_BodyContext);
				return;
			}

			PageNamespace.Set(ns);

			var table = new Table(ns)
			{
				BordersVisible = true
			};

			// remember selection cursor
			var cursor = page.GetTextCursor();

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

			if (// cursor is not null if selection range is empty
				cursor != null &&
				// selection range is a single line containing a hyperlink
				!(page.SelectionSpecial && page.SelectionScope == SelectionScope.Empty))
			{
				// empty text cursor found, add default content
				cell.SetContent(MakeDefaultContent(addTitle));
				page.AddNextParagraph(table.Root);
			}
			else
			{
				// selection range found so move it into snippet
				var content = page.ExtractSelectedContent(out var firstParent);
				cell.SetContent(content);

				var shading = DetermineShading(page, content);
				if (shading != null)
				{
					cell.ShadingColor = shading;
				}

				if (firstParent.HasElements)
				{
					// selected text was a subset of runs under an OE
					firstParent.AddAfterSelf(new XElement(ns + "OE", table.Root));
				}
				else
				{
					// selected text was all of an OE
					firstParent.Add(table.Root);
				}
			}

			await one.Update(page);
		}


		private float CalculateWidth(XElement cursor, XElement root)
		{
			// if selected range
			if (cursor == null)
			{
				cursor = root.Elements(ns + "Outline")
					.Descendants(ns + "T")
					.FirstOrDefault(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

				if (cursor == null)
				{
					// shouldn't happen
					return DefaultWidth;
				}
			}

			// if insertion point is within a table cell then assume the width of that cell

			var cell = cursor.Ancestors(ns + "Cell").FirstOrDefault();
			if (cell != null)
			{
				var index = cell.ElementsBeforeSelf(ns + "Cell").Count().ToString();
				var column = cell.Ancestors(ns + "Table")
					.Elements(ns + "Columns").Elements(ns + "Column")
					.FirstOrDefault(e => e.Attribute("index")?.Value == index);

				if (column != null)
				{
					return (float)Math.Floor(double.Parse(
						column.Attribute("width").Value, CultureInfo.InvariantCulture));
				}
			}

			// if insertion point is within an Outline with a width set by the users
			// then assume the width of the Outline

			var size = cursor.Ancestors(ns + "Outline").Elements(ns + "Size")
				.FirstOrDefault(e => e.Attribute("isSetByUser")?.Value == "true");

			if (size != null)
			{
				return (float)Math.Floor(double.Parse(
					size.Attribute("width").Value, CultureInfo.InvariantCulture));
			}

			return DefaultWidth;
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
				new Paragraph(Resx.InsertBoxCommand_Text),
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

				if (dark && bright)
				{
					// page is dark and text background is all light then return
					// light background color
					background = mostFrequent;
				}
				else if (!dark && !bright)
				{
					// page is light and text background is all dark then return
					// dark background color
					background = mostFrequent;
				}
			}

			return background == null ? null : ColorTranslator.FromHtml(background).ToRGBHtml();
		}
	}
}
/*
<one:Table bordersVisible="true">
  <one:Columns>
    <one:Column index="0" width="550" isLocked="true" />
  </one:Columns>
  <one:Row>
    <one:Cell shadingColor="#F2F2F2">
      <one:OEChildren>
        <one:OE style="font-family:'Segoe UI';font-size:11.0pt;color:black">
          <one:T><![CDATA[<span style='font-weight:bold;background:white'>Code</span>]]></one:T>
        </one:OE>
      </one:OEChildren>
    </one:Cell>
  </one:Row>
  <one:Row>
    <one:Cell>
      <one:OEChildren selected="partial">
        <one:OE style="font-family:'Lucida Console';font-size:9.0pt">
          <one:T><![CDATA[Your code here...]]></one:T>
        </one:OE>
      </one:OEChildren>
    </one:Cell>
  </one:Row>
</one:Table>
*/
