//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Identity
{
	using System;
	using System.Collections.Generic;
	using System.Linq;


	/// <summary>
	/// A page found in the hierarchy, with what later stages need to act on it without
	/// reading the hierarchy again.
	/// </summary>
	internal sealed class IdentityPage
	{
		public IdentityPage(PageRef page, string notebookID, string sectionID, string sectionPath, bool isTagIndex)
		{
			Ref = page;
			NotebookID = notebookID;
			SectionID = sectionID;
			SectionPath = sectionPath;
			IsTagIndex = isTagIndex;
		}


		/// <summary>The page as the identity layer sees it.</summary>
		public PageRef Ref { get; }

		/// <summary>The OneNote ID of the notebook, valid only until it is next reopened.</summary>
		public string NotebookID { get; }

		/// <summary>The OneNote ID of the section, valid only until its notebook is reopened.</summary>
		public string SectionID { get; }

		/// <summary>The path of the section by name, such as /Notebook/Group/Section.</summary>
		public string SectionPath { get; }

		/// <summary>True for a page OneMore generated to index hashtags, which is not content.</summary>
		public bool IsTagIndex { get; }

		/// <summary>Gets how the page was matched to its identity; set by the pass.</summary>
		public PageResolution Resolution { get; internal set; }

		/// <summary>Gets the page key, or zero if the page has not been resolved.</summary>
		public long PageKey => Resolution?.PageKey ?? 0;

		/// <summary>
		/// Gets the page GUID of a hyperlink to the page, or null if it has not been read yet; see
		/// <see cref="LinkGuids"/>. It is not unique, so more than one page can share it.
		/// </summary>
		public string PageGuid { get; internal set; }
	}


	/// <summary>
	/// The kinds of container, other than a notebook, that can hold pages.
	/// </summary>
	internal enum ContainerKind
	{
		Section,
		SectionGroup
	}


	/// <summary>
	/// A section or a section group, found in the hierarchy whether or not it holds any pages.
	/// </summary>
	internal sealed class IdentityContainer
	{
		public IdentityContainer(ContainerKind kind, string id, string name, string sectionKey, string path)
		{
			Kind = kind;
			ID = id;
			Name = name;
			SectionKey = sectionKey;
			Path = path;
		}


		public ContainerKind Kind { get; }

		/// <summary>The OneNote ID, valid only until the notebook is next reopened.</summary>
		public string ID { get; }

		public string Name { get; }

		/// <summary>Identifies it within its notebook by the names of its groups and its own name;
		/// see <see cref="PageIdentityKeys.SectionKey"/>.</summary>
		public string SectionKey { get; }

		/// <summary>The path by name, such as /Notebook/Group/Section.</summary>
		public string Path { get; }
	}


	/// <summary>
	/// An open notebook and the pages found in it.
	/// </summary>
	internal sealed class IdentityNotebook
	{
		public IdentityNotebook(string id, string name, string key)
		{
			ID = id;
			Name = name;
			Key = key;
			Path = "/" + name;
		}


		/// <summary>The OneNote ID, valid only until the notebook is next reopened.</summary>
		public string ID { get; }

		public string Name { get; }

		/// <summary>Identifies the notebook by path or name; see <see cref="PageIdentityKeys"/>.</summary>
		public string Key { get; }

		/// <summary>The path of the notebook by name, the start of every section path.</summary>
		public string Path { get; }

		/// <summary>
		/// Gets whether the notebook's hierarchy could be read. A notebook that could not be
		/// read is not the same as an empty one, so its stored pages are left alone.
		/// </summary>
		public bool Listed { get; internal set; }

		public List<IdentityPage> Pages { get; } = new List<IdentityPage>();

		/// <summary>
		/// Gets every section and section group of the notebook, so a favorite of one that holds no
		/// pages can be found, and so can a section group.
		/// </summary>
		public List<IdentityContainer> Containers { get; } = new List<IdentityContainer>();
	}


	/// <summary>
	/// What one identity pass found, handed to the stages that follow it.
	/// </summary>
	internal sealed class IdentitySnapshot
	{
		public IdentitySnapshot(
			IReadOnlyList<IdentityNotebook> notebooks,
			IReadOnlyList<string> skippedSections,
			IReadOnlyList<long> purgedKeys,
			TimeSpan duration,
			int guidsPending = 0)
		{
			GuidsPending = guidsPending;
			Notebooks = notebooks;
			SkippedSections = skippedSections;
			PurgedKeys = purgedKeys;
			Duration = duration;
		}


		public IReadOnlyList<IdentityNotebook> Notebooks { get; }

		/// <summary>
		/// Gets the sections that could not be listed, as built by
		/// <see cref="PageIdentityKeys.SectionScope"/>, such as locked sections.
		/// </summary>
		public IReadOnlyList<string> SkippedSections { get; }

		/// <summary>
		/// Gets the keys of pages that have been missing so long that they were deleted, so the
		/// caller can remove everything it stored against them.
		/// </summary>
		public IReadOnlyList<long> PurgedKeys { get; }

		public TimeSpan Duration { get; }

		/// <summary>
		/// Gets the number of pages whose hyperlink GUID has not been read yet. They are filled in a
		/// few at a time on each pass, so a consumer that needs a GUID should wait, and not treat a
		/// missing one as a page that has none, until this is zero.
		/// </summary>
		public int GuidsPending { get; }

		/// <summary>Gets every page of every notebook that was read.</summary>
		public IEnumerable<IdentityPage> Pages => Notebooks.SelectMany(n => n.Pages);
	}
}
