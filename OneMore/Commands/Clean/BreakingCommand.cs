//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
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
			using var guard = EnterOnce();
			if (guard is null) { return; }

			using var dialog = new BreakingDialog();
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			await using var one = new OneNote(out var page, out var ns);
			logger.StartClock();

			if (Run(page, dialog.SingleSpace))
			{
				await one.Update(page);
			}

			logger.StopClock();
		}


		/// <summary>
		/// Compresses or expands sentence break spacing throughout the page.
		/// </summary>
		/// <param name="page">The page to update</param>
		/// <param name="singleSpace">
		/// True to collapse double spaces to one; false to expand single spaces to two
		/// </param>
		/// <returns>True if the page was modified</returns>
		internal bool Run(Models.Page page, bool singleSpace)
		{
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

			var nodes = page.Root.DescendantNodes().OfType<XCData>()
				.Where(n => n.Value.Contains('.'));

			var updated = false;

			foreach (var cdata in nodes)
			{
				var replaced = regex.Replace(cdata.Value, replacement);
				if (replaced != cdata.Value)
				{
					cdata.Value = replaced;
					updated = true;
				}
			}

			return updated;
		}
	}
}