//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class SearchCommand : Command
	{
		public SearchCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// pattern to remove SPAN|A elements and &#nn; escaped characters
			var regex = new Regex(
				@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
				RegexOptions.Compiled);

			await using var one = new OneNote(out var page, out var ns);

			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.Where(e => e.Elements(ns + "T").Any());

			if (paragraphs.Any())
			{
				foreach (var paragraph in paragraphs)
				{
					var text = string.Empty;
					paragraph.Elements(ns + "T").ForEach(e =>
					{
						var line = e.TextValue(true).Trim();
						if (line.Length > 0)
						{
							text = $"{text}{line} ";
						}
					});

					text = regex.Replace(text.Trim(), string.Empty);
					if (text.Length > 0)
					{
						logger.WriteLine($"paragraph [{text}]");
					}
				}
			}
		}
	}
}
