//************************************************************************************************
// Export and storage helpers for OneSearch
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.OneSearch
{
	using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;
using System.Web.Script.Serialization;


	internal sealed class ExportSummary
	{
		public int Exported { get; set; }
		public int Skipped { get; set; }
		public int Failed { get; set; }
	}


	internal sealed class NotebookManifest
	{
		private const string ManifestFileName = "notebook.manifest.json";

		private readonly string manifestPath;
		private readonly Dictionary<string, NotebookEntry> map;


		public NotebookManifest(string cacheRoot)
		{
			manifestPath = Path.Combine(cacheRoot, ManifestFileName);
			map = Load(manifestPath);
		}


		public bool IsNotebookCurrent(string notebookId, string lastModified)
		{
			// disable notebook-level skipping to avoid false positives
			return false;
		}


		public bool IsSectionCurrent(string notebookId, string sectionId, string lastModified)
		{
			// disable section-level skipping to avoid false positives
			return false;
		}


		public void Save()
		{
			var json = new JavaScriptSerializer().Serialize(map);
			File.WriteAllText(manifestPath, json, Encoding.UTF8);
		}


		private static Dictionary<string, NotebookEntry> Load(string path)
		{
			try
			{
				if (File.Exists(path))
				{
					var json = File.ReadAllText(path, Encoding.UTF8);
					var loaded = new JavaScriptSerializer().Deserialize<Dictionary<string, NotebookEntry>>(json);
					if (loaded != null)
					{
						return loaded;
					}
				}
			}
			catch
			{
				// ignore malformed manifest, start fresh
			}

			return new Dictionary<string, NotebookEntry>(StringComparer.OrdinalIgnoreCase);
		}


		internal sealed class NotebookEntry
		{
			public string LastModified { get; set; }
			public Dictionary<string, string> Sections { get; set; } = new(StringComparer.OrdinalIgnoreCase);
		}
	}


	internal sealed class SyncProgress
	{
		public int Processed { get; set; }
		public int Total { get; set; }
		public int Exported { get; set; }
		public int Skipped { get; set; }
		public int Failed { get; set; }
		public string CurrentTitle { get; set; }
		public string CurrentNotebook { get; set; }
	}


internal sealed class OneNotePageInfo
{
	public string PageId { get; set; }
	public string PageTitle { get; set; }
		public string LastModifiedRaw { get; set; }
		public string SectionId { get; set; }
		public string SectionName { get; set; }
		public string NotebookId { get; set; }
		public string NotebookName { get; set; }
		public string SectionLastModified { get; set; }
		public string NotebookLastModified { get; set; }
	}


	internal sealed class OneNoteHierarchyReader
	{
		public int LastPageCount { get; private set; }


		public IEnumerable<OneNotePageInfo> GetAllPages()
		{
			using var one = new OneNote();
			var root = one.GetNotebooks(OneNote.Scope.Pages).GetAwaiter().GetResult();

			if (root == null)
			{
				yield break;
			}

			LastPageCount = root.Descendants()
				.Count(e => string.Equals(e.Name.LocalName, "Page", StringComparison.OrdinalIgnoreCase));

			var ns = root.Name.Namespace;

			foreach (var notebook in root.Elements(ns + "Notebook"))
			{
				foreach (var page in Walk(notebook, null, null, null, null, null, null))
				{
					yield return page;
				}
			}
		}


		private static IEnumerable<OneNotePageInfo> Walk(
			XElement node,
			string notebookId,
			string notebookName,
			string notebookModified,
			string sectionId,
			string sectionName,
			string sectionModified)
		{
			var localName = node.Name.LocalName;
			var nodeId = (string)node.Attribute("ID");
			var nodeName = (string)node.Attribute("name");
			var lastModified = (string)node.Attribute("lastModifiedTime");

			if (string.Equals(localName, "Notebook", StringComparison.OrdinalIgnoreCase))
			{
				notebookId = nodeId ?? notebookId;
				notebookName = nodeName ?? notebookName;
				notebookModified = lastModified ?? notebookModified;
			}
			else if (string.Equals(localName, "Section", StringComparison.OrdinalIgnoreCase))
			{
				sectionId = nodeId ?? sectionId;
				sectionName = nodeName ?? sectionName;
				sectionModified = lastModified ?? sectionModified;
			}
			else if (string.Equals(localName, "Page", StringComparison.OrdinalIgnoreCase))
			{
				yield return new OneNotePageInfo
				{
					PageId = nodeId,
					PageTitle = nodeName,
					LastModifiedRaw = lastModified,
					SectionId = sectionId,
					SectionName = sectionName,
					NotebookId = notebookId,
					NotebookName = notebookName,
					SectionLastModified = sectionModified,
					NotebookLastModified = notebookModified
				};
				yield break;
			}

			foreach (var child in node.Elements())
			{
				foreach (var page in Walk(child,
					notebookId, notebookName, notebookModified,
					sectionId, sectionName, sectionModified))
				{
					yield return page;
				}
			}
		}
	}


	internal sealed class MarkdownMetadata
	{
		public string PageId { get; set; }
		public string PageTitle { get; set; }
		public string PageLastModified { get; set; }
		public string SectionId { get; set; }
		public string SectionName { get; set; }
		public string NotebookId { get; set; }
		public string NotebookName { get; set; }


		public IEnumerable<string> ToHeaderLines()
		{
			yield return $"<!-- OneSearch:pageId={PageId ?? string.Empty} -->";
			yield return $"<!-- OneSearch:pageTitle={PageTitle ?? string.Empty} -->";
			yield return $"<!-- OneSearch:pageLastModified={PageLastModified ?? string.Empty} -->";
			yield return $"<!-- OneSearch:sectionId={SectionId ?? string.Empty} -->";
			yield return $"<!-- OneSearch:sectionName={SectionName ?? string.Empty} -->";
			yield return $"<!-- OneSearch:notebookId={NotebookId ?? string.Empty} -->";
			yield return $"<!-- OneSearch:notebookName={NotebookName ?? string.Empty} -->";
			yield return string.Empty;
		}


		public static MarkdownMetadata Parse(string content, out string body)
		{
			var metadata = new MarkdownMetadata();
			body = content ?? string.Empty;
			if (string.IsNullOrEmpty(content))
			{
				return metadata;
			}

			var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			var startLine = 0;
			var foundMetadata = false;

			for (var i = 0; i < lines.Length; i++)
			{
				var line = lines[i].Trim();
				if (line.StartsWith("<!-- OneSearch:", StringComparison.Ordinal) &&
					line.EndsWith("-->", StringComparison.Ordinal))
				{
					foundMetadata = true;
					var inner = line.Substring("<!-- OneSearch:".Length);
					inner = inner.Substring(0, inner.Length - "-->".Length).Trim();
					var parts = inner.Split(new[] { '=' }, 2);
					if (parts.Length == 2)
					{
						Assign(metadata, parts[0].Trim(), parts[1].Trim());
					}
					startLine = i + 1;
					continue;
				}

				if (foundMetadata && (line.Length == 0 || !line.StartsWith("<!--", StringComparison.Ordinal)))
				{
					if (line.Length == 0)
					{
						startLine = i + 1;
					}
					break;
				}
			}

			if (startLine > 0 && startLine < lines.Length)
			{
				body = string.Join(Environment.NewLine, lines, startLine, lines.Length - startLine);
			}

			return metadata;
		}


		private static void Assign(MarkdownMetadata metadata, string key, string value)
		{
			switch (key)
			{
				case "pageId":
					metadata.PageId = value;
					break;
				case "pageTitle":
					metadata.PageTitle = value;
					break;
				case "pageLastModified":
					metadata.PageLastModified = value;
					break;
				case "sectionId":
					metadata.SectionId = value;
					break;
				case "sectionName":
					metadata.SectionName = value;
					break;
				case "notebookId":
					metadata.NotebookId = value;
					break;
				case "notebookName":
					metadata.NotebookName = value;
					break;
			}
		}
	}


	internal static class MarkdownStore
	{
		public static string GetFileId(string pageId)
		{
			if (string.IsNullOrWhiteSpace(pageId))
			{
				return "unknown";
			}

			string core = pageId;

			var braceMatches = Regex.Matches(pageId, @"\{([^}]*)\}");
			if (braceMatches.Count > 0)
			{
				for (var i = braceMatches.Count - 1; i >= 0; i--)
				{
					var candidate = braceMatches[i].Groups[1].Value;
					if (!string.IsNullOrWhiteSpace(candidate))
					{
						core = candidate;
						break;
					}
				}
			}

			var sb = new StringBuilder(core.Length);
			foreach (var ch in core)
			{
				if (char.IsLetterOrDigit(ch))
				{
					sb.Append(ch);
				}
				else
				{
					sb.Append('_');
				}
			}

			var cleaned = sb.ToString().Trim('_');
			if (string.IsNullOrEmpty(cleaned))
			{
				cleaned = "unknown";
			}

			if (cleaned.Length > 80)
			{
				cleaned = cleaned.Substring(cleaned.Length - 80);
			}

			return cleaned;
		}


		public static string GetPagePath(string cacheRoot, OneNotePageInfo page)
		{
			var notebookFolder = $"{SanitizePathSegment(page.NotebookName ?? page.NotebookId ?? "Notebook")}_{ShortId(page.NotebookId)}";
			var sectionFolder = $"{SanitizePathSegment(page.SectionName ?? page.SectionId ?? "Section")}_{ShortId(page.SectionId)}";
			var fileName = $"{GetFileId(page.PageId)}.md";
			return Path.Combine(cacheRoot, notebookFolder, sectionFolder, fileName);
		}


		public static bool IsUpToDate(string filePath, OneNotePageInfo page)
		{
			if (string.IsNullOrWhiteSpace(filePath) || page == null || string.IsNullOrWhiteSpace(page.LastModifiedRaw))
			{
				return false;
			}

			if (!File.Exists(filePath))
			{
				return false;
			}

			if (!TryReadMetadata(filePath, out var metadata))
			{
				return false;
			}

			if (!string.Equals(metadata.PageId, page.PageId, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return string.Equals(metadata.PageLastModified, page.LastModifiedRaw, StringComparison.OrdinalIgnoreCase);
		}


		public static bool TryReadMetadata(string filePath, out MarkdownMetadata metadata)
		{
			metadata = null;
			if (!File.Exists(filePath))
			{
				return false;
			}

			var content = File.ReadAllText(filePath, Encoding.UTF8);
			metadata = MarkdownMetadata.Parse(content, out _);
			return metadata != null;
		}


		public static void WritePage(string filePath, MarkdownMetadata metadata, string body)
		{
			var directory = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var sb = new StringBuilder();
			foreach (var line in metadata.ToHeaderLines())
			{
				sb.AppendLine(line);
			}
			sb.AppendLine(body ?? string.Empty);

			File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(false));
		}


		private static string ShortId(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return "unknown";
			}

			var sb = new StringBuilder();
			foreach (var ch in id)
			{
				if (char.IsLetterOrDigit(ch))
				{
					sb.Append(ch);
				}
			}

			var cleaned = sb.ToString();
			if (cleaned.Length == 0)
			{
				return "unknown";
			}

			if (cleaned.Length > 12)
			{
				cleaned = cleaned.Substring(cleaned.Length - 12);
			}

			return cleaned;
		}


		private static string SanitizePathSegment(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return "Unknown";
			}

			var trimmed = value.Trim();
			if (trimmed.Length == 0)
			{
				return "Unknown";
			}

			var sb = new StringBuilder(trimmed.Length);
			foreach (var ch in trimmed)
			{
				if (char.IsLetterOrDigit(ch) || ch == ' ')
				{
					sb.Append(ch);
				}
				else
				{
					sb.Append('_');
				}
			}

			var cleaned = sb.ToString().Trim().TrimEnd('.');
			if (string.IsNullOrEmpty(cleaned))
			{
				cleaned = "Unknown";
			}

			if (cleaned.Length > 80)
			{
				cleaned = cleaned.Substring(0, 80);
			}

			return cleaned;
		}
	}


	internal static class CacheManager
	{
		public static void ClearCache(string cacheRoot)
		{
			if (string.IsNullOrWhiteSpace(cacheRoot))
			{
				return;
			}

			if (Directory.Exists(cacheRoot))
			{
				Directory.Delete(cacheRoot, true);
			}

			Directory.CreateDirectory(cacheRoot);
		}
	}


	internal static class OneNoteTextExtractor
	{
		private static readonly Regex BreakRegex = new Regex(@"<\s*br\s*/?>", RegexOptions.IgnoreCase);
		private static readonly Regex ParagraphEndRegex = new Regex(@"</\s*p\s*>", RegexOptions.IgnoreCase);
		private static readonly Regex TagRegex = new Regex(@"<[^>]+>", RegexOptions.IgnoreCase);


		public static string ExtractPlainText(string pageXml)
		{
			if (string.IsNullOrWhiteSpace(pageXml))
			{
				return string.Empty;
			}

			var doc = XDocument.Parse(pageXml);
			var root = doc.Root;
			var ns = root?.Name.Namespace ?? XNamespace.None;
			var chunks = new List<string>();

			foreach (var textNode in doc.Descendants(ns + "T"))
			{
				var raw = textNode.Value;
				if (string.IsNullOrWhiteSpace(raw))
				{
					continue;
				}

				var plain = HtmlToText(raw);
				if (!string.IsNullOrWhiteSpace(plain))
				{
					chunks.Add(plain.Trim());
				}
			}

			if (chunks.Count == 0)
			{
				return string.Empty;
			}

			return string.Join("\r\n", chunks);
		}


		private static string HtmlToText(string html)
		{
			var text = html;
			text = BreakRegex.Replace(text, "\n");
			text = ParagraphEndRegex.Replace(text, "\n");
			text = TagRegex.Replace(text, string.Empty);
			text = System.Net.WebUtility.HtmlDecode(text);
			text = NormalizeWhitespace(text);
			return text;
		}


		private static string NormalizeWhitespace(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			text = text.Replace("\r\n", "\n").Replace("\r", "\n");
			var sb = new StringBuilder(text.Length);
			var lastWasNewline = false;

			foreach (var ch in text)
			{
				if (ch == '\n')
				{
					if (!lastWasNewline)
					{
						sb.Append("\r\n");
						lastWasNewline = true;
					}
					continue;
				}

				if (char.IsControl(ch))
				{
					continue;
				}

				sb.Append(ch);
				lastWasNewline = false;
			}

			return sb.ToString().Trim();
		}
	}


	internal sealed class OneNoteExporter
	{
		private readonly ILogger logger;

		public OneNoteExporter(ILogger logger)
	{
		this.logger = logger;
	}


		public ExportSummary ExportAll(
			string cacheRoot,
			IProgress<SyncProgress> progress = null,
			System.Threading.CancellationToken cancellationToken = default,
			System.Threading.ManualResetEventSlim pauseEvent = null)
	{
		if (string.IsNullOrWhiteSpace(cacheRoot))
		{
			throw new InvalidOperationException("Cache root is empty.");
		}

		var summary = new ExportSummary();
		var reader = new OneNoteHierarchyReader();
		var pages = reader.GetAllPages();
		var total = reader.LastPageCount;

		Directory.CreateDirectory(cacheRoot);

		using var one = new OneNote();
		var index = 0;
		var sectionManifests = new Dictionary<string, SectionManifest>(StringComparer.OrdinalIgnoreCase);
		var notebookManifest = new NotebookManifest(cacheRoot);
		var sectionCurrentIds = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		var currentSectionKey = string.Empty;

		foreach (var page in pages)
		{
			cancellationToken.ThrowIfCancellationRequested();
			pauseEvent?.Wait(cancellationToken);

			// Skip entire notebook if unchanged
			if (notebookManifest.IsNotebookCurrent(page.NotebookId, page.NotebookLastModified))
			{
				summary.Skipped++;
				Report(progress, summary, page, total, index);
				index++;
				continue;
			}

			var manifest = GetManifest(cacheRoot, page, sectionManifests);
			if (notebookManifest.IsSectionCurrent(page.NotebookId, page.SectionId, page.SectionLastModified))
			{
				summary.Skipped++;
				Report(progress, summary, page, total, index);
				index++;
				continue;
			}

			if (!string.Equals(currentSectionKey, manifest.SectionKey, StringComparison.OrdinalIgnoreCase))
			{
				FlushSectionManifest(currentSectionKey, sectionManifests, sectionCurrentIds);
				currentSectionKey = manifest.SectionKey;
			}

			try
			{
				var fileId = MarkdownStore.GetFileId(page.PageId);

				if (manifest.IsCurrent(fileId, page.LastModifiedRaw))
				{
					summary.Skipped++;
					Report(progress, summary, page, total, index);
					Track(sectionCurrentIds, fileId, page);
					index++;
					continue;
				}

				var filePath = MarkdownStore.GetPagePath(cacheRoot, page);
				if (MarkdownStore.IsUpToDate(filePath, page))
				{
					manifest.Touch(fileId, page.LastModifiedRaw);
					summary.Skipped++;
					Report(progress, summary, page, total, index);
					Track(sectionCurrentIds, fileId, page);
					index++;
					continue;
				}

				var pageXml = one.GetPage(page.PageId, OneNote.PageDetail.Basic)
					.GetAwaiter()
					.GetResult();

				if (pageXml == null)
				{
					summary.Failed++;
					index++;
					continue;
				}

				var body = OneNoteTextExtractor.ExtractPlainText(
					pageXml.Root.ToString(SaveOptions.DisableFormatting));

				var metadata = new MarkdownMetadata
				{
					PageId = page.PageId,
					PageTitle = page.PageTitle,
					PageLastModified = page.LastModifiedRaw,
					SectionId = page.SectionId,
					SectionName = page.SectionName,
					NotebookId = page.NotebookId,
					NotebookName = page.NotebookName
				};

				MarkdownStore.WritePage(filePath, metadata, body);
				manifest.Touch(fileId, page.LastModifiedRaw);
				summary.Exported++;
				Track(sectionCurrentIds, fileId, page);
			}
			catch (Exception exc)
			{
				logger?.WriteLine($"OneSearch export failed for {page.PageId}", exc);
				summary.Failed++;
			}

			Report(progress, summary, page, total, index);
			index++;
		}

		FlushSectionManifest(currentSectionKey, sectionManifests, sectionCurrentIds);

		try
		{
			notebookManifest.Save();
		}
		catch (Exception exc)
		{
			logger?.WriteLine("OneSearch notebook manifest save failed", exc);
		}

		return summary;
	}


	private void FlushSectionManifest(
		string sectionKey,
		Dictionary<string, SectionManifest> manifests,
		Dictionary<string, HashSet<string>> sectionCurrentIds)
	{
		if (string.IsNullOrWhiteSpace(sectionKey))
		{
			return;
		}

		if (!manifests.TryGetValue(sectionKey, out var manifest))
		{
			return;
		}

		try
		{
			if (sectionCurrentIds.TryGetValue(sectionKey, out var ids))
			{
				manifest.DeleteStaleFiles(ids);
			}

			manifest.Save();
		}
		catch (Exception exc)
		{
			logger?.WriteLine("OneSearch manifest save failed", exc);
		}
	}


	private static void Report(
		IProgress<SyncProgress> progress,
		ExportSummary summary,
		OneNotePageInfo page,
		int total,
		int index)
	{
		progress?.Report(new SyncProgress
		{
			Processed = index + 1,
			Total = total > 0 ? total : index + 1,
			Exported = summary.Exported,
			Skipped = summary.Skipped,
			Failed = summary.Failed,
			CurrentTitle = page.PageTitle,
			CurrentNotebook = page.NotebookName
		});
	}


	private static SectionManifest GetManifest(
		string cacheRoot, OneNotePageInfo page,
		Dictionary<string, SectionManifest> manifests)
	{
		var key = page.SectionId ?? page.SectionName ?? "Section";
		if (manifests.TryGetValue(key, out var manifest))
		{
			return manifest;
		}

		var sectionFolder = Path.GetDirectoryName(MarkdownStore.GetPagePath(cacheRoot, page));
		manifest = new SectionManifest(sectionFolder, key);
		manifests[key] = manifest;
		return manifest;
	}


	private static void Track(
		Dictionary<string, HashSet<string>> sectionCurrentIds,
		string fileId,
		OneNotePageInfo page)
	{
		var key = page.SectionId ?? page.SectionName ?? "Section";
		if (!sectionCurrentIds.TryGetValue(key, out var set))
		{
			set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			sectionCurrentIds[key] = set;
		}

		if (!string.IsNullOrWhiteSpace(fileId))
		{
			set.Add(fileId);
		}
	}
}


internal sealed class SectionManifest
{
	private const string ManifestFileName = "manifest.json";

	private readonly string manifestPath;
	private readonly Dictionary<string, string> map;
	public string SectionKey { get; }


	public SectionManifest(string sectionFolder, string key)
	{
		SectionKey = key;
		Directory.CreateDirectory(sectionFolder);
		manifestPath = Path.Combine(sectionFolder, ManifestFileName);
		map = Load(manifestPath);
	}


	public bool IsCurrent(string fileId, string lastModifiedRaw)
	{
		if (string.IsNullOrWhiteSpace(fileId) || string.IsNullOrWhiteSpace(lastModifiedRaw))
		{
			return false;
		}

		return map.TryGetValue(fileId, out var stamp) &&
			string.Equals(stamp, lastModifiedRaw, StringComparison.OrdinalIgnoreCase);
	}


	public void Touch(string fileId, string lastModifiedRaw)
	{
		if (!string.IsNullOrWhiteSpace(fileId))
		{
			map[fileId] = lastModifiedRaw ?? string.Empty;
		}
	}


	public void DeleteStaleFiles(ICollection<string> currentPageIds)
	{
		if (currentPageIds == null || currentPageIds.Count == 0)
		{
			return;
		}

		var folder = Path.GetDirectoryName(manifestPath);
		if (folder == null || !Directory.Exists(folder))
		{
			return;
		}

		var files = Directory.GetFiles(folder, "*.md");
		foreach (var file in files)
		{
			var name = Path.GetFileNameWithoutExtension(file);
			var stillExists = currentPageIds.Contains(name) || map.ContainsKey(name);

			if (!stillExists)
			{
				try { File.Delete(file); } catch { /* ignore */ }
			}
		}

		// remove manifest entries that no longer exist
		var obsolete = map.Keys.Where(k => !currentPageIds.Contains(k)).ToList();
		foreach (var key in obsolete)
		{
			map.Remove(key);
		}
	}


		public void Save()
		{
			var json = new JavaScriptSerializer().Serialize(map);
			File.WriteAllText(manifestPath, json, Encoding.UTF8);
		}


	private static Dictionary<string, string> Load(string path)
	{
		try
			{
				if (File.Exists(path))
				{
					var json = File.ReadAllText(path, Encoding.UTF8);
					var loaded = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(json);
					if (loaded != null) return loaded;
				}
			}
			catch
		{
			// ignore malformed manifest, start fresh
		}

		return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	}
}
}
