//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ImportOutlookTasksCommand : Command
	{
		private const string TableMeta = "omOutlookTasks";
		private const string RefreshMeta = "omOutlookTasksRefresh";

		private const string HeaderShading = "#DEEBF6";         // blue
		private const string OverdueShading = "#FADBD2";        // red
		private const string CompletedColor = "#70AD47";        // green
		private const string CompletedShading = "#E2EFD9";      // green
		private const string NotStartedShading = "#FFF2CC";     // yellow
		private const string HighImportanceColor = "#E84C22";   // red
		private const string NormalImportanceColor = "#5B9BD5"; // blue

		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";

		private OneNote one;
		private Page page;
		private XNamespace ns;
		private string[] importances;
		private string[] statuses;
		private DateTime now;
		private int citeIndex;


		public ImportOutlookTasksCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			IEnumerable<OutlookTask> tasks = null;

			if (args.Length > 0 && args[0] is string refreshArg && refreshArg == "refresh")
			{
				await UpdateTableReport();
				return;
			}

			var genTable = false;
			using (var outlook = new Outlook())
			{
				var folders = outlook.GetTaskHierarchy();

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

					genTable = dialog.ShowDetailedTable;
				}
			}

			//logger.WriteLine($"selected {tasks.Count()} tasks");

			using (one = new OneNote(out page, out ns))
			{
				if (genTable)
				{
					await GenerateTableReport(tasks);
				}
				else
				{
					await GenerateListReport(tasks);
				}

				BindTasks(tasks);
			}
		}


		private async Task UpdateTableReport()
		{
			using (one = new OneNote(out page, out ns))
			{
				var element = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == TableMeta)
					.Select(e => e.Parent.Element(ns + "Table"))
					.FirstOrDefault();

				if (element == null)
				{
					UIHelper.ShowInfo("Outlook task table not found. It may have been deleted");
					return;
				}

				var table = new Table(element);

				var taskIDs = table.Root.Descendants(ns + "OutlookTask")
					.Select(e => e.Attribute("guidTask").Value)
					// ref by list so they're not disposed when we clear the table XElement
					.ToList();

				if (!taskIDs.Any())
				{
					UIHelper.ShowInfo("Table contains no Outlook tasks. Rows may have been deleted");
					return;
				}

				taskIDs.ForEach(t => logger.WriteLine($"taskid={t}"));

				table.Clear(true);

				using (var outlook = new Outlook())
				{
					var tasks = outlook.LoadTasksByID(taskIDs);
					PopulateTable(table, tasks);
				}

				await one.Update(page);
			}
		}


		private async Task GenerateTableReport(IEnumerable<OutlookTask> tasks)
		{
			PageNamespace.Set(ns);
			var heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;

			var table = new Table(ns, 1, 6)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 220);
			table.SetColumnWidth(1, 70);
			table.SetColumnWidth(2, 140);
			table.SetColumnWidth(3, 130);
			table.SetColumnWidth(4, 60);
			table.SetColumnWidth(5, 60);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph(Resx.OutlookTaskReport_Task).SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(Resx.OutlookTaskReport_Status).SetStyle(HeaderCss));
			row[2].SetContent(new Paragraph(Resx.OutlookTaskReport_DateStarted).SetStyle(HeaderCss));
			row[3].SetContent(new Paragraph(Resx.OutlookTaskReport_DateDue).SetStyle(HeaderCss));
			row[4].SetContent(new Paragraph(Resx.OutlookTaskReport_Importance).SetStyle(HeaderCss));
			row[5].SetContent(new Paragraph(Resx.OutlookTaskReport_Percent).SetStyle(HeaderCss));

			PopulateTable(table, tasks);

			var nowf = DateTime.Now.ToShortFriendlyString();

			page.AddNextParagraph(
				new Paragraph(Resx.OutlookTaskReport_Title).SetQuickStyle(heading2Index),
				new Paragraph($"{Resx.ReminderReport_LastUpdated} {nowf} " +
					$"(<a href=\"onemore://ImportOutlookTasksCommand/refresh\">{Resx.word_Refresh}</a>)")
					.SetMeta(RefreshMeta, Guid.NewGuid().ToString("b").ToUpper()),
				new Paragraph(string.Empty),
				new Paragraph(table.Root).SetMeta(TableMeta, Guid.NewGuid().ToString("b").ToUpper()),
				new Paragraph(string.Empty),
				new Paragraph(string.Empty)
				);

			await one.Update(page);
		}


		private void PopulateTable(Table table, IEnumerable<OutlookTask> tasks)
		{
			PageNamespace.Set(ns);
			citeIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

			// for some Github cloners, multiline items in the Resx file are delimeted
			// wth only CR instead of NLCR so allow for any possibility
			var delims = new[] { Environment.NewLine, "\r", "\n" };

			importances = Resx.OutlookTaskReport_importances
				.Split(delims, StringSplitOptions.RemoveEmptyEntries);

			statuses = Resx.OutlookTaskReport_statuses
				.Split(delims, StringSplitOptions.RemoveEmptyEntries);

			now = DateTime.UtcNow;

			foreach (var task in OrderTasks(tasks))
			{
				var row = table.AddRow();
				PopulateRow(row, task);
			}
		}

		private void PopulateRow(TableRow row, OutlookTask task)
		{
			row[0].SetContent(MakeTaskReference(task));

			row[1].SetContent(statuses[(int)task.Status]);
			if (task.Status == OutlookTaskStatus.Complete)
			{
				row[1].ShadingColor = CompletedShading;
			}
			else if (now.CompareTo(task.DueDate) > 0)
			{
				row[1].ShadingColor = OverdueShading;
			}
			else if (task.Status == OutlookTaskStatus.NotStarted &&
				now.CompareTo(task.StartDate) > 0)
			{
				row[1].ShadingColor = NotStartedShading;
			}
			// TODO: set default shading color?

			row[2].SetContent(task.StartDate.Year > OutlookTask.UnspecifiedYear
				? string.Empty
				: task.DueDate.ToShortFriendlyString());

			if (task.PercentComplete == 100 && task.DateCompleted.Year < OutlookTask.UnspecifiedYear)
			{
				if (task.DueDate.Year < OutlookTask.UnspecifiedYear)
				{
					row[3].SetContent(new XElement(ns + "OEChildren",
						new Paragraph($"<span style='color:{CompletedColor}'>{task.DateCompleted.ToShortFriendlyString()}</span>"),
						new Paragraph($"Due: {task.DueDate.ToShortFriendlyString()}").SetQuickStyle(citeIndex)
						));
				}
				else
				{
					row[3].SetContent(task.DateCompleted.ToShortFriendlyString());
				}
			}
			else if (task.DueDate.Year < OutlookTask.UnspecifiedYear)
			{
				var woy = string.Format(Resx.OutlookTaskReport_Week, task.WoYear);
				row[3].SetContent(new XElement(ns + "OEChildren",
					new Paragraph(task.DueDate.ToShortFriendlyString()),
					new Paragraph(woy).SetQuickStyle(citeIndex)
					));
			}

			row[4].SetContent(MakeImportance(task.Importance));
			row[5].SetContent((task.PercentComplete / 100.0).ToString("P0"));

		}

		private IEnumerable<OutlookTask> OrderTasks(IEnumerable<OutlookTask> tasks)
		{
			var culture = System.Globalization.CultureInfo.CurrentUICulture;
			var calendar = culture.Calendar;
			var weekRule = culture.DateTimeFormat.CalendarWeekRule;
			var firstDay = culture.DateTimeFormat.FirstDayOfWeek;

			foreach (var task in tasks)
			{
				task.Year = task.DueDate.Year;
				task.WoYear = calendar.GetWeekOfYear(task.DueDate, weekRule, firstDay);
			}

			return tasks.OrderBy(t => t.FolderPath)
				.ThenBy(t => t.Year)
				.ThenBy(t => t.WoYear)
				.ThenBy(t => t.Subject);
		}


		private XElement MakeTaskReference(OutlookTask task)
		{
			task.OneNoteTaskID = Guid.NewGuid().ToString("b").ToUpper();

			var subject = task.Subject;
			var index = task.FolderPath.IndexOf(OutlookTask.PathDelimeter);
			if (index > 0)
			{
				var path = task.FolderPath.Substring(index + 1) + OutlookTask.PathDelimeter;
				subject = $"<span style='color:#7F7F7F'>{path}</span>{task.Subject}";
			}

			return new XElement(ns + "OE",
				new XElement(ns + "OutlookTask",
					new XAttribute("startDate", task.CreationTime.ToZuluString()),
					new XAttribute("dueDate", task.DueDate.ToZuluString()),
					new XAttribute("guidTask", task.OneNoteTaskID),
					new XAttribute("completed", task.Complete.ToString().ToLower()),
					new XAttribute("creationDate", task.CreationTime.ToZuluString())
					),
				new XElement(ns + "T",
					new XCData(subject))
				);
		}


		private XElement MakeImportance(OutlookImportance importance)
		{
			var paragraph = new Paragraph(importances[(int)importance]);
			if (importance == OutlookImportance.High)
			{
				paragraph.SetAttributeValue("style", $"color:{HighImportanceColor};");
			}
			else if (importance == OutlookImportance.Normal)
			{
				paragraph.SetAttributeValue("style", $"color:{NormalImportanceColor};");
			}

			return paragraph;
		}



		private async Task GenerateListReport(IEnumerable<OutlookTask> tasks)
		{
			var ordered = tasks.OrderBy(t => t.FolderPath).ThenBy(t => t.Subject);

			foreach (var task in ordered)
			{
				//logger.WriteLine($"importing \"{task.FolderPath}/{task.Subject}\"");
				page.InsertParagraph(MakeTaskReference(task));
			}

			await one.Update(page);
		}


		private void BindTasks(IEnumerable<OutlookTask> tasks)
		{
			// re-fetch page to get IDs of new paragraphs...
			page = one.GetPage(page.PageId, OneNote.PageDetail.Basic);
			ns = page.Namespace;

			// find the containing Outline to optimize the lookup loop below
			var outline = page.Root.Descendants(ns + "OutlookTask")
				.Where(e => e.Attribute("guidTask").Value == tasks.First().OneNoteTaskID)
				.Select(e => e.FirstAncestor(ns + "Outline"))
				.First();

			using (var outlook = new Outlook())
			{
				foreach (var task in tasks)
				{
					var paragraph = outline.Descendants(ns + "OutlookTask")
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
