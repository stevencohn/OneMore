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


	[TestClass]
	public class ReminderSerializerTests
	{
		private static Reminder MakeReminder(string objectId, string subject)
		{
			return new Reminder(objectId)
			{
				AnchorId = $"anchor-{objectId}",
				Subject = subject,
				Assignee = "Alice"
			};
		}


		[TestMethod]
		public void EncodeContent_Empty_ReturnsEmptyString()
		{
			var serializer = new ReminderSerializer();
			var encoded = serializer.EncodeContent(new List<Reminder>());

			Assert.AreEqual(string.Empty, encoded);
		}


		[TestMethod]
		public void DecodeContent_EmptyString_ReturnsEmptyList()
		{
			var serializer = new ReminderSerializer();
			var reminders = serializer.DecodeContent(string.Empty);

			Assert.IsNotNull(reminders);
			Assert.AreEqual(0, reminders.Count);
		}


		[TestMethod]
		public void EncodeDecodeContent_SingleReminder_RoundTrips()
		{
			var serializer = new ReminderSerializer();
			var original = MakeReminder("obj-1", "single reminder");

			var encoded = serializer.EncodeContent(new List<Reminder> { original });
			var decoded = serializer.DecodeContent(encoded);

			Assert.AreEqual(1, decoded.Count);
			Assert.AreEqual(original.ObjectId, decoded[0].ObjectId);
			Assert.AreEqual(original.AnchorId, decoded[0].AnchorId);
			Assert.AreEqual(original.Subject, decoded[0].Subject);
			Assert.AreEqual(original.Assignee, decoded[0].Assignee);
		}


		[TestMethod]
		public void EncodeDecodeContent_ManyReminders_RoundTrips()
		{
			var serializer = new ReminderSerializer();
			var originals = Enumerable.Range(1, 10)
				.Select(i => MakeReminder($"obj-{i}", $"reminder {i}"))
				.ToList();

			var encoded = serializer.EncodeContent(originals);
			var decoded = serializer.DecodeContent(encoded);

			Assert.AreEqual(originals.Count, decoded.Count);
			for (var i = 0; i < originals.Count; i++)
			{
				Assert.AreEqual(originals[i].ObjectId, decoded[i].ObjectId);
				Assert.AreEqual(originals[i].AnchorId, decoded[i].AnchorId);
				Assert.AreEqual(originals[i].Subject, decoded[i].Subject);
			}
		}


		[TestMethod]
		public void EncodeContent_ManyReminders_IsSmallerThanLegacyPerRecordEncoding()
		{
			var serializer = new ReminderSerializer();
			var reminders = Enumerable.Range(1, 10)
				.Select(i => MakeReminder($"obj-{i}", $"a representative subject line {i}"))
				.ToList();

			var batched = serializer.EncodeContent(reminders);
			var legacy = string.Join(";", reminders.Select(r => serializer.Encode(r)));

			Assert.IsTrue(batched.Length < legacy.Length,
				$"expected batched ({batched.Length}) to be smaller than legacy ({legacy.Length})");
		}


		[TestMethod]
		public void DecodeContent_LegacyPerRecordFormat_DecodesCorrectly()
		{
			var serializer = new ReminderSerializer();
			var originals = Enumerable.Range(1, 3)
				.Select(i => MakeReminder($"obj-{i}", $"legacy reminder {i}"))
				.ToList();

			// simulate Meta content written by the pre-batching implementation: each
			// reminder gzip+base64 encoded independently and joined with ';'
			var legacyContent = string.Join(";", originals.Select(r => serializer.Encode(r)));

			var decoded = serializer.DecodeContent(legacyContent);

			Assert.AreEqual(originals.Count, decoded.Count);
			for (var i = 0; i < originals.Count; i++)
			{
				Assert.AreEqual(originals[i].ObjectId, decoded[i].ObjectId);
				Assert.AreEqual(originals[i].Subject, decoded[i].Subject);
			}
		}


		[TestMethod]
		public void DecodeContent_SingleLegacyRecord_DecodesCorrectly()
		{
			var serializer = new ReminderSerializer();
			var original = MakeReminder("obj-1", "lone legacy reminder");

			// a single legacy record has no delimiter at all, so its base64 blob decodes
			// to a JSON object rather than a JSON array -- this must still fall back
			// correctly rather than being misread as a batch
			var legacyContent = serializer.Encode(original);

			var decoded = serializer.DecodeContent(legacyContent);

			Assert.AreEqual(1, decoded.Count);
			Assert.AreEqual(original.ObjectId, decoded[0].ObjectId);
			Assert.AreEqual(original.Subject, decoded[0].Subject);
		}


		[TestMethod]
		public void StoreReminder_NewPage_WritesBatchedFormatDecodableByDecodeContent()
		{
			var page = new Page(new PageBuilder().BuildElement());
			var serializer = new ReminderSerializer();
			var reminder = MakeReminder("obj-1", "first reminder on page");

			serializer.StoreReminder(page, reminder);

			var meta = page.Root.Elements(page.Namespace + "Meta")
				.First(e => e.Attribute("name").Value == MetaNames.Reminder);

			// the batched encoding of a single reminder is one unbroken base64 blob;
			// the legacy encoding of a single reminder also happens to be one blob, so
			// the real assertion is that round-tripping through the serializer works
			var reloaded = serializer.LoadReminders(page);

			Assert.AreEqual(1, reloaded.Count);
			Assert.AreEqual(reminder.ObjectId, reloaded[0].ObjectId);
			Assert.AreEqual(reminder.Subject, reloaded[0].Subject);
			Assert.IsFalse(string.IsNullOrEmpty(meta.Attribute("content").Value));
		}


		[TestMethod]
		public void StoreReminder_ExistingLegacyContent_RewritesInBatchedFormat()
		{
			var serializer = new ReminderSerializer();
			var existing = MakeReminder("obj-1", "existing legacy reminder");
			var legacyContent = serializer.Encode(existing) + ";" + serializer.Encode(
				MakeReminder("obj-2", "second existing legacy reminder"));

			var page = new Page(new PageBuilder()
				.WithMeta(MetaNames.Reminder, legacyContent)
				.BuildElement());

			var incoming = MakeReminder("obj-3", "newly added reminder");
			serializer.StoreReminder(page, incoming);

			var meta = page.Root.Elements(page.Namespace + "Meta")
				.First(e => e.Attribute("name").Value == MetaNames.Reminder);

			// rewritten content must no longer contain the legacy ';' delimiter, since
			// the new format is a single gzip+base64 blob over the whole JSON array
			Assert.IsFalse(meta.Attribute("content").Value.Contains(";"));

			var reloaded = serializer.LoadReminders(page);
			Assert.AreEqual(3, reloaded.Count);
		}
	}
}
