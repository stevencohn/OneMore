//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class InsertSubpageCommand : Command
	{

		public InsertSubpageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				one.SelectLocation(
					"Select Page", "Select page to embed on this page",
					OneNote.Scope.Pages, Callback);
			}

			await Task.Yield();
		}


		private async Task Callback(string sourceId)
		{
			if (string.IsNullOrEmpty(sourceId))
			{
				// cancelled
				return;
			}

			using (var one = new OneNote(out var page, out var ns))
			{
				var source = one.GetPage(sourceId, OneNote.PageDetail.BinaryData);
				var oelement = source.Root.Elements(source.Namespace + "Outline").FirstOrDefault();
				if (oelement == null)
				{
					UIHelper.ShowInfo(one.Window, "Source page contains no content1");
					return;
				}

				PageNamespace.Set(ns);
				var outline = new Outline(oelement);

				var children = outline.Elements(ns + "OEChildren")
					.Where(e => !e.Elements(ns + "OE")
						.Elements(ns + "Meta").Attributes(Page.EmbeddingsMetaName).Any());

				if (children == null || !children.Any())
				{
					UIHelper.ShowInfo(one.Window, "Source page contains no content2");
					return;
				}

				page.EnsureContentContainer();
				var citationIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

				var container = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Ancestors(ns + "OEChildren")
					.FirstOrDefault();

				if (container == null)
				{
					UIHelper.ShowInfo(one.Window, "Position cursor in body of page");
					return;
				}

				var table = new Table(ns, 1, 1)
				{
					BordersVisible = true,
				};

				var width = outline.GetWidth();
				table.SetColumnWidth(0, width == 0 ? 500 : width);

				var cell = table[0][0];

				var link = one.GetHyperlink(source.PageId, string.Empty);
				var url = $"<a href=\"{link}\">Embedded from {source.Title}</a>";

				cell.SetContent(new XElement(ns + "OEChildren",
					new Paragraph(url)
						.SetQuickStyle(citationIndex)
						.SetStyle("font-style:italic")
						.SetAlignment("right")
					));

				foreach (var child in children)
				{
					cell.Root.Add(child);
				}

				var paragraph = new Paragraph(table.Root);
				paragraph.AddFirst(new XElement(ns + "Meta",
					new XAttribute("name", Page.EmbeddedMetaName),
					new XAttribute("content", source.PageId)
					));

				container.Add(paragraph);
				await one.Update(page);
			}
		}
	}
}
/*
<one:Outline selected="partial">
  <one:Position x="36.0" y="86.4000015258789" z="0" />
  <one:Size width="538.3199462890624" height="40.28314971923828" isSetByUser="true" />

<one:OE alignment="left" selected="partial">
  <one:Table bordersVisible="true" hasHeaderRow="false" selected="partial">
    <one:Columns>
      <one:Column index="0" width="498.6100158691406" isLocked="true" />
    </one:Columns>
    <one:Row selected="partial">
      <one:Cell selected="partial">
        <one:OEChildren selected="partial">
          <one:OE alignment="right" quickStyleIndex="2">
            <one:T><![CDATA[<a href="onenote:#Shared%20Page&amp;section-id={A640CEA0-536E-4ED0-ACC1-428AAB96501F}&amp;page-id={BB5B6321-4470-4DB8-A0E2-375A068184EC}&amp;end&amp;base-path=https://d.docs.live.net/6925d0374517d4b4/Documents/Flux/Testing.one"><span style='font-style:italic'>Embedded from Shared Page </span></a>]]></one:T>
          </one:OE>
          <one:OE alignment="left" quickStyleIndex="2" selected="partial">
            <one:T><![CDATA[xxx]]></one:T>
            <one:T selected="all"><![CDATA[]]></one:T>
          </one:OE>
        </one:OEChildren>
      </one:Cell>
    </one:Row>
  </one:Table>
</one:OE>
*/
