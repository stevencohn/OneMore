//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json.Linq;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Inserts a snippet that resembles the Confluence Info or Warning macros.
	/// </summary>
	internal class InsertInfoBoxCommand : Command
	{

		public InsertInfoBoxCommand()
		{
		}


		/// <summary>
		/// Insert a new info or warning table with starter content
		/// </summary>
		/// <param name="keyword">
		/// The keyword associated wit the type of block to insert: info, note, warn
		/// </param>
		public override async Task Execute(params object[] args)
		{
			var keyword = (string)args[0];
			Resx.Culture = AddIn.Culture;

			await using var one = new OneNote(out var page, out var ns);
			if (!page.ConfirmBodyContext())
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			var theme = JObject.Parse(Resx.InfoBoxThemes)[keyword];

			var symbolStyle =
				$"font-family:'{theme["symbolFont"]}';font-size:{theme["symbolSize"]}.0pt;" +
				$"color:{theme["symbolColor"]};text-align:center";

			var normalStyle = page.GetQuickStyle(StandardStyles.Normal);
			normalStyle.Color = theme["textColor"].ToString();

			// find anchor and optional selected content...

			var cursor = page.GetTextCursor();
			XElement content;
			XElement anchor = null;

			if (// cursor is not null if selection range is empty
				cursor != null &&
				// selection range is a single line containing a hyperlink
				!(page.SelectionSpecial && page.SelectionScope == SelectionScope.Empty))
			{
				content = new XElement(ns + "OE",
					new XAttribute("style", normalStyle.ToCss()),
					new XElement(ns + "T", new XCData(Resx.phrase_YourContentHere)
					));
			}
			else
			{
				var editor = new PageEditor(page);
				content = await editor.ExtractSelectedContent();
				anchor = editor.Anchor;

				content.Descendants().Attributes()
					.Where(a => a.Name == "selected")
					.Remove();
			}

			// inner table...

			var inner = new Table(ns);
			inner.AddColumn(37f, true);
			inner.AddColumn(100f);

			var row = inner.AddRow();

			var symbol = char.ConvertFromUtf32(
				int.Parse(theme["symbol"].ToString(), System.Globalization.NumberStyles.HexNumber));

			row.Cells.ElementAt(0).SetContent(
				new XElement(ns + "OE",
					new XAttribute("alignment", "center"),
					new XAttribute("style", symbolStyle),
					new Meta(ns, Style.HintMeta, "skip"),
					new XElement(ns + "T",
						new XCData($"<span style='font-weight:bold'>{symbol}</span>"))
				));

			var title = Resx.ResourceManager.GetString(theme["titlex"].ToString(), AddIn.Culture);

			normalStyle.Color = theme["titleColor"].ToString();
			normalStyle.IsBold = true;

			row.Cells.ElementAt(1).SetContent(
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("style", normalStyle.ToCss()),
						new XElement(ns + "T",
							new XCData($"<span style='font-weight:bold'>{title}</span>"))
						),
					content.Name.LocalName == "OEChildren" ? content.Elements() : content
				));

			// outer table...

			var outer = new Table(ns)
			{
				BordersVisible = true
			};

			outer.AddColumn(600f, true);
			row = outer.AddRow();

			var cell = row.Cells.ElementAt(0);

			cell.ShadingColor = theme["shading"].ToString();
			cell.SetContent(inner);

			// update...

			if (anchor == null)
			{
				page.AddNextParagraph(outer.Root);
				//page.InsertParagraph(outer.Root, true);
			}
			else
			{
				var localName = anchor.Name.LocalName;
				var box = new XElement(ns + "OE", outer.Root);

				if (localName.In("OE", "HTMLBlock"))
				{
					anchor.AddAfterSelf(box);
				}
				else // if (localName.In("OEChildren", "Outline"))
				{
					anchor.AddFirst(box);
				}
			}

			await one.Update(page);
		}
	}
}
/*
<one:OE>
  <one:Table bordersVisible="true">
    <one:Columns>
      <one:Column index="0" width="500" isLocked="true" />
    </one:Columns>
    <one:Row>
      <one:Cell shadingColor="#FFF8F7">
        <one:OEChildren>
          <one:OE>
            <one:Table bordersVisible="false">
              <one:Columns>
                <one:Column index="0" />
                <one:Column index="1" />
              </one:Columns>
              <one:Row>
                <one:Cell>
                  <one:OEChildren>
                    <one:OE style="font-family:'Segoe UI Symbol';font-size:22.0pt;color:#B43512;text-align:center">
                      <one:T><![CDATA[<span style='font-weight:bold'>⚠</span>]]></one:T>
                    </one:OE>
                  </one:OEChildren>
                </one:Cell>
                <one:Cell>
                  <one:OEChildren>
                    <one:OE style="font-family:'Segoe UI';font-size:11.0pt;color:black">
                      <one:T><![CDATA[<span style='font-weight:bold;background:white'>Warning</span>]]></one:T>
                    </one:OE>
                    <one:OE style="font-family:'Segoe UI';font-size:11.0pt;color:#333333">
                      <one:T><![CDATA[Warning block]]></one:T>
                    </one:OE>
                  </one:OEChildren>
                </one:Cell>
              </one:Row>
            </one:Table>
          </one:OE>
        </one:OEChildren>
      </one:Cell>
    </one:Row>
  </one:Table>
</one:OE>
*/
