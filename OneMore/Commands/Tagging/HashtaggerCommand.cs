//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Inserts cherry-picked hashtags onto the page, either at the current cursor location
	/// or into the page tag bank.
	/// </summary>
	internal class HashtaggerCommand : Command
	{

		public HashtaggerCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var converter = new LegacyTaggingConverter();
			await converter.UpgradeLegacyTags();

			await using var one = new OneNote(out var page, out var ns);

			using var dialog = new HashtaggerDialog(page);
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			var updated = false;
			if (dialog.AddToBank)
			{
				var banker = new TagBankCommand();
				banker.MakeWordBank(page, ns);
				if (banker.BankOutline is not null)
				{
					var run = banker.BankOutline.Descendants(ns + "T").FirstOrDefault();
					if (run.GetCData() is XCData cdata)
					{
						cdata.Value = $"{cdata.Value} {dialog.Tags}";
						updated = true;
					}
				}
			}
			else
			{
				var editor = new PageEditor(page);
				editor.InsertOrReplace(dialog.Tags);
				updated = true;
			}

			if (updated)
			{

				await one.Update(page);
			}
		}
	}
}
