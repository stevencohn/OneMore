//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Inserts a specialized table to mirror the Info or Warning macros of Confluence
	/// </summary>
	internal class InsertInfoBlockCommand : Command
	{
		private const string InfoShading = "#F6FAFF";
		private const string InfoShadingDark = "#323F4F";
		private const string InfoShadingBlack = "#DEEBF6";
		private const string InfoSymbolColor = "#2E75B5";
		private const string InfoSymbolBlack = "#2E75B5";
		private const string WarnShading = "#FFF8F7";
		private const string WarnShadingDark = "#78230C";
		private const string WarnShadingBlack = "#FADBD2";
		private const string WarnSymbolColor = "#E68C74";
		private const string WarnSymbolBlack = "#8B3119";
		private const string TitleColor = "#000000";
		private const string TItleDark = "#FFFFFF";
		private const string TItleBlack = "#000000";
		private const string TextColor = "#333333";
		private const string TextDark = "#D8D8D8";
		private const string TextBlack = "#000000";


		public InsertInfoBlockCommand()
		{
		}


		/// <summary>
		/// Insert a new info or warning table with starter content
		/// </summary>
		/// <param name="warning">
		/// True to generate a Warning table; other an Info table is generated
		/// </param>
		public override async Task Execute(params object[] args)
		{
			var warning = (bool)args[0];

			using var one = new OneNote(out var page, out var ns);
			if (!page.ConfirmBodyContext())
			{
				UIHelper.ShowError(Resx.Error_BodyContext);
				return;
			}

			var dark = page.GetPageColor(out _, out var black).GetBrightness() < 0.5;

			string title, symbol, titleColor, symbolColor, textColor, shading;

			if (warning)
			{
				title = "Warning";
				symbol = "\u26a0"; // !-triangle
				symbolColor = black ? WarnSymbolBlack : (dark ? WarnSymbolBlack : WarnSymbolColor);
				shading = black ? WarnShadingBlack : (dark ? WarnShadingDark : WarnShading);
			}
			else
			{
				title = "Information";
				symbol = "\U0001F6C8"; // i-circle
				symbolColor = black ? InfoSymbolBlack : InfoSymbolColor;
				shading = black ? InfoShadingBlack : (dark ? InfoShadingDark : InfoShading);
			}

			titleColor = black ? TItleBlack : (dark ? TItleDark : TitleColor);
			textColor = black ? TextBlack : (dark ? TextDark : TextColor);

			var normalStyle = $"font-family:'Segoe UI';font-size:11.0pt;color:{textColor}";
			var symbolStyle = $"font-family:'Segoe UI Symbol';font-size:22.0pt;color:{symbolColor};text-align:center";

			// find anchor and optional selected content...

			var cursor = page.GetTextCursor();
			XElement content;
			XElement anchor = null;
			if (cursor != null)
			{
				content = new XElement(ns + "OE",
					new XAttribute("style", normalStyle),
					new XElement(ns + "T", new XCData("Your content here...")
					));
			}
			else
			{
				content = page.ExtractSelectedContent(out anchor);
				
				content.Descendants().Attributes()
					.Where(a => a.Name == "selected")
					.Remove();
			}

			// inner table...

			var inner = new Table(ns);
			inner.AddColumn(37f, true);
			inner.AddColumn(100f);

			var row = inner.AddRow();

			row.Cells.ElementAt(0).SetContent(
				new XElement(ns + "OE",
					new XAttribute("alignment", "center"),
					new XAttribute("style", symbolStyle),
					new XElement(ns + "T",
						new XCData($"<span style='font-weight:bold'>{symbol}</span>"))
				));

			row.Cells.ElementAt(1).SetContent(
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("style", normalStyle),
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

			cell.ShadingColor = shading;
			cell.SetContent(inner);

			// update...

			if (anchor == null)
			{
				page.InsertParagraph(outer.Root, true);
			}
			else if (anchor.HasElements)
			{
				// selected text was a subset of runs under an OE
				anchor.AddAfterSelf(new XElement(ns + "OE", outer.Root));
			}
			else
			{
				// selected text was all of an OE
				anchor.Add(outer.Root);
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
