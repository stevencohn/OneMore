//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Models
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;

	/*
	 * Test Protocol - HierarchyDiffBuilder
	 *
	 * This is pure logic exercised entirely through hand-built hierarchy XML, so these
	 * unit tests are the primary verification for the diff-matching algorithm; there is no
	 * separate manual protocol.
	 */

	[TestClass]
	public class HierarchyDiffTests
	{
		private static XElement Section(string id, string name, string modified, params XElement[] children)
		{
			var section = new XElement("Section",
				new XAttribute("ID", id),
				new XAttribute("name", name),
				new XAttribute("lastModifiedTime", modified));

			foreach (var child in children)
			{
				section.Add(child);
			}

			return section;
		}


		private static XElement Page(string id, string name, string modified)
		{
			return new XElement("Page",
				new XAttribute("ID", id),
				new XAttribute("name", name),
				new XAttribute("lastModifiedTime", modified));
		}


		[TestMethod]
		public void Build_SameTimestamp_ReturnsSame()
		{
			var left = Section("L1", "Architecture", "2026-08-29T00:00:00.000Z");
			var right = Section("R1", "Architecture", "2026-08-29T00:00:00.000Z");

			var node = HierarchyDiffBuilder.Build(left, right);

			Assert.AreEqual(DiffStatus.Same, node.Status);
			Assert.AreEqual(OneNote.NodeType.Section, node.NodeType);
			Assert.AreEqual("L1", node.LeftId);
			Assert.AreEqual("R1", node.RightId);
		}


		[TestMethod]
		public void Build_DifferentTimestamp_ReturnsDifferentTimestamps()
		{
			var left = Section("L1", "Architecture", "2026-08-29T00:00:00.000Z");
			var right = Section("R1", "Architecture", "2026-08-20T00:00:00.000Z");

			var node = HierarchyDiffBuilder.Build(left, right);

			Assert.AreEqual(DiffStatus.DifferentTimestamps, node.Status);
		}


		[TestMethod]
		public void Build_ChildOnlyOnLeft_ReturnsOrphanLeft()
		{
			var left = Section("L1", "Engineering", "2026-08-29T00:00:00.000Z",
				Page("LP1", "API Spec", "2026-08-29T00:00:00.000Z"));

			var right = Section("R1", "Engineering", "2026-08-29T00:00:00.000Z");

			var node = HierarchyDiffBuilder.Build(left, right);

			Assert.AreEqual(1, node.Children.Count);
			var child = node.Children[0];
			Assert.AreEqual(DiffStatus.OrphanLeft, child.Status);
			Assert.AreEqual("API Spec", child.Name);
			Assert.AreEqual("LP1", child.LeftId);
			Assert.IsNull(child.RightId);
		}


		[TestMethod]
		public void Build_ChildOnlyOnRight_ReturnsOrphanRight()
		{
			var left = Section("L1", "Engineering", "2026-08-29T00:00:00.000Z");

			var right = Section("R1", "Engineering", "2026-08-29T00:00:00.000Z",
				Page("RP1", "Test Plan", "2026-08-20T00:00:00.000Z"));

			var node = HierarchyDiffBuilder.Build(left, right);

			Assert.AreEqual(1, node.Children.Count);
			var child = node.Children[0];
			Assert.AreEqual(DiffStatus.OrphanRight, child.Status);
			Assert.AreEqual("Test Plan", child.Name);
			Assert.IsNull(child.LeftId);
			Assert.AreEqual("RP1", child.RightId);
		}


		[TestMethod]
		public void Build_NestedMatchedChildren_PairsByTypeAndName()
		{
			var left = Section("L1", "Engineering", "2026-08-29T00:00:00.000Z",
				Page("LP1", "System Design", "2026-08-29T00:00:00.000Z"),
				Page("LP2", "API Spec", "2026-08-29T00:00:00.000Z"));

			var right = Section("R1", "Engineering", "2026-08-20T00:00:00.000Z",
				Page("RP1", "System Design", "2026-08-29T00:00:00.000Z"));

			var node = HierarchyDiffBuilder.Build(left, right);

			Assert.AreEqual(DiffStatus.DifferentTimestamps, node.Status);
			Assert.AreEqual(2, node.Children.Count);

			var design = node.Children.Single(c => c.Name == "System Design");
			Assert.AreEqual(DiffStatus.Same, design.Status);

			var spec = node.Children.Single(c => c.Name == "API Spec");
			Assert.AreEqual(DiffStatus.OrphanLeft, spec.Status);
		}


		[TestMethod]
		public void Build_SectionAndPageWithSameName_AreNotPaired()
		{
			var left = Section("L1", "Root", "2026-08-29T00:00:00.000Z",
				Page("LP1", "Notes", "2026-08-29T00:00:00.000Z"));

			var right = Section("R1", "Root", "2026-08-29T00:00:00.000Z",
				new XElement("Section",
					new XAttribute("ID", "RS1"),
					new XAttribute("name", "Notes"),
					new XAttribute("lastModifiedTime", "2026-08-29T00:00:00.000Z")));

			var node = HierarchyDiffBuilder.Build(left, right);

			Assert.AreEqual(2, node.Children.Count);
			Assert.IsTrue(node.Children.Any(c =>
				c.NodeType == OneNote.NodeType.Page && c.Status == DiffStatus.OrphanLeft));
			Assert.IsTrue(node.Children.Any(c =>
				c.NodeType == OneNote.NodeType.Section && c.Status == DiffStatus.OrphanRight));
		}


		[TestMethod]
		public void Build_RecycleBinChild_IsExcluded()
		{
			var left = Section("L1", "Engineering", "2026-08-29T00:00:00.000Z",
				new XElement("Section",
					new XAttribute("ID", "LS1"),
					new XAttribute("name", "Deleted Section"),
					new XAttribute("lastModifiedTime", "2026-08-29T00:00:00.000Z"),
					new XAttribute("isRecycleBin", "true")));

			var right = Section("R1", "Engineering", "2026-08-29T00:00:00.000Z");

			var node = HierarchyDiffBuilder.Build(left, right);

			Assert.AreEqual(0, node.Children.Count);
		}
	}
}
