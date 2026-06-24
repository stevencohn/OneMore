//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Removes all hyperlinks from the current page.
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

				await using var one = new OneNote(out var page, out var ns);

				var runs = page.BodyOutlines.Descendants(ns + "T");
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
					logger.WriteLine($"removed {count} hyperlinks on page");
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
