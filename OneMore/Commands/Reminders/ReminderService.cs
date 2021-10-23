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
	using System.Xml.Linq;
	using Windows.UI.Notifications;


	internal class ReminderService : Loggable
	{
		private const int sleep = 10000;

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

				// ignore recycle bins
				hierarchy.Elements(ns + "Notebook").Elements(ns + "SectionGroup")
					.Where(e => e.Attribute("isRecycleBin") != null)
					.ToList()
					.ForEach(e => e.Remove());

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
					foreach (var reminder in reminders)
					{
						Test(reminder);
					}
				}
			}

			await Task.Yield();
		}


		private void Test(Reminder reminder)
		{
			if (reminder.Status == ReminderStatus.NotStarted ||
				reminder.Status == ReminderStatus.Waiting)
			{
				if (DateTime.UtcNow.CompareTo(reminder.Start) > 0)
				{
					logger.WriteLine($"reminder {reminder.Status} is post-start: {reminder.Subject}");
				}
			}
			else if (reminder.Status == ReminderStatus.InProgress)
			{
				if (DateTime.UtcNow.CompareTo(reminder.Due) > 0)
				{
					logger.WriteLine($"reminder {reminder.Status} is post-due: {reminder.Subject}");
				}
			}
		}


		private void Send(string message)
		{
			// get a toast XML template
			var doc = ToastNotificationManager
				.GetTemplateContent(ToastTemplateType.ToastImageAndText01);

			// fill in the text elements
			var texts = doc.GetElementsByTagName("text");
			texts[0].AppendChild(doc.CreateTextNode(message));

			// fill in the image
			if (imageCache == null || !File.Exists(imageCache))
			{
				imageCache = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Properties.Resources.Logo.Save(imageCache);
			}

			// this should work with a pack:// resource but I can't figure out the URI!
			var images = doc.GetElementsByTagName("image");
			images[0].Attributes.GetNamedItem("src").NodeValue = imageCache;

			// send the notification
			var toast = new ToastNotification(doc);
			var appID = "OneMore Reminder"; // TODO: translate this!

			ToastNotificationManager.CreateToastNotifier(appID).Show(toast);
		}
	}
}
