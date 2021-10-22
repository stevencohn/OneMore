//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.IO;
	using Windows.UI.Notifications;


	internal class Notifier
	{
		private static string imageCache;


		public void Send(string message)
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
