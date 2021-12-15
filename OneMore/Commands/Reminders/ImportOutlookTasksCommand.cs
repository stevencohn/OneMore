//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ImportOutlookTasksCommand : Command
	{

		public ImportOutlookTasksCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var outlook = new Outlook())
			{
				var folders = outlook.GetTaskHierarchy();
				IEnumerable<OutlookTask> tasks;

				using (var dialog = new ImportOutlookTasksDialog(folders))
				{
					if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
					{
						return;
					}

					tasks = dialog.SelectedTasks;
					if (!tasks.Any())
					{
						return;
					}
				}

				logger.WriteLine($"selected {tasks.Count()} tasks");
				return;

				using (var one = new OneNote(out var page, out var ns))
				{
					foreach (var task in tasks)
					{
						task.OneNoteTaskID = Guid.NewGuid().ToString("b").ToUpper();

						page.AddNextParagraph(
							new XElement(ns + "OE",
								new XElement(ns + "OutlookTask",
									new XAttribute("startDate", task.CreationTime.ToZuluString()),
									new XAttribute("dueDate", task.DueDate.ToZuluString()),
									new XAttribute("guidTask", task.OneNoteTaskID),
									new XAttribute("completed", task.Complete.ToString().ToLower()),
									new XAttribute("creationDate", task.CreationTime.ToZuluString())
									),
								new XElement(ns + "T",
									new XCData(task.Subject))
							));
					}

					await one.Update(page);

					// re-fetch page to get IDs of new paragraphs...
					page = one.GetPage(page.PageId, OneNote.PageDetail.Basic);
					ns = page.Namespace;

					foreach (var task in tasks)
					{
						var paragraph = page.Root.Descendants(ns + "OutlookTask")
							.Where(e => e.Attribute("guidTask").Value == task.OneNoteTaskID)
							.Select(e => e.Parent)
							.FirstOrDefault();

						if (paragraph != null)
						{
							var id = paragraph.Attribute("objectID").Value;
							task.OneNoteURL = one.GetHyperlink(page.PageId, id);

							outlook.SaveTask(task);
						}
					}
				}
			}
		}
	}
}
