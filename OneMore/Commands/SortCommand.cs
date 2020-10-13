//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class SortCommand : Command
	{
		private class PageNode
		{
			public XElement Page;
			public List<XElement> Children;
			public PageNode(XElement page) { this.Page = page; this.Children = new List<XElement>(); }
		};


		public SortCommand() : base()
		{
		}


		public void Execute()
		{
			logger.WriteLine("SortCommand.Execute()");

			HierarchyScope scope;
			SortDialog.Sortings sorting;
			SortDialog.Directions direction;
			bool pinNotes;

			using (var dialog = new SortDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				scope = dialog.Scope;
				sorting = dialog.Soring;
				direction = dialog.Direction;
				pinNotes = dialog.PinNotes;
			}

			logger.WriteLine($"- sort scope:{scope} sorting:{sorting} direction:{direction}");

			try
			{
				switch (scope)
				{
					case HierarchyScope.hsPages:
						SortPages(sorting, direction);
						break;

					case HierarchyScope.hsSections:
						SortSections(sorting, direction, pinNotes);
						break;

					case HierarchyScope.hsNotebooks:
						SortNotebooks(sorting, direction);
						break;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error sorting", exc);
			}
		}

		/*
		private static void SortPages(SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				var ns = section.GetNamespaceOfPrefix("one");

				var tree = new List<PageNode>();
				var list = section.Elements(ns + "Page").ToList();
				BuildTree(tree, list, 0, 0);
			}
		}


		private static int BuildTree(List<PageNode> tree, List<XElement> elements, int level, int index)
		{
			if (index >= elements.Count)
			{
				return index;
			}

			var element = elements[index];
			var pageLevel = int.Parse(element.Attribute("pageLevel").Value);

			if (pageLevel < level)
			{
				return index;
			}

			if (pageLevel > level)
			{
				index = BuildTree(tree, elements, pageLevel, index + 1);
			}

			return index;
		}
		*/


		private static void SortPages(SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			#region Notes
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
			#endregion Notes

			using (var manager = new ApplicationManager())
			{
				var root = manager.CurrentSection();
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

				if (sorting == SortDialog.Sortings.ByName)
				{
					pages =
						(from p in pages
						 let key = EmojiDialog.RemoveEmojis(p.Page.Attribute("name").Value)
						 orderby key
						 select p).ToList();
				}
				else
				{
					var key = sorting == SortDialog.Sortings.ByCreated ? "dateTime" : "lastModifiedTime";
					pages = pages.OrderBy(p => EmojiDialog.RemoveEmojis(p.Page.Attribute(key).Value)).ToList();
				}

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

				//Logger.Current.WriteLine(root.ToString());
				manager.UpdateHierarchy(root);
			}
		}


		private static void SortSections(
			SortDialog.Sortings sorting, SortDialog.Directions direction, bool pinNotes)
		{
			#region Notes
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
			#endregion Notes

			using (var manager = new ApplicationManager())
			{
				var root = manager.GetHierarchy(HierarchyScope.hsSections);
				var ns = root.GetNamespaceOfPrefix("one");

				// find current notebook node; this is the one we want to edit
				var notebook =
					(from n in root.Elements(ns + "Notebook")
					 where n.Attribute("isCurrentlyViewed")?.Value == "true"
					 select n).FirstOrDefault();

				if (notebook == null)
				{
					return;
				}

				IEnumerable<XElement> sections;
				if (sorting == SortDialog.Sortings.ByName)
				{
					sections =
						from s in notebook.Elements(ns + "Section")
						let key = EmojiDialog.RemoveEmojis(s.Attribute("name").Value)
						orderby key
						select s;
				}
				else
				{
					sections = notebook.Elements(ns + "Section")
						.OrderBy(s => s.Attribute("lastModifiedTime").Value);
				}

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

				//Logger.Current.WriteLine(root.ToString());
				manager.UpdateHierarchy(root);
			}
		}


		private static void SortNotebooks(SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			#region Notes
			/*
			 * <one:Notebook
			 *		name="Personal"
			 *		nickname="Personal"
			 *		ID="{CAE56365-6026-4E6C-A313-667D6FEBE5D8}{1}{B0}"
			 *		path="https://d.docs.live.net/../Personal/"
			 *		lastModifiedTime="2020-03-04T00:00:32.000Z"
			 *		color="#FFD869" />
			 */
			#endregion Notes

			using (var manager = new ApplicationManager())
			{
				var root = manager.GetHierarchy(HierarchyScope.hsNotebooks);
				var ns = root.GetNamespaceOfPrefix("one");

				IEnumerable<XElement> books;
				if (sorting == SortDialog.Sortings.ByName)
				{
					books =
						from s in root.Elements(ns + "Notebook")
						// nickname is display name whereas name is the folder name
						let key = EmojiDialog.RemoveEmojis(s.Attribute("nickname").Value)
						orderby key
						select s;
				}
				else
				{
					books = root.Elements(ns + "Notebook")
						.OrderBy(s => s.Attribute("lastModifiedTime").Value);
				}

				if (direction == SortDialog.Directions.Descending)
				{
					books = books.Reverse();
				}

				root.ReplaceNodes(books);

				//Logger.Current.WriteLine(root.ToString());
				manager.UpdateHierarchy(root);
			}
		}
	}
}
