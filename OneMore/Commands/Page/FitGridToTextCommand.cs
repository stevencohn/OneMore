//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class FitGridToTextCommand : Command
	{

		public FitGridToTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic))
			{
                var rules = page.Root.Elements(ns + "RuleLines")
                    .FirstOrDefault(e => e.Attribute("visible")?.Value == "true");

                if (rules == null)
				{
                    UIHelper.ShowMessage("Enable grid lines before using this command");
                    return;
				}

                var styles = page.GetQuickStyles().Where(s => s.Name == "p");
                var pindexes = styles.Select(s => s.Index.ToString());

                var common = page.Root.Descendants(ns + "OE")
                    .Where(e => pindexes.Contains(e.Attribute("index").Value))
                    .Select(e => new
                    {
                        Index = e.Attribute("index").Value,
                        Css = e.Attribute("style")?.Value
                    })
                    .GroupBy(o => o.Index)
                    .Select(group => new
                    {
                        Index = group.Key,
                        Css = group.Select(g => g.Css),
                        Count = group.Count()
                    })
                    .OrderByDescending(g => g.Count)
                    .Take(1);


				await one.Update(page);
			}
		}
	}
}
/*
<one:Page xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote" ID="{7377F553-6139-0F25-0E86-0E2F8B7B75C2}{1}{E19559681762731575340520157162409995572660691}" name="Grid" dateTime="2021-11-05T11:46:21.000Z" lastModifiedTime="2021-11-05T11:52:52.000Z" pageLevel="1" isCurrentlyViewed="true" selected="partial" lang="en-US">
  <one:QuickStyleDef index="0" name="PageTitle" fontColor="automatic" highlightColor="automatic" font="Calibri Light" fontSize="20.0" spaceBefore="0.0" spaceAfter="0.0" />
  <one:QuickStyleDef index="1" name="p" fontColor="automatic" highlightColor="automatic" font="Calibri" fontSize="11.0" spaceBefore="0.0" spaceAfter="0.0" />
  <one:PageSettings RTL="false" color="automatic">
    <one:PageSize>
      <one:Automatic />
    </one:PageSize>
    <one:RuleLines visible="true">
      <one:Horizontal color="#CAEBFD" spacing="13.42771530151367" />
      <one:Margin color="#FF5050" />
    </one:RuleLines>
  </one:PageSettings>
  <one:Title lang="en-US">
    <one:OE alignment="left" quickStyleIndex="0">
      <one:T><![CDATA[Grid]]></one:T>
    </one:OE>
  </one:Title>
  <one:Outline selected="partial">
    <one:Position x="36.0" y="87.6421890258789" z="0" />
    <one:Size width="72.0" height="1879.880493164062" />
    <one:OEChildren selected="partial">
      <one:OE alignment="left" quickStyleIndex="1" style="font-family:Calibri;font-size:14.0pt">
        <one:T><![CDATA[A]]></one:T>
      </one:OE>
      <one:OE alignment="left" quickStyleIndex="1" selected="partial" style="font-family:Calibri;font-size:14.0pt">
        <one:T><![CDATA[A]]></one:T>
        <one:T selected="all"><![CDATA[]]></one:T>
      </one:OE>
      <one:OE alignment="left" quickStyleIndex="1" style="font-family:Calibri;font-size:14.0pt">
 */