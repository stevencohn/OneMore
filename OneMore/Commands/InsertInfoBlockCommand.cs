//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Xml.Linq;


	/// <summary>
	/// Inserts a specialized table to mirror the Info or Warning macros of Confluence
	/// </summary>
	internal class InsertInfoBlockCommand : Command
	{
		private static readonly string InfoShading = "#F6FAFF";
		private static readonly string InfoShadingDark = "#323F4F";
		private static readonly string InfoSymbolColor = "#2E75B5";
		private static readonly string WarnShading = "#FFF8F7";
		private static readonly string WarnShadingDark = "#78230C";
		private static readonly string WarnSymbolColor = "#FFF8F7";
		private static readonly string TitleColor = "#000000";
		private static readonly string TitleColorDark = "#FFFFFF";
		private static readonly string TextColor = "#333333";
		private static readonly string TextColorDark = "#D8D8D8";


		public InsertInfoBlockCommand() : base()
		{
		}


		/// <summary>
		/// Insert a new info or warning table with starter content
		/// </summary>
		/// <param name="warning">
		/// True to generate a Warning table; other an Info table is generated
		/// </param>
		public void Execute(bool warning)
		{
			try
			{
				InsertInfoBlock(warning);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(InsertInfoBlockCommand)}", exc);
			}
		}


		private void InsertInfoBlock(bool warning)
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());
				var ns = page.Namespace;

				var dark = page.GetPageColor().GetBrightness() < 0.5;
				string title, symbol, titleColor, symbolColor, textColor, shading;

				if (warning)
				{
					title = "Warning";
					symbol = "\u26a0";
					symbolColor = WarnSymbolColor;
					shading = dark ? WarnShadingDark : WarnShading;
				}
				else
				{
					title = "Information";
					symbol = "\U0001F6C8";
					symbolColor = InfoSymbolColor;
					shading = dark ? InfoShadingDark : InfoShading;
				}

				titleColor = dark ? TitleColorDark : TitleColor;
				textColor = dark ? TextColorDark : TextColor;

				// table...

				var inner = new Table(ns);
				inner.AddColumn(37f, true);
				inner.AddColumn(100f);

				var row = inner.AddRow();

				row.SetCellContent(0, new XElement(ns + "OE",
					new XAttribute("alignment", "center"),
					new XAttribute("style", $"font-family:'Segoe UI Symbol';font-size:22.0pt;color:{symbolColor};&#xA;text-align:center"),
					new XElement(ns + "T",
						new XCData($"<span style='font-weight:bold'>{symbol}</span>"))
					));

				row.SetCellContent(1, new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("style", $"font-family:'Segoe UI';font-size:11.0pt;color:{titleColor}"),
						new XElement(ns + "T",
							new XCData($"<span style='font-weight:bold'>{title}</span>"))
						),
					new XElement(ns + "OE",
						new XAttribute("style", $"font-family:'Segoe UI';font-size:11.0pt;color:{textColor}"),
						new XElement(ns + "T",
							new XCData("Your content here..."))
					)));

				var outer = new Table(ns, shading);
				outer.SetBordersVisible(true);
				outer.AddColumn(600f, true);
				row = outer.AddRow();
				row.SetCellContent(0, inner);

				page.AddNextParagraph(outer.Root);
				manager.UpdatePageContent(page.Root);
			}
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
