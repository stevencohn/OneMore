//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class RemindCommand : Command
	{
		private Page page;
		private XNamespace ns;


		public RemindCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (args.Length > 0 && args[0] is string arg)
			{
				// received via toast activation
				await NavigateToReminder(arg);
				return;
			}

			using (var one = new OneNote(out page, out ns))
			{
				PageNamespace.Set(ns);

				var paragraph = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Parent)
					.FirstOrDefault();

				if (paragraph == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.RemindCommand_noContext);
					return;
				}

				var reminder = GetReminder(paragraph);

				using (var dialog = new RemindDialog(reminder, false))
				{
					if (dialog.ShowDialog(owner) == DialogResult.OK)
					{
						if (SetReminder(paragraph, dialog.Reminder))
						{
							await one.Update(page);
						}
					}
				}
			}
		}


		private async Task NavigateToReminder(string args)
		{
			var parts = args.Split(';');
			var pageId = parts[0];
			var objectId = parts[1];

			using (var one = new OneNote())
			{
				Native.SetForegroundWindow(one.WindowHandle);
				await one.NavigateTo(pageId, objectId);

				page = one.GetPage(pageId);
				ns = page.Namespace;

				var paragraph = page.Root.Descendants(ns + "OE")
					.FirstOrDefault(e => e.Attribute("objectID").Value == objectId);

				var reminder = GetReminder(paragraph);
				if (reminder == null)
				{
					// TODO: message?
					return;
				}

				using (var dialog = new RemindDialog(reminder, true))
				{
					if (dialog.ShowDialog(owner) == DialogResult.OK)
					{
						if (SetReminder(paragraph, dialog.Reminder))
						{
							await one.Update(page);
						}
					}
				}

				if (reminder.Silent || reminder.Snooze != SnoozeRange.None)
				{
					RemindScheduler.CancelOverride(reminder);
				}
				else
				{
					var started = (
						reminder.Status == ReminderStatus.InProgress ||
						reminder.Status == ReminderStatus.Waiting) && 
						DateTime.UtcNow.CompareTo(reminder.Started) > 0;

					RemindScheduler.ScheduleNotification(reminder, started);
				}
			}
		}


		private Reminder GetReminder(XElement paragraph)
		{
			Reminder reminder;
			XElement tag;

			var objectID = paragraph.Attribute("objectID").Value;

			var reminders = new ReminderSerializer().LoadReminders(page);
			if (reminders.Any())
			{
				reminder = reminders.FirstOrDefault(r => r.ObjectId == objectID);

				if (reminder != null &&
					!string.IsNullOrEmpty(reminder.Symbol) && reminder.Symbol != "0")
				{
					// check tag still exists
					var index = page.GetTagDefIndex(reminder.Symbol);
					if (index != null)
					{
						tag = paragraph.Elements(ns + "Tag")
							.FirstOrDefault(e => e.Attribute("index").Value == index);

						if (tag != null)
						{
							// synchronize reminder with tag
							if (tag.Attribute("completed").Value == "true" &&
								reminder.Status != ReminderStatus.Completed)
							{
								reminder.Status = ReminderStatus.Completed;
								reminder.Percent = 100;
								reminder.Completed = DateTime.Parse(tag.Attribute("completionDate").Value);
							}

							return reminder;
						}
					}
				}
			}

			// either no meta or meta is orphaned from its tag so create a new one...

			var text = paragraph.Value;

			// get only raw text without <span> et al. This direct pattern match feels risky but
			// it seems to work for Unicode strings like Chinese whereas XElement.Parse will fail
			text = Regex.Replace(text, "<[^>]+>", string.Empty);
			if (text.Length > 40)
			{
				text = text.Substring(0, 40) + "...";
			}

			reminder = new Reminder(objectID)
			{
				Subject = text
			};

			tag = paragraph.Elements(ns + "Tag").FirstOrDefault();
			if (tag != null)
			{
				// use existing tag on paragraph, synchronize reminder with this tag
				reminder.TagIndex = tag.Attribute("index").Value;
				reminder.Symbol = page.GetTagDefSymbol(reminder.TagIndex);

				reminder.Created = DateTime.Parse(tag.Attribute("creationDate").Value);

				var completionDate = tag.Attribute("creationDate");
				if (completionDate != null)
				{
					reminder.Completed = DateTime.Parse(completionDate.Value);
				}
			}
			else
			{
				// use default symbol
				// if dialog is cancelled, OneNote will clean up the unused TagDef
				reminder.TagIndex = page.AddTagDef(reminder.Symbol,
					string.Format(Resx.RemindCommand_nameFormat, reminder.Due.ToFriendlyString()));
			}

			return reminder;
		}


		private bool SetReminder(XElement paragraph, Reminder reminder)
		{
			var index = page.GetTagDefIndex(reminder.Symbol);
			if (index == null)
			{
				index = page.AddTagDef(reminder.Symbol,
					string.Format(Resx.RemindCommand_nameFormat, reminder.Due.ToFriendlyString()));

				reminder.TagIndex = index;
			}

			var tag = paragraph.Elements(ns + "Tag")
				.FirstOrDefault(e => e.Attribute("index").Value == index);

			if (tag == null)
			{
				// tags must be ordered by index even within their containing paragraph
				// so take all, remove from paragraph, append, sort, re-add...

				var tags = paragraph.Elements(ns + "Tag").ToList();
				tags.ForEach(t => t.Remove());

				// synchronize tag with reminder
				var completed = reminder.Status == ReminderStatus.Completed
					? "true" : "false";

				tag = new XElement(ns + "Tag",
					new XAttribute("index", index),
					new XAttribute("completed", completed),
					new XAttribute("disabled", "false")
					);

				tags.Add(tag);

				paragraph.AddFirst(tags.OrderBy(t => t.Attribute("index").Value));
			}
			else
			{
				// synchronize tag with reminder
				var tcompleted = tag.Attribute("completed").Value == "true";
				var rcompleted = reminder.Status == ReminderStatus.Completed;
				if (tcompleted != rcompleted)
				{
					tag.Attribute("completed").Value = rcompleted ? "true" : "false";
				}
			}

			new ReminderSerializer().StoreReminder(page, reminder);

			return true;
		}
	}
}
