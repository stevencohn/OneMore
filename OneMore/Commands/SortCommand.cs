//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Dialogs;


	internal class SortCommand : Command
	{
		private class PageNode
		{
			public XElement Page;
			public List<XElement> Children;
			public PageNode(XElement page) { this.Page = page; this.Children = new List<XElement>(); }
		};


		private HierarchyScope scope;
		private SortDialog.Sortings sorting;
		private SortDialog.Directions direction;
		private bool pinNotes;


		public SortCommand() : base()
		{
		}


		public void Execute()
		{
			logger.WriteLine("SortCommand.Execute()");

			var result = DialogResult.None;

			using (var dialog = new SortDialog())
			{
				result = dialog.ShowDialog(owner);
				dialog.Focus();

				result = dialog.DialogResult;
				scope = dialog.Scope;
				sorting = dialog.Soring;
				direction = dialog.Direction;
				pinNotes = dialog.PinNotes;
			}

			if (result == DialogResult.OK)
			{
				logger.WriteLine($"- sort scope [{scope}]");
				logger.WriteLine($"- sort sorting[{sorting}]");
				logger.WriteLine($"- sort direction [{direction}]");

				using (var manager = new ApplicationManager())
				{
					XElement root;

					if (scope == HierarchyScope.hsPages)
					{
						// get just the current section with all pages as child elements
						root = manager.CurrentSection();
						root = SortPages(root, sorting, direction);
					}
					else
					{
						root = manager.GetHierarchy(scope);

						if (scope == HierarchyScope.hsNotebooks)
						{
							// notebooks is a simple flat list
							root = SortNotebooks(root, sorting, direction);
						}
						else
						{
							// sections will include all sections for the current notebook
							root = SortSections(root, sorting, direction, pinNotes);
						}
					}

					if (root != null)
					{
						manager.UpdateHierarchy(root);
					}
				}
			}
		}


		private XElement SortPages(
			XElement root, SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			/*
			 * <one:Page ID="{B49..." 
			 *		name="Notes"
			 *		dateTime="2018-01-24T03:23:12.000Z"
			 *		lastModifiedTime="2019-10-26T12:56:53.000Z"
			 *		pageLevel="1" />
			 *		
			 * Pages within a section are stored as a flat list of elements with
			 * indented pages indicated by pageLevel only - they are not recursive children.
			 * So the code below must group child pages with their parent so all parents
			 * can be sorted correctly. Children are not sorted.
			 */

			var ns = root.GetNamespaceOfPrefix("one");

			var pages = new List<PageNode>();

			foreach (var child in root.Elements(ns + "Page"))
			{
				if (child.Attribute("pageLevel").Value == "1")
				{
					// found the next parent page
					pages.Add(new PageNode(child));
				}
				else
				{
					// grouping child pages with the top parent
					pages[pages.Count - 1].Children.Add(child);
				}
			}

			string keyName;

			if (sorting == SortDialog.Sortings.ByCreated) keyName = "dateTime";
			else if (sorting == SortDialog.Sortings.ByModified) keyName = "lastModifiedTime";
			else keyName = "name";

			// sort all parents by chosen key.
			// regex keeps only printable characters (so we don't sort on title emoticons!)
			pages =
				(from p in pages
				 let key = Regex.Replace(p.Page.Attribute(keyName).Value, @"[^ -~]", "")
				 orderby key
				 select p).ToList();

			// reverse if descending
			if (direction == SortDialog.Directions.Descending)
			{
				pages.Reverse();
			}

			root.RemoveNodes();

			// recreate flat list
			foreach (var page in pages)
			{
				root.Add(page.Page);

				foreach (var child in page.Children)
				{
					root.Add(child);
				}
			}

			return root;
		}


		private XElement SortSections(
			XElement root, SortDialog.Sortings sorting, SortDialog.Directions direction, bool pinNotes)
		{
			/*
			 * <one:Notebooks xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote">
			 *   <one:Notebook name="Personal" nickname="Personal" ID="{CA.."
			 *      path="https://d.docs.live.net/../Personal/"
			 *      lastModifiedTime="2020-03-04T22:28:34.000Z"
			 *      color="#FFD869"
			 *      isCurrentlyViewed="true">
			 *     <one:Section 
			 *          name="Notes" 
			 *          ID="{41.." 
			 *          path="https://d.docs.live.net/../Personal/Notes.one" 
			 *          lastModifiedTime="2020-03-03T15:59:21.000Z" 
			 *          color="#F6B078" />
			 *     <one:Section name="Automobiles" ID="{EC.." path="https://d.docs.live.net/../Personal/Automobiles.one" lastModifiedTime="2018-09-27T16:03:28.000Z" color="#9BBBD2" />
			 *     <one:SectionGroup name="OneNote_RecycleBin" ID="{09.." path="https://d.docs.live.net/../Personal/OneNote_RecycleBin/" lastModifiedTime="2020-03-04T00:00:32.000Z" isRecycleBin="true">
			 *       <one:Section name="Deleted Pages" ID="{8A.." path="https://d.docs.live.net/../Personal/OneNote_RecycleBin/OneNote_DeletedPages.one" lastModifiedTime="2020-03-04T00:00:32.000Z" color="#E1E1E1" isInRecycleBin="true" isDeletedPages="true" />
			 *     </one:SectionGroup>
			 *   </one:Notebook>
			 * </one:Notebooks>
			 *
			 * Sort top-level sections only; ignore section groups.
			 */

			var ns = root.GetNamespaceOfPrefix("one");

			// find current notebook node; this is the one we want to edit
			var notebook =
				(from n in root.Elements(ns + "Notebook")
				 where n.Attribute("isCurrentlyViewed")?.Value == "true"
				 select n).FirstOrDefault();

			if (notebook != null)
			{
				var keyName = sorting == SortDialog.Sortings.ByModified
					? "lastModifiedTime"
					: "name";

				// get all top level sections sorted by key
				var sections =
					from s in notebook.Elements(ns + "Section")
					let key = Regex.Replace(s.Attribute(keyName).Value, @"[^ -~]", "")
					orderby key
					select s;

				// reverse if descending
				if (direction == SortDialog.Directions.Descending)
				{
					sections = sections.Reverse();
				}

				XElement notes = null;
				XElement quick = null;
				var list = new List<XElement>();

				// .Remove will remove from "sections" IEnumerable so add to new list
				foreach (var section in sections)
				{
					if (pinNotes && section.Attribute("name").Value.Equals("Notes"))
					{
						notes = section;
					}
					else if (pinNotes && section.Attribute("name").Value.Equals("Quick Notes"))
					{
						quick = section;
					}
					else
					{
						list.Insert(0, section);
					}

					section.Remove();
				}

				// re-insert sorted sections into notebook

				if (quick != null)
				{
					notebook.AddFirst(quick);
				}

				foreach (var section in list)
				{
					notebook.AddFirst(section);
				}

				if (notes != null)
				{
					notebook.AddFirst(notes);
				}

				return root;
			}

			return null;
		}


		private XElement SortNotebooks(
			XElement root, SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			/*
			 * <one:Notebook
			 *		name="Personal"
			 *		nickname="Personal"
			 *		ID="{CAE56365-6026-4E6C-A313-667D6FEBE5D8}{1}{B0}"
			 *		path="https://d.docs.live.net/../Personal/"
			 *		lastModifiedTime="2020-03-04T00:00:32.000Z"
			 *		color="#FFD869" />
			 */

			// nickname is display name whereas name is the folder name
			var keyName = sorting == SortDialog.Sortings.ByModified 
				? "lastModifiedTime" 
				: "nickname";

			var ns = root.GetNamespaceOfPrefix("one");

			// sort all notebooks by chosen key.
			// regex keeps only printable characters (so we don't sort on title emoticons!)
			var books =
				from p in root.Elements(ns + "Notebook")
				let key = Regex.Replace(p.Attribute(keyName).Value, @"[^ -~]", "")
				orderby key
				select p;

			if (direction == SortDialog.Directions.Descending)
			{
				books = books.Reverse();
			}

			root.ReplaceNodes(books);

			return root;
		}
	}
}
