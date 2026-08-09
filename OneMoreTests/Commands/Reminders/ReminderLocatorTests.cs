//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Reminders
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	[TestClass]
	public class ReminderLocatorTests
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static Page MakePage(params XElement[] oes)
		{
			var builder = new PageBuilder();
			foreach (var oe in oes)
			{
				builder.WithElement(oe);
			}

			return new Page(builder.BuildElement());
		}


		[TestMethod]
		public void GetAnchorId_NoMeta_ReturnsNull()
		{
			var oe = new XElement(Ns + "OE", new XElement(Ns + "T", new XCData("hello")));
			var page = MakePage(oe);
			var locator = new ReminderLocator(null, page, null);

			Assert.IsNull(locator.GetAnchorId(oe));
		}


		[TestMethod]
		public void EnsureAnchor_NoExistingMeta_CreatesAnchorAfterTag()
		{
			var tag = new XElement(Ns + "Tag", new XAttribute("index", "0"));
			var oe = new XElement(Ns + "OE", tag, new XElement(Ns + "T", new XCData("hello")));
			var page = MakePage(oe);
			var locator = new ReminderLocator(null, page, null);

			var anchorId = locator.EnsureAnchor(oe);

			Assert.IsFalse(string.IsNullOrEmpty(anchorId));
			Assert.AreEqual(anchorId, locator.GetAnchorId(oe));

			// Meta must be inserted immediately after Tag per OE schema sequence
			var children = oe.Elements().ToList();
			Assert.AreEqual(0, children.IndexOf(tag));
			Assert.AreEqual("Meta", children[1].Name.LocalName);
		}


		[TestMethod]
		public void EnsureAnchor_ExistingMeta_ReturnsSameValueWithoutDuplicating()
		{
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "Meta",
					new XAttribute("name", MetaNames.ReminderAnchor),
					new XAttribute("content", "abc123")),
				new XElement(Ns + "T", new XCData("hello")));

			var page = MakePage(oe);
			var locator = new ReminderLocator(null, page, null);

			var anchorId = locator.EnsureAnchor(oe);

			Assert.AreEqual("abc123", anchorId);
			Assert.AreEqual(1, oe.Elements(Ns + "Meta")
				.Count(e => e.Attribute("name").Value == MetaNames.ReminderAnchor));
		}


		[TestMethod]
		public void EnsureAnchor_ForceNew_ReplacesContentInPlace()
		{
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "Meta",
					new XAttribute("name", MetaNames.ReminderAnchor),
					new XAttribute("content", "abc123")),
				new XElement(Ns + "T", new XCData("hello")));

			var page = MakePage(oe);
			var locator = new ReminderLocator(null, page, null);

			var anchorId = locator.EnsureAnchor(oe, forceNew: true);

			Assert.AreNotEqual("abc123", anchorId);
			Assert.AreEqual(anchorId, locator.GetAnchorId(oe));
			Assert.AreEqual(1, oe.Elements(Ns + "Meta")
				.Count(e => e.Attribute("name").Value == MetaNames.ReminderAnchor));
		}


		[TestMethod]
		public async System.Threading.Tasks.Task Resolve_CanonicalOE_ReturnsOriginalReminderUnchanged()
		{
			const string anchorId = "shared-anchor";

			var canonicalOE = new XElement(Ns + "OE",
				new XElement(Ns + "Meta",
					new XAttribute("name", MetaNames.ReminderAnchor),
					new XAttribute("content", anchorId)),
				new XElement(Ns + "T", new XCData("original")));

			var duplicateOE = new XElement(Ns + "OE",
				new XElement(Ns + "Meta",
					new XAttribute("name", MetaNames.ReminderAnchor),
					new XAttribute("content", anchorId)),
				new XElement(Ns + "T", new XCData("copy")));

			var page = MakePage(canonicalOE, duplicateOE);

			var original = new Reminder("obj-1") { AnchorId = anchorId, Subject = "original" };
			var reminders = new List<Reminder> { original };

			var locator = new ReminderLocator(null, page, reminders);
			var resolved = await locator.Resolve(canonicalOE);

			Assert.AreSame(original, resolved);
			Assert.AreEqual(1, reminders.Count);
		}


		[TestMethod]
		public async System.Threading.Tasks.Task Resolve_SamePageDuplicateAnchor_SplitsIntoNewReminder()
		{
			const string anchorId = "shared-anchor";

			var canonicalOE = new XElement(Ns + "OE",
				new XElement(Ns + "Meta",
					new XAttribute("name", MetaNames.ReminderAnchor),
					new XAttribute("content", anchorId)),
				new XElement(Ns + "T", new XCData("original")));

			// no objectID attribute: keeps CloneAsNewAnchor from needing a live OneNote
			// instance to regenerate a hyperlink
			var duplicateOE = new XElement(Ns + "OE",
				new XElement(Ns + "Meta",
					new XAttribute("name", MetaNames.ReminderAnchor),
					new XAttribute("content", anchorId)),
				new XElement(Ns + "T", new XCData("copy")));

			var page = MakePage(canonicalOE, duplicateOE);

			var original = new Reminder("obj-1")
			{
				AnchorId = anchorId,
				Subject = "original",
				Assignee = "Alice"
			};
			var reminders = new List<Reminder> { original };

			var locator = new ReminderLocator(null, page, reminders);
			var resolved = await locator.Resolve(duplicateOE);

			Assert.AreNotSame(original, resolved);
			Assert.AreEqual(2, reminders.Count);
			Assert.AreNotEqual(original.AnchorId, resolved.AnchorId);
			Assert.AreEqual("Alice", resolved.Assignee, "fields should be copied as-is from the source");
			Assert.AreEqual(resolved.AnchorId, locator.GetAnchorId(duplicateOE));

			// original's own anchor and record must be untouched
			Assert.AreEqual(anchorId, original.AnchorId);
			Assert.AreEqual(anchorId, locator.GetAnchorId(canonicalOE));
		}


		[TestMethod]
		public async System.Threading.Tasks.Task Resolve_NoAnchorYet_MigratesLegacyRecordByObjectId()
		{
			var oe = new XElement(Ns + "OE",
				new XAttribute("objectID", "legacy-obj"),
				new XElement(Ns + "T", new XCData("legacy")));

			var page = MakePage(oe);

			var legacy = new Reminder("legacy-obj") { AnchorId = null, Subject = "legacy" };
			var reminders = new List<Reminder> { legacy };

			var locator = new ReminderLocator(null, page, reminders);
			var resolved = await locator.Resolve(oe);

			Assert.AreSame(legacy, resolved);
			Assert.IsFalse(string.IsNullOrEmpty(resolved.AnchorId));
			Assert.AreEqual(resolved.AnchorId, locator.GetAnchorId(oe));
		}


		[TestMethod]
		public async System.Threading.Tasks.Task Resolve_NoAnchorAndNoObjectId_ReturnsNull()
		{
			// no objectID attribute at all -- MigrateLegacy must bail out before touching
			// `one` (which is null here), rather than throwing
			var oe = new XElement(Ns + "OE", new XElement(Ns + "T", new XCData("brand new")));
			var page = MakePage(oe);
			var reminders = new List<Reminder>();

			var locator = new ReminderLocator(null, page, reminders);
			var resolved = await locator.Resolve(oe);

			Assert.IsNull(resolved);
		}
	}
}
