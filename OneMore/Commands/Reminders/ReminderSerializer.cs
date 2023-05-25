//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
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
				page.SetMeta(MetaNames.Reminder, Encode(reminder));
				return;
			}

			var reminders = DecodeContent(meta.Attribute("content").Value);

			var old = reminders.FirstOrDefault(r => r.ObjectId == reminder.ObjectId);
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
		/// </summary>
		/// <param name="reminders">A collection of reminders, normally a List</param>
		/// <returns>A string of encoded reminders</returns>
		public string EncodeContent(IEnumerable<Reminder> reminders)
		{
			var builder = new StringBuilder();
			foreach (var reminder in reminders)
			{
				builder.Append(Encode(reminder));
				builder.Append(Delimiter);
			}

			if (builder.Length == 0)
			{
				return string.Empty;
			}

			// strip off last delimiter
			return builder.ToString(0, builder.Length - 1);
		}


		/// <summary>
		/// Decodes a string of serialized reminders.
		/// </summary>
		/// <param name="content">The string to decode</param>
		/// <returns>A List of reminders deserialized from the string</returns>
		public List<Reminder> DecodeContent(string content)
		{
			var reminders = new List<Reminder>();
			if (string.IsNullOrWhiteSpace(content))
			{
				return reminders;
			}

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

				// convert kind from Unspecified to Utc
				content.Completed = DateTime.SpecifyKind(content.Completed, DateTimeKind.Utc);
				content.Due = DateTime.SpecifyKind(content.Due, DateTimeKind.Utc);
				content.SnoozeTime = DateTime.SpecifyKind(content.SnoozeTime, DateTimeKind.Utc);
				content.Start = DateTime.SpecifyKind(content.Start, DateTimeKind.Utc);
				content.Started = DateTime.SpecifyKind(content.Started, DateTimeKind.Utc);

				return content;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error decoding reminder", exc);
			}

			return null;
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
