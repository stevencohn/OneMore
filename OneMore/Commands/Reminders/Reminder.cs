//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;


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
		public string Version;			// schema version
		public string ObjectId;			// -> parent object ID
		public int TagIndex;			// -> Tag index
		public int Priority;
		public int Percent;
		public DateTime Start;			// stored as UTC
		public DateTime Started;		// stored as UTC
		public DateTime Due;			// stored as UTC
		public string Subject;			// restrict to 200 chars

		// Note the following properties are managed directly from the Tag:
		//public bool Disabled;			// -> Tag.disabled
		//public DateTime Created;		// -> Tag.creationDate
		//public DateTime Completed;	// -> Tag.completionDate
	}
}
