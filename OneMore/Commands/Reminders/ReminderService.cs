//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Win32;
	using River.OneMoreAddIn.Models;
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Windows.UI.Notifications;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ReminderService : Loggable
	{
		private static string logoFile;


		public void Startup()
		{
			logger.WriteLine("starting reminder service");

			var thread = new Thread(async () =>
			{
				// 'errors' allows repeated consecutive exceptions but limits that to 5 so we
				// don't fall into an infinite loop. If it somehow miraculously recovers then
				// errors is reset back to zero and normal processing continues...

				var errors = 0;
				while (errors < 5)
				{
					try
					{
						await Scan();
						errors = 0;
					}
					catch (Exception exc)
					{
						logger.WriteLine($"reminder service exception {errors}", exc);
						errors++;
					}

					await Task.Delay(RemindScheduler.SleepyTime);
				}

				logger.WriteLine("reminder service has stopped; check for exceptions above");
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}


		private async Task Scan()
		{
			using (var one = new OneNote())
			{
				var hierarchy = await one.SearchMeta(string.Empty, MetaNames.Reminder);
				if (hierarchy == null)
				{
					// may need to restart OneNote
					return;
				}

				var ns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);

				var metas = hierarchy.Descendants(ns + "Meta").Where(e =>
					e.Attribute("name").Value == MetaNames.Reminder &&
					e.Attribute("content").Value.Length > 0);

				if (!metas.Any())
				{
					return;
				}

				var serializer = new ReminderSerializer();
				foreach (var meta in metas)
				{
					var reminders = serializer.DecodeContent(meta.Attribute("content").Value);
					var pageID = meta.Parent.Attribute("ID").Value;
					foreach (var reminder in reminders)
					{
						if (reminder.Silent)
						{
							continue;
						}

						if (reminder.Snooze != SnoozeRange.None &&
							DateTime.UtcNow.CompareTo(reminder.SnoozeTime) < 0)
						{
							continue;
						}

						if (RemindScheduler.WaitingOn(reminder))
						{
							continue;
						}

						await Test(reminder, pageID, one);
					}
				}
			}
		}


		private async Task Test(Reminder reminder, string pageID, OneNote one)
		{
			// is it not started but after the planned start date?
			if (reminder.Status == ReminderStatus.NotStarted ||
				reminder.Status == ReminderStatus.Waiting)
			{
				if (DateTime.UtcNow.CompareTo(reminder.Start) > 0 &&
					await ReminderIsValid(reminder, pageID, one))
				{
					Send(
						string.Format(
							Resx.Reminder_PastStart,
							reminder.Due.ToShortFriendlyString(),
							reminder.Subject
							),
						$"{pageID};{reminder.ObjectId}"
						);

					RemindScheduler.ScheduleNotification(reminder, false);
				}
			}
			// is it not completed but after the planned due date?
			else if (reminder.Status == ReminderStatus.InProgress)
			{
				if (DateTime.UtcNow.CompareTo(reminder.Due) > 0 &&
					await ReminderIsValid(reminder, pageID, one))
				{
					Send(
						string.Format(
							Resx.Reminder_PastDue,
							reminder.Due.ToShortFriendlyString(),
							reminder.Subject
							),
						$"{pageID};{reminder.ObjectId}"
						);

					RemindScheduler.ScheduleNotification(reminder, true);
				}
			}
		}


		// if the user deletes the paragraph containing a reminder, the Meta will be orphaned
		// so this verifies that the paragraph still exists and clears the Meta if it does not
		private async Task<bool> ReminderIsValid(Reminder reminder, string pageID, OneNote one)
		{
			var page = one.GetPage(pageID, OneNote.PageDetail.Basic);
			if (page == null)
			{
				// must be an error?
				logger.WriteLine($"reminder page not found {pageID}");
				return false;
			}

			if (page.Root.Descendants(page.Namespace + "OE")
				.Any(e => e.Attribute("objectID").Value == reminder.ObjectId))
			{
				// reminder is valid, keep it!
				return true;
			}

			// clear the orphaned reminder...

			var serializer = new ReminderSerializer();
			var reminders = serializer.LoadReminders(page);
			if (!reminders.Any())
			{
				// must be an error?
				logger.WriteLine($"reminder not found on page {pageID}");
				return false;
			}

			var orphan = reminders.FirstOrDefault(r => r.ObjectId == reminder.ObjectId);
			if (orphan == null)
			{
				// must be an error?
				logger.WriteLine($"reminder not found in page meta {pageID}");
				return false;
			}

			reminders.Remove(orphan);
			page.SetMeta(MetaNames.Reminder, serializer.EncodeContent(reminders));
			await one.Update(page);

			return false;
		}


		private bool warned;
		private void Send(string message, string args)
		{
			// this is for debugging; if SilentReminders value exists then only log
			using (var key = Registry.ClassesRoot.OpenSubKey(@"River.OneMoreAddIn", false))
			{
				if (key != null)
				{
					if (!warned)
					{
						logger.WriteLine("HKCR::River.OneMoreAddin SilientReminders is set to true");
						warned = true;
					}

					if ((string)key.GetValue("SilentReminders") == "true")
					{
						logger.WriteLine($"Toast: {message}");
						return;
					}
				}
			}

			/*
			<toast launch="onemore://RemindCommand/{pageid};{objectid}" activationType="protocol">
			  <visual>
				<binding template="ToastImageAndText01">
				  <image id="1" src="C:\Users\steve\AppData\Local\Temp\ylgs0dca.wry" />
				  <text id="1">Task is past its start date of Nov 1, 2021 9:05 AM, "Now is the time". Click here to navigate to this task</text>
				</binding>
			  </visual>
			</toast>
			*/

			// get a toast XML template
			var doc = ToastNotificationManager
				.GetTemplateContent(ToastTemplateType.ToastImageAndText01);

			// fill in the text elements
			var texts = doc.GetElementsByTagName("text");
			texts[0].AppendChild(doc.CreateTextNode(message));

			// fill the image...
			if (logoFile == null || !File.Exists(logoFile))
			{
				logoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Resx.Logo.Save(logoFile);
			}

			// this should work with a pack:// resource but I can't figure out the URI!
			var images = doc.GetElementsByTagName("image");
			images[0].Attributes.GetNamedItem("src").NodeValue = logoFile;

			// launch arguments; setting protocol is key to making this work with onemore://
			// and not having to register a Start Menu application with an AppID
			doc.DocumentElement.SetAttribute("launch", $"onemore://RemindCommand/{args}");
			doc.DocumentElement.SetAttribute("activationType", "protocol");

			//logger.WriteLine(System.Xml.Linq.XElement.Parse(doc.DocumentElement.GetXml()));

			// send the notification
			ToastNotificationManager
				.CreateToastNotifier(Resx.Reminder_ToastTitle)
				.Show(new ToastNotification(doc));
		}
	}
}
