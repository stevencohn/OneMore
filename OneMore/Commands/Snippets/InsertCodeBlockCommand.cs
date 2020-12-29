//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;


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

		private string shading;
		private string titleColor;
		private string textColor;

		public InsertCodeBlockCommand()
		{
		}


		/// <summary>
		/// Insert a new Code table with starter content
		/// </summary>
		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				if (!page.ConfirmBodyContext(true))
				{
					return;
				}

				DetermineCellColors(page);

				var table = new Table(ns)
				{
					BordersVisible = true
				};

				table.AddColumn(600f, true);

				// title row...

				var row = table.AddRow();
				var cell = row.Cells.First();

				cell.SetContent(
					new XElement(ns + "OE",
						new XAttribute("style", $"font-family:'Segoe UI';font-size:11.0pt;color:{titleColor}"),
						new XElement(ns + "T", new XCData("<span style='font-weight:bold'>Code</span>"))
						));

				cell.ShadingColor = shading;

				// body row...

				row = table.AddRow();
				cell = row.Cells.First();

				var cursor = page.GetTextCursor();
				if (cursor != null)
				{
					// empty text cursor found, add default content
					cell.SetContent(MakeDefaultContent(ns));
					page.AddNextParagraph(table.Root);
				}
				else
				{
					// selection range found so move it into snippet
					cell.SetContent(MoveSelectedIntoContent(page, ns, out var firstParent));

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

				one.Update(page);
			}
		}


		private void DetermineCellColors(Page page)
		{
			var dark = page.GetPageColor(out _, out var black).GetBrightness() < 0.5;

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


		private XElement MakeDefaultContent(XNamespace ns)
		{
			return new XElement(ns + "OEChildren",
				new XElement(ns + "OE",
					new XAttribute("style", $"font-family:Consolas;font-size:10.0pt;color:{textColor}"),
					new XElement(ns + "T", new XCData(""))
					),
				new XElement(ns + "OE",
					new XAttribute("style", $"font-family:Consolas;font-size:10.0pt;color:{textColor}"),
					new XElement(ns + "T", new XCData("Your code here..."))
					),
				new XElement(ns + "OE",
					new XAttribute("style", $"font-family:Consolas;font-size:10.0pt;color:{textColor}"),
					new XElement(ns + "T", new XCData(""))
					)
				);
		}


		private XElement MoveSelectedIntoContent(Page page, XNamespace ns, out XElement firstParent)
		{
			var content = new XElement(ns + "OEChildren");
			firstParent = null;

			var runs = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"))
				.ToList();

			if (runs.Count > 0)
			{
				firstParent = runs[0].Parent;

				foreach (var run in runs)
				{
					// add run to new parent
					content.Add(new XElement(ns + "OE",
						run.Parent.Attributes(),
						new XElement(run)
						));

					// remove run from current parent
					run.Remove();
				}
			}

			return content;
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
