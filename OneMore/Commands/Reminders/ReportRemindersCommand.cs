//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ReportRemindersCommand : Command
	{
		private class Item
		{
			public XElement Meta;
			public Reminder Reminder;
			public string Path;
			public int Year;
			public int WoYear;
		}


		private const string HeaderShading = "#DEEBF6";
		private const string NotStartedShading = "#FFF2CC";
		private const string OverdueShading = "#FADBD2";
		private const string CompletedShading = "#E2EFD9";
		private const string HighPriorityColor = "#E84C22";
		private const string MediumPriorityColor = "#5B9BD5";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";

		private OneNote one;
		private Page page;
		private XNamespace ns;
		private XElement container;
		private int heading2Index;
		private int citeIndex;

		private readonly List<Item> active;
		private readonly List<Item> inactive;
		private string[] priorities;
		private string[] statuses;


		public ReportRemindersCommand()
		{
			active = new List<Item>();
			inactive = new List<Item>();

			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote())
			{
				if (!await CollectReminders())
				{
					return;
				}

				string pageId = null;
				if (args.Length > 0 && args[0] is string refreshArg && refreshArg == "refresh")
				{
					page = one.GetPage();
					ns = page.Namespace;
					pageId = page.PageId;
					ClearContent();
				}
				else
				{
					pageId = await FindReport();
					if (pageId == string.Empty)
					{
						return;
					}

					if (pageId == null)
					{
						one.CreatePage(one.CurrentSectionId, out pageId);
						page = one.GetPage(pageId);
						ns = page.Namespace;
						page.SetMeta(MetaNames.ReminderReport, OneNote.Scope.Notebooks.ToString());
						page.Title = Resx.ReminderReport_Title;
					}
					else
					{
						page = one.GetPage(pageId);
						ns = page.Namespace;
						ClearContent();
					}
				}

				PageNamespace.Set(ns);
				heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;
				citeIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;
				container = page.EnsureContentContainer();

				var now = DateTime.Now.ToShortFriendlyString();
				container.Add(
					new Paragraph($"{Resx.ReminderReport_LastUpdated} {now} " +
						$"(<a href=\"onemore://ReportRemindersCommand/refresh\">{Resx.word_Refresh}</a>)"),
					new Paragraph(string.Empty)
					);

				// for some Github cloners, multiline items in the Resx file are delimeted
				// wth only CR instead of NLCR so allow for any possibility
				var delims = new[] { Environment.NewLine, "\r", "\n" };

				priorities = Resx.RemindDialog_priorityBox_Text
					.Split(delims, StringSplitOptions.RemoveEmptyEntries);

				statuses = Resx.RemindDialog_statusBox_Text
					.Split(delims, StringSplitOptions.RemoveEmptyEntries);

				if (active.Any())
				{
					ReportActiveTasks();
				}

				if (inactive.Any())
				{
					ReportInactiveTasks();
				}

				await one.Update(page);
				await one.NavigateTo(pageId);
			}
		}


		private async Task<bool> CollectReminders()
		{
			var hierarchy = await one.SearchMeta(string.Empty, MetaNames.Reminder);
			if (hierarchy == null)
			{
				UIHelper.ShowInfo(one.Window, "Could not create report at this time. Restart OneNote");
				return false;
			}

			var hns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);

			var metas = hierarchy.Descendants(hns + "Meta").Where(e =>
				e.Attribute("name").Value == MetaNames.Reminder &&
				e.Attribute("content").Value.Length > 0);

			if (!metas.Any())
			{
				UIHelper.ShowInfo(one.Window, Resx.ReminderReport_noReminders);
				return false;
			}

			var culture = System.Globalization.CultureInfo.CurrentUICulture;
			var calendar = culture.Calendar;
			var weekRule = culture.DateTimeFormat.CalendarWeekRule;
			var firstDay = culture.DateTimeFormat.FirstDayOfWeek;

			var serializer = new ReminderSerializer();
			foreach (var meta in metas)
			{
				var path = meta.Ancestors().Reverse()
					.Where(m => m.Attribute("name") != null)
					.Select(m => m.Attribute("name").Value)
					.Aggregate((a, b) => $"{a} > {b}");

				var reminders = serializer.DecodeContent(meta.Attribute("content").Value);
				foreach (var reminder in reminders)
				{
					var item = new Item
					{
						Meta = meta,
						Reminder = reminder,
						Path = path,
						Year = reminder.Due.Year,
						WoYear = calendar.GetWeekOfYear(reminder.Due, weekRule, firstDay)
					};

					if (reminder.Status == ReminderStatus.Completed ||
						reminder.Status == ReminderStatus.Deferred)
					{
						inactive.Add(item);
					}
					else
					{
						active.Add(item);
					}
				}
			}

			return true;
		}


		private async Task<string> FindReport()
		{
			var hierarchy = await one.SearchMeta(string.Empty, MetaNames.ReminderReport);
			if (hierarchy == null)
			{
				return null;
			}

			var hns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);

			var pageId = hierarchy.Descendants(hns + "Meta")
				.Where(e => e.Attribute("name").Value == MetaNames.ReminderReport)
				.Select(e => e.Parent.Attribute("ID").Value)
				.FirstOrDefault();

			if (pageId == null)
			{
				return null;
			}

			var current = one.GetPageInfo();
			var target = one.GetPageInfo(pageId);

			await one.NavigateTo(pageId, string.Empty);
			// absurd but NavigateTo needs time to settle down
			await Task.Delay(100);

			var answer = UIHelper.ShowQuestion(Resx.RemindCommand_Reuse, true, true);
			if (answer == System.Windows.Forms.DialogResult.Yes)
			{
				return pageId;
			}

			await one.NavigateTo(current.PageId, string.Empty);
			await Task.Delay(100);

			// kind of hacky but ok...
			return answer == System.Windows.Forms.DialogResult.Cancel ? String.Empty : null;
		}


		private void ClearContent()
		{
			var chalkOutlines = page.Root.Elements(ns + "Outline");
			if (chalkOutlines != null)
			{
				// assume the first outline is the report, reuse it as our container
				chalkOutlines.First().Elements().Remove();

				// remove remaining outlines; this will NOT delete them from the page without
				// and explicit DeletePageContent but this is OK because it will make our
				// EnsureContainer select the proper outline
				chalkOutlines.Skip(1).Remove();
			}
		}


		private void ReportActiveTasks()
		{
			container.Add(
				new Paragraph(Resx.ReminderReport_ActiveReminders).SetQuickStyle(heading2Index),
				new Paragraph(Resx.ReminderReport_ActiveSummary),
				new Paragraph(ns, string.Empty)
				);

			var table = new Table(ns, 1, 6)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 220);
			table.SetColumnWidth(1, 70);
			table.SetColumnWidth(2, 145);
			table.SetColumnWidth(3, 130);
			table.SetColumnWidth(4, 60);
			table.SetColumnWidth(5, 60);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph(Resx.ReminderReport_ReminderColumn).SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(Resx.RemindDialog_statusLabel_Text).SetStyle(HeaderCss));
			row[2].SetContent(new Paragraph(Resx.RemindDialog_startDateLabel_Text).SetStyle(HeaderCss));
			row[3].SetContent(new Paragraph(Resx.RemindDialog_dueDateLabel_Text).SetStyle(HeaderCss));
			row[4].SetContent(new Paragraph(Resx.RemindDialog_priorityLabel_Text).SetStyle(HeaderCss));
			row[5].SetContent(new Paragraph(Resx.RemindDialog_percentLabel_Text).SetStyle(HeaderCss));

			var now = DateTime.UtcNow;

			foreach (var item in active
				.OrderBy(i => i.Year)
				.ThenBy(i => i.WoYear)
				.ThenByDescending(i => i.Reminder.Priority))
			{
				row = table.AddRow();
				row[0].SetContent(MakeReminder(one, item));

				if (item.Reminder.Silent)
				{
					row[1].SetContent(new XElement(ns + "OEChildren",
						new Paragraph(statuses[(int)item.Reminder.Status]),
						new Paragraph(Resx.word_Silenced).SetQuickStyle(citeIndex)
						));
				}
				else
				{
					row[1].SetContent(statuses[(int)item.Reminder.Status]);
				}

				if (item.Reminder.Snooze != SnoozeRange.None &&
					item.Reminder.SnoozeTime.CompareTo(now) > 0)
				{
					var zmsg = string.Format(Resx.ReminderReport_SnoozedUntil,
						item.Reminder.SnoozeTime.ToShortFriendlyString());

					row[2].SetContent(new XElement(ns + "OEChildren",
						new Paragraph(item.Reminder.Start.ToShortFriendlyString()),
						new Paragraph(zmsg).SetQuickStyle(citeIndex)
						));
				}
				else
				{
					row[2].SetContent(item.Reminder.Start.ToShortFriendlyString());
				}

				var woy = string.Format(Resx.ReminderReport_WeekOfYear, item.WoYear);
				row[3].SetContent(new XElement(ns + "OEChildren",
					new Paragraph(item.Reminder.Due.ToShortFriendlyString()),
					new Paragraph(woy).SetQuickStyle(citeIndex)
					));

				row[4].SetContent(MakePriority(item.Reminder.Priority));
				row[5].SetContent((item.Reminder.Percent / 100.0).ToString("P0"));

				if (now.CompareTo(item.Reminder.Due) > 0)
				{
					row[1].ShadingColor = OverdueShading;
				}
				else if (item.Reminder.Status == ReminderStatus.NotStarted &&
					now.CompareTo(item.Reminder.Start) > 0)
				{
					row[1].ShadingColor = NotStartedShading;
				}
			}

			container.Add(
				new Paragraph(ns, table.Root),
				new Paragraph(ns, string.Empty),
				new Paragraph(ns, string.Empty)
				);
		}


		private void ReportInactiveTasks()
		{
			container.Add(
				new Paragraph(Resx.ReminderReport_InactiveReminders).SetQuickStyle(heading2Index),
				new Paragraph(Resx.ReminderReport_InactiveSummary),
				new Paragraph(ns, string.Empty)
				);

			var table = new Table(ns, 1, 5)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 220);
			table.SetColumnWidth(1, 70);
			table.SetColumnWidth(2, 150);
			table.SetColumnWidth(3, 170);
			table.SetColumnWidth(4, 60);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph(Resx.ReminderReport_ReminderColumn).SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(Resx.RemindDialog_statusLabel_Text).SetStyle(HeaderCss));
			row[2].SetContent(new Paragraph(Resx.word_Planned).SetStyle(HeaderCss));
			row[3].SetContent(new Paragraph(Resx.word_Actual).SetStyle(HeaderCss));
			row[4].SetContent(new Paragraph(Resx.RemindDialog_priorityLabel_Text).SetStyle(HeaderCss));

			foreach (var item in inactive
				.OrderBy(i => i.Reminder.Status)
				.ThenByDescending(i => i.Reminder.Completed)
				.ThenByDescending(i => i.Reminder.Priority))
			{
				row = table.AddRow();
				row[0].SetContent(MakeReminder(one, item));
				row[1].SetContent(statuses[(int)item.Reminder.Status]);

				row[2].SetContent(
					new Paragraph(
						$"{Resx.word_Start}: {item.Reminder.Start.ToShortFriendlyString()}<br/>\n" +
						$"{Resx.word_Due}: {item.Reminder.Due.ToShortFriendlyString()}")
					);

				row[4].SetContent(MakePriority(item.Reminder.Priority));

				if (item.Reminder.Status == ReminderStatus.Completed)
				{
					row[1].ShadingColor = CompletedShading;
					row[3].SetContent(
						new Paragraph(
							$"{Resx.word_Started}: {item.Reminder.Started.ToShortFriendlyString()}<br/>\n" +
							$"{Resx.word_Completed}: {item.Reminder.Completed.ToShortFriendlyString()}")
						);
				}
			}

			container.Add(
				new Paragraph(ns, table.Root),
				new Paragraph(ns, string.Empty)
				);
		}


		private XElement MakeReminder(OneNote one, Item item)
		{
			var index = page.AddTagDef(item.Reminder.Symbol, string.Empty);
			var uri = one.GetHyperlink(item.Meta.Parent.Attribute("ID").Value, item.Reminder.ObjectId);

			// uri might be null if making a cross-machine query since objectIDs are ephemeral
			var text = uri != null
				? $"<a href='{uri}'>{item.Reminder.Subject}</a>"
				: item.Reminder.Subject;

			return new XElement(ns + "OEChildren",
				new XElement(ns + "OE",
					new Tag(index, item.Reminder.Status == ReminderStatus.Completed).SetEnabled(false),
					new XElement(ns + "T", new XCData(text))
					),
				new Paragraph(item.Path).SetQuickStyle(citeIndex)
				);
		}


		private XElement MakePriority(ReminderPriority priority)
		{
			var paragraph = new Paragraph(priorities[(int)priority]);
			if (priority == ReminderPriority.High)
			{
				paragraph.SetAttributeValue("style", $"color:{HighPriorityColor};");
			}
			else if (priority == ReminderPriority.Medium)
			{
				paragraph.SetAttributeValue("style", $"color:{MediumPriorityColor};");
			}

			return paragraph;
		}
	}
}
