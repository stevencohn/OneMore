//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class AnalyzeCommand : Command
	{
		private OneNote one;


		public AnalyzeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote())
			{
				var (backupFolder, _, _) = one.GetFolders();
				//builder.AppendLine($"Backup  path: {backupFolder}");

				one.CreatePage(one.CurrentSectionId, out var pageId);
				var page = one.GetPage(pageId);
				page.Title = "OneNote Storage Diagnostics";

				var table = new Table(page.Namespace);

				// ...

				var container = page.EnsureContentContainer();

				container.Add(table.Root);

				await one.Update(page);
			}
		}


		public IEnumerable<XElement> ReportSection(string sectionId, bool deep)
		{
			var content = new List<XElement>();


			return content;
		}
	}
}
