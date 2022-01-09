//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3241 // Methods should not return values that are never used

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class SortCommand : Command
	{
		private sealed class PageNode
		{
			public XElement Page;
			public List<XElement> Children;
			public PageNode(XElement page) { this.Page = page; this.Children = new List<XElement>(); }
		};


		public SortCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			OneNote.Scope scope;
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

			switch (scope)
			{
				case OneNote.Scope.Pages:
					SortPages(sorting, direction);
					break;

				case OneNote.Scope.Sections:
					await SortSections(sorting, direction, pinNotes);
					break;

				case OneNote.Scope.Notebooks:
					await SortNotebooks(sorting, direction);
					break;
			}

			await Task.Yield();
		}

		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Pages

		private void SortPages(SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			#region Notes
			/*
			 * <one:Page ID="" name="Notes" pageLevel="1" />
			 *		
			 * Pages within a section are stored as a flat list of elements with
			 * indented pages indicated by pageLevel only - they are not recursive children.
			 * So the code below must group child pages with their parent so all parents
			 * can be sorted correctly. Children are not sorted.
			 */
			#endregion Notes

			logger.StartClock();

			using (var one = new OneNote())
			{
				var root = one.GetSection();
				var ns = one.GetNamespace(root);

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
							p => AddTitleIconDialog.RemoveEmojis(p.Page.Attribute("name").Value)).ToList();
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
							p => AddTitleIconDialog.RemoveEmojis(p.Page.Attribute("name").Value)).ToList();
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

				//logger.WriteLine(root);
				one.UpdateHierarchy(root);
			}

			logger.WriteTime(nameof(SortPages));
		}

		/*
		private static void SortPages(SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				var ns = section.GetNamespaceOfPrefix(OneNote.Prefix);

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


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Sections

		private async Task SortSections(
			SortDialog.Sortings sorting, SortDialog.Directions direction, bool pinNotes)
		{
			#region Notes
			/*
			 * <one:Notebook name="Personal" nickname="Personal" ID="">
			 *   <one:Section name="Notes" ID="" />
			 *   <one:Section name="Automobiles" ID="" />
			 *   <one:SectionGroup name="OneNote_RecycleBin" ID="" isRecycleBin="true">
			 *     <one:Section name="Deleted Pages" ID="" isInRecycleBin="true" isDeletedPages="true" />
			 *   </one:SectionGroup>
			 * </one:Notebook>
			 * 
			 * Sort top-level sections only; ignore section groups.
			 */
			#endregion Notes

			logger.StartClock();

			using (var one = new OneNote())
			{
				// get the current notebook with its sections
				var notebook = await one.GetNotebook();

				if (notebook == null)
				{
					return;
				}

				var ns = one.GetNamespace(notebook);

				var key = sorting == SortDialog.Sortings.ByName
					? "name"
					: "lastModifiedTime";

				SortSection(notebook, ns,
					key, direction == SortDialog.Directions.Ascending, pinNotes);

				//logger.WriteLine(notebook);
				one.UpdateHierarchy(notebook);
			}

			logger.WriteTime(nameof(SortSections));
		}


		private void SortSection(XElement parent, XNamespace ns, string key, bool ascending, bool pin)
		{
			IEnumerable<XElement> sections;
			if (ascending)
			{
				sections = parent.Elements(ns + "Section")
					.OrderBy(s => s.Attribute(key).Value)
					.ToList();
			}
			else
			{
				sections = parent.Elements(ns + "Section")
					.OrderByDescending(s => s.Attribute(key).Value)
					.ToList();
			}

			parent.Elements(ns + "Section").Remove();

			// Sections must precede any SectionGroups; so use first group as a marker to insert before
			var marker = parent.Elements(ns + "SectionGroup").FirstOrDefault();

			foreach (var section in sections)
			{
				if (marker == null)
				{
					parent.Add(section);
				}
				else
				{
					marker.AddBeforeSelf(section);
				}
			}

			if (pin)
			{
				// move Notes to the beginning
				var element = parent.Elements(ns + "Section")
					.FirstOrDefault(e => e.Attribute("name").Value == "Notes");

				if (element?.PreviousNode != null)
				{
					element.Remove();
					parent.AddFirst(element);
				}

				// move Quick Notes to the end
				element = parent.Elements(ns + "Section")
					.FirstOrDefault(e => e.Attribute("name").Value == "Quick Notes");

				if (element != null)
				{
					// regardless of where it is, just remove it and add it back in.
					// easier than dealing with case like before/after SectionGroups
					element.Remove();
					if (marker == null)
					{
						parent.Add(element);
					}
					else
					{
						marker.AddBeforeSelf(element);
					}
				}
			}

			var groups = parent.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") == null)
				.ToList();

			foreach (var group in groups)
			{
				SortSection(group, ns, key, ascending, pin);
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Notebooks

		private async Task SortNotebooks(SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			#region Notes
			/*
			 * <one:Notebook name="Personal" nickname="Personal" ID="" path="" />
			 */
			#endregion Notes

			logger.StartClock();

			using (var one = new OneNote())
			{
				var root = await one.GetNotebooks();
				var ns = one.GetNamespace(root);

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

				//logger.WriteLine(root);
				one.UpdateHierarchy(root);
			}

			logger.WriteTime(nameof(SortNotebooks));
		}
	}
}
