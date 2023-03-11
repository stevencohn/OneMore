//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Security.Policy;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Add a caption to the selected image.
	/// </summary>
	/// <remarks>
	/// This is done by moving the image into a one-row, one-column table and centering the
	/// word "Caption" below the image, which of course you should edit immediately. 
	/// If your custom styles has a style named exactly "Caption" then that style will be applied,
	/// otherwise, a default caption style is used.
	/// </remarks>
	internal class AddCaptionCommand : Command
	{
		private XNamespace ns;


		public AddCaptionCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out ns, OneNote.PageDetail.All);
			var element = page.Root.Descendants(ns + "Image")?
				.FirstOrDefault(e => e.Attribute("selected")?.Value == "all");

			if (element == null)
			{
				UIHelper.ShowError(Resx.Error_SelectImage);
				return;
			}

			if (AlreadyCaptioned(element))
			{
				return;
			}

			element.Attribute("selected").Remove();

			// try to detect PlantuML title...

			string caption = "Caption";

			var uml = PlantUmlHelper.ExtractUmlFromImageData(element.Element(ns + "Data").Value);
			if (uml != null)
			{
				logger.WriteLine(uml);
				var title = PlantUmlHelper.ReadTitle(uml);
				if (!string.IsNullOrWhiteSpace(title))
				{
					caption = title;
				}
			}

			// add caption...

			var table = MakeCaptionTable(ns, element, caption, out var cdata);

			var style = GetStyle();
			new Stylizer(style).ApplyStyle(cdata);

			if (element.Parent.Name.LocalName.Equals("Page"))
			{
				// image.ReplaceWith seems to insert the new Outline but doesn't remove the top
				// level image, so force the deletion by its objectID
				var imageId = element.Attribute("objectID").Value;
				one.DeleteContent(page.Root.Attribute("ID").Value, imageId);

				element.ReplaceWith(WrapInOutline(table.Root, element));
			}
			else
			{
				element.ReplaceWith(table.Root);
			}

			await one.Update(page);
		}


		public static Table MakeCaptionTable(
			XNamespace ns, XElement content, string caption, out XCData cdata)
		{
			var table = new Table(ns);
			table.AddColumn(0f); // OneNote will set width accordingly

			var row = table.AddRow();
			var cell = row.Cells.First();

			cdata = new XCData(caption);

			cell.SetContent(
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("alignment", "center"),
						content),
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

			return table;
		}


		private static Style GetStyle()
		{
			Style style = null;

			// use custom Caption style if it exists

			var styles = new ThemeProvider().Theme.GetStyles();
			if (styles?.Count > 0)
			{
				style = styles.FirstOrDefault(s => s.Name.Equals("Caption"));
			}

			// otherwise use default style

			style ??= new Style
			{
				Color = "#5B9BD5", // close to CornflowerBlue
				FontSize = "10pt",
				IsBold = true
			};

			return style;
		}


		private bool AlreadyCaptioned(XElement image)
		{
			if (image.Parent.ElementsAfterSelf().FirstOrDefault()?
				.Elements(ns + "Meta")
				.Any(e => e.Attribute("name").Value.Equals("om") &&
					 e.Attribute("content").Value.Equals("caption")) == true)
			{
				UIHelper.ShowInfo(Resx.AddCaptionCommand_Captioned);
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
