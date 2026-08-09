//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Resolves the Reminder record associated with a tagged paragraph (OE), using a
	/// OneMore-owned GUID anchor nested in a one:Meta on the OE as the authoritative,
	/// cross-machine-stable identity. Consolidates lookup logic previously duplicated
	/// across RemindCommand, DeleteReminderCommand, and CompleteReminderCommand.
	/// </summary>
	internal class ReminderLocator : Loggable
	{
		private readonly OneNote one;
		private readonly Page page;
		private readonly XNamespace ns;
		private readonly List<Reminder> reminders;


		/// <summary>
		/// Initialize a new locator bound to the given page and its currently loaded
		/// reminders. The caller remains responsible for persisting reminders (via
		/// ReminderSerializer.StoreReminder or page.SetMeta+EncodeContent) and the page
		/// (via one.Update) exactly as it did before; this locator only resolves identity
		/// and, where safe, mutates the paragraph's anchor Meta and the reminders list.
		/// </summary>
		public ReminderLocator(OneNote one, Page page, List<Reminder> reminders)
		{
			this.one = one;
			this.page = page;
			ns = page.Namespace;
			this.reminders = reminders ?? new List<Reminder>();
		}


		/// <summary>
		/// Resolves the Reminder associated with the given paragraph. Migrates a legacy
		/// pre-anchor record (matched by ObjectId/ObjectUri, scoped to this page) in place,
		/// and splits a copy/paste duplicate anchor (same-page, or copied in from another
		/// page/section/notebook) into its own independent record. Returns null if no
		/// reminder is currently associated with this paragraph, in which case the caller
		/// should create a new one (see CreateAnchor).
		/// </summary>
		public async Task<Reminder> Resolve(XElement paragraph)
		{
			var anchorId = GetAnchorId(paragraph);

			if (!string.IsNullOrEmpty(anchorId))
			{
				var reminder = reminders.Find(r => r.AnchorId == anchorId);
				if (reminder != null)
				{
					// same-page copy/paste collision: the first OE found in document
					// order owns the anchor; any other OE sharing it gets split off
					var canonical = FindCanonicalOE(anchorId);
					return canonical != null && !ReferenceEquals(canonical, paragraph)
						? CloneAsNewAnchor(paragraph, reminder)
						: reminder;
				}

				// anchor present but unknown on this page: paragraph was likely
				// copy/pasted here from another page, section, or notebook
				return await CloneFromElsewhere(paragraph, anchorId);
			}

			// no anchor yet: try to migrate a legacy pre-anchor record
			return MigrateLegacy(paragraph);
		}


		/// <summary>
		/// Locates the OE currently anchored to the given reminder, migrating a legacy
		/// pre-anchor record in place if found by its ObjectId/ObjectUri. Read-only and
		/// safe to call from a background thread; unlike Resolve, never splits a same-page
		/// duplicate anchor since that requires a user-visible page mutation.
		/// </summary>
		public XElement Locate(Reminder reminder)
		{
			if (!string.IsNullOrEmpty(reminder.AnchorId))
			{
				return FindCanonicalOE(reminder.AnchorId);
			}

			var oe = page.Root.Descendants(ns + "OE")
				.FirstOrDefault(e => e.Attribute("objectID")?.Value == reminder.ObjectId);

			if (oe == null && !string.IsNullOrEmpty(reminder.ObjectUri))
			{
				oe = page.Root.Descendants(ns + "OE").FirstOrDefault(e =>
				{
					var id = e.Attribute("objectID")?.Value;
					return id != null && one.GetHyperlink(page.PageId, id) == reminder.ObjectUri;
				});
			}

			if (oe != null)
			{
				reminder.AnchorId = EnsureAnchor(oe);
			}

			return oe;
		}


		/// <summary>
		/// Stamps a fresh anchor onto a newly created reminder's paragraph.
		/// </summary>
		public string CreateAnchor(XElement paragraph)
		{
			return EnsureAnchor(paragraph);
		}


		private Reminder MigrateLegacy(XElement paragraph)
		{
			var objectID = paragraph.Attribute("objectID")?.Value;
			if (string.IsNullOrEmpty(objectID))
			{
				return null;
			}

			var legacy = reminders.Find(r =>
				string.IsNullOrEmpty(r.AnchorId) && r.ObjectId == objectID);

			if (legacy == null)
			{
				var uri = one.GetHyperlink(page.PageId, objectID);
				legacy = reminders.Find(r =>
					string.IsNullOrEmpty(r.AnchorId) && r.ObjectUri == uri);
			}

			if (legacy != null)
			{
				legacy.AnchorId = EnsureAnchor(paragraph);
			}

			return legacy;
		}


		private Reminder CloneAsNewAnchor(XElement paragraph, Reminder source)
		{
			// deep-copy the source's fields as-is rather than hand-listing properties
			var serializer = new ReminderSerializer();
			var clone = serializer.Decode(serializer.Encode(source));

			clone.ObjectId = paragraph.Attribute("objectID")?.Value;
			clone.ObjectUri = string.IsNullOrEmpty(clone.ObjectId)
				? null
				: one.GetHyperlink(page.PageId, clone.ObjectId);
			clone.AnchorId = EnsureAnchor(paragraph, forceNew: true);

			reminders.Add(clone);
			return clone;
		}


		private async Task<Reminder> CloneFromElsewhere(XElement paragraph, string anchorId)
		{
			var hierarchy = await one.SearchMeta(string.Empty, MetaNames.Reminder);
			if (hierarchy == null)
			{
				return null;
			}

			var hns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);
			var serializer = new ReminderSerializer();

			var found = hierarchy.Descendants(hns + "Meta")
				.Where(e => e.Attribute("name").Value == MetaNames.Reminder)
				.SelectMany(e => serializer.DecodeContent(e.Attribute("content").Value))
				.FirstOrDefault(r => r.AnchorId == anchorId);

			if (found == null)
			{
				// dangling/foreign anchor with no discoverable source; leave the
				// paragraph's anchor untouched, a later interaction can retry
				logger.WriteLine($"reminder anchor {anchorId} not found anywhere in hierarchy");
				return null;
			}

			return CloneAsNewAnchor(paragraph, found);
		}


		private XElement FindCanonicalOE(string anchorId)
		{
			return page.Root.Descendants(ns + "OE")
				.FirstOrDefault(oe => GetAnchorId(oe) == anchorId);
		}


		/// <summary>
		/// Reads the anchor Meta content nested in the given OE, or null if not anchored.
		/// </summary>
		internal string GetAnchorId(XElement oe)
		{
			return oe.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name")?.Value == MetaNames.ReminderAnchor)
				?.Attribute("content")?.Value;
		}


		/// <summary>
		/// Finds or creates the anchor Meta on the given OE, respecting OE schema element
		/// order (MediaIndex, Tag, OutlookTask, then Meta, then List/content). When forceNew
		/// is true, always mints a fresh GUID even if one already exists, used to split a
		/// copy/paste duplicate off from its source's anchor.
		/// </summary>
		internal string EnsureAnchor(XElement oe, bool forceNew = false)
		{
			var meta = oe.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name")?.Value == MetaNames.ReminderAnchor);

			if (meta != null && !forceNew)
			{
				return meta.Attribute("content").Value;
			}

			var anchorId = Guid.NewGuid().ToString("N");

			if (meta != null)
			{
				meta.Attribute("content").Value = anchorId;
				return anchorId;
			}

			meta = new XElement(ns + "Meta",
				new XAttribute("name", MetaNames.ReminderAnchor),
				new XAttribute("content", anchorId));

			var anchorEl =
				oe.Elements(ns + "OutlookTask").LastOrDefault() ??
				oe.Elements(ns + "Tag").LastOrDefault() ??
				oe.Elements(ns + "MediaIndex").LastOrDefault();

			if (anchorEl is null) oe.AddFirst(meta);
			else anchorEl.AddAfterSelf(meta);

			return anchorId;
		}
	}
}
