//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System;


	/// <summary>
	/// User selected status
	/// </summary>
	internal enum ReminderStatus
	{
		NotStarted,
		InProgress,
		Completed,
		Waiting,
		Deferred
	}


	/// <summary>
	/// User selected priority
	/// </summary>
	internal enum ReminderPriority
	{
		Low,
		Medium,
		High
	}


	/// <summary>
	/// Describes a reminder associated with a tag.
	/// </summary>
	/// <remarks>
	/// Reminders are serialized, compressed, and encoded per Meta.EncodeContent. Given that
	/// meta content attibutes can be up to 262,144 chars long, this means we can store almost
	/// 1000 reminders in a single meta; more than enough for a single page!
	/// </remarks>
	internal class Reminder
	{
		public int Version { get; set; }

		// parent object Id
		public string ObjectId { get; set; }

		// one:Tag.index
		public string TagIndex { get; set; }

		// one:TagDef.symbol
		[JsonIgnore]
		public string Symbol { get; set; }

		// one:Tag.disabled
		[JsonIgnore]
		public bool Disabled { get; set; }

		public ReminderStatus Status { get; set; }

		public ReminderPriority Priority { get; set; }

		public int Percent { get; set; }

		// one:Tab:creationDate
		[JsonIgnore]
		public DateTime Created { get; set; }

		// stored as UTC, displayed local
		public DateTime Start { get; set; }

		// stored as UTC, displayed local
		public DateTime Started { get; set; }

		// stored as UTC, displayed local
		public DateTime Due { get; set; }

		// one:Tag:completionDate
		[JsonIgnore]
		public DateTime Completed { get; set; }

		// maxlen 200 chars
		public string Subject { get; set; }


		public Reminder()
		{
			Version = 1;
			Created = DateTime.Now;
			Start = DateTime.Now;
			Started = DateTime.Now;
			Due = DateTime.Now;
			Completed = DateTime.Now;
		}
	}
}
