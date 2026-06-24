//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;


	/// <summary>
	/// Removes hyperlinks from the selected range of text, or the entire page if
	/// nothing is selected.
	/// </summary>
	internal class RemoveHyperlinksCommand : Command
	{
		private static bool commandIsActive = false;


		public RemoveHyperlinksCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				var count = 0;
				var regex = new Regex(@"<a\b[^>]*>(.*?)</a>",
					RegexOptions.Singleline | RegexOptions.IgnoreCase);

				await using var one = new OneNote(out var page, out _);

				var range = new SelectionRange(page);
				var runs = range.GetSelections(defaulToAnytIfNoRange: true);

				foreach (var run in runs)
				{
					var original = run.Value;
					var replaced = regex.Replace(original, m => m.Groups[1].Value);

					if (replaced != original)
					{
						run.Value = replaced;
						count++;
					}
				}

				if (count > 0)
				{
					logger.WriteLine($"removed {count} hyperlinks");
					await one.Update(page);
				}
			}
			finally
			{
				commandIsActive = false;
			}
		}
	}
}
