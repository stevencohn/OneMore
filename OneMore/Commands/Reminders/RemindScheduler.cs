//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Text;


	/// <summary>
	/// Tracks Reminder notifications that have been delivered but not yet activated.
	/// Allows ReminderService to avoid duplicate notifications for each reminder in between
	/// user intervention - or at least be less annoying to the user by spreading them out
	/// in a progressive pattern.
	/// 
	/// Note however, there is currently no way of telling if a toast notification was
	/// dismissed but not activated. No further notification will be sent out, it's similar
	/// to silencing the reminder.
	/// </summary>
	internal static class RemindScheduler
	{
		private const int SleepyMinutes = 5;

		/// <summary>
		/// Amount of time (ms) the ReminderService sleeps in between its scans
		/// </summary>
		public const int SleepyTime = 60000 * SleepyMinutes;


		private static readonly Dictionary<string, Reminder> cache 
			= new Dictionary<string, Reminder>();


		/// <summary>
		/// Schedule the next notification for the given reminder using a progressive pattern
		/// based on the distance from the target time: Start if not started or Due if not
		/// completed.
		/// </summary>
		/// <param name="reminder">The reminder to schedule</param>
		/// <param name="started">True if the task is started; false if not completed</param>
		public static void ScheduleNotification(Reminder reminder, bool started)
		{
			// always replace in the cache so it's updated with latest start/due values
			Reminder rem = reminder;
			if (cache.ContainsKey(reminder.ObjectId))
			{
				cache[reminder.ObjectId] = reminder;
			}
			else
			{
				cache.Add(reminder.ObjectId, reminder);
			}

			var now = DateTime.UtcNow;
			var span = now.Subtract(started ? rem.Due : rem.Start);

			//if (started)
			//{
			//	Logger.Current.WriteLine($"check due {rem.Due.ToLocalTime()} span={span.TotalHours}/{span.TotalMinutes}");
			//}
			//else
			//{
			//	Logger.Current.WriteLine($"check start {rem.Start.ToLocalTime()} span={span.TotalHours}/{span.TotalMinutes}");
			//}

			// Pattern rules...

			if (span.TotalHours > 4)
			{
				rem.ScheduledNotification = now.AddHours(12);
				//Logger.Current.WriteLine($"scheduled+12h {rem.ScheduledNotification.ToLocalTime()}");
			}
			else if (span.TotalMinutes >= 60)
			{
				// will fire 3x (2hr .. 4hr)
				rem.ScheduledNotification = now.AddHours(1);
				//Logger.Current.WriteLine($"scheduled+1h {rem.ScheduledNotification.ToLocalTime()}");
			}
			else if (span.TotalMinutes >= (SleepyMinutes * 4))
			{
				// with sleep=5mins this will fire 3x (@ 20, 40, and 60mins)
				rem.ScheduledNotification = now.AddMinutes(SleepyMinutes * 2);
				//Logger.Current.WriteLine($"scheduled+2 {rem.ScheduledNotification.ToLocalTime()}");
			}
			else
			{
				// with sleep=5mins this will fire 3x after initial scan (5, 10, and 15mins)
				rem.ScheduledNotification = now.AddMinutes(SleepyMinutes);
				//Logger.Current.WriteLine($"scheduled+1 {rem.ScheduledNotification.ToLocalTime()}");
			}
		}


		/// <summary>
		/// Removes the specified reminder from the scheduler after user intervention
		/// </summary>
		/// <param name="reminder">The reminder to remove</param>
		public static void CancelOverride(Reminder reminder)
		{
			if (cache.ContainsKey(reminder.ObjectId))
			{
				cache.Remove(reminder.ObjectId);
				reminder.ScheduledNotification = DateTime.MaxValue;
			}
		}


		/// <summary>
		/// Determines if we're still waiting on the next scheduled notification for the
		/// given reminder, meaning the next notification is scheduled for the future.
		/// </summary>
		/// <param name="reminder">The reminder to test</param>
		/// <returns>True if we're still waiting; do not send a toast!</returns>
		public static bool WaitingOn(Reminder reminder)
		{
			if (!cache.ContainsKey(reminder.ObjectId))
			{
				return false;
			}

			var rem = cache[reminder.ObjectId];
			return DateTime.UtcNow.CompareTo(rem.ScheduledNotification) < 0;
		}


		internal static void ReportDiagnostics(StringBuilder builder)
		{
			builder.AppendLine();
			builder.AppendLine("RemindScheduler Cache");
			foreach (var reminder in cache.Values)
			{
				var subject = reminder.Subject;
				if (subject.Length > 40) subject = subject.Substring(0, 38);

				var next = reminder.ScheduledNotification.ToLocalTime().ToString();
				var start = reminder.Start.ToLocalTime().ToString();
				var due = reminder.Due.ToLocalTime().ToString();
				builder.AppendLine($"{subject,-20} next:{next,-23} start:{start,-23} due:{due}");
			}
		}
	}
}
