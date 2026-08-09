//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Text;


	/// <summary>
	/// Serializes and deserializes a Reminder for compact storage in a Meta tag. This solves
	/// two problems. First, to obfuscate nested quotes within the XML. Second, to compress
	/// the objects as compact as possible. The disadvange is that they have to be decoded
	/// and deserialized before interrogation but it should be OK, right?!
	/// </summary>
	internal class ReminderSerializer
	{
		private const string JDateFormat = "yyyy-MM-ddTHH:mm";
		private const char Delimiter = ';';
		private readonly ILogger logger;


		public ReminderSerializer()
		{
			logger = Logger.Current;
		}


		/// <summary>
		/// Retrieves the collection of reminders for the given page.
		/// </summary>
		/// <param name="page">The page containing reminders</param>
		/// <returns>A List of Reminders; this may be an empty list</returns>
		public List<Reminder> LoadReminders(Page page)
		{
			var meta = page.Root.Elements(page.Namespace + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.Reminder);

			if (meta != null)
			{
				return DecodeContent(meta.Attribute("content").Value);
			}

			return new List<Reminder>();
		}


		/// <summary>
		/// Stores the given reminder on the page, possibly appending to a collection of
		/// reminders that already exist on the page.
		/// </summary>
		/// <param name="page">The page to update</param>
		/// <param name="reminder">The reminder to store</param>
		public void StoreReminder(Page page, Reminder reminder)
		{
			var meta = page.Root.Elements(page.Namespace + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.Reminder);

			if (meta == null)
			{
				page.SetMeta(MetaNames.Reminder, EncodeContent(new[] { reminder }));
				return;
			}

			var reminders = DecodeContent(meta.Attribute("content").Value);

			// match by AnchorId when known, but also fall back to an ObjectId match among
			// not-yet-anchored records; the incoming reminder may have just been migrated
			// to an anchor in this same call, in which case the on-page copy being replaced
			// here (freshly reloaded above) still predates that migration and has no AnchorId
			var old = (!string.IsNullOrEmpty(reminder.AnchorId)
					? reminders.Find(r => r.AnchorId == reminder.AnchorId)
					: null)
				?? reminders.Find(r => string.IsNullOrEmpty(r.AnchorId) && r.ObjectId == reminder.ObjectId);

			if (old != null)
			{
				reminders.Remove(old);
			}

			reminders.Add(reminder);

			page.SetMeta(MetaNames.Reminder, EncodeContent(reminders));
		}


		/// <summary>
		/// Encodes a collection of reminders into a string suited for storage in a single
		/// page level one:Meta.content attribute. This page meta appears in the hierarchy
		/// returned by one.FindMeta so becomes quickly searchable without fetching pages.
		/// The entire collection is serialized as a single JSON array and compressed in one
		/// gzip+base64 pass so compression can exploit redundancy across records, rather
		/// than compressing each reminder independently.
		/// </summary>
		/// <param name="reminders">A collection of reminders, normally a List</param>
		/// <returns>A string of encoded reminders</returns>
		public string EncodeContent(IEnumerable<Reminder> reminders)
		{
			var list = reminders as ICollection<Reminder> ?? reminders.ToList();
			if (list.Count == 0)
			{
				return string.Empty;
			}

			try
			{
				var json = JsonConvert.SerializeObject(list,
					new JsonSerializerSettings { DateFormatString = JDateFormat });

				using var stream = new MemoryStream();

				// do not simplify this statement to ensure stream is Flushed and Closed
				using (var zipper = new GZipStream(stream, CompressionMode.Compress))
				{
					var bytes = Encoding.UTF8.GetBytes(json);
					zipper.Write(bytes, 0, bytes.Length);
				}

				return Convert.ToBase64String(stream.ToArray());
			}
			catch (Exception exc)
			{
				logger.WriteLine("error encoding reminders", exc);
			}

			return string.Empty;
		}


		/// <summary>
		/// Decodes a string of serialized reminders, produced by either the current batched
		/// EncodeContent or the legacy per-record, delimited format.
		/// </summary>
		/// <param name="content">The string to decode</param>
		/// <returns>A List of reminders deserialized from the string</returns>
		public List<Reminder> DecodeContent(string content)
		{
			if (string.IsNullOrWhiteSpace(content))
			{
				return new List<Reminder>();
			}

			// try the current single-blob format first; a legacy, per-record blob will
			// fail here, either because it contains the ';' delimiter (invalid base64) or,
			// for a lone legacy reminder, because it decodes to a JSON object rather than
			// a JSON array
			return DecodeBatch(content) ?? DecodeLegacyContent(content);
		}


		/// <summary>
		/// Decodes a single gzip+base64 blob containing the entire reminder collection for
		/// a page, as produced by EncodeContent.
		/// </summary>
		/// <param name="content">The string to decode</param>
		/// <returns>A List of reminders, or null if content is not in this format</returns>
		private List<Reminder> DecodeBatch(string content)
		{
			try
			{
				var bytes = Convert.FromBase64String(content);
				using var stream = new MemoryStream(bytes);
				using var zipper = new GZipStream(stream, CompressionMode.Decompress);
				using var reader = new StreamReader(zipper, Encoding.UTF8);
				var json = reader.ReadToEnd();

				var reminders = JsonConvert.DeserializeObject<List<Reminder>>(
					json,
					new JsonSerializerSettings { DateFormatString = JDateFormat });

				if (reminders is null)
				{
					return null;
				}

				reminders.ForEach(Normalize);
				return reminders;
			}
			catch
			{
				// not the current batched format; caller falls back to legacy decoding
				return null;
			}
		}


		/// <summary>
		/// Decodes a legacy Meta content string consisting of individually gzip+base64
		/// encoded reminders joined by Delimiter.
		/// </summary>
		/// <param name="content">The string to decode</param>
		/// <returns>A List of reminders deserialized from the string</returns>
		private List<Reminder> DecodeLegacyContent(string content)
		{
			var reminders = new List<Reminder>();

			var parts = content.Split(Delimiter);
			foreach (var part in parts)
			{
				if (!string.IsNullOrWhiteSpace(part))
				{
					var reminder = Decode(part);
					// possible deserialization error
					if (reminder != null)
					{
						reminders.Add(reminder);
					}
				}
			}

			return reminders;
		}


		/// <summary>
		/// Decodes the encoded content of this meta and returns it as a deserialized object
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize</typeparam>
		/// <returns>An instance of T</returns>
		public Reminder Decode(string value)
		{
			try
			{
				var bytes = Convert.FromBase64String(value);
				using var stream = new MemoryStream(bytes);
				using var zipper = new GZipStream(stream, CompressionMode.Decompress);
				using var reader = new StreamReader(zipper, Encoding.UTF8);
				var json = reader.ReadToEnd();

				// TODO: read-makes-right version check here...

				var content = JsonConvert.DeserializeObject<Reminder>(
					json,
					new JsonSerializerSettings { DateFormatString = JDateFormat });

				Normalize(content);
				return content;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error decoding reminder", exc);
			}

			return null;
		}


		/// <summary>
		/// Normalizes a reminder freshly deserialized from JSON: DateTime properties come
		/// back with DateTimeKind.Unspecified and must be marked Utc, and records serialized
		/// before Assignee existed deserialize it as null.
		/// </summary>
		/// <param name="content">The reminder to normalize in place</param>
		private static void Normalize(Reminder content)
		{
			content.Completed = DateTime.SpecifyKind(content.Completed, DateTimeKind.Utc);
			content.Due = DateTime.SpecifyKind(content.Due, DateTimeKind.Utc);
			content.SnoozeTime = DateTime.SpecifyKind(content.SnoozeTime, DateTimeKind.Utc);
			content.Start = DateTime.SpecifyKind(content.Start, DateTimeKind.Utc);
			content.Started = DateTime.SpecifyKind(content.Started, DateTimeKind.Utc);

			content.Assignee ??= string.Empty;
		}


		/// <summary>
		/// Sets teh value of this meta element to a serialized, compressed, and encoded string
		/// </summary>
		/// <typeparam name="T">The Type of the content object to serialize</typeparam>
		/// <param name="content">The object to serialize</param>
		/// <returns></returns>
		public string Encode(Reminder content)
		{
			// serialize the object to JSON
			// compress using gzip which should decrease size 6:1
			// encode as base64 which will increase size 1:3 (base64 = [A-Za-z0-9+/])

			try
			{
				var json = JsonConvert.SerializeObject(content,
					new JsonSerializerSettings { DateFormatString = JDateFormat });

				using var stream = new MemoryStream();

				// do not simplify this statement to ensure stream is Flushed and Closed
				using (var zipper = new GZipStream(stream, CompressionMode.Compress))
				{
					var bytes = Encoding.UTF8.GetBytes(json);
					zipper.Write(bytes, 0, bytes.Length);
				}

				return Convert.ToBase64String(stream.ToArray());
			}
			catch (Exception exc)
			{
				logger.WriteLine("error encoding reminder", exc);
			}

			return null;
		}
	}
}
