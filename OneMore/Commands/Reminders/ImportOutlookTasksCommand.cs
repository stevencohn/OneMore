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
		private const string HeaderShading = "#DEEBF6";
		private const string OverdueShading = "#FADBD2";
		private const string CompletedShading = "#E2EFD9";
		private const string HighImportanceColor = "#E84C22";
		private const string NormalImportanceColor = "#5B9BD5";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";

		private OneNote one;
		private Page page;
		private XNamespace ns;
		private string[] importances;


		public ImportOutlookTasksCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			IEnumerable<OutlookTask> tasks = null;

			if (args.Length > 0 && args[0] is string refreshArg && refreshArg == "refresh")
			{
				tasks = ExtractTasks();
				await GenerateTableReport(tasks);
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

			logger.WriteLine($"selected {tasks.Count()} tasks");

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
			}
		}


		private IEnumerable<OutlookTask> ExtractTasks()
		{
			return new List<OutlookTask>();
		}


		private async Task GenerateTableReport(IEnumerable<OutlookTask> tasks)
		{
			using (one = new OneNote(out page, out ns))
			{
				await GenerateTable(tasks);
				await one.Update(page);
			}
		}


		private async Task GenerateTable(IEnumerable<OutlookTask> tasks)
		{
			PageNamespace.Set(ns);
			var heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;
			var citeIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

			// for some Github cloners, multiline items in the Resx file are delimeted
			// wth only CR instead of NLCR so allow for any possibility
			var delims = new[] { Environment.NewLine, "\r", "\n" };

			importances = Resx.OutlookTaskReport_importances
				.Split(delims, StringSplitOptions.RemoveEmptyEntries);

			var statuses = Resx.OutlookTaskReport_statuses
				.Split(delims, StringSplitOptions.RemoveEmptyEntries);

			var table = new Table(ns, 1, 6)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 220);
			table.SetColumnWidth(1, 70);
			table.SetColumnWidth(2, 115);
			table.SetColumnWidth(3, 110);
			table.SetColumnWidth(4, 60);
			table.SetColumnWidth(5, 60);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph(Resx.OutlookTaskReport_Task).SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(Resx.OutlookTaskReport_Status).SetStyle(HeaderCss));
			row[2].SetContent(new Paragraph(Resx.OutlookTaskReport_DueDate).SetStyle(HeaderCss));
			row[3].SetContent(new Paragraph(Resx.OutlookTaskReport_DateCompleted).SetStyle(HeaderCss));
			row[4].SetContent(new Paragraph(Resx.OutlookTaskReport_Importance).SetStyle(HeaderCss));
			row[5].SetContent(new Paragraph(Resx.OutlookTaskReport_Percent).SetStyle(HeaderCss));

			var now = DateTime.UtcNow;

			foreach (var task in OrderTasks(tasks))
			{
				row = table.AddRow();
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

				if (task.DueDate.Year == OutlookTask.UnspecifiedYear)
				{
					row[2].SetContent("-");
				}
				else
				{
					var woy = string.Format(Resx.OutlookTaskReport_Week, task.WoYear);
					row[2].SetContent(new XElement(ns + "OEChildren",
						new Paragraph(task.DueDate.ToShortFriendlyString()),
						new Paragraph(woy).SetQuickStyle(citeIndex)
						));
				}

				if (task.PercentComplete < 100 || task.DateCompleted.Year == OutlookTask.UnspecifiedYear)
				{
					row[3].SetContent("-");
				}
				else
				{
					row[3].SetContent(task.DateCompleted.ToShortFriendlyString());
				}

				row[4].SetContent(MakeImportance(task.Importance));
				row[5].SetContent((task.PercentComplete / 100.0).ToString("P0"));
			}

			var nowf = DateTime.Now.ToShortFriendlyString();

			page.AddNextParagraph(
				new Paragraph(Resx.ReminderReport_ActiveReminders).SetQuickStyle(heading2Index),
				new Paragraph($"{Resx.ReminderReport_LastUpdated} {nowf} " +
					$"(<a href=\"onemore://ReportRemindersCommand/refresh\">{Resx.word_Refresh}</a>)"),
				new Paragraph(string.Empty),
				new Paragraph(table.Root).SetMeta(TableMeta, Guid.NewGuid().ToString("b").ToUpper()),
				new Paragraph(string.Empty),
				new Paragraph(string.Empty)
				);

			await one.Update(page);
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

			return tasks.OrderBy(t => t.FolderPath).ThenBy(t => t.Subject);
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

			// re-fetch page to get IDs of new paragraphs...
			page = one.GetPage(page.PageId, OneNote.PageDetail.Basic);
			ns = page.Namespace;

			// find the containing Outline to optimize the lookup loop below
			var outline = page.Root.Descendants(ns + "OutlookTask")
				.Where(e => e.Attribute("guidTask").Value == ordered.First().OneNoteTaskID)
				.Select(e => e.FirstAncestor(ns + "Outline"))
				.First();

			using (var outlook = new Outlook())
			{
				foreach (var task in ordered)
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
