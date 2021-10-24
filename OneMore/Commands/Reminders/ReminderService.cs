//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Windows.UI.Notifications;


	internal class ReminderService : Loggable
	{
		private const int sleep = 60000;

		private static string imageCache;
		private string appId;


		public void Startup()
		{
			var thread = new Thread(async () =>
			{
				LookupAppId();

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


		private void LookupAppId()
		{
			var builder = new StringBuilder();
			using (var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
					Arguments = @"-command ""(Get-StartApps 'OneMore Reminder').AppID""",
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true
				}
			})
			{
				if (process.Start())
				{
					while (!process.HasExited)
					{
						builder.Append(process.StandardOutput.ReadToEnd());
					}
				}
			}

			// output might only be a newline so ensure there's "enough" content
			if (builder.Length > 2)
			{
				logger.WriteLine($"OneNoteProtocolHandler.appId={appId}");
				appId = builder.ToString();
			}
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

			await Task.Yield();
		}


		private void Test(Reminder reminder, string pageID)
		{
			if (reminder.Status == ReminderStatus.NotStarted ||
				reminder.Status == ReminderStatus.Waiting)
			{
				if (DateTime.UtcNow.CompareTo(reminder.Start) > 0)
				{
					var msg = $"reminder {reminder.Status} is post-start: {reminder.Subject}";
					Send(msg, $"{pageID};{reminder.ObjectId}");
					logger.WriteLine(msg);
				}
			}
			else if (reminder.Status == ReminderStatus.InProgress)
			{
				if (DateTime.UtcNow.CompareTo(reminder.Due) > 0)
				{
					var msg = $"reminder {reminder.Status} is post-due: {reminder.Subject}";
					Send(msg, $"{pageID};{reminder.ObjectId}");
					logger.WriteLine(msg);
				}
			}
		}


		private void Send(string message, string args)
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

			// launch arguments
			doc.DocumentElement.SetAttribute("launch", args);

			logger.WriteLine(XElement.Parse(doc.GetXml()).ToString());

			// send the notification
			var toast = new ToastNotification(doc);

			var id = appId ?? "OneMore Reminder";
			ToastNotificationManager.CreateToastNotifier(id).Show(toast);
		}
	}
}
