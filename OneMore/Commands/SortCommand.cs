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


		public SortCommand()
		{
		}


		public override void Execute(params object[] args)
		{
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

			logger.WriteLine($"sort scope:{scope} sorting:{sorting} direction:{direction}");

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
				logger.WriteLine("ERROR sorting", exc);
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


		private void SortPages(SortDialog.Sortings sorting, SortDialog.Directions direction)
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

			logger.StartClock();

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

				if (direction == SortDialog.Directions.Descending)
				{
					if (sorting == SortDialog.Sortings.ByName)
					{
						pages = pages.OrderByDescending(
							p => EmojiDialog.RemoveEmojis(p.Page.Attribute("name").Value)).ToList();
					}
					else
					{
						var key = sorting == SortDialog.Sortings.ByCreated
							? "dateTime" : "lastModifiedTime";

						pages = pages.OrderByDescending(
							p => p.Page.Attribute(key).Value).ToList();
					}
				}
				else
				{
					if (sorting == SortDialog.Sortings.ByName)
					{
						pages = pages.OrderBy(
							p => EmojiDialog.RemoveEmojis(p.Page.Attribute("name").Value)).ToList();
					}
					else
					{
						var key = sorting == SortDialog.Sortings.ByCreated
							? "dateTime" : "lastModifiedTime";

						pages = pages.OrderBy(
							p => p.Page.Attribute(key).Value).ToList();
					}
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

				//logger.WriteLine(root.ToString());
				manager.UpdateHierarchy(root);
			}

			logger.WriteTime(nameof(SortPages));
		}


		private void SortSections(
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

			logger.StartClock();

			using (var manager = new ApplicationManager())
			{
				// find the ID of the current notebook
				var notebooks = manager.GetHierarchy(HierarchyScope.hsNotebooks);
				var ns = notebooks.GetNamespaceOfPrefix("one");
				var sectionId = notebooks.Elements(ns + "Notebook")
					.Where(e => e.Attribute("isCurrentlyViewed")?.Value == "true")
					.Select(e => e.Attribute("ID").Value).FirstOrDefault();

				// get the current notebook with its sections
				var notebook = manager.GetHierarchySection(sectionId);

				if (notebook == null)
				{
					return;
				}

				var key = sorting == SortDialog.Sortings.ByName
					? "name"
					: "lastModifiedTime";

				IEnumerable<XElement> sections;
				if (direction == SortDialog.Directions.Descending)
				{
					sections = notebook.Elements(ns + "Section")
						.OrderByDescending(s => s.Attribute(key).Value);
				}
				else
				{
					sections = notebook.Elements(ns + "Section")
						.OrderBy(s => s.Attribute(key).Value);
				}

				notebook.ReplaceNodes(sections);

				if (pinNotes)
				{
					// move Notes to the beginning
					var element = notebook.Elements(ns + "Section")
						.FirstOrDefault(e => e.Attribute("name").Value == "Notes");

					if (element?.PreviousNode != null)
					{
						element.Remove();
						notebook.AddFirst(element);
					}

					// move Quick Notes to the end
					element = notebook.Elements(ns + "Section")
						.FirstOrDefault(e => e.Attribute("name").Value == "Quick Notes");

					if (element?.NextNode != null)
					{
						element.Remove();
						notebook.Add(element);
					}
				}

				//logger.WriteLine(notebook.ToString());
				manager.UpdateHierarchy(notebook);
			}

			logger.WriteTime(nameof(SortSections));
		}


		private void SortNotebooks(SortDialog.Sortings sorting, SortDialog.Directions direction)
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

			logger.StartClock();

			using (var manager = new ApplicationManager())
			{
				var root = manager.GetHierarchy(HierarchyScope.hsNotebooks);
				var ns = root.GetNamespaceOfPrefix("one");

				// nickname is display name whereas name is the folder name
				var key = sorting == SortDialog.Sortings.ByName
					? "nickname"
					: "lastModifiedTime";

				IEnumerable<XElement> books;
				if (direction == SortDialog.Directions.Descending)
				{
					books = root.Elements(ns + "Notebook")
						.OrderByDescending(s => s.Attribute(key).Value);
				}
				else
				{
					books = root.Elements(ns + "Notebook")
						.OrderBy(s => s.Attribute(key).Value);
				}

				root.ReplaceNodes(books);

				//logger.WriteLine(root.ToString());
				manager.UpdateHierarchy(root);
			}

			logger.WriteTime(nameof(SortNotebooks));
		}
	}
}
