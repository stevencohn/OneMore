//************************************************************************************************
// Copyright © 2024 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	internal class CleanRemindersCommand : Command
	{
		public CleanRemindersCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			var serializer = new ReminderSerializer();
			var reminders = serializer.LoadReminders(page);
			if (!reminders.Any())
			{
				logger.WriteLine("no reminders found on current page");
				return;
			}

			var map = new List<string>();
			page.Root.Descendants(ns + "OE").ForEach(e =>
			{
				map.Add(one.GetHyperlink(page.PageId, e.Attribute("objectID").Value));
			});

			var count = 0;
			for (var i = reminders.Count - 1; i >= 0; i--)
			{
				var reminder = reminders[i];

				var subject = reminder.Subject;
				if (subject.Length > 40) subject = subject.Substring(0, 38);
				var start = reminder.Start.ToLocalTime().ToString();
				var due = reminder.Due.ToLocalTime().ToString();

				if (!page.Root.Descendants(ns + "OE").Any(e =>
					e.Attribute("objectID").Value == reminder.ObjectId))
				{
					if (string.IsNullOrEmpty(reminder.ObjectUri) ||
						!map.Contains(reminder.ObjectUri))
					{
						logger.WriteLine($"removing orphaned reminder:{start,22} due:{due,22} \"{subject}\"");
						reminders.RemoveAt(i);
						count++;
					}
				}
			}

			if (count > 0)
			{
				ShowInfo(string.Format(Resx.CleanRemindersCommand_count, count));

				page.SetMeta(MetaNames.Reminder, serializer.EncodeContent(reminders));
				await one.Update(page);
			}
		}
	}
}

