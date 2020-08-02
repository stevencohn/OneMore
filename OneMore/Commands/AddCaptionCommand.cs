//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;


	internal class AddCaptionCommand : Command
	{
		public AddCaptionCommand()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage(Microsoft.Office.Interop.OneNote.PageInfo.piAll));
				var ns = page.Namespace;

				var image = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected").Value.Equals("all"))
					.FirstOrDefault();

				if (image != null)
				{
					var table = new Table(ns);

					// OneNote will set the width accordingly
					table.AddColumn(0f);

					var row = table.AddRow();
					row.SetCellContent(0,
						new XElement(ns + "OEChildren",
							new XElement(ns + "OE",
								new XAttribute("alignment", "center"),
								image),
							new XElement(ns + "OE",
								new XAttribute("alignment", "center"),
								new XAttribute("style", "font-size:10pt;color:#5B9BD5;text-align:center"),
								new XElement(ns + "Meta",
									new XAttribute("name", "om"),
									new XAttribute("content", "caption")),
								new XElement(ns + "T",
									new XCData(@"<span style=""font-weight:bold"">Caption</style>")
									)
							)
						));

					image.ReplaceWith(table.Root);
					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
/*
  <one:OEChildren>
    <one:OE>
      <one:Table bordersVisible="false">
        <one:Columns>
          <one:Column index="0" />
        </one:Columns>
        <one:Row>
          <one:Cell>
            <one:OEChildren>
              <one:OE alignment="center">
                <one:Image>
                  <one:Size width="280.0" height="68.5" isSetByUser="true" />
                </one:Image>
              </one:OE>
              <one:OE alignment="center" quickStyleIndex="3" style="... text-align:center">
                <one:Meta name="om" content="figure"/>
                <one:T><![CDATA[Figure Blah...</span>]]></one:T>
              </one:OE>
            </one:OEChildren>
          </one:Cell>
        </one:Row>
      </one:Table>
    </one:OE>
  </one:OEChildren>
*/
