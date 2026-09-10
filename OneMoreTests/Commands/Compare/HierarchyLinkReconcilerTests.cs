//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Compare
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands.Compare;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol - HierarchyLinkReconciler
	 *
	 * HasReplacementRisk and ExtractPageId are pure logic, exercised directly against
	 * hand-built DiffNode trees and sample href strings. RelinkPage needs a OneNote wrapper
	 * (to generate the replacement hyperlink for a rewritten anchor), so those tests run
	 * through MockApplication via TestBase like other OneNote-touching command tests.
	 * MockApplication.GetHyperlinkToObject synthesizes a deterministic href whose page-id is
	 * simply the given internal id wrapped in braces - not what real OneNote produces (see
	 * HierarchyLinkReconciler.ExtractPageId's own doc comment on why hyperlink-space and
	 * internal hierarchy ids are different spaces), but stable enough to assert against.
	 * There is no separate manual protocol for this pure/mocked logic.
	 */

	[TestClass]
	public class HierarchyLinkReconcilerTests : TestBase
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static DiffNode MakePage(DiffStatus status, string name = "Page")
		{
			return new DiffNode
			{
				Name = name,
				NodeType = OneNote.NodeType.Page,
				Status = status,
				LeftId = status != DiffStatus.OrphanRight ? "L1" : null,
				RightId = status != DiffStatus.OrphanLeft ? "R1" : null
			};
		}


		private static DiffNode MakeSection(params DiffNode[] children)
		{
			var node = new DiffNode
			{
				Name = "Section",
				NodeType = OneNote.NodeType.Section,
				Status = DiffStatus.Same,
				LeftId = "LS1",
				RightId = "RS1"
			};

			foreach (var child in children)
			{
				child.Parent = node;
				node.Children.Add(child);
			}

			return node;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// HasReplacementRisk

		[TestMethod]
		public void HasReplacementRisk_MatchedPage_ReturnsTrue()
		{
			var node = MakePage(DiffStatus.Same);
			Assert.IsTrue(HierarchyLinkReconciler.HasReplacementRisk(node));
		}


		[TestMethod]
		public void HasReplacementRisk_DifferentTimestampsPage_ReturnsTrue()
		{
			var node = MakePage(DiffStatus.DifferentTimestamps);
			Assert.IsTrue(HierarchyLinkReconciler.HasReplacementRisk(node));
		}


		[TestMethod]
		public void HasReplacementRisk_OrphanPage_ReturnsFalse()
		{
			Assert.IsFalse(HierarchyLinkReconciler.HasReplacementRisk(MakePage(DiffStatus.OrphanLeft)));
			Assert.IsFalse(HierarchyLinkReconciler.HasReplacementRisk(MakePage(DiffStatus.OrphanRight)));
		}


		[TestMethod]
		public void HasReplacementRisk_MatchedGrandchild_ReturnsTrue()
		{
			var root = MakeSection(MakePage(DiffStatus.OrphanRight, "A"));
			var child = MakeSection(MakePage(DiffStatus.Same, "B"));
			child.Parent = root;
			root.Children.Add(child);

			Assert.IsTrue(HierarchyLinkReconciler.HasReplacementRisk(root));
		}


		[TestMethod]
		public void HasReplacementRisk_NoMatchedDescendants_ReturnsFalse()
		{
			var root = MakeSection(
				MakePage(DiffStatus.OrphanLeft, "A"),
				MakePage(DiffStatus.OrphanRight, "B"));

			Assert.IsFalse(HierarchyLinkReconciler.HasReplacementRisk(root));
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// ExtractPageId

		[TestMethod]
		public void ExtractPageId_PageLevelHref_ReturnsBracedId()
		{
			var href = "onenote:https://d.docs.live.net/x/Notebook/Section.one#Page" +
				"&section-id={AAAAAAAA-0000-0000-0000-000000000000}" +
				"&page-id={BBBBBBBB-0000-0000-0000-000000000000}&end";

			Assert.AreEqual("{BBBBBBBB-0000-0000-0000-000000000000}",
				HierarchyLinkReconciler.ExtractPageId(href));
		}


		[TestMethod]
		public void ExtractPageId_ObjectIdHref_ReturnsPageIdIgnoringObjectId()
		{
			var href = "onenote:https://d.docs.live.net/x/Notebook/Section.one#Page" +
				"&section-id={AAAAAAAA-0000-0000-0000-000000000000}" +
				"&page-id={BBBBBBBB-0000-0000-0000-000000000000}" +
				"&object-id={CCCCCCCC-0000-0000-0000-000000000000}&E";

			Assert.AreEqual("{BBBBBBBB-0000-0000-0000-000000000000}",
				HierarchyLinkReconciler.ExtractPageId(href));
		}


		[TestMethod]
		public void ExtractPageId_NonOneNoteHref_ReturnsNull()
		{
			Assert.IsNull(HierarchyLinkReconciler.ExtractPageId("https://example.com/page"));
		}


		[TestMethod]
		public void ExtractPageId_NullOrEmpty_ReturnsNull()
		{
			Assert.IsNull(HierarchyLinkReconciler.ExtractPageId(null));
			Assert.IsNull(HierarchyLinkReconciler.ExtractPageId(string.Empty));
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// RelinkPage

		private static string AnchorHref(string bracedPageId, bool withObjectId = false)
		{
			var objectPart = withObjectId ? "&object-id={ZZZZZZZZ-0000-0000-0000-000000000000}" : "";
			var terminator = withObjectId ? "E" : "end";

			return "onenote:https://d.docs.live.net/x/Notebook/Section.one#Old" +
				"&section-id={AAAAAAAA-0000-0000-0000-000000000000}" +
				$"&page-id={bracedPageId}{objectPart}&{terminator}";
		}


		[TestMethod]
		public async Task RelinkPage_AnchorMatchingRenameMap_RewritesHref()
		{
			const string PageId = "page-1";
			const string OldTargetId = "{old-target}";
			const string NewTargetId = "new-target-2";

			var page = new PageBuilder(PageId)
				.WithParagraph($"<a href=\"{AnchorHref(OldTargetId)}\">Link</a>")
				.Build();

			SetupPage(PageId, page);

			var renameMap = new Dictionary<string, string> { [OldTargetId] = NewTargetId };

			await using var one = new OneNote();
			await HierarchyLinkReconciler.RelinkPage(one, PageId, renameMap);

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var text = updated.Descendants(Ns + "T").First().Value;
			StringAssert.Contains(text, $"page-id={{{NewTargetId}}}");
			Assert.IsFalse(text.Contains(OldTargetId), "old target id should no longer appear");
		}


		[TestMethod]
		public async Task RelinkPage_ObjectIdAnchor_DegradesToPageLevelLink()
		{
			const string PageId = "page-1";
			const string OldTargetId = "{old-target}";
			const string NewTargetId = "new-target-3";

			var page = new PageBuilder(PageId)
				.WithParagraph($"<a href=\"{AnchorHref(OldTargetId, withObjectId: true)}\">Link</a>")
				.Build();

			SetupPage(PageId, page);

			var renameMap = new Dictionary<string, string> { [OldTargetId] = NewTargetId };

			await using var one = new OneNote();
			await HierarchyLinkReconciler.RelinkPage(one, PageId, renameMap);

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var text = updated.Descendants(Ns + "T").First().Value;
			StringAssert.Contains(text, $"page-id={{{NewTargetId}}}");
			Assert.IsFalse(text.Contains("object-id="), "paragraph anchor should degrade to page-level");
		}


		[TestMethod]
		public async Task RelinkPage_NoMatchingAnchor_LeavesPageUnchanged()
		{
			const string PageId = "page-1";
			const string UnrelatedId = "{unrelated}";

			var originalHref = AnchorHref(UnrelatedId);

			var page = new PageBuilder(PageId)
				.WithParagraph($"<a href=\"{originalHref}\">Link</a>")
				.Build();

			SetupPage(PageId, page);

			// renameMap has no entry for {unrelated} - nothing on this page should change
			var renameMap = new Dictionary<string, string> { ["{some-other-id}"] = "new-target-4" };

			await using var one = new OneNote();
			await HierarchyLinkReconciler.RelinkPage(one, PageId, renameMap);

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated);

			var text = updated.Descendants(Ns + "T").First().Value;
			StringAssert.Contains(text, originalHref);
		}


		[TestMethod]
		public async Task RelinkPage_EmptyRenameMap_DoesNotThrow()
		{
			const string PageId = "page-1";

			var page = new PageBuilder(PageId)
				.WithParagraph("plain text, no links")
				.Build();

			SetupPage(PageId, page);

			await using var one = new OneNote();
			await HierarchyLinkReconciler.RelinkPage(one, PageId, new Dictionary<string, string>());

			// no assertion needed beyond "didn't throw"; an empty map simply matches nothing
		}
	}
}
