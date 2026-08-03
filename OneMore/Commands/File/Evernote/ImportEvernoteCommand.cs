//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Imports one or more Evernote .enex export files, converting each note to Markdown
	/// and reusing OneMore's Markdown-to-page pipeline (OneMoreDig / MarkdownConverter)
	/// to create the resulting OneNote pages, one new section per source notebook.
	/// </summary>
	/// <seealso cref="https://evernote.com/blog/how-evernotes-xml-export-format-works"/>
	/// <seealso cref="https://dev.evernote.com/doc/articles/enml.php"/>
	internal class ImportEvernoteCommand : Command
	{
		private static readonly Dictionary<string, string> MimeExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			["image/png"] = ".png",
			["image/jpeg"] = ".jpg",
			["image/gif"] = ".gif",
			["image/bmp"] = ".bmp",
			["image/tiff"] = ".tiff",
			["image/webp"] = ".webp",
			["application/pdf"] = ".pdf",
			["audio/mpeg"] = ".mp3",
			["audio/wav"] = ".wav",
			["video/mp4"] = ".mp4",
			["application/msword"] = ".doc",
			["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = ".docx",
			["application/zip"] = ".zip"
		};


		public override async Task Execute(params object[] args)
		{
			string pathSpec;
			bool includeSubfolders;
			bool abortOnEncrypted;

			using (var dialog = new ImportEvernoteDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				pathSpec = dialog.FilePath;
				includeSubfolders = dialog.IncludeSubfolders;
				abortOnEncrypted = dialog.AbortOnEncrypted;
			}

			logger.StartClock();

			var files = ResolveFiles(pathSpec, includeSubfolders);

			foreach (var file in files)
			{
				await ImportEnexFile(file, abortOnEncrypted);
			}

			logger.WriteTime("evernote import complete");
		}


		/// <summary>
		/// Expands a pipe-delimited path specification (as produced by ImportEvernoteDialog)
		/// into a flat, de-duplicated list of .enex file paths. Each entry may be an explicit
		/// file, a wildcard pattern, or a folder (scanned for *.enex, optionally recursively).
		/// </summary>
		private static string[] ResolveFiles(string pathSpec, bool includeSubfolders)
		{
			var results = new List<string>();

			foreach (var entry in pathSpec.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var trimmed = entry.Trim();
				if (trimmed.Length == 0)
				{
					continue;
				}

				if (Directory.Exists(trimmed))
				{
					results.AddRange(Directory.GetFiles(trimmed, "*.enex",
						includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
				}
				else if (PathHelper.HasWildFileName(trimmed))
				{
					results.AddRange(Directory.GetFiles(
						Path.GetDirectoryName(trimmed), Path.GetFileName(trimmed)));
				}
				else
				{
					results.Add(trimmed);
				}
			}

			return results
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}


		private async Task ImportEnexFile(string filepath, bool abortOnEncrypted)
		{
			logger.WriteLine($"importing evernote export {filepath}");

			List<EvernoteNote> notes;
			try
			{
				notes = EnexReader.ReadNotes(filepath).ToList();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading {filepath}", exc);
				ShowError(string.Format(Resx.ImportEvernoteCommand_ReadError, filepath));
				return;
			}

			if (notes.Count == 0)
			{
				logger.WriteLine($"no notes found in {filepath}");
				return;
			}

			var notebookName = Path.GetFileNameWithoutExtension(filepath);
			var attachmentDir = Path.Combine(
				Path.GetDirectoryName(filepath) ?? string.Empty, $"{notebookName}_attachments");

			var tempRoot = Path.Combine(
				Path.GetTempPath(), "OneMoreEvernoteImport", Guid.NewGuid().ToString("N"));

			var created = 0;
			var skipped = 0;
			var encrypted = new List<string>();
			var unattached = new List<string>();
			var errors = new List<string>();

			var progress = new ProgressDialog(async (self, token) =>
			{
				await using var one = new OneNote();

				var sectionId = await FindOrCreateSection(one, notebookName);
				var existingGuids = await LoadExistingGuids(one, sectionId);

				self.SetMaximum(notes.Count);

				foreach (var note in notes)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					self.SetMessage(string.Format(Resx.ImportEvernoteCommand_Importing, note.Title));
					self.Increment();

					var identity = ComputeIdentity(note);
					if (existingGuids.Contains(identity))
					{
						logger.WriteLine($"skipping duplicate note {note.Title}");
						skipped++;
						continue;
					}

					var noteTempDir = Path.Combine(tempRoot, Guid.NewGuid().ToString("N"));
					Directory.CreateDirectory(noteTempDir);

					try
					{
						var converter = new EnmlConverter(
							note,
							resource => WriteImageResource(noteTempDir, resource),
							resource => WriteAttachmentResource(attachmentDir, note, resource));

						var markdown = converter.Convert();

						if (converter.EncounteredEncryption)
						{
							encrypted.Add(note.Title);

							if (abortOnEncrypted)
							{
								logger.WriteLine(
									$"skipping note {note.Title} due to encrypted content");
								continue;
							}
						}

						unattached.AddRange(converter.UnattachedFiles);

						var mdPath = Path.Combine(noteTempDir, "note.md");
						File.WriteAllText(mdPath, markdown);

						await ImportNote(one, sectionId, note, identity, mdPath);
						created++;
					}
					catch (Exception exc)
					{
						logger.WriteLine($"error importing note {note.Title}", exc);
						errors.Add(note.Title);
					}
					finally
					{
						try
						{
							Directory.Delete(noteTempDir, true);
						}
						catch (IOException)
						{
							// best-effort cleanup; leave for manual removal if locked
						}
					}
				}
			});

			progress.RunModeless((sender, e) =>
			{
				logger.WriteTime(
					$"evernote: imported {created} of {notes.Count} note(s) from {notebookName}, " +
					$"{skipped} skipped as duplicates, {encrypted.Count} with encrypted content, " +
					$"{errors.Count} errors");

				if (unattached.Count > 0)
				{
					logger.WriteLine(
						$"evernote: {unattached.Count} attachment(s) linked but not embedded:");
					unattached.ForEach(f => logger.WriteLine($"  {f}"));
				}
			});
		}


		private async Task<string> FindOrCreateSection(OneNote one, string name)
		{
			var notebook = await one.GetNotebook();
			var ns = one.GetNamespace(notebook);

			// exclude the notebook's recycle bin (and anything inside it) so a deleted
			// section isn't mistaken for a live one and reused, which would resurrect
			// its old pages for duplicate-detection purposes
			var existing = notebook.Descendants(ns + "Section")
				.Where(e => e.Attribute("isRecycleBin") is null && e.Attribute("isInRecycleBin") is null)
				.FirstOrDefault(e => string.Equals(
					e.Attribute("name")?.Value, name, StringComparison.OrdinalIgnoreCase));

			if (existing != null)
			{
				return existing.Attribute("ID").Value;
			}

			var section = await one.CreateSection(name);
			return section.Attribute("ID").Value;
		}


		private async Task<HashSet<string>> LoadExistingGuids(OneNote one, string sectionId)
		{
			var guids = new HashSet<string>();

			var section = await one.GetSection(sectionId);
			if (section == null)
			{
				return guids;
			}

			var ns = one.GetNamespace(section);

			// defensive: a page recently moved to the recycle bin shouldn't normally
			// still be listed under its original section, but exclude it explicitly
			// to match this codebase's convention everywhere else recycle bin content
			// could otherwise leak into a query
			foreach (var pageElement in section.Elements(ns + "Page")
				.Where(e => e.Attribute("isInRecycleBin") is null))
			{
				var pageId = pageElement.Attribute("ID")?.Value;
				if (string.IsNullOrEmpty(pageId))
				{
					continue;
				}

				var existingPage = await one.GetPage(pageId, OneNote.PageDetail.Basic);
				var guid = existingPage?.GetMetaContent(MetaNames.EvernoteGuid);
				if (!string.IsNullOrEmpty(guid))
				{
					guids.Add(guid);
				}
			}

			return guids;
		}


		private async Task ImportNote(
			OneNote one, string sectionId, EvernoteNote note, string identity, string mdPath)
		{
			var text = File.ReadAllText(mdPath);
			var body = OneMoreDig.ConvertMarkdownToHtml(mdPath, text, preserveBlankLines: true);

			if (string.IsNullOrEmpty(body))
			{
				return;
			}

			one.CreatePage(sectionId, out var pageId);
			var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
			var ns = page.Namespace;

			page.Title = string.IsNullOrWhiteSpace(note.Title) ? "Untitled" : note.Title;

			var container = page.EnsureContentContainer();
			container.Add(new XElement(ns + "HTMLBlock",
				new XElement(ns + "Data",
					new XCData($"<html><body>{body}</body></html>")
					)
				));

			var converter = new MarkdownConverter(page);
			converter.RewriteHeadings();

			await one.Update(page);

			page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
			converter = new MarkdownConverter(page);
			converter.RewriteHeadings();
			converter.RewriteBlankLines();
			converter.RewriteTodo();
			converter.RewriteCode();
			converter.RewriteInlineCode();

			page.SetMeta(MetaNames.EvernoteGuid, identity);

			await one.Update(page);

			if (note.Created.HasValue || note.Updated.HasValue)
			{
				await ApplyTimestamps(one, sectionId, pageId, note.Created, note.Updated);
			}

			await one.NavigateTo(pageId);
		}


		private static async Task ApplyTimestamps(
			OneNote one, string sectionId, string pageId, DateTime? created, DateTime? updated)
		{
			var section = await one.GetSection(sectionId);
			var ns = one.GetNamespace(section);

			var pageElement = section.Descendants(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID")?.Value == pageId);

			if (pageElement is null)
			{
				return;
			}

			if (created.HasValue)
			{
				pageElement.SetAttributeValue("dateTime", FormatTimestamp(created.Value));
			}

			if (updated.HasValue)
			{
				pageElement.SetAttributeValue("lastModifiedTime", FormatTimestamp(updated.Value));
			}

			one.UpdateHierarchy(section);
		}


		private static string FormatTimestamp(DateTime value) =>
			value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);


		private static string ComputeIdentity(EvernoteNote note)
		{
			// ENEX carries no service-assigned note GUID, so a stable substitute identity
			// is derived from the title and creation timestamp (both fixed at export time)
			var key = $"{note.Title}|{note.Created:O}";
			using var sha = SHA256.Create();
			var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
			return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
		}


		private static string WriteImageResource(string tempDir, EvernoteResource resource)
		{
			var name = string.IsNullOrEmpty(resource.FileName)
				? $"{resource.Hash}{GuessExtension(resource.Mime)}"
				: resource.FileName;

			var path = Path.Combine(tempDir, MakeSafeFileName(name));
			File.WriteAllBytes(path, resource.Data);
			return path;
		}


		private static string WriteAttachmentResource(
			string attachmentDir, EvernoteNote note, EvernoteResource resource)
		{
			Directory.CreateDirectory(attachmentDir);

			var baseName = string.IsNullOrEmpty(resource.FileName)
				? $"{resource.Hash}{GuessExtension(resource.Mime)}"
				: resource.FileName;

			// prefix with the source note's title and a resource-hash fragment so
			// attachments from different notes never collide in the shared folder
			var prefix = MakeSafeFileName(string.IsNullOrWhiteSpace(note.Title) ? "note" : note.Title);
			var hashFragment = resource.Hash?.Substring(0, Math.Min(8, resource.Hash.Length)) ?? "0";
			var name = $"{prefix}_{hashFragment}_{MakeSafeFileName(baseName)}";

			var path = Path.Combine(attachmentDir, name);
			if (!File.Exists(path))
			{
				File.WriteAllBytes(path, resource.Data);
			}

			return path;
		}


		private static string GuessExtension(string mime)
		{
			if (!string.IsNullOrEmpty(mime) && MimeExtensions.TryGetValue(mime, out var ext))
			{
				return ext;
			}

			return ".bin";
		}


		private static string MakeSafeFileName(string name)
		{
			var invalid = Path.GetInvalidFileNameChars();
			var builder = new StringBuilder(name.Length);

			foreach (var c in name)
			{
				builder.Append(invalid.Contains(c) ? '_' : c);
			}

			return builder.ToString();
		}
	}
}
