//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using System;
	using System.Linq;
	using System.Xml.Linq;


	internal class WordCountCommand : Command
	{

		public WordCountCommand()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage(PageInfo.piSelection);
				var cdatas = page.DescendantNodes().OfType<XCData>();

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
				UIHelper.ShowMessage($"Total words on page: {count}");
			}
		}
	}
}