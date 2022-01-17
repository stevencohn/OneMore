//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3241 // Methods should not return values that are never used

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class SortCommand : Command
	{
		public enum SortBy
		{
			Name,
			Created,
			Modified
		}

		private sealed class PageNode
		{
			public XElement Root;
			public List<PageNode> Nodes;
			public PageNode(XElement root)
			{
				Root = root;
				Nodes = new List<PageNode>();
			}
		};


		public SortCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			OneNote.Scope scope;
			SortBy sorting;
			bool ascending;
			bool pinNotes;

			using (var dialog = new SortDialog())
			{
				if (args != null && args.Length > 0 && args[0] is OneNote.Scope scopeArg)
				{
					dialog.SetScope(scopeArg);
				}

				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				scope = dialog.Scope;
				sorting = dialog.Sorting;
				ascending = dialog.Direction == SortDialog.Directions.Ascending;
				pinNotes = dialog.PinNotes;
			}

			logger.WriteLine($"sort scope:{scope} sorting:{sorting} ascending:{ascending}");

			switch (scope)
			{
				case OneNote.Scope.Children:
					SortPages(sorting, ascending, true);
					break;

				case OneNote.Scope.Pages:
					SortPages(sorting, ascending, false);
					break;

				case OneNote.Scope.Sections:
					await SortSections(sorting, ascending, pinNotes);
					break;

				case OneNote.Scope.Notebooks:
					await SortNotebooks(sorting, ascending);
					break;
			}

			await Task.Yield();
		}

		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Pages

		private void SortPages(SortBy sorting, bool ascending, bool children)
		{
			#region Notes
			/*
			 * <one:Page ID="" name="Notes" pageLevel="1" />
			 *		
			 * Pages within a section are stored as a flat list of elements where indented
			 * page are indicated by pageLevel; they are not recursive child elements.
			 */
			#endregion Notes

			logger.StartClock();

			using (var one = new OneNote())
			{
				var section = one.GetSection();
				var ns = section.GetNamespaceOfPrefix(OneNote.Prefix);

				var pages = section.Elements(ns + "Page").ToList();

				var tree = new List<PageNode>();
				MakePageTree(tree, pages, 0, 1);

				var cleaner = new Func<XElement, string>((e) => sorting == SortBy.Name
					? AddTitleIconDialog.RemoveEmojis(e.Attribute("name").Value)
					: sorting == SortBy.Created
						? e.Attribute("dateTime").Value
						: e.Attribute("lastModifiedTime").Value
					);

				if (children)
				{
					// sub-pages of currently selected page
					var root = FindStartingNode(tree, one.CurrentPageId);
					if (root?.Nodes.Any() == true)
					{
						root.Nodes = SortPageTree(root.Nodes, ascending, cleaner);
					}
				}
				else
				{
					// pages within section
					tree = SortPageTree(tree, ascending, cleaner);
				}

				section.Elements().Remove();
				section.Add(FlattenPageTree(tree));
				//logger.WriteLine(section);

				one.UpdateHierarchy(section);
			}

			logger.WriteTime(nameof(SortPages));
		}


		private int MakePageTree(List<PageNode> tree, List<XElement> list, int index, int level)
		{
			while (index < list.Count)
			{
				var pageLevel = int.Parse(list[index].Attribute("pageLevel").Value);
				if (pageLevel < level)
				{
					return index;
				}
				else if (pageLevel == level)
				{
					tree.Add(new PageNode(list[index]));
					index++;
				}
				else
				{
					var node = tree[tree.Count - 1];
					node.Nodes.Add(new PageNode(list[index]));
					index = MakePageTree(node.Nodes, list, index + 1, pageLevel);
				}
			}

			return index;
		}


		private PageNode FindStartingNode(List<PageNode> tree, string pageID)
		{
			var start = tree.FirstOrDefault(n => n.Root.Attribute("ID").Value == pageID);
			if (start == null)
			{
				foreach (var node in tree.Where(n => n.Nodes.Any()))
				{
					start = FindStartingNode(node.Nodes, pageID);
					if (start != null)
					{
						break;
					}
				}
			}

			return start;
		}


		private List<PageNode> SortPageTree(
			List<PageNode> tree, bool ascending, Func<XElement, string> clean)
		{
			var comparer = StringComparer.InvariantCultureIgnoreCase;

			tree = ascending
				? tree.OrderBy(t => clean(t.Root), comparer).ToList()
				: tree.OrderByDescending(t => clean(t.Root), comparer).ToList();

			foreach (var node in tree)
			{
				node.Nodes = SortPageTree(node.Nodes, ascending, clean);
			}

			return tree;
		}


		private IEnumerable<XElement> FlattenPageTree(List<PageNode> tree)
		{
			foreach (var node in tree)
			{
				yield return node.Root;

				foreach (var n in FlattenPageTree(node.Nodes))
				{
					yield return n;
				}
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Sections

		private async Task SortSections(SortBy sorting, bool ascending, bool pinNotes)
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

				var key = sorting == SortBy.Name
					? "name"
					: "lastModifiedTime";

				SortSection(notebook, ns, key, ascending, pinNotes);

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


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Notebooks

		private async Task SortNotebooks(SortBy sorting, bool ascending)
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
				var key = sorting == SortBy.Name
					? "nickname"
					: "lastModifiedTime";

				IEnumerable<XElement> books;
				if (ascending)
				{
					books = root.Elements(ns + "Notebook")
						.OrderBy(s => s.Attribute(key).Value);
				}
				else
				{
					books = root.Elements(ns + "Notebook")
						.OrderByDescending(s => s.Attribute(key).Value);
				}

				root.ReplaceNodes(books);

				//logger.WriteLine(root);
				one.UpdateHierarchy(root);
			}

			logger.WriteTime(nameof(SortNotebooks));
		}
	}
}
