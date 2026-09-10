//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
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

		public OneNote.NodeType NodeType { get; set; }

		public string LeftId { get; set; }

		public string RightId { get; set; }

		public DateTime? LeftModified { get; set; }

		public DateTime? RightModified { get; set; }

		public DiffStatus Status { get; set; }

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
			return Pair(left, right);
		}


		private static DiffNode Pair(XElement left, XElement right)
		{
			var source = left ?? right;

			var node = new DiffNode
			{
				Name = (string)source.Attribute("name"),
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

			var leftChildren = ChildElements(left).ToList();
			var rightChildren = ChildElements(right).ToList();

			// preserve left-side ordering first, then append right-only names, so the
			// resulting row order is stable and deterministic for both display and tests
			var names = new List<string>();
			var seen = new HashSet<string>(StringComparer.Ordinal);

			foreach (var element in leftChildren.Concat(rightChildren))
			{
				var key = Key(element);
				if (seen.Add(key))
				{
					names.Add(key);
				}
			}

			foreach (var key in names)
			{
				var l = leftChildren.FirstOrDefault(e => Key(e) == key);
				var r = rightChildren.FirstOrDefault(e => Key(e) == key);
				node.Children.Add(Pair(l, r));
			}

			return node;
		}


		/// <summary>
		/// Returns the direct child hierarchy elements (Notebook, SectionGroup, Section, or
		/// Page) of the given element, excluding recycle-bin and unfiled-notes nodes.
		/// </summary>
		private static IEnumerable<XElement> ChildElements(XElement element)
		{
			if (element is null)
			{
				return Enumerable.Empty<XElement>();
			}

			return element.Elements().Where(e =>
				IsHierarchyElement(e.Name.LocalName) &&
				(string)e.Attribute("isRecycleBin") != "true" &&
				(string)e.Attribute("isInRecycleBin") != "true");
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
