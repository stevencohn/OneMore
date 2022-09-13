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
				table.AddColumn(CalculateWidth(cursor), true);

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

				if (cursor != null)
				{
					// empty text cursor found, add default content
					cell.SetContent(MakeDefaultContent(addTitle));
					page.AddNextParagraph(table.Root);
				}
				else
				{
					// selection range found so move it into snippet
					var content = MoveSelectedIntoContent(page, out var firstParent);
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


		private float CalculateWidth(XElement cursor)
		{
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


		private XElement MoveSelectedIntoContent(Page page, out XElement firstParent)
		{
			var content = new XElement(ns + "OEChildren");
			firstParent = null;

			var runs = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"))
				.ToList();

			if (runs.Count > 0)
			{
				// content will eventually be added after the first parent
				firstParent = runs[0].Parent;

				// if text is in the middle of a soft-break block then need to split the block
				// into two so the code box can be inserted, maintaining its relative position
				if (runs[runs.Count - 1].NextNode != null)
				{
					var nextNodes = runs[runs.Count - 1].NodesAfterSelf().ToList();
					nextNodes.Remove();

					firstParent.AddAfterSelf(new XElement(ns + "OE",
						firstParent.Attributes(),
						nextNodes
						));
				}

				// collect the content
				foreach (var run in runs)
				{
					// new OE for run
					var oe = new XElement(ns + "OE", run.Parent.Attributes());

					// remove run from current parent
					run.Remove();

					// add run into new OE parent
					oe.Add(run);

					// add new OE to content
					content.Add(oe);
				}
			}

			return content;
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
