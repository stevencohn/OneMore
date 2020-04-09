//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Xml.Linq;


	/// <summary>
	/// Inserts a specialized table to mirror the Code macro of Confluence
	/// </summary>
	internal class InsertCodeBlockCommand : Command
	{

		public InsertCodeBlockCommand() : base()
		{
		}


		/// <summary>
		/// Insert a new Code table with starter content
		/// </summary>
		public void Execute()
		{
			try
			{
				InsertCodeBlock();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(InsertCodeBlockCommand)}", exc);
			}
		}


		private void InsertCodeBlock()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());
				var ns = page.Namespace;

				var table = new Table(ns);
				table.SetBordersVisible(true);
				table.AddColumn(550f, true);

				// title row...
				var row = table.AddRow();

				row.SetCellContent(0, new XElement(ns + "OE",
					new XAttribute("style", $"font-family:'Segoe UI';font-size:11.0pt;color:black"),
					new XElement(ns + "T", new XCData("<span style='font-weight:bold;background:white'>Code</span>"))
					));

				row.SetCellShading(0, "#F2F2F2");

				// body row...
				row = table.AddRow();

				row.SetCellContent(0, new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("style", "font-family:'Lucida Console';font-size:9.5pt;color:black"),
						new XElement(ns + "T", new XCData(""))
						),
					new XElement(ns + "OE",
						new XAttribute("style", "font-family:'Lucida Console';font-size:9.5pt;color:black"),
						new XElement(ns + "T", new XCData("Your code here..."))
						),
					new XElement(ns + "OE",
						new XAttribute("style", "font-family:'Lucida Console';font-size:9.5pt;color:black"),
						new XElement(ns + "T", new XCData(""))
						)
					));

				page.AddNextParagraph(table.Root);
				manager.UpdatePageContent(page.Root);
			}
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
