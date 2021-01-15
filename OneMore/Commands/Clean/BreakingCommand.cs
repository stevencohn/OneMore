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
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Compress or expand sentence break spacing, i.e. one space or two spaces between sentences
	/// </summary>
	internal class BreakingCommand : Command
	{
		private const string OneSpacePattern = @"(\w)\.(\<[^>]+\>)? (\<[^>]+\>)? (\<[^>]+\>)?(\w)";
		private const string TwoSpacePattern = @"(\w)\.(\<[^>]+\>)? (\<[^>]+\>)?(\w)";


		public BreakingCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var answer = MessageBox.Show(
				Resx.BreakingCommand_OptionsPrompt,
				Resx.BreakingCommand_OptionsTitle,
				MessageBoxButtons.YesNoCancel,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1,
				MessageBoxOptions.DefaultDesktopOnly);

			if (answer == DialogResult.Cancel)
			{
				return;
			}

			Regex regex;
			string replacement;
			if (answer == DialogResult.Yes)
			{
				regex = new Regex(OneSpacePattern);
				replacement = "$1. $2$3$4$5";
			}
			else
			{
				regex = new Regex(TwoSpacePattern);
				replacement = "$1.  $2$3$4";
			}
			
			using (var one = new OneNote(out var page, out var ns))
			{
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
}