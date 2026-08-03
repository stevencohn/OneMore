//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Xml.Linq;


	/// <summary>
	/// A single note parsed from an Evernote .enex export file.
	/// </summary>
	internal class EvernoteNote
	{

		public EvernoteNote()
		{
			Tags = new List<string>();
			Resources = new List<EvernoteResource>();
		}


		/// <summary>
		/// The note title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The root &lt;en-note&gt; element parsed from the note's ENML content.
		/// </summary>
		public XElement Content { get; set; }

		/// <summary>
		/// The note's creation timestamp, if present.
		/// </summary>
		public DateTime? Created { get; set; }

		/// <summary>
		/// The note's last-modified timestamp, if present.
		/// </summary>
		public DateTime? Updated { get; set; }

		/// <summary>
		/// Per-note tag labels, in the order they appeared in the ENEX file.
		/// </summary>
		public List<string> Tags { get; }

		/// <summary>
		/// Attachments (images, files, audio, etc.) belonging to this note.
		/// </summary>
		public List<EvernoteResource> Resources { get; }
	}
}
