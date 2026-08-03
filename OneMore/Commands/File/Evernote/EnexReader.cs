//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Parses an Evernote .enex export file into a sequence of EvernoteNote records.
	/// </summary>
	/// <seealso cref="https://evernote.com/blog/how-evernotes-xml-export-format-works"/>
	/// <seealso cref="https://dev.evernote.com/doc/articles/enml.php"/>
	internal static class EnexReader
	{
		// ENEX created/updated timestamps use Evernote's fixed "basic format" UTC form,
		// e.g. "20130202T172415Z"
		private const string TimestampFormat = "yyyyMMdd'T'HHmmss'Z'";

		private static readonly Regex PrologPattern =
			new(@"<\?xml[^>]*\?>|<!DOCTYPE[^>]*>", RegexOptions.Singleline);

		// ENML content is its own XML document with a DOCTYPE referencing Evernote's
		// enml2.dtd, which declares extra named entities (e.g. &nbsp;) beyond the five
		// predefined XML entities. Rather than fetch that DTD over the network (slow,
		// fragile, and a potential XXE vector), the prolog/DOCTYPE is stripped and any
		// non-XML named entity is pre-decoded to its literal character.
		private static readonly Regex NamedEntityPattern =
			new(@"&(?!amp;|lt;|gt;|quot;|apos;|#)([a-zA-Z][a-zA-Z0-9]*);");


		/// <summary>
		/// Streams the notes contained in the given .enex file. Resources are decoded
		/// and hashed as they're read; ENML content is sanitized and parsed into an
		/// en-note XElement tree.
		/// </summary>
		public static IEnumerable<EvernoteNote> ReadNotes(string path)
		{
			var settings = new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Ignore,
				XmlResolver = null
			};

			using var reader = XmlReader.Create(path, settings);

			reader.MoveToContent();

			// XNode.ReadFrom already advances the reader past the element it consumed,
			// so calling Read() unconditionally at the top of every iteration would skip
			// the sibling <note> immediately following the one just yielded; only advance
			// manually when the current node wasn't already consumed that way
			while (true)
			{
				if (reader.NodeType == XmlNodeType.Element &&
					reader.Name == "note")
				{
					var element = (XElement)XNode.ReadFrom(reader);
					yield return ReadNote(element);
				}
				else if (!reader.Read())
				{
					yield break;
				}
			}
		}


		private static EvernoteNote ReadNote(XElement noteElement)
		{
			var note = new EvernoteNote
			{
				Title = noteElement.Element("title")?.Value,
				Created = ParseTimestamp(noteElement.Element("created")?.Value),
				Updated = ParseTimestamp(noteElement.Element("updated")?.Value)
			};

			note.Tags.AddRange(noteElement.Elements("tag").Select(e => e.Value));

			foreach (var resourceElement in noteElement.Elements("resource"))
			{
				var resource = ReadResource(resourceElement);
				if (resource != null)
				{
					note.Resources.Add(resource);
				}
			}

			var contentText = noteElement.Element("content")?.Value;
			if (!string.IsNullOrWhiteSpace(contentText))
			{
				note.Content = ParseEnml(contentText);
			}

			return note;
		}


		private static EvernoteResource ReadResource(XElement resourceElement)
		{
			var dataElement = resourceElement.Element("data");
			if (dataElement is null || string.IsNullOrWhiteSpace(dataElement.Value))
			{
				return null;
			}

			byte[] data;
			try
			{
				data = Convert.FromBase64String(dataElement.Value);
			}
			catch (FormatException)
			{
				return null;
			}

			using var md5 = MD5.Create();
			var hash = BitConverter.ToString(md5.ComputeHash(data))
				.Replace("-", string.Empty)
				.ToLowerInvariant();

			return new EvernoteResource
			{
				Hash = hash,
				Data = data,
				Mime = resourceElement.Element("mime")?.Value,
				FileName = resourceElement
					.Element("resource-attributes")?
					.Element("file-name")?
					.Value
			};
		}


		private static XElement ParseEnml(string content)
		{
			content = PrologPattern.Replace(content, string.Empty);

			content = NamedEntityPattern.Replace(content, m =>
			{
				var decoded = System.Net.WebUtility.HtmlDecode(m.Value);
				return decoded == m.Value ? " " : decoded;
			});

			return XElement.Parse(content);
		}


		private static DateTime? ParseTimestamp(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			return DateTime.TryParseExact(
				value, TimestampFormat, CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
				out var result)
				? result
				: null;
		}
	}
}
