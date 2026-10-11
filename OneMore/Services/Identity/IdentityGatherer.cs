//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Identity
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Turns the hierarchy of one notebook into the pages it contains. Pure logic over XML,
	/// with no OneNote access, so it can be tested with fixtures.
	/// </summary>
	/// <remarks>
	/// The conditions for what to skip are the ones the hashtag scanner has always used:
	/// recycle bin groups and sections, pages in the recycle bin, and locked sections.
	/// </remarks>
	internal static class IdentityGatherer
	{
		/// <summary>
		/// Gathers the pages of a notebook.
		/// </summary>
		/// <param name="listing">The Notebook element from the list of open notebooks, which
		/// supplies its ID, name, and path</param>
		/// <param name="hierarchy">The notebook with its sections and pages, or null if it
		/// could not be read</param>
		/// <param name="skipped">Receives the sections that could not be listed</param>
		public static IdentityNotebook Gather(
			XElement listing, XElement hierarchy, List<string> skipped)
		{
			var id = listing.Attribute("ID")?.Value;
			var name = listing.Attribute("name")?.Value;
			var path = listing.Attribute("path")?.Value;

			var notebook = new IdentityNotebook(id, name, PageIdentityKeys.NotebookKey(path, name));

			if (hierarchy is null)
			{
				return notebook;
			}

			notebook.Listed = true;
			Walk(hierarchy, notebook, new List<string>(), skipped);
			return notebook;
		}


		// lists the pages of every section under the parent, recursing into section groups
		private static void Walk(
			XElement parent, IdentityNotebook notebook, List<string> groups, List<string> skipped)
		{
			var ns = parent.Name.Namespace;

			foreach (var child in parent.Elements())
			{
				if (child.Name == ns + "SectionGroup")
				{
					if (IsRecycled(child) || IsTrue(child, "locked"))
					{
						continue;
					}

					var groupName = child.Attribute("name")?.Value;
					notebook.Containers.Add(new IdentityContainer(
						ContainerKind.SectionGroup,
						child.Attribute("ID")?.Value,
						groupName,
						PageIdentityKeys.SectionKey(groups, groupName),
						PathOf(notebook, groups, groupName)));

					groups.Add(groupName);
					Walk(child, notebook, groups, skipped);
					groups.RemoveAt(groups.Count - 1);
				}
				else if (child.Name == ns + "Section")
				{
					if (IsRecycled(child) || IsTrue(child, "isDeletedPages"))
					{
						continue;
					}

					// a section is recorded whether or not it holds pages, and even if it is locked,
					// because it still exists and can be a favorite
					var sectionName = child.Attribute("name")?.Value;
					notebook.Containers.Add(new IdentityContainer(
						ContainerKind.Section,
						child.Attribute("ID")?.Value,
						sectionName,
						PageIdentityKeys.SectionKey(groups, sectionName),
						PathOf(notebook, groups, sectionName)));

					GatherSection(child, notebook, groups, skipped);
				}
			}
		}


		private static void GatherSection(
			XElement section, IdentityNotebook notebook, List<string> groups, List<string> skipped)
		{
			var ns = section.Name.Namespace;
			var sectionName = section.Attribute("name")?.Value;
			var sectionKey = PageIdentityKeys.SectionKey(groups, sectionName);

			// a locked section cannot be listed, which is not the same as having no pages
			if (IsTrue(section, "locked"))
			{
				skipped.Add(PageIdentityKeys.SectionScope(notebook.Key, sectionKey));
				return;
			}

			var sectionID = section.Attribute("ID")?.Value;
			var sectionPath = PathOf(notebook, groups, sectionName);

			foreach (var page in section.Elements(ns + "Page"))
			{
				if (IsTrue(page, "isInRecycleBin"))
				{
					continue;
				}

				var reference = new PageRef(
					page.Attribute("ID")?.Value,
					notebook.Key,
					sectionKey,
					page.Attribute("name")?.Value,
					page.Attribute("dateTime")?.Value,
					page.Attribute("lastModifiedTime")?.Value,
					int.TryParse(page.Attribute("pageLevel")?.Value, out var level) ? level : 1);

				notebook.Pages.Add(new IdentityPage(
					reference, notebook.ID, sectionID, sectionPath, IsTagIndex(page, ns)));
			}
		}


		// the path by name of a section or section group, such as /Notebook/Group/Section
		private static string PathOf(IdentityNotebook notebook, List<string> groups, string name)
		{
			return notebook.Path + string.Concat(groups.Select(g => "/" + g)) + "/" + name;
		}


		// a page that OneMore generated to list hashtags
		private static bool IsTagIndex(XElement page, XNamespace ns)
		{
			return page.Elements(ns + "Meta").Any(m =>
				m.Attribute("name")?.Value == MetaNames.TagIndex &&
				m.Attribute("content")?.Value == "true");
		}


		private static bool IsRecycled(XElement element)
		{
			return IsTrue(element, "isRecycleBin") || IsTrue(element, "isInRecycleBin");
		}


		private static bool IsTrue(XElement element, string attribute)
		{
			return string.Equals(
				element.Attribute(attribute)?.Value, "true", StringComparison.OrdinalIgnoreCase);
		}
	}
}
