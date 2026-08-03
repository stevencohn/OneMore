//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Converts a single Evernote note's ENML content tree into Markdown text compatible
	/// with OneMore's existing Markdown importer (OneMoreDig / MarkdownConverter), so
	/// that pipeline can be reused to create the resulting OneNote page.
	/// </summary>
	internal class EnmlConverter
	{
		private static readonly Regex EscapePattern = new(@"([\\`*_\[\]<>])");
		private static readonly Regex InvalidTagCharPattern = new(@"[^\w\-]+");
		private static readonly Regex BoldPattern = new(@"font-weight\s*:\s*bold", RegexOptions.IgnoreCase);
		private static readonly Regex ItalicPattern = new(@"font-style\s*:\s*italic", RegexOptions.IgnoreCase);
		private static readonly Regex UnderlinePattern = new(@"text-decoration\s*:[^;]*underline", RegexOptions.IgnoreCase);
		private static readonly Regex StrikePattern = new(@"text-decoration\s*:[^;]*line-through", RegexOptions.IgnoreCase);

		private readonly EvernoteNote note;
		private readonly Dictionary<string, EvernoteResource> resourcesByHash;
		private readonly Func<EvernoteResource, string> imageWriter;
		private readonly Func<EvernoteResource, string> attachmentWriter;


		/// <summary>
		/// Creates a converter for the given note.
		/// </summary>
		/// <param name="note">The note to convert</param>
		/// <param name="imageWriter">
		/// Callback invoked once per referenced image resource; must write the resource's
		/// bytes to disk (typically the same temp directory as the generated .md file)
		/// and return the full path written to.
		/// </param>
		/// <param name="attachmentWriter">
		/// Callback invoked once per referenced non-image resource; must write the
		/// resource's bytes to disk (typically a durable folder alongside the source
		/// .enex, since only a link is preserved, not embedded content) and return the
		/// full path written to.
		/// </param>
		public EnmlConverter(
			EvernoteNote note,
			Func<EvernoteResource, string> imageWriter,
			Func<EvernoteResource, string> attachmentWriter)
		{
			this.note = note;
			this.imageWriter = imageWriter;
			this.attachmentWriter = attachmentWriter;

			resourcesByHash = note.Resources
				.Where(r => r.Hash != null)
				.GroupBy(r => r.Hash)
				.ToDictionary(g => g.Key, g => g.First());
		}


		/// <summary>
		/// True if this note contained one or more en-crypt (encrypted) sections that
		/// could not be decrypted and were replaced with a placeholder.
		/// </summary>
		public bool EncounteredEncryption { get; private set; }

		/// <summary>
		/// Full paths of non-image attachments that were extracted and linked, but not
		/// embedded as real page content, for reporting in the run summary.
		/// </summary>
		public List<string> UnattachedFiles { get; } = new();


		/// <summary>
		/// Converts the note to Markdown text, including a leading line of inline
		/// #hashtags derived from the note's Evernote tags.
		/// </summary>
		public string Convert()
		{
			var sb = new StringBuilder();

			var hashtags = note.Tags
				.Select(Slug)
				.Where(t => t != null)
				.ToList();

			if (hashtags.Count > 0)
			{
				sb.AppendLine(string.Join(" ", hashtags));
				sb.AppendLine();
			}

			if (note.Content != null)
			{
				WriteChildren(note.Content, sb);
			}

			return sb.ToString();
		}


		private void WriteChildren(XElement parent, StringBuilder sb)
		{
			foreach (var node in parent.Nodes())
			{
				WriteNode(node, sb);
			}
		}


		private void WriteNode(XNode node, StringBuilder sb)
		{
			if (node is XText text)
			{
				sb.Append(EscapeMarkdown(text.Value));
				return;
			}

			if (node is not XElement element)
			{
				return;
			}

			switch (element.Name.LocalName)
			{
				case "en-note":
					WriteChildren(element, sb);
					break;

				case "div":
					WriteDiv(element, sb);
					break;

				case "br":
					sb.AppendLine("  ");
					break;

				case "b":
				case "strong":
					WriteWrapped(element, sb, "**");
					break;

				case "i":
				case "em":
					WriteWrapped(element, sb, "*");
					break;

				case "s":
				case "strike":
				case "del":
					WriteWrapped(element, sb, "~~");
					break;

				case "u":
					WriteHtmlWrapped(element, sb, "u");
					break;

				case "sup":
					WriteHtmlWrapped(element, sb, "sup");
					break;

				case "sub":
					WriteHtmlWrapped(element, sb, "sub");
					break;

				case "span":
				case "font":
					WriteStyledSpan(element, sb);
					break;

				case "a":
					WriteAnchor(element, sb);
					break;

				case "ul":
					WriteList(element, sb, ordered: false, depth: 0);
					break;

				case "ol":
					WriteList(element, sb, ordered: true, depth: 0);
					break;

				case "table":
					WriteTable(element, sb);
					break;

				case "hr":
					sb.AppendLine();
					sb.AppendLine("---");
					break;

				case "en-media":
					WriteMedia(element, sb);
					break;

				case "en-todo":
					// only reached when an en-todo appears outside the expected
					// "<div><en-todo/>text</div>" shape; best-effort inline rendering
					sb.Append(element.Attribute("checked")?.Value == "true" ? "[x] " : "[ ] ");
					break;

				case "en-crypt":
					WriteCrypt(element, sb);
					break;

				default:
					// unsupported element (e.g. recognition/application-data markup):
					// descend into children so text content isn't silently lost
					WriteChildren(element, sb);
					break;
			}
		}


		private void WriteDiv(XElement element, StringBuilder sb)
		{
			var todo = element.Elements("en-todo").FirstOrDefault();
			if (todo != null)
			{
				sb.Append("- ").Append(todo.Attribute("checked")?.Value == "true" ? "[x] " : "[ ] ");
				foreach (var node in element.Nodes())
				{
					if (node != todo)
					{
						WriteNode(node, sb);
					}
				}
				sb.AppendLine();
				return;
			}

			WriteChildren(element, sb);
			sb.AppendLine();
		}


		private void WriteWrapped(XElement element, StringBuilder sb, string marker)
		{
			var inner = new StringBuilder();
			WriteChildren(element, inner);
			if (inner.Length > 0)
			{
				sb.Append(marker).Append(inner).Append(marker);
			}
		}


		private void WriteHtmlWrapped(XElement element, StringBuilder sb, string tag)
		{
			var inner = new StringBuilder();
			WriteChildren(element, inner);
			if (inner.Length > 0)
			{
				sb.Append('<').Append(tag).Append('>').Append(inner).Append("</").Append(tag).Append('>');
			}
		}


		private void WriteStyledSpan(XElement element, StringBuilder sb)
		{
			var style = element.Attribute("style")?.Value ?? string.Empty;

			var inner = new StringBuilder();
			WriteChildren(element, inner);
			var text = inner.ToString();
			if (text.Length == 0)
			{
				return;
			}

			if (UnderlinePattern.IsMatch(style)) { text = $"<u>{text}</u>"; }
			if (StrikePattern.IsMatch(style)) { text = $"~~{text}~~"; }
			if (ItalicPattern.IsMatch(style)) { text = $"*{text}*"; }
			if (BoldPattern.IsMatch(style)) { text = $"**{text}**"; }

			sb.Append(text);
		}


		private void WriteAnchor(XElement element, StringBuilder sb)
		{
			var href = element.Attribute("href")?.Value;

			var inner = new StringBuilder();
			WriteChildren(element, inner);
			var text = inner.ToString();

			if (string.IsNullOrEmpty(href))
			{
				sb.Append(text);
			}
			else if (string.IsNullOrEmpty(text))
			{
				sb.Append('<').Append(href).Append('>');
			}
			else
			{
				sb.Append('[').Append(text).Append("](").Append(href).Append(')');
			}
		}


		private void WriteList(XElement element, StringBuilder sb, bool ordered, int depth)
		{
			sb.AppendLine();

			var index = 1;
			foreach (var li in element.Elements("li"))
			{
				var nestedLists = li.Elements()
					.Where(e => e.Name.LocalName is "ul" or "ol")
					.ToList();

				sb.Append(' ', depth * 2);
				sb.Append(ordered ? $"{index}. " : "- ");
				index++;

				var inline = new StringBuilder();
				foreach (var node in li.Nodes())
				{
					if (node is XElement e && (e.Name.LocalName is "ul" or "ol"))
					{
						continue;
					}
					WriteNode(node, inline);
				}

				sb.Append(inline.ToString().Trim());
				sb.AppendLine();

				foreach (var nested in nestedLists)
				{
					WriteList(nested, sb, nested.Name.LocalName == "ol", depth + 1);
				}
			}

			sb.AppendLine();
		}


		private void WriteTable(XElement element, StringBuilder sb)
		{
			var rows = element.Descendants("tr").ToList();
			if (rows.Count == 0)
			{
				return;
			}

			sb.AppendLine();

			var columnCount = rows.Max(r => r.Elements().Count(e => e.Name.LocalName is "td" or "th"));

			for (var i = 0; i < rows.Count; i++)
			{
				var cells = rows[i].Elements().Where(e => e.Name.LocalName is "td" or "th").ToList();

				sb.Append('|');
				foreach (var cell in cells)
				{
					var inner = new StringBuilder();
					WriteChildren(cell, inner);
					var text = inner.ToString().Replace("\r", string.Empty).Replace("\n", " ").Trim();
					sb.Append(' ').Append(text).Append(" |");
				}
				for (var c = cells.Count; c < columnCount; c++)
				{
					sb.Append("  |");
				}
				sb.AppendLine();

				if (i == 0)
				{
					sb.Append('|');
					for (var c = 0; c < columnCount; c++)
					{
						sb.Append(" --- |");
					}
					sb.AppendLine();
				}
			}

			sb.AppendLine();
		}


		private void WriteMedia(XElement element, StringBuilder sb)
		{
			var hash = element.Attribute("hash")?.Value;
			if (hash is null || !resourcesByHash.TryGetValue(hash, out var resource))
			{
				sb.Append("*[missing attachment]*");
				return;
			}

			if (resource.IsImage)
			{
				var path = imageWriter(resource);
				sb.Append("![").Append(EscapeMarkdown(resource.FileName ?? string.Empty))
					.Append("](").Append(ToMarkdownLink(path)).Append(')');
			}
			else
			{
				var path = attachmentWriter(resource);
				var name = string.IsNullOrEmpty(resource.FileName) ? Path.GetFileName(path) : resource.FileName;
				sb.Append('[').Append(EscapeMarkdown(name)).Append("](").Append(ToMarkdownLink(path)).Append(')');
				UnattachedFiles.Add(path);
			}
		}


		private void WriteCrypt(XElement element, StringBuilder sb)
		{
			EncounteredEncryption = true;

			var hint = element.Attribute("hint")?.Value;
			sb.Append(string.IsNullOrEmpty(hint)
				? "\U0001F512 *[Encrypted content omitted]*"
				: $"\U0001F512 *[Encrypted content omitted — hint: {EscapeMarkdown(hint)}]*");
		}


		private static string ToMarkdownLink(string fullPath) => new Uri(fullPath).AbsoluteUri;


		private static string EscapeMarkdown(string text) => EscapePattern.Replace(text, "\\$1");


		private static string Slug(string tag)
		{
			var slug = InvalidTagCharPattern.Replace(tag.Trim(), "-").Trim('-');
			if (slug.Length == 0)
			{
				return null;
			}

			return (char.IsDigit(slug[0]) ? "##" : "#") + slug;
		}
	}
}
