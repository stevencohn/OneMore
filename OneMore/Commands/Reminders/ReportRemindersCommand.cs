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


	internal class ReportRemindersCommand : Command
	{
		private class Item
		{
			public XElement Meta;
			public Reminder Reminder;
		}


		private const string HeaderShading = "#DEEBF6";
		private const string Header2Shading = "#F2F2F2";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string LineCss = "font-family:'Courier New';font-size:10.0pt";

		private XNamespace ns;
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
						var item = new Item { Meta = meta, Reminder = reminder };

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

				ns = page.Namespace;
				PageNamespace.Set(ns);

				heading1Index = page.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
				heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;
			}

			await Task.Yield();
		}
	}
}
