//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class InsertExpandCommand : Command
	{

		public InsertExpandCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);
			if (!page.ConfirmBodyContext())
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			var table = new Table(ns)
			{
				BordersVisible = true
			};

			table.AddColumn(550, true);
			var row = table.AddRow();

			var cell = row.Cells.First();
			cell.SetContent("Your content here");

			var expand = new XElement(ns + "OE",
				new XAttribute("explicitExpandCollapseEnabled", "true"),
				new XElement(ns + "T", new XCData("Your title here")),
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE", table.Root),
					new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty)))
					)
				);

			// find the first (the default) paragraph style so we can apply it to the
			// new content; this help when the page background is dark
			var quickdef = page.Root.Elements(ns + "QuickStyleDef")
				.Where(e => e.Attribute("name").Value == "p")
				.Select(e => e.Attribute("index")?.Value)
				.FirstOrDefault();

			if (quickdef != null)
			{
				expand.Add(new XAttribute("quickStyleIndex", quickdef));
				foreach (var oe in expand.Descendants(ns + "OE"))
				{
					oe.Add(new XAttribute("quickStyleIndex", quickdef));
				}
			}

			var editor = new PageEditor(page);
			editor.AddNextParagraph(expand);

			await one.Update(page);
		}
	}
}
/*
<one:OE explicitExpandCollapseEnabled="true">
  <one:T><![CDATA[Your title here...]]></one:T>
  <one:OEChildren>
	<one:OE>
	  <one:Table bordersVisible="true">
		<one:Columns>
		  <one:Column index="0" width="550" isLocked="true"/>
		</one:Columns>
		<one:Row>
		  <one:Cell>
			<one:OEChildren>
			  <one:OE>
				<one:T><![CDATA[Your content here]]></one:T>
			  </one:OE>
			</one:OEChildren>
		  </one:Cell>
		</one:Row>
	  </one:Table>
	</one:OE>
	<one:OE>
	  <one:T><![CDATA[]]></one:T>
	</one:OE>
  </one:OEChildren>
</one:OE>
*/
