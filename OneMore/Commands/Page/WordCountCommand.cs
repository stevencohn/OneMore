﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Chinese;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class WordCountCommand : Command
	{
		private const string CJKPattern = @"\p{IsCJKUnifiedIdeographs}+";


		public WordCountCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out _, OneNote.PageDetail.Selection))
			{
				var runs = page.GetSelectedElements(true);
				var count = 0;

				var regex = new Regex(CJKPattern);

				foreach (var run in runs)
				{
					var cdatas = run.DescendantNodes().OfType<XCData>();
					foreach (var cdata in cdatas)
					{
						var text = cdata.GetWrapper().Value.Trim();
						if (text.Length > 0)
						{
							//logger.WriteLine($"counting '{text}'");

							// if Chinese, Japanese, or Korean then get transliterated text
							// that can be used to estimate words
							if (regex.IsMatch(text))
							{
								count += ChineseTokenizer.SplitWords(text).Count();
							}
							else
							{
								count += Regex.Matches(text, @"[\w]+").Count;
							}
						}
					}
				}

				if (page.SelectionScope == SelectionScope.Empty)
				{
					UIHelper.ShowMessage(string.Format(Resx.WordCountCommand_Count, count));
				}
				else
				{
					UIHelper.ShowMessage(string.Format(Resx.WordCountCommand_Selected, count));
				}
			}

			await Task.Yield();
		}
	}
}
