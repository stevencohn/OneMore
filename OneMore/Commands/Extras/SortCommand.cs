//************************************************************************************************
// Copyright © 2019 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3241 // Methods should not return values that are never used

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Sorts pages, sections, or notebooks.
	/// </summary>
	/// <remarks>
	/// Pages are sorted within the current section only, not recursively throughout the notebook.
	/// Sections are sorted throughout the current notebook recursively.
	/// </remarks>
	internal class SortCommand : Command
	{
		private static bool commandIsActive = false;


		public enum SortBy
		{
			Name,
			Created,
			Modified
		}

		private sealed class PageNode : Sortable
		{
			public List<PageNode> Nodes;
			public PageNode(XElement root)
			{
				Root = root;
				Nodes = new List<PageNode>();
			}
			public PageNode(SortCommand cmd, XElement root)
				: this(root)
			{
				Name = cmd.Emojis.RemoveEmojis(root.Attribute("name").Value);
				ParseName(cmd);
			}
		}

		private class Sortable
		{
			public XElement Root;
			public string Name;
			public int Sequence;
			public string Text;

			public Sortable() { }
			public Sortable(SortCommand cmd, XElement book, string key)
			{
				Root = book;
				Name = book.Attribute(key).Value;
				ParseName(cmd);
			}
			protected void ParseName(SortCommand cmd)
			{
				var match = cmd.SortablePattern.Match(Name);
				if (match.Success)
				{
					Sequence = int.Parse(match.Groups[1].Value);
					Text = match.Groups[2].Value;
				}
				else
				{
					Sequence = int.MaxValue;
					Text = Name;
				}
			}
		}


		private Emojis Emojis;
		private Regex SortablePattern;


		public SortCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				using var dialog = new SortDialog();

				if (args != null && args.Length > 0 && args[0] is OneNote.Scope scopeArg)
				{
					dialog.SetScope(scopeArg);
				}

				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				var sorting = dialog.Sorting;
				var ascending = dialog.Direction == SortDialog.Directions.Ascending;
				logger.WriteLine($"sort scope:{dialog.Scope} sorting:{sorting} ascending:{ascending}");

				if (sorting == SortBy.Name)
				{
					// create a pattern to match the sequence number at the start of the name
					// e.g. "1 - Color", "2) Thing", "(3) Notes", "[4] Automobiles"
					SortablePattern = new Regex(@"^(?:[([{]\s*)?(\d+)(?:\s*[)\]}])?\s*(.*)");
				}

				switch (dialog.Scope)
				{
					case OneNote.Scope.Children:
						await SortPages(sorting, ascending, true);
						break;

					case OneNote.Scope.Pages:
						await SortPages(sorting, ascending, false);
						break;

					case OneNote.Scope.Sections:
						await SortSections(sorting, ascending, dialog.PinNotes, false);
						break;

					case OneNote.Scope.SectionGroups:
						await SortSections(sorting, ascending, dialog.PinNotes, true);
						break;

					case OneNote.Scope.Notebooks:
						await SortNotebooks(sorting, ascending);
						break;
				}
			}
			finally
			{
				commandIsActive = false;
			}
		}

		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Pages

		private async Task SortPages(SortBy sorting, bool ascending, bool children)
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

			await using var one = new OneNote();
			var section = await one.GetSection();
			var ns = section.GetNamespaceOfPrefix(OneNote.Prefix);

			var pages = section.Elements(ns + "Page").ToList();

			var tree = new List<PageNode>();
			using (Emojis = new Emojis())
			{
				MakePageTree(tree, pages, 0, 1, sorting);
			}

			if (children)
			{
				// sub-pages of currently selected page
				var root = FindStartingNode(tree, one.CurrentPageId);
				if (root?.Nodes.Any() == true)
				{
					root.Nodes = SortPageTree(root.Nodes, sorting, ascending);
				}
			}
			else
			{
				// pages within section
				tree = SortPageTree(tree, sorting, ascending);
			}

			section.Elements().Remove();
			section.Add(FlattenPageTree(tree));

			//logger.WriteLine(section);
			one.UpdateHierarchy(section);

			logger.WriteTime(nameof(SortPages));
		}


		private int MakePageTree(List<PageNode> tree, List<XElement> list, int index, int level, SortBy sorting)
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
					var page = sorting == SortBy.Name
						? new PageNode(this, list[index])
						: new PageNode(list[index]);

					tree.Add(page);
					index++;
				}
				else
				{
					var node = tree[tree.Count - 1];

					var page = sorting == SortBy.Name
						? new PageNode(this, list[index])
						: new PageNode(list[index]);

					node.Nodes.Add(page);
					index = MakePageTree(node.Nodes, list, index + 1, pageLevel, sorting);
				}
			}

			return index;
		}


		private PageNode FindStartingNode(List<PageNode> tree, string pageID)
		{
			var start = tree.Find(n => n.Root.Attribute("ID").Value == pageID);
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
			List<PageNode> tree, SortBy sorting, bool ascending)
		{
			if (sorting == SortBy.Name)
			{
				tree = ascending
					? tree.OrderBy(t => t.Sequence).ThenBy(t => t.Text).ToList()
					: tree.OrderByDescending(t => t.Sequence).ThenByDescending(t => t.Text).ToList();
			}
			else
			{
				var key = sorting == SortBy.Created ? "dateTime" : "lastModifiedTime";

				tree = ascending
					? tree.OrderBy(t => t.Root.Attribute(key).Value).ToList()
					: tree.OrderByDescending(t => t.Root.Attribute(key).Value).ToList();
			}

			foreach (var node in tree)
			{
				node.Nodes = SortPageTree(node.Nodes, sorting, ascending);
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

		private async Task SortSections(
			SortBy sorting, bool ascending, bool pinNotes, bool groupSort)
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

			await using var one = new OneNote();

			// get the current notebook with its sections
			var notebook = await one.GetNotebook();

			if (notebook == null)
			{
				return;
			}

			var ns = one.GetNamespace(notebook);

			if (groupSort)
			{
				var group = notebook.Descendants(ns + "Section")
					.Where(e => e.Attribute("isCurrentlyViewed")?.Value == "true")
					.Select(e => e.Parent)
					.FirstOrDefault();

				if (group != null)
				{
					SortSection(group, ns, sorting, ascending, pinNotes, true);
				}
			}
			else
			{
				SortSection(notebook, ns, sorting, ascending, pinNotes, false);
			}

			//logger.WriteLine(notebook);
			one.UpdateHierarchy(notebook);

			logger.WriteTime(nameof(SortSections));
		}


		private void SortSection(
			XElement parent, XNamespace ns, SortBy sorting, bool ascending, bool pin, bool groupSort)
		{
			IEnumerable<XElement> sections;

			if (sorting == SortBy.Name)
			{
				// nickname is display name whereas name is the folder name

				if (ascending)
				{
					sections = parent.Elements(ns + "Section")
						.Select(b => new Sortable(this, b, "name"))
						.OrderBy(b => b.Sequence).ThenBy(b => b.Text)
						.Select(b => b.Root)
						.ToList();
				}
				else
				{
					sections = parent.Elements(ns + "Section")
						.Select(b => new Sortable(this, b, "name"))
						.OrderByDescending(b => b.Sequence).ThenByDescending(b => b.Text)
						.Select(b => b.Root)
						.ToList();
				}
			}
			else
			{
				if (ascending)
				{
					sections = parent.Elements(ns + "Section")
						.OrderBy(s => s.Attribute("lastModifiedTime").Value)
						.ToList();
				}
				else
				{
					sections = parent.Elements(ns + "Section")
						.OrderByDescending(s => s.Attribute("lastModifiedTime").Value)
						.ToList();
				}
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

			if (!groupSort)
			{
				var groups = parent.Elements(ns + "SectionGroup")
					.Where(e => e.Attribute("isRecycleBin") == null)
					.ToList();

				foreach (var group in groups)
				{
					SortSection(group, ns, sorting, ascending, pin, false);
				}
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
			await using var one = new OneNote();

			var root = await one.GetNotebooks();
			var ns = one.GetNamespace(root);

			IEnumerable<XElement> books;

			if (sorting == SortBy.Name)
			{
				// nickname is display name whereas name is the folder name

				if (ascending)
				{
					books = root.Elements(ns + "Notebook")
						.Select(b => new Sortable(this, b, "nickname"))
						.OrderBy(b => b.Sequence).ThenBy(b => b.Text)
						.Select(b => b.Root);
				}
				else
				{
					books = root.Elements(ns + "Notebook")
						.Select(b => new Sortable(this, b, "nickname"))
						.OrderByDescending(b => b.Sequence).ThenByDescending(b => b.Text)
						.Select(b => b.Root);
				}
			}
			else
			{
				if (ascending)
				{
					books = root.Elements(ns + "Notebook")
						.OrderBy(s => s.Attribute("lastModifiedTime").Value);
				}
				else
				{
					books = root.Elements(ns + "Notebook")
						.OrderByDescending(s => s.Attribute("lastModifiedTime").Value);
				}
			}

			root.ReplaceNodes(books);

			//logger.WriteLine(root);
			one.UpdateHierarchy(root);

			logger.WriteTime(nameof(SortNotebooks));
		}
	}
}
