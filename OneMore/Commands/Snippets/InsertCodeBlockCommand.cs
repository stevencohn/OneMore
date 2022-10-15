//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Inserts a specialized table to mirror the Code macro of Confluence
	/// </summary>
	internal class InsertCodeBlockCommand : Command
	{
		private const string Shading = "#F2F2F2";
		private const string ShadingDark = "#222A35";
		private const string ShadingBlack = "#B2D0EB";
		private const string TitleColor = "#000000";
		private const string TitleColorDark = "#F2F2F2";
		private const string TextColor = "#000000";
		private const string TextColorDark = "#FFFFFF";

		private const float DefaultWidth = 600f;

		private bool dark;
		private string shading;
		private string titleColor;
		private string textColor;
		private XNamespace ns;

		public InsertCodeBlockCommand()
		{
		}


		/// <summary>
		/// Insert a new Code table with starter content
		/// </summary>
		public override async Task Execute(params object[] args)
		{
			var addTitle = args.Length == 0 || (bool)args[0];

			using (var one = new OneNote(out var page, out ns))
			{
				if (!page.ConfirmBodyContext())
				{
					UIHelper.ShowError(Resx.Error_BodyContext);
					return;
				}

				DetermineCellColors(page);

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

					cell.SetContent(
						new XElement(ns + "OE",
							new XAttribute("style", $"font-family:'Segoe UI';font-size:11.0pt;color:{titleColor}"),
							new XElement(ns + "T", new XCData("<span style='font-weight:bold'>Code</span>"))
							));

					cell.ShadingColor = shading;
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

					shading = DetermineShading(content);
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
		}


		private void DetermineCellColors(Page page)
		{
			dark = page.GetPageColor(out _, out var black).GetBrightness() < 0.5;

			// table...

			if (dark)
			{
				shading = ShadingDark;
				titleColor = TitleColorDark;
				textColor = TextColorDark;
			}
			else
			{
				shading = black ? ShadingBlack : Shading;
				titleColor = TitleColor;
				textColor = TextColor;
			}
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
					return (float)Math.Floor(double.Parse(column.Attribute("width").Value));
				}
			}

			// if insertion point is within an Outline with a width set by the users
			// then assume the width of the Outline

			var size = cursor.Ancestors(ns + "Outline").Elements(ns + "Size")
				.FirstOrDefault(e => e.Attribute("isSetByUser")?.Value == "true");

			if (size != null)
			{
				return (float)Math.Floor(double.Parse(size.Attribute("width").Value));
			}

			return DefaultWidth;
		}


		private XElement MakeDefaultContent(bool withTitle)
		{
			if (withTitle)
			{
				return new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("style", $"font-family:Consolas;font-size:10.0pt;color:{textColor}"),
						new XElement(ns + "T", new XCData(string.Empty))
						),
					new XElement(ns + "OE",
						new XAttribute("style", $"font-family:Consolas;font-size:10.0pt;color:{textColor}"),
						new XElement(ns + "T", new XCData(Resx.InsertCodeBlockCommand_Code))
						),
					new XElement(ns + "OE",
						new XAttribute("style", $"font-family:Consolas;font-size:10.0pt;color:{textColor}"),
						new XElement(ns + "T", new XCData(string.Empty))
						)
					);
			}

			return new XElement(ns + "OEChildren",
				new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))),
				new XElement(ns + "OE", new XElement(ns + "T", new XCData(Resx.InsertCodeBlockCommand_Text))),
				new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty)))
				);
		}


		private string DetermineShading(XElement content)
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

			// if all runs have background styles (this is an estimate since it is possible
			// for there to be more backgrounds than runs if there are multiples in each run)
			if (grounds.Count >= runs.Count())
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
