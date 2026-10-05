//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	/// <summary>
	/// What identifies a favorite's target in a file that is carried to another database. Nothing
	/// else a favorite stores can: the OneNote IDs are different on every machine, and the page
	/// key is local to one database and is never written to a file.
	/// </summary>
	/// <remarks>
	/// For a page it is the title and creation time together with the keys of its notebook and
	/// section. For a notebook, a section group or a section it is just the keys. The keys are
	/// made of the path or name of the notebook and the names of the section and its groups, so
	/// they mean the same thing on another machine that has the same notebook.
	/// </remarks>
	[Newtonsoft.Json.JsonObject(ItemNullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
	internal sealed class TargetFingerprint
	{
		public string Title { get; set; }

		public string Created { get; set; }

		public string NotebookKey { get; set; }

		public string SectionKey { get; set; }


		/// <summary>
		/// Gets whether there is enough to find a page by: its title, creation time and the keys of
		/// its notebook and section.
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		public bool IdentifiesAPage =>
			!string.IsNullOrEmpty(Title) &&
			!string.IsNullOrEmpty(Created) &&
			NotebookKey is not null &&
			SectionKey is not null;
	}
}
