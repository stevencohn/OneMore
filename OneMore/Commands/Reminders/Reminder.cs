//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
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
		// listed in sort order...

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
		// listed in sort order...

		Low,
		Medium,
		High
	}


	/// <summary>
	/// User selected snooze time
	/// </summary>
	internal enum SnoozeRange
	{
		None,
		S5minutes,
		S10minutes,
		S15minutes,
		S30minutes,
		S1hour,
		S2hours,
		S4hours,
		S1day,
		S2days,
		S3days,
		S1week,
		S2weeks
	}


	/// <summary>
	/// Describes a reminder associated with a tag.
	/// </summary>
	/// <remarks>
	/// Reminders are serialized, compressed, encoded, and stored in a page level Meta. Storing
	/// at the page level is necessary so they are discoverable using the one.FindMeta API, which
	/// unfortunately does not find Meta elements nested within the body of a page. But given that
	/// Meta content attibutes can be up to 262,144 chars long, this means we can store almost
	/// 1000 encoded reminders in a single Meta; more than enough for a single page!
	/// </remarks>
	internal class Reminder
	{
		private const string BellSymbol = "97";


		/// <summary>
		/// Gets the schema version of this model
		/// </summary>
		public int Version { get; set; }


		/// <summary>
		/// Gets or sets the ID of the tagged paragraph. This is a same-session fast-path
		/// hint only; it is not stable across machines and must never be relied on as the
		/// sole source of truth for identity. See AnchorId.
		/// </summary>
		public string ObjectId { get; set; }


		/// <summary>
		/// Gets or sets the URI of the tagged paragraph
		/// </summary>
		public string ObjectUri { get; set; }


		/// <summary>
		/// Gets or sets the OneMore-owned GUID anchor stored in a dedicated one:Meta nested
		/// in the tagged OE. This is the authoritative, cross-machine-stable identity of the
		/// reminder's paragraph; ObjectId and ObjectUri are both native OneNote IDs that are
		/// only reliable within a single client/session. Null for reminders created before
		/// this field was introduced, until migrated by ReminderLocator.
		/// </summary>
		public string AnchorId { get; set; }


		/// <summary>
		/// Gets or sets the free-text name or identifier of the person this reminder is
		/// assigned to. Not validated against notebook contributors since the assignee may
		/// not correspond to anyone with current access to the notebook. Informational only;
		/// does not affect notification delivery.
		/// </summary>
		public string Assignee { get; set; }


		// one:Tag.index
		[JsonIgnore]
		public string TagIndex { get; set; }


		/// <summary>
		/// Gets or sets the associated tag symbol ID, corresponds to one:TagDef.symbol
		/// </summary>
		public string Symbol { get; set; }


		// one:Tag.disabled
		[JsonIgnore]
		public bool Disabled { get; set; }


		/// <summary>
		/// Gets or sets the assigned status of this reminder
		/// </summary>
		public ReminderStatus Status { get; set; }


		/// <summary>
		/// Gets or sets the assigned priority of this reminders
		/// </summary>
		public ReminderPriority Priority { get; set; }


		/// <summary>
		/// Gets or sets the silenced mode; true if silenced
		/// </summary>
		public bool Silent { get; set; }


		/// <summary>
		/// Gets or set the assigned snooze range
		/// </summary>
		public SnoozeRange Snooze { get; set; }


		/// <summary>
		/// Gets or sets the time until which this reminder is snoozed
		/// </summary>
		public DateTime SnoozeTime { get; set; }


		/// <summary>
		/// Gets or sets the percent complete
		/// </summary>
		public int Percent { get; set; }


		// one:Tab:creationDate
		[JsonIgnore]
		public DateTime Created { get; set; }


		/// <summary>
		/// Gets or sets the planned start date, stored as UTC, dispalyed as local
		/// </summary>
		public DateTime Start { get; set; }


		/// <summary>
		/// Gets or sets the actual start date, stored as UTC, dispalyed as local
		/// </summary>
		public DateTime Started { get; set; }


		/// <summary>
		/// Gets or sets the planned due date, stored as UTC, dispalyed as local
		/// </summary>
		public DateTime Due { get; set; }


		/// <summary>
		/// Gets or sets the actual completin date, stored as UTC, dispalyed as local.
		/// This is a copy of the Tag.completionDate attribute but we persist it here as
		/// well so the Report command doesn't need to dive into the page to look at the tag
		/// </summary>
		public DateTime Completed { get; set; }


		/// <summary>
		/// Gets or sets the subject, max is 200 characters.
		/// Defaults to the text of the paragraph but can be overriden by the user
		/// </summary>
		public string Subject { get; set; }


		[JsonIgnore]
		public DateTime ScheduledNotification { get; set; }


		/// <summary>
		/// Initialize a new instance bound to the given objectId
		/// </summary>
		/// <param name="objectId">The ID of the containing paragraph OE</param>
		public Reminder(string objectId)
		{
			var now = DateTime.UtcNow;

			Version = 3;
			ObjectId = objectId;
			Symbol = BellSymbol;
			Assignee = string.Empty;
			Created = now;
			Start = now;
			Started = now;
			Due = now;
			Completed = now;
		}
	}
}
