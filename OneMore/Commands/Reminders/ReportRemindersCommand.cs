//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
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
		}


		private const string HeaderShading = "#DEEBF6";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";

		private XNamespace ns;
		private XElement container;
		private int heading1Index;
		private int heading2Index;

		private readonly List<Item> active;
		private readonly List<Item> inactive;


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
					UIHelper.ShowInfo(one.Window, "No reminders to report");
					return;
				}

				var serializer = new ReminderSerializer();
				foreach (var meta in metas)
				{
					var reminders = serializer.DecodeContent(meta.Attribute("content").Value);
					foreach (var reminder in reminders)
					{
						var item = new Item
						{
							Meta = meta,
							Reminder = reminder
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

				one.CreatePage(one.CurrentSectionId, out var pageId);
				var page = one.GetPage(pageId);
				page.Title = "Reminder Report"; // Resx.AnalyzeCommand_Title;
				container = page.EnsureContentContainer();

				ns = page.Namespace;
				PageNamespace.Set(ns);

				heading1Index = page.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
				heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;

				container.Add(
					new Paragraph("Reminder Summary").SetQuickStyle(heading1Index),
					new Paragraph("blah blah blah)"),
					new Paragraph(string.Empty)
					);

				ReportActiveTasks();

				await one.Update(page);
				await one.NavigateTo(pageId);
			}
		}


		private void ReportActiveTasks()
		{
			container.Add(
				new Paragraph("Active Reminders").SetQuickStyle(heading2Index),
				new Paragraph("blah blah blah"),
				new Paragraph(ns, string.Empty)
				);

			var table = new Table(ns, 1, 6)
			{
				HasHeaderRow = true
				//,BordersVisible = true
			};

			table.SetColumnWidth(0, 200);
			table.SetColumnWidth(1, 70);
			table.SetColumnWidth(2, 130);
			table.SetColumnWidth(3, 130);
			table.SetColumnWidth(4, 60);
			table.SetColumnWidth(5, 60);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph("Reminder").SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph("Status").SetStyle(HeaderCss));
			row[2].SetContent(new Paragraph("Start").SetStyle(HeaderCss));
			row[3].SetContent(new Paragraph("Due").SetStyle(HeaderCss));
			row[4].SetContent(new Paragraph("Priority").SetStyle(HeaderCss));
			row[5].SetContent(new Paragraph("% Complete").SetStyle(HeaderCss).SetAlignment("center"));

			foreach (var item in active)
			{
				row = table.AddRow();
				row[0].SetContent(item.Reminder.Subject);
				row[1].SetContent(item.Reminder.Status.ToString());
				row[2].SetContent(item.Reminder.Start.ToFriendlyString());
				row[3].SetContent(item.Reminder.Due.ToFriendlyString());
				row[4].SetContent(item.Reminder.Priority.ToString());
				row[5].SetContent(item.Reminder.Percent.ToString("P"));
			}

			container.Add(
				new Paragraph(ns, table.Root),
				new Paragraph(ns, string.Empty)
				);
		}
	}
}
