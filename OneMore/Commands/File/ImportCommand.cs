//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Helpers.Office;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class ImportCommand : Command
	{

		public ImportCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new ImportDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				if (dialog.WordFile)
				{
					ImportWord(dialog.FilePath, dialog.AppendToPage);
				}
				else
				{
					ImportPowerpoint(dialog.FilePath, dialog.AppendToPage, dialog.SplitSlides);
				}
			}
		}

		private void ImportWord(string path, bool append)
		{
			if (!Office.IsWordInstalled())
			{
				UIHelper.ShowMessage("Word is not installed");
			}

			string html;
			using (var word = new Word())
			{
				html = word.ConvertFileToHtml(path);
				logger.WriteLine(html);
			}

			if (append)
			{
				logger.WriteLine("Adding HTML block");
				using (var one = new OneNote(out var page, out var ns))
				{
					var outline = page.Root
						.Elements(ns + "Outline")
						.Elements(ns + "OEChildren")
						.FirstOrDefault();

					outline.Add(new XElement(ns + "HTMLBlock",
						new XElement(ns + "Data", new XCData(html))
						));

					one.Update(page);
				}
			}
		}


		private void ImportPowerpoint(string path, bool append, bool split)
		{
			//
		}
	}
}
