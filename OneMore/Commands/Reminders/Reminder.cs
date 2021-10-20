//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using Windows.UI.Notifications;


	/*
	<one:Tag index="0" completed="false" disabled="false" creationDate="2021-10-17T16:18:32.000Z" />
	<one:Tag index="0" completed="true" disabled="false" creationDate="2021-10-17T16:18:32.000Z" completionDate="2021-10-17T17:18:10.000Z" />
	*/

	internal class Reminder
	{
		public string ObjectId;
		public int TagIndex;			// Tag.index
		public int Priority;
		public int Percent;
		//public bool Disabled;			// Tag.disabled
		//public DateTime Created;		// Tag.creationDate
		public DateTime Start;
		public DateTime Started;
		public DateTime Due;
		//public DateTime Completed;	// Tag.completionDate


		public static void X()
		{
			// get a toast XML template
			var doc = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

			// fill in the text elements
			var texts = doc.GetElementsByTagName("text");
			for (int i = 0; i < texts.Length; i++)
			{
				texts[i].AppendChild(doc.CreateTextNode("Line " + i));
			}

			var toast = new ToastNotification(doc);

			var appID = "OneMore Reminder";
			ToastNotificationManager.CreateToastNotifier(appID).Show(toast);
		}
	}
}
