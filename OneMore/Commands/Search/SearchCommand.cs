//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class SearchCommand : Command
	{
		public SearchCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.Where(e => e.Elements(ns + "T").Any());

			if (paragraphs.Any())
			{
				foreach (var paragraph in paragraphs)
				{
				}
			}
		}
	}
}
