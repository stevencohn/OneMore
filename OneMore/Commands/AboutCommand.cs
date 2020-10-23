//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#define Testingx

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;


	internal class AboutCommand : Command
	{
		public AboutCommand()
		{
		}


		public void Execute()
		{
			logger.WriteLine("AboutCommand.Execute()");

#if Testing
			if (Office.IsWordInstalled())
			{
				using (var word = new Word())
				{
					//var html = word.ConvertFileToHtml(@"C:\users\steven\downloads\foo.docx");
					var html = word.ConvertClipboardToHtml();

					logger.WriteLine(html);

					logger.WriteLine("Adding HTML blcok");
					using (var manager = new ApplicationManager())
					{
						var page = new Page(manager.CurrentPage(PageInfo.piBasic));
						var ns = page.Namespace;

						var outline = page.Root.Elements(ns + "Outline").Elements(ns + "OEChildren").FirstOrDefault();

						outline.Add(new XElement(ns + "HTMLBlock",
							new XElement(ns + "Data", new XCData(html))
							));

						manager.UpdatePageContent(page.Root);
						return;
					}
				}
			}
#else
			using (var dialog = new Dialogs.AboutDialog())
			{
				dialog.ShowDialog(owner);
			}
#endif
		}
	}
}
