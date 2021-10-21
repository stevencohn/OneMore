//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Windows.UI.Notifications;


	internal class Notifier
	{
		public void Send(string message)
		{
			// get a toast XML template
			var doc = ToastNotificationManager
				.GetTemplateContent(ToastTemplateType.ToastImageAndText01);

			// fill in the text elements
			var texts = doc.GetElementsByTagName("text");
			texts[0].AppendChild(doc.CreateTextNode(message));

			// send the notification
			var toast = new ToastNotification(doc);
			var appID = "OneMore Reminder"; // TODO: translate this!

			ToastNotificationManager.CreateToastNotifier(appID).Show(toast);
		}
	}
}
