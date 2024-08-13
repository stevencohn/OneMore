//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class SearchWebCommand : Command
	{
		public SearchWebCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var uri = (string)args[0];
			string text;

			await using var one = new OneNote(out var page, out _);
			text = new Models.PageEditor(page).GetSelectedText();

			if (text.Length > 0)
			{
				var url = string.Format(uri, text);
				logger.WriteLine($"search query {url}");
				System.Diagnostics.Process.Start(url);
			}
			else
			{
				logger.WriteLine("search phrase is empty");
			}

			await Task.Yield();
		}
	}
}
