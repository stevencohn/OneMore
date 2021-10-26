//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;


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
			Reminder rem;
			if (cache.ContainsKey(reminder.ObjectId))
			{
				rem = cache[reminder.ObjectId];
			}
			else
			{
				cache.Add(reminder.ObjectId, reminder);
				rem = reminder;
			}

			var now = DateTime.UtcNow;
			var span = now.Subtract(started ? rem.Due : rem.Start);

			// Pattern looks like this:
			//  <  6 mins = every  2 mins -- 2x (  1min ..  7mins)
			//  < 36 mins = every 10 mins -- 3x ( 7mins .. 36mins)
			//  <  4 hrs  = every  2 hour -- 3x (36mins .. 4h36m)
			//  >  4 hrs  = every 12 hours

			if (span.TotalHours > 4)
			{
				rem.ScheduledNotification = now.AddHours(12);
			}
			else if (span.TotalMinutes >= 36)
			{
				rem.ScheduledNotification = now.AddHours(1);
			}
			else if (span.TotalMinutes >= 6)
			{
				rem.ScheduledNotification = now.AddMinutes(10);
			}
			else
			{
				rem.ScheduledNotification = now.AddMinutes(2);
			}

			Logger.Current.WriteLine($"next notification {rem.ScheduledNotification.ToLocalTime()}");
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
				reminder.ScheduledNotification = DateTime.MinValue;
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
			return DateTime.UtcNow.CompareTo(rem.ScheduledNotification) > 0;
		}
	}
}
