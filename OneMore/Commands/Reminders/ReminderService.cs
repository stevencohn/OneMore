//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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
		// check one a minute
		private const int sleep = 60000;

		private static string imageCache;


		public void Startup()
		{
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

					await Task.Delay(sleep);
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
						if (!reminder.Silent)
						{
							Test(reminder, pageID);
						}
					}
				}
			}
		}


		private void Test(Reminder reminder, string pageID)
		{
			// is it currently snoozed?
			if (reminder.Snooze != SnoozeRange.None &&
				DateTime.UtcNow.CompareTo(reminder.SnoozeTime) < 0)
			{
				return;
			}

			// is it not started but after the planned start date?
			if (reminder.Status == ReminderStatus.NotStarted ||
				reminder.Status == ReminderStatus.Waiting)
			{
				if (DateTime.UtcNow.CompareTo(reminder.Start) > 0)
				{
					Send(
						string.Format(
							Resx.Reminder_PastStart,
							reminder.Due.ToShortFriendlyString(),
							reminder.Subject
							),
						$"{pageID};{reminder.ObjectId}"
						);
				}
			}
			// is it not completed but after the planne due date?
			else if (reminder.Status == ReminderStatus.InProgress)
			{
				if (DateTime.UtcNow.CompareTo(reminder.Due) > 0)
				{
					Send(
						string.Format(
							Resx.Reminder_PastDue, 
							reminder.Due.ToShortFriendlyString(),
							reminder.Subject
							),
						$"{pageID};{reminder.ObjectId}"
						);
				}
			}
		}


		private void Send(string message, string args)
		{
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
			if (imageCache == null || !File.Exists(imageCache))
			{
				imageCache = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Resx.Logo.Save(imageCache);
			}

			// this should work with a pack:// resource but I can't figure out the URI!
			var images = doc.GetElementsByTagName("image");
			images[0].Attributes.GetNamedItem("src").NodeValue = imageCache;

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
