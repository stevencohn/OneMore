//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ToCaseCommand : Command
	{
		public const int Lowercase = 0;
		public const int Uppercase = 1;
		public const int Titlecase = 3;

		private const string pattern =
			@"(?:[""']|[^:;,./<>?!%\)])\s+(and|an|as|at|but|by|for|if|in|nor|of|off|or|per|so|to|up|via|yet)\W";


		public ToCaseCommand ()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var casing = (int)args[0];

			using var one = new OneNote(out var page, out var ns);
			var updated = page.EditSelected((s) =>
			{
				if (s is XText text)
				{
					text.Value = Recase(text.Value, casing);
					return text;
				}

				var element = (XElement)s;
				element.Value = Recase(element.Value, casing);
				return element;
			});

			if (updated)
			{
				await one.Update(page);
			}
		}


		private string Recase(string text, int casing)
		{
			var info = Thread.CurrentThread.CurrentCulture;

			if (casing == Lowercase)
			{
				return info.TextInfo.ToLower(text);
			}

			if (casing == Uppercase)
			{
				return info.TextInfo.ToUpper(text);
			}

			if (info.TwoLetterISOLanguageName == "en")
			{
				return ToProperTitleCase(info, text);
			}

			return info.TextInfo.ToTitleCase(text);
		}


		/*
		In title case, capitalize the following words in a title or heading
		- the first word of the title or heading, even if it is a minor word [The | A]
		- the first word of a subtitle
		- the first word after a colon, em dash, or end punctuation in a heading
		- major words including the 2nd part of hyphenated major words [Self-Report, not Self-report]
		- words of four letters or more [With | Between | From]

		Lowercase only minor words that are three letters or fewer in a title or heading except
		the first word in a title or subtitle or the first word after a colon, em dash, or end
		punctuation in a heading
		- short conjunctions [and | as | but | for | if | nor | or | so | yet]
		- articles [a | an | the]
		- short prepositions [as | at | by | for | in | of | off | on | per | to | up | via]
		 */
		private string ToProperTitleCase(CultureInfo info, string text)
		{
			// capitalize each word
			text = info.TextInfo.ToTitleCase(text.ToLower(info));

			// lowercase the exceptions...

			var chars = text.ToCharArray();
			var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);

			foreach (Match match in matches)
			{
				var index = match.Groups[1].Index;
				chars[index] = Char.ToLower(chars[index]);
			}

			return new string(chars);
		}

	}
}
