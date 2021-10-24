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
		}


		private const string HeaderShading = "#DEEBF6";
		private const string OverdueShading = "#FADBD2";
		private const string CompletedShading = "#E2EFD9";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string DateFormat = "MMM d, yyyy h:mm tt";

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
			using (var one = new OneNote())
			{
				var hierarchy = await one.SearchMeta(string.Empty, MetaNames.Reminder);
				var hns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);

				var metas = hierarchy.Descendants(hns + "Meta").Where(e =>
					e.Attribute("name").Value == MetaNames.Reminder &&
					e.Attribute("content").Value.Length > 0);

				if (!metas.Any())
				{
					UIHelper.ShowInfo(one.Window, Resx.ReminderReport_noReminders);
					return;
				}

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
							Path = path
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

				Page page;
				string pageId = null;
				if (args.Length > 0 && args[0] is string refreshArg && refreshArg == "refresh")
				{
					page = one.GetPage();
					pageId = page.PageId;
					container = page.EnsureContentContainer();
					container.Elements().Remove();
				}
				else
				{
					one.CreatePage(one.CurrentSectionId, out pageId);
					page = one.GetPage(pageId);
					page.Title = Resx.ReminderReport_Title;
					container = page.EnsureContentContainer();
				}

				ns = page.Namespace;
				PageNamespace.Set(ns);
				heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;
				citeIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

				container.Add(
					new Paragraph($"{Resx.ReminderReport_LastUpdated} {DateTime.Now.ToString(DateFormat)} " +
						$"(<a href=\"onemore://ReportRemindersCommand/refresh\">" +
						$"{Resx.ReminderReport_Refresh}</a>)"),
					new Paragraph(string.Empty)
					);

				priorities = Resx.RemindDialog_priorityBox_Text
					.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				statuses = Resx.RemindDialog_statusBox_Text
					.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				if (active.Any())
				{
					ReportActiveTasks(one);
				}

				if (inactive.Any())
				{
					ReportInactiveTasks(one);
				}

				await one.Update(page);
				await one.NavigateTo(pageId);
			}
		}


		private void ReportActiveTasks(OneNote one)
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
			table.SetColumnWidth(2, 130);
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

			foreach (var item in active
				.OrderByDescending(i => i.Reminder.Priority)
				.ThenBy(i => i.Reminder.Due))
			{
				row = table.AddRow();

				var uri = one.GetHyperlink(item.Meta.Parent.Attribute("ID").Value, item.Reminder.ObjectId);
				row[0].SetContent(new XElement(ns + "OEChildren",
					new Paragraph($"<a href='{uri}'>{item.Reminder.Subject}</a>"),
					new Paragraph(item.Path).SetQuickStyle(citeIndex)
					));

				row[1].SetContent(statuses[(int)item.Reminder.Status]);
				if (DateTime.UtcNow.CompareTo(item.Reminder.Due) > 0)
				{
					row[1].ShadingColor = OverdueShading;
				}

				row[2].SetContent(item.Reminder.Start.ToString(DateFormat));
				row[3].SetContent(item.Reminder.Due.ToString(DateFormat));
				row[4].SetContent(priorities[(int)item.Reminder.Priority]);
				row[5].SetContent((item.Reminder.Percent / 100.0).ToString("P0"));
			}

			container.Add(
				new Paragraph(ns, table.Root),
				new Paragraph(ns, string.Empty),
				new Paragraph(ns, string.Empty)
				);
		}


		private void ReportInactiveTasks(OneNote one)
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
			table.SetColumnWidth(2, 130);
			table.SetColumnWidth(3, 130);
			table.SetColumnWidth(4, 60);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph(Resx.ReminderReport_ReminderColumn).SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(Resx.RemindDialog_statusLabel_Text).SetStyle(HeaderCss));
			row[2].SetContent(new Paragraph(Resx.RemindDialog_startDateLabel_Text).SetStyle(HeaderCss));
			row[3].SetContent(new Paragraph(Resx.RemindDialog_completedLabel_Text).SetStyle(HeaderCss));
			row[4].SetContent(new Paragraph(Resx.RemindDialog_priorityLabel_Text).SetStyle(HeaderCss));

			foreach (var item in inactive
				.OrderBy(i => i.Reminder.Status)
				.ThenByDescending(i => i.Reminder.Completed)
				.ThenByDescending(i => i.Reminder.Priority))
			{
				row = table.AddRow();

				var uri = one.GetHyperlink(item.Meta.Parent.Attribute("ID").Value, item.Reminder.ObjectId);
				row[0].SetContent(new XElement(ns + "OEChildren",
					new Paragraph($"<a href='{uri}'>{item.Reminder.Subject}</a>"),
					new Paragraph(item.Path).SetQuickStyle(citeIndex)
					));

				row[1].SetContent(statuses[(int)item.Reminder.Status]);
				if (item.Reminder.Status == ReminderStatus.Completed)
				{
					row[1].ShadingColor = CompletedShading;
				}

				row[2].SetContent(item.Reminder.Start.ToString(DateFormat));

				if (item.Reminder.Status == ReminderStatus.Completed)
				{
					row[3].SetContent(item.Reminder.Completed.ToString(DateFormat));
				}

				row[4].SetContent(priorities[(int)item.Reminder.Priority]);
			}

			container.Add(
				new Paragraph(ns, table.Root),
				new Paragraph(ns, string.Empty)
				);
		}
	}
}
