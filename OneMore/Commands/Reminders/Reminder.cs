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


		public int Version { get; set; }

		// parent object Id
		public string ObjectId { get; set; }

		// one:Tag.index
		[JsonIgnore]
		public string TagIndex { get; set; }

		// one:TagDef.symbol
		public string Symbol { get; set; }

		// one:Tag.disabled
		[JsonIgnore]
		public bool Disabled { get; set; }

		public ReminderStatus Status { get; set; }

		public ReminderPriority Priority { get; set; }

		public bool Silent { get; set; }

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


		/// <summary>
		/// Initialize a new instance bound to the given objectId
		/// </summary>
		/// <param name="objectId">The ID of the containing paragraph OE</param>
		public Reminder(string objectId)
		{
			Version = 1;
			ObjectId = objectId;
			Symbol = BellSymbol;
			Created = DateTime.Now;
			Start = DateTime.Now;
			Started = DateTime.Now;
			Due = DateTime.Now;
			Completed = DateTime.Now;
		}
	}
}
