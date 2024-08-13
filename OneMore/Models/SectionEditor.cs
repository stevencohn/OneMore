//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Common features of managing a Section, such as positioning child pages and ensuring
	/// unique page names.
	/// </summary>
	internal class SectionEditor
	{
		#region Regex Explanations
		//
		// Pattern must match a title with any number of "(n)", taking only the last as index.
		// For example
		//
		//   - the index of "foo" is 0 with a root name of "foo"
		//   - the index of "foo (1)" is 1 with a root name of "foo"
		//   - the index of "foo (1) (2)" is 2 with a root name of "foo (1)"
		//
		// This is to accomodate commands like Duplicate Page and Copy/Move Page to start
		// with any source page and fabricate a reasonably named target page.
		//
		#endregion

		private readonly XNamespace ns;


		public SectionEditor(XElement section)
		{
			Section = section;
			ns = section.GetNamespaceOfPrefix(OneNote.Prefix);
		}


		public XElement Section { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public string FindLastIndexedPageIDByTitle(string title)
		{
			var pattern = new Regex($@"(?:{Escape(title)}.*?)(?:\s*\((\d+)\))?$");

			var last = Section.Elements(ns + "Page")
				.LastOrDefault(e => pattern.Match(e.Attribute("name").Value).Success);

			return last?.Attribute("ID").Value;
		}


		private string Escape(string title)
		{
			return Regex.Escape(title).Replace("/", @"\/");
		}


		/// <summary>
		/// Moves the specified page so it appears in order immeidately after the given anchor.
		/// </summary>
		/// <param name="pageID">The ID of the page to move</param>
		/// <param name="anchorID">The ID of the anchor page</param>
		/// <returns>True if the page was moved, otherwise it was not moved</returns>
		public bool MovePageAfterAnchor(string pageID, string anchorID)
		{
			var page = Section.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == pageID);

			if (page is not null)
			{
				var anchor = Section.Elements(ns + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == anchorID);

				if (anchor is not null)
				{
					page.Remove();
					page.SetAttributeValue("pageLevel", anchor.Attribute("pageLevel").Value);
					anchor.AddAfterSelf(page);

					return true;
				}
			}

			return false;
		}


		/// <summary>
		/// Moves the specified page to the first child position under the given parent.
		/// </summary>
		/// <param name="pageID">The ID of the page to move under the parent</param>
		/// <param name="parentID">The ID of the parent page</param>
		/// <returns>True if the page was moved, otherwise False</returns>
		public bool MovePageToParent(string pageID, string parentID)
		{
			var parent = Section.Elements(ns + "Page")
				.First(e => e.Attribute("ID").Value == parentID);

			var child = Section.Elements(ns + "Page")
				.First(e => e.Attribute("ID").Value == pageID);

			if (child is null || child.Parent == parent)
			{
				// already a child
				return false;
			}

			var updated = false;
			if (child != parent.NextNode)
			{
				// move page to top of the list of children
				child.Remove();
				parent.AddAfterSelf(child);
				updated = true;
			}

			var mustLevel = int.Parse(parent.Attribute("pageLevel").Value) + 1;
			var childLevel = int.Parse(child.Attribute("pageLevel").Value);
			if (childLevel != mustLevel)
			{
				child.Attribute("pageLevel").Value = $"{mustLevel}";
				updated = true;
			}

			return updated;
		}


		/// <summary>
		/// Examines the title of the given page and updates it to ensure that its name
		/// is unique within the scope of the current section.
		/// </summary>
		/// <param name="page">A Page that requires a unique title</param>
		/// <returns>True if the title was updated, otherwise False</returns>
		public bool SetUniquePageTitle(Page page)
		{
			// start by extracting just name part without " (index)" suffix
			// this matches only the last "(n)" as an index, meaning multiples are allowed but
			// prior "(n)" indexes are considered part of the root name
			var match = Regex.Match(Escape(page.Title), @"(.+?)(?:\s*\((\d+)\))?$");

			// page.Title gets plain text
			var title = match.Groups.Count > 1 && match.Groups[2].Success
				? match.Groups[2].Value.Trim()
				: page.Title;

			// match all pages in section on "<name> (index)"
			var regex = new Regex($@"({Escape(title)}.*?)(?:\s*\((\d+)\))?$");

			// scan the section for collisions, focusing only on the scope of immediate child
			// pages, ignoring deeper SectionGroup pages which have their own scoped uniqueness

			// this will also include the current page so should result in a max value
			var index = Section.Elements(ns + "Page")
				.Select(e => regex.Match(e.Attribute("name").Value))
				.Where(m => m.Success)
				.Max(m => m.Groups.Count > 1 && m.Groups[2].Success
					? int.Parse(m.Groups[2].Value) : 0) + 1;

			// get the sytlized content so we can update the index in place

			var run = page.Root.Elements(ns + "Title")
				.Elements(ns + "OE")
				.Elements(ns + "T")
				.LastOrDefault(e => !string.IsNullOrWhiteSpace(e.GetCData().Value));

			if (run is null)
			{
				// shouldn't happen?
				page.Title = $"{page.Title} ({index})";
				page.Root.SetAttributeValue("name", page.Title);
				return false;
			}

			// update with new index value... either append or replace

			var wrapper = run.GetCData().GetWrapper();
			var node = wrapper.Nodes().Last();

			var text = node.NodeType == System.Xml.XmlNodeType.Text
				? (XText)node
				: ((XElement)node).Nodes().OfType<XText>().Last();

			regex = new Regex($@"(?:{Escape(title)}.*?)(?:\s*\((\d+)\))?$");
			match = regex.Match(text.Value);
			if (match.Success)
			{
				if (match.Groups.Count > 1 && match.Groups[1].Success)
				{
					// has root name (possibly with static indexes) and a valid index
					// so replace the valid index value with our new index value
					text.Value = text.Value
						.Remove(match.Groups[1].Index, match.Groups[1].Length)
						.Insert(match.Groups[1].Index, index.ToString());
				}
				else
				{
					// append root name with a new index
					text.Value = $"{text.Value} ({index})";
				}
			}
			else
			{
				// append root name with a new index
				text.Value = $"{text.Value} ({index})";
			}

			// results...

			title = wrapper.GetInnerXml();
			if (page.Title == title)
			{
				return false;
			}

			page.Title = title;
			page.Root.SetAttributeValue("name", title);
			return true;
		}
	}
}
