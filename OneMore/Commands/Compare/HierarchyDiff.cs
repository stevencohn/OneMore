//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Describes how a single paired hierarchy row compares between the source (left) and
	/// target (right) branches being compared by the Compare Hierarchy command.
	/// </summary>
	internal enum DiffStatus
	{
		/// <summary>
		/// The node exists on both sides with the same modified timestamp.
		/// </summary>
		Same,

		/// <summary>
		/// The node exists on both sides but their modified timestamps differ.
		/// </summary>
		DifferentTimestamps,

		/// <summary>
		/// The node exists only on the left (source) side; there is no corresponding node
		/// on the right (target) side.
		/// </summary>
		OrphanLeft,

		/// <summary>
		/// The node exists only on the right (target) side; there is no corresponding node
		/// on the left (source) side.
		/// </summary>
		OrphanRight
	}


	/// <summary>
	/// One paired row in a hierarchy comparison, matched by node type and name at the same
	/// level and under the same parent on both sides. Pages, sections, section groups, and
	/// notebooks are all represented the same way.
	/// </summary>
	internal class DiffNode
	{
		public string Name { get; set; }

		/// <summary>
		/// The name of the node on the left side, or null if it exists only on the right.
		/// Differs from RightName only for the comparison root, since descendants are paired
		/// by name.
		/// </summary>
		public string LeftName { get; set; }

		/// <summary>
		/// The name of the node on the right side, or null if it exists only on the left.
		/// </summary>
		public string RightName { get; set; }

		public OneNote.NodeType NodeType { get; set; }

		public string LeftId { get; set; }

		public string RightId { get; set; }

		public DateTime? LeftModified { get; set; }

		public DateTime? RightModified { get; set; }

		public DiffStatus Status { get; set; }

		/// <summary>
		/// The page-content similarity score (0.0-1.0) from the last "Compare contents..." or
		/// deep-scan run against this pair, or null if it has never been scored. Cleared
		/// implicitly on every rebuild, since a score is a snapshot of content that may have
		/// changed since.
		/// </summary>
		public double? Similarity { get; set; }

		/// <summary>
		/// The enclosing DiffNode, or null for the comparison root. Used by hierarchy
		/// actions (copy/mirror) to resolve the destination parent when a node doesn't yet
		/// exist on the target side.
		/// </summary>
		public DiffNode Parent { get; set; }

		public List<DiffNode> Children { get; } = new();
	}


	/// <summary>
	/// Builds a paired DiffNode tree comparing two OneNote hierarchy branches (a notebook,
	/// section group, or section) purely from the hierarchy XML returned by
	/// OneNote.GetHierarchy - no page content is read or compared, matching the "Compare
	/// Hierarchy" command's folder-diff-style scope.
	/// </summary>
	internal static class HierarchyDiffBuilder
	{
		/// <summary>
		/// Builds a DiffNode tree comparing the given source (left) and target (right)
		/// hierarchy roots. Both elements must be the same OneNote node type (Notebook,
		/// SectionGroup, or Section) and should have been fetched with a scope deep enough
		/// to include Page elements.
		/// </summary>
		/// <param name="left">The source (left) hierarchy root element</param>
		/// <param name="right">The target (right) hierarchy root element</param>
		/// <returns>The root DiffNode, paired with its full descendant tree</returns>
		public static DiffNode Build(XElement left, XElement right)
		{
			// subpages are not nested in the hierarchy XML; they're sibling Page elements
			// distinguished only by pageLevel. This map records, per page element, the
			// subpages that follow it, filled in as each section's pages are organized
			var subpages = new Dictionary<XElement, List<XElement>>();
			return Pair(left, right, subpages);
		}


		private static DiffNode Pair(XElement left, XElement right,
			Dictionary<XElement, List<XElement>> subpages)
		{
			var source = left ?? right;

			var node = new DiffNode
			{
				Name = (string)source.Attribute("name"),
				LeftName = (string)left?.Attribute("name"),
				RightName = (string)right?.Attribute("name"),
				NodeType = GetNodeType(source),
				LeftId = (string)left?.Attribute("ID"),
				RightId = (string)right?.Attribute("ID"),
				LeftModified = GetModified(left),
				RightModified = GetModified(right)
			};

			node.Status =
				left is null ? DiffStatus.OrphanRight :
				right is null ? DiffStatus.OrphanLeft :
				node.LeftModified != node.RightModified ? DiffStatus.DifferentTimestamps :
				DiffStatus.Same;

			var leftChildren = KeyChildren(ChildElements(left, subpages));
			var rightChildren = KeyChildren(ChildElements(right, subpages));

			// preserve left-side ordering first, then append right-only names, so the
			// resulting row order is stable and deterministic for both display and tests
			var keys = new List<string>();
			var seen = new HashSet<string>(StringComparer.Ordinal);

			foreach (var key in leftChildren.Keys.Concat(rightChildren.Keys))
			{
				if (seen.Add(key))
				{
					keys.Add(key);
				}
			}

			foreach (var key in keys)
			{
				leftChildren.TryGetValue(key, out var l);
				rightChildren.TryGetValue(key, out var r);
				var child = Pair(l, r, subpages);
				child.Parent = node;
				node.Children.Add(child);
			}

			return node;
		}


		/// <summary>
		/// Returns the direct child hierarchy elements (Notebook, SectionGroup, Section, or
		/// Page) of the given element, excluding recycle-bin and unfiled-notes nodes. A
		/// section's children are its top-level pages; a page's children are its subpages.
		/// </summary>
		private static List<XElement> ChildElements(XElement element,
			Dictionary<XElement, List<XElement>> subpages)
		{
			if (element is null)
			{
				return new List<XElement>();
			}

			if (element.Name.LocalName == "Page")
			{
				return subpages.TryGetValue(element, out var subs) ? subs : new List<XElement>();
			}

			var children = element.Elements().Where(e =>
				IsHierarchyElement(e.Name.LocalName) &&
				(string)e.Attribute("isRecycleBin") != "true" &&
				(string)e.Attribute("isInRecycleBin") != "true")
				.ToList();

			var pages = children.Where(e => e.Name.LocalName == "Page").ToList();
			if (pages.Count == 0)
			{
				return children;
			}

			children.RemoveAll(e => e.Name.LocalName == "Page");
			children.AddRange(NestPages(pages, subpages));
			return children;
		}


		// Organizes a section's flat, ordered page list into a tree by pageLevel: a page's
		// parent is the nearest preceding page with a lower level, which also tolerates
		// level jumps (e.g. a level 3 page directly after a level 1 page). Returns the
		// top-level pages and records each page's subpages in the map.
		private static List<XElement> NestPages(List<XElement> pages,
			Dictionary<XElement, List<XElement>> subpages)
		{
			var top = new List<XElement>();
			var ancestors = new Stack<(XElement Page, int Level)>();

			foreach (var page in pages)
			{
				var level = (int?)page.Attribute("pageLevel") ?? 1;

				while (ancestors.Count > 0 && ancestors.Peek().Level >= level)
				{
					ancestors.Pop();
				}

				if (ancestors.Count == 0)
				{
					top.Add(page);
				}
				else
				{
					var parent = ancestors.Peek().Page;
					if (!subpages.TryGetValue(parent, out var list))
					{
						list = new List<XElement>();
						subpages[parent] = list;
					}

					list.Add(page);
				}

				ancestors.Push((page, level));
			}

			return top;
		}


		// Keys each child by type+name plus an occurrence number, so sibling pages sharing
		// a title are paired first-with-first, second-with-second rather than collapsed
		private static Dictionary<string, XElement> KeyChildren(List<XElement> children)
		{
			var map = new Dictionary<string, XElement>(StringComparer.Ordinal);
			var counts = new Dictionary<string, int>(StringComparer.Ordinal);

			foreach (var element in children)
			{
				var key = Key(element);
				counts.TryGetValue(key, out var count);
				counts[key] = count + 1;
				map[$"{key}\n{count}"] = element;
			}

			return map;
		}


		private static bool IsHierarchyElement(string localName)
		{
			return localName is "Notebook" or "SectionGroup" or "Section" or "Page";
		}


		// key is node-type + name so a Section and a Page that happen to share a name at
		// the same level are never paired with each other
		private static string Key(XElement element)
		{
			return $"{element.Name.LocalName}{(string)element.Attribute("name")}";
		}


		private static OneNote.NodeType GetNodeType(XElement element)
		{
			return element.Name.LocalName switch
			{
				"Notebook" => OneNote.NodeType.Notebook,
				"SectionGroup" => OneNote.NodeType.SectionGroup,
				"Section" => OneNote.NodeType.Section,
				_ => OneNote.NodeType.Page
			};
		}


		private static DateTime? GetModified(XElement element)
		{
			var value = (string)element?.Attribute("lastModifiedTime");
			return string.IsNullOrEmpty(value)
				? null
				: DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		}
	}
}
