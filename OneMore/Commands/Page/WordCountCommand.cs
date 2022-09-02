//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using GTranslate.Translators;
	using System;
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
							// if Chinese, Japanese, or Korean then get transliterated text
							// that can be used to estimate words
							if (regex.IsMatch(text))
							{
								text = await Transliterate(text);
							}

							//logger.WriteLine($"counting '{text}'");

							count += Regex.Matches(text, @"[\w]+").Count;
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


		private async Task<string> Transliterate(string text)
		{
			Func<Task<string>> func = async () =>
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
			};

			try
			{
				text = await await SingleThreaded.Invoke(func);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			return text;
		}
	}
}
