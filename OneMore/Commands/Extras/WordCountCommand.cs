//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class WordCountCommand : Command
	{

		public WordCountCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Selection))
			{
				var cdatas = page.Root
					.Descendants(ns + "Outline")
					.Where(e => e.Elements(ns + "Meta").Attributes("name").Equals(Page.TagBankMetaName))
					.DescendantNodes().OfType<XCData>();

				/*
  <one:Outline>
    <one:Position x="235.0" y="43.0" z="0" />
    <one:Size width="400.0000305175781" height="10.98629760742187" isSetByUser="true" />
    <one:Meta name="omTaggingBank" content="1" />
				*/
				var count = 0;
				foreach (var cdata in cdatas)
				{
					var text = cdata.GetWrapper().Value;
					if (text.Trim().Length > 0)
					{
						count += text.Split(new char[] { ' ' },
							StringSplitOptions.RemoveEmptyEntries).Length;
					}
				}

				UIHelper.ShowMessage(string.Format(Resx.WordCountCommand_Count, count));
			}
		}
	}
}