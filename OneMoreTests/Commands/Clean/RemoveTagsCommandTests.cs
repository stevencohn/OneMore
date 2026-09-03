//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol - RemoveTagsCommand
	 * Removes all tags from the current page, except those that are associated with
	 * OneMore Reminders.
	 *
	 *   1. More/Clean/Remove Tags
	 *   2. Confirm all inter and intra paragraph spacing is removed
	 */

	[TestClass]
	public class RemoveTagsCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static Task ExecuteCommand()
		{
			var cmd = new RemoveTagsCommand();
			cmd.SetLogger(Logger.Current);
			return cmd.Execute();
		}


		private static XElement MakeTagDef(int index, int type, int symbol)
		{
			return new XElement(Ns + "TagDef",
				new XAttribute("index", index.ToString()),
				new XAttribute("type", type.ToString()),
				new XAttribute("symbol", symbol.ToString()),
				new XAttribute("fontColor", "automatic"),
				new XAttribute("highlightColor", "none"));
		}


		private static XElement MakeTaggedOE(
			string objectId, int tagIndex, bool completed, string text)
		{
			return new XElement(Ns + "OE",
				new XAttribute("objectID", objectId),
				new XElement(Ns + "Tag",
					new XAttribute("index", tagIndex.ToString()),
					new XAttribute("completed", completed ? "true" : "false"),
					new XAttribute("disabled", "false")),
				new XElement(Ns + "T", new XCData(text)));
		}


		[TestMethod]
		public async Task RemoveTags_MixedTagsWithoutReminders_RemovesAllTags()
		{
			// Arrange: two paragraphs tagged with ordinary (non-reminder) tags
			var oe1 = MakeTaggedOE("oe-1", 0, true, "paragraph one");
			var oe2 = MakeTaggedOE("oe-2", 1, false, "paragraph two");

			var page = new PageBuilder(PageId, "Remove Tags Test")
				.WithElement(oe1)
				.WithElement(oe2)
				.BuildElement();

			page.AddFirst(
				MakeTagDef(0, 1, 13),   // Important
				MakeTagDef(1, 0, 3));   // To Do

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");
			Assert.IsFalse(updated.Descendants(Ns + "Tag").Any(),
				"Expected all tags to be removed");
		}


		[TestMethod]
		public async Task RemoveTags_ReminderLinkedTag_IsPreserved()
		{
			// Arrange: one ordinary tag to be removed, one reminder-linked tag to keep
			const string reminderObjectId = "oe-reminder";
			const string reminderSymbol = "97";

			var oe1 = MakeTaggedOE("oe-1", 0, true, "paragraph one");
			var oe2 = MakeTaggedOE(reminderObjectId, 1, false, "paragraph two");

			var page = new PageBuilder(PageId, "Remove Tags Reminder Test")
				.WithElement(oe1)
				.WithElement(oe2)
				.BuildElement();

			page.AddFirst(
				MakeTagDef(0, 1, 13),                            // Important
				MakeTagDef(1, 0, int.Parse(reminderSymbol)));    // Reminder bell

			var reminder = new Reminder(reminderObjectId) { Symbol = reminderSymbol };
			var content = new ReminderSerializer().EncodeContent(new[] { reminder });
			page.Add(new XElement(Ns + "Meta",
				new XAttribute("name", MetaNames.Reminder),
				new XAttribute("content", content)));

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var remainingTags = updated.Descendants(Ns + "Tag").ToList();
			Assert.AreEqual(1, remainingTags.Count, "Expected only the reminder tag to remain");
			Assert.AreEqual(reminderObjectId, remainingTags[0].Parent.Attribute("objectID").Value,
				"Expected the surviving tag to be on the reminder-linked paragraph");
		}


		[TestMethod]
		public async Task RemoveTags_NoTagsOnPage_DoesNotCallUpdate()
		{
			// Arrange: page with plain paragraphs and no tags at all
			var xml = new PageBuilder(PageId, "No Tags Test")
				.WithParagraph("paragraph one")
				.WithParagraph("paragraph two")
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand();

			// Assert: page XML is unchanged because UpdatePageContent was never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(originalXml, storedXml,
				"Page should not have been updated when no tags exist");
		}
	}
}
