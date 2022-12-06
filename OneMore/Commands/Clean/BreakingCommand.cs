//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Compress or expand sentence break spacing, i.e. one space or two spaces between sentences
	/// </summary>
	internal class BreakingCommand : Command
	{
		// these patterns allow opening or closing SPAN elements among the <word>.<spaces><word>
		// sequence of characters. Also recognizes period, question mark, and semi-colon

		// search for two spaces to be replaced by one
		private const string OneSpacePattern = @"(\w[\.?;])(\<[^>]+\>)?[\s]+(\<[^>]+\>)?\s(\<[^>]+\>)?(\w)";

		// search for one space to be replaced by two
		private const string TwoSpacePattern = @"(\w[\.?;])(\<[^>]+\>)?[\s]+(\<[^>]+\>)?(\w)";


		public BreakingCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			bool singleSpace = true;
			using (var dialog = new BreakingDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				singleSpace = dialog.SingleSpace;
			}

			Regex regex;
			string replacement;
			if (singleSpace)
			{
				regex = new Regex(OneSpacePattern);
				replacement = "$1 $2$3$4$5";
			}
			else
			{
				regex = new Regex(TwoSpacePattern);
				replacement = "$1  $2$3$4";
			}

			using var one = new OneNote(out var page, out var ns);
			logger.StartClock();

			var nodes = page.Root.DescendantNodes().OfType<XCData>()
				.Where(n => n.Value.Contains('.'));

			if (nodes != null && nodes.Any())
			{
				var updated = false;

				foreach (var cdata in nodes)
				{
					cdata.Value = regex.Replace(cdata.Value, replacement);
					updated = true;
				}

				if (updated)
				{
					await one.Update(page);
				}
			}

			logger.StopClock();
		}
	}
}