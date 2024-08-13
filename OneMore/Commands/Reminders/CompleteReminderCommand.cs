//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class CompleteReminderCommand : Command
	{
		private const string dateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'000K";


		public CompleteReminderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);
			var paragraph = page.Root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.Select(e => e.Parent)
				.FirstOrDefault();

			if (paragraph is null)
			{
				ShowInfo(Resx.RemindCommand_noContext);
				return;
			}

			var serializer = new ReminderSerializer();
			var reminders = serializer.LoadReminders(page);
			if (!reminders.Any())
			{
				ShowError(Resx.RemindCommand_noReminder);
				return;
			}

			var objectID = paragraph.Attribute("objectID").Value;
			var reminder = reminders.Find(r => r.ObjectId == objectID);
			if (reminder is null)
			{
				// second-chance for multi-client users
				var uri = one.GetHyperlink(page.PageId, objectID);
				reminder = reminders.Find(r => r.ObjectUri == uri);
			}

			if (reminder is null)
			{
				ShowError(Resx.RemindCommand_noReminder);
				return;
			}

			XElement tag = null;
			if (!string.IsNullOrEmpty(reminder.Symbol) && reminder.Symbol != "0")
			{
				reminder.TagIndex = page.GetTagDefIndex(reminder.Symbol);
				if (reminder.TagIndex != null)
				{
					// confirm tag still exists
					tag = paragraph.Elements(ns + "Tag")
						.FirstOrDefault(e => e.Attribute("index").Value == reminder.TagIndex);
				}
			}

			if (tag is null)
			{
				reminders.Remove(reminder);
				page.SetMeta(MetaNames.Reminder, serializer.EncodeContent(reminders));
				await one.Update(page);

				ShowError(Resx.RemindCommand_noReminder);
				return;
			}

			// complete the reminder...

			reminder.Status = ReminderStatus.Completed;
			reminder.Percent = 100;
			reminder.Completed = DateTime.UtcNow;
			serializer.StoreReminder(page, reminder);

			tag.Attribute("completed").Value = "true";
			tag.SetAttributeValue("completionDate", reminder.Completed.ToString(dateFormat));

			await one.Update(page);
		}
	}
}
