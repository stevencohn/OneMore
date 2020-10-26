//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
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
				var cdatas = page.Root.DescendantNodes().OfType<XCData>();

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