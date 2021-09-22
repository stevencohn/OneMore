//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class WordCountCommand : Command
	{

		public WordCountCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out _, OneNote.PageDetail.Selection))
			{
				var runs = page.GetSelectedElements(true);
				var count = 0;

				foreach (var run in runs)
				{
					var cdatas = run.DescendantNodes().OfType<XCData>();
					foreach (var cdata in cdatas)
					{
						var text = cdata.GetWrapper().Value.Trim();
						if (text.Length > 0)
						{
							count += Regex.Matches(text, @"[\w]+").Count;
						}
					}
				}

				UIHelper.ShowMessage(string.Format(Resx.WordCountCommand_Count, count));
			}

			await Task.Yield();
		}
	}
}