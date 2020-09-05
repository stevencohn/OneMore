//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Models;


	internal class AddCaptionCommand : Command
	{
		private Page page;
		private XNamespace ns;


		public AddCaptionCommand()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage(Microsoft.Office.Interop.OneNote.PageInfo.piAll));
				ns = page.Namespace;

				var image = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all")
					.FirstOrDefault();

				if (image != null)
				{
					if (AlreadyCaptioned(image))
					{
						return;
					}

					image.Attribute("selected").Remove();

					var table = new Table(ns);
					table.AddColumn(0f); // OneNote will set width accordingly

					var cdata = new XCData("Caption");

					var row = table.AddRow();
					row.SetCellContent(0,
						new XElement(ns + "OEChildren",
							new XElement(ns + "OE",
								new XAttribute("alignment", "center"),
								image),
							new XElement(ns + "OE",
								new XAttribute("alignment", "center"),
								new XElement(ns + "Meta",
									new XAttribute("name", "om"),
									new XAttribute("content", "caption")),
								new XElement(ns + "T",
									new XAttribute("selected", "all"),
									cdata)
							)
						));

					var style = GetStyle();
					new Stylizer(style).ApplyStyle(cdata);

					if (image.Parent.Name.LocalName.Equals("Page"))
					{
						// image.ReplaceWith seems to insert the new Outline but doesn't remove the top
						// level image, so force the deletion by its objectID
						var imageId = image.Attribute("objectID").Value;
						manager.Application.DeletePageContent(page.Root.Attribute("ID").Value, imageId);

						image.ReplaceWith(WrapInOutline(table.Root, image));
					}
					else
					{
						image.ReplaceWith(table.Root);
					}

					manager.UpdatePageContent(page.Root);
				}
			}
		}


		private Style GetStyle()
		{
			Style style = null;

			// use custom Caption style if it exists

			var styles = new StyleProvider().GetStyles();
			if (styles?.Count > 0)
			{
				style = styles.Where(s => s.Name.Equals("Caption")).FirstOrDefault();
			}

			// otherwise use default style

			if (style == null)
			{
				style = new Style
				{
					Color = "#5B9BD5", // close to CornflowerBlue
					FontSize = "10pt",
					IsBold = true
				};
			}

			return style;
		}


		private bool AlreadyCaptioned(XElement image)
		{
			if (image.Parent.ElementsAfterSelf().FirstOrDefault()?
				.Elements(ns + "Meta")
				.Any(e => e.Attribute("name").Value.Equals("om") &&
					 e.Attribute("content").Value.Equals("caption")) == true)
			{
				UIHelper.ShowMessage(owner, "Image already has a caption");
				return true;
			}

			return false;
		}


		private XElement WrapInOutline(XElement table, XElement image)
		{
			var position = image.Element(ns + "Position");
			position.Remove();

			var outline = new XElement(ns + "Outline",
				position,
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						table
					)
				)
			);

			return outline;
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
