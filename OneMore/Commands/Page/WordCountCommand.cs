//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Chinese;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Displays the number of words on the current page or in the selected region. 
	/// </summary>
	/// <remarks>
	/// The word count may differ - be slightly lower - than the word count reported by Microsoft
	/// Word because Word counts things like URLs as a single word but OneMore separates the
	/// individual words in the URL. For example, Word reports one word in
	/// "https://github.com/OneMore" whereas OneMore counts it as four words.
	/// </remarks>
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

							if (regex.IsMatch(text))
							{
								// works well for Chinese but is questionable for JP and KO
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


		/*
		 * test using GTranslate48 nuget...
		 * 
		private async Task<string> Transliterate(string text)
		{
			try
			{
				// this nonsense is all rather absurd but it works...

				text = await await SingleThreaded.Invoke(async () =>
				{
					using (var translator = new AggregateTranslator())
					{
						// verify language
						var language = await translator.DetectLanguageAsync(text);
						logger.WriteLine($"detected language '{language.ISO6391}'");
						if (language.ISO6391 == "zh-CN" ||
							language.ISO6391 == "jp-JP" ||
							language.ISO6391 == "ko-KR")
						{
							text = (await translator.TransliterateAsync(
								text, "en", language.ISO6391)).Transliteration;

							return text;
						}
					}

					return null;
				});
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			return text;
		}
		*/
	}
}
