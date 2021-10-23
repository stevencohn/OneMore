//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class CompleteReminderCommand : Command
	{
		private const string dateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'000K";


		public CompleteReminderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var paragraph = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Parent)
					.FirstOrDefault();

				if (paragraph == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.RemindCommand_noContext);
					return;
				}

				var serializer = new ReminderSerializer();
				var reminders = serializer.LoadReminders(page);
				if (!reminders.Any())
				{
					UIHelper.ShowError(one.Window, Resx.RemindCommand_noReminder);
					return;
				}

				var objectID = paragraph.Attribute("objectID").Value;
				var reminder = reminders.FirstOrDefault(r => r.ObjectId == objectID);
				if (reminder == null)
				{
					UIHelper.ShowError(one.Window, Resx.RemindCommand_noReminder);
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

				if (tag == null)
				{
					reminders.Remove(reminder);
					page.SetMeta(MetaNames.Reminder, serializer.EncodeContent(reminders));
					await one.Update(page);

					UIHelper.ShowError(one.Window, Resx.RemindCommand_noReminder);
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
}
