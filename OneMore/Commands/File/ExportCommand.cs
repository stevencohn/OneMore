//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S2696

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Export one or more selected pages, optionally with attachments, to a folder
	/// </summary>
	internal class ExportCommand : Command, ICliPageCommand
	{
		private static bool commandIsActive = false;

		private OneNote one;
		private int quickCount = 0;
		private string pageId;


		public ExportCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "Export";

		public string Description => "Export pages to a folder in the specified format";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook to process", required: true)
			.AddString("section", "Path of section to process (omit for all sections)", required: false)
			.AddString("page", "Name of page to process (omit or * for all pages in section)", required: false)
			.AddPath("outpath", "Output folder path", required: true)
			.AddEnum("format", "Export format",
				new[] { "HTML", "PDF", "Word", "XML", "Markdown", "OneNote" },
				required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			if (runningFromCli)
			{
				// CommandService passes its own long-lived OneNote connection here when
				// batching this command across many pages (notebook- or section-level
				// export); reuse it instead of opening a fresh COM connection per page.
				// We don't own that instance's lifetime, so never dispose it. If none was
				// supplied, fall back to creating and disposing our own as before.
				var suppliedOne = args.Length > 1 ? args[1] as OneNote : null;
				var ownsOne = suppliedOne == null;
				var cliOne = suppliedOne ?? new OneNote();

				try
				{
					var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
					//var notebook  = cliParams?.Get<string>("notebook");
					cliParams.TryGet<string>("section", out var section);    // optional
					var outpath = cliParams?.Get<string>("outpath");
					var formatStr = cliParams?.Get<string>("format");

					if (!Enum.TryParse<OneNote.ExportFormat>(
						formatStr, ignoreCase: true, out var format))
					{
						logger.WriteLine($"unknown export format: {formatStr}");
						return;
					}

					var ext = GetExtForFormat(format);
					if (ext == null) return;

					// pageId is always injected by the CLI framework (once per resolved page)
					cliParams.TryGet("pageId", out pageId);

					if (string.IsNullOrWhiteSpace(section))
					{
						// Notebook-level: framework iterates all pages; rebuild each page's full
						// section-group ancestry so same-named sections in different groups
						// don't collide into a single folder
						var pageInfo = await cliOne.GetPageInfo(pageId);
						var sectInfo = await cliOne.GetSectionInfo(pageInfo.SectionId);
						var sectionFolder = PathHelper.BuildSectionFolderPath(
							outpath, sectInfo.SectionGroups, sectInfo.Name);

						Directory.CreateDirectory(sectionFolder);
						await ExportOneCli(pageId, cliOne, sectionFolder, format, ext);
					}
					else
					{
						await ExportOneCli(pageId, cliOne, outpath, format, ext);
					}

					return;
				}
				finally
				{
					if (ownsOne)
					{
						await cliOne.DisposeAsync();
					}

					commandIsActive = false;
				}
			}

			try
			{
				await using (one = new OneNote())
				{
					var section = await one.GetSection();
					var ns = one.GetNamespace(section);

					var pageIDs = section.Elements(ns + "Page")
						.Where(e => e.Attribute("selected")?.Value == "all")
						.Select(e => e.Attribute("ID").Value)
						.ToList();

					if (pageIDs.Count == 0)
					{
						pageIDs.Add(one.CurrentPageId);
						await Export(pageIDs);
					}
					else
					{
						await Export(pageIDs);
					}
				}

				await Task.Yield();
			}
			finally
			{
				commandIsActive = false;
			}
		}


		private async Task Export(List<string> pageIDs)
		{
			OneNote.ExportFormat format;
			string path;
			bool withAttachments;
			bool embedded;
			bool useUnderscores;

			// dialog...

			using (var dialog = new ExportDialog(pageIDs.Count))
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				path = dialog.FolderPath;
				format = dialog.Format;
				withAttachments = dialog.WithAttachments;
				embedded = dialog.Embedded;
				useUnderscores = dialog.UseUnderscores;
			}

			// prepare...

			string ext = null;
			switch (format)
			{
				case OneNote.ExportFormat.HTML: ext = ".htm"; break;
				case OneNote.ExportFormat.PDF: ext = ".pdf"; break;
				case OneNote.ExportFormat.Word: ext = ".docx"; break;
				case OneNote.ExportFormat.XML: ext = ".xml"; break;
				case OneNote.ExportFormat.Markdown: ext = ".md"; break;
				case OneNote.ExportFormat.OneNote: ext = ".one"; break;
			}

			// export...

			var savedCount = 0;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(pageIDs.Count);
				progress.Show();

				var archivist = new Archivist(one);

				foreach (var pageID in pageIDs)
				{
					var page = await one.GetPage(pageID, OneNote.PageDetail.BinaryData);
					if (page == null)
					{
						logger.WriteLine($"export skipped, page not found [{pageID}]");
						continue;
					}

					if (page.Title == null)
					{
						page.SetTitle(quickCount == 0
							? Resx.phrase_QuickNote
							: $"{Resx.phrase_QuickNote} ({quickCount})");

						quickCount++;
					}

					var title = await GetBestTitle(one, page, path, ext, useUnderscores);
					if (!string.IsNullOrWhiteSpace(title))
					{
						// cleaned, sized, and ready go; now make sure it's unique!
						var filename = PathHelper.GetUniqueQualifiedFileName(path, title, ext);

						progress.SetMessage(filename);
						progress.Increment();

						if (format == OneNote.ExportFormat.HTML)
						{
							if (withAttachments)
							{
								await archivist.ExportHTML(page, filename);
							}
							else
							{
								await archivist.Export(
									page.PageId, filename, OneNote.ExportFormat.HTML);
							}
						}
						else if (format == OneNote.ExportFormat.XML)
						{
							archivist.ExportXML(page.Root, filename, withAttachments);
						}
						else if (format == OneNote.ExportFormat.Markdown)
						{
							archivist.ExportMarkdown(page, filename, withAttachments);
						}
						else
						{
							await archivist.Export(
								page.PageId, filename, format, withAttachments, embedded);
						}

						savedCount++;
					}
					else
					{
						logger.WriteLine($"export path too long [{path}\\{title}{ext}]");
					}
				}
			}

			if (savedCount > 0)
			{
				SaveDefaultPath(path);
				ShowMessage(string.Format(Resx.SaveAsMany_Success, savedCount, pageIDs.Count, path));
			}
		}


		private static async Task<string> GetBestTitle(
			OneNote one, Models.Page page, string path, string ext, bool useUnderscores)
		{
			const string Untitled = " Untitled Page";
			const int MaxTitleName = 40;

			// (-2) to account for path separators, one for path\ and one for section\
			var maxLength = PathHelper.MAX_PATH - path.Length - ext.Length - 2;

			// first chance, use the page title
			var title = page.Title?.Trim() ?? string.Empty;

			// second chance, use page content, like OneNote does when title is empty
			if (string.IsNullOrWhiteSpace(title))
			{
				title = page.BodyOutlines
					.Descendants(page.Namespace + "OE")
					.FirstOrDefault(e =>
						e.Elements(page.Namespace + "T").Any(t =>
							t.GetCData()?.GetWrapper().Value.Trim().Length > 0))?
					.TextValue(true)
					.Trim();
			}

			if (!string.IsNullOrWhiteSpace(title) && title.Length > MaxTitleName)
			{
				title = title.Substring(0, MaxTitleName).Trim();
			}

			// third chance, default to "<section> Untitled Page"
			if (string.IsNullOrWhiteSpace(title))
			{
				var pageInfo = await one.GetPageInfo(page.PageId);
				var sectInfo = await one.GetSectionInfo(pageInfo.SectionId);
				var secName = PathHelper.CleanFileName(sectInfo.Name);
				if (path.Length + 1 + secName.Length + Untitled.Length + ext.Length > maxLength)
				{
					// shrink section name rather than Untitled part
					var secLength = Math.Max(0, Math.Min(maxLength - Untitled.Length, secName.Length));
					secName = secName.Substring(0, secLength).Trim();
				}

				title = $"{secName}{Untitled}";
			}
			else if (path.Length + 1 + title.Length + ext.Length > maxLength)
			{
				// shrink title and keep room for (counter) if needed
				var titLength = Math.Max(0, Math.Min(maxLength, title.Length));
				title = title.Substring(0, titLength).Trim();
			}

			title = PathHelper.CleanFileName(title);

			if (useUnderscores)
			{
				title = title.Replace(' ', '_');
			}

			return title;
		}


		/// <summary>
		/// Returns the file extension for the given export format, or null (and logs an error)
		/// if the format is not supported in the CLI path.
		/// </summary>
		private string GetExtForFormat(OneNote.ExportFormat format)
		{
			switch (format)
			{
				case OneNote.ExportFormat.HTML:     return ".htm";
				case OneNote.ExportFormat.PDF:      return ".pdf";
				case OneNote.ExportFormat.Word:     return ".docx";
				case OneNote.ExportFormat.XML:      return ".xml";
				case OneNote.ExportFormat.Markdown: return ".md";
				case OneNote.ExportFormat.OneNote:  return ".one";
				default:
					logger.WriteLine($"unsupported CLI export format: {format}");
					return null;
			}
		}


		/// <summary>
		/// Exports a single page identified by <paramref name="pageId"/> to
		/// <paramref name="path"/> using the given <paramref name="one"/> connection.
		/// </summary>
		private async Task ExportOneCli(
			string pageId, OneNote one, string path, OneNote.ExportFormat format, string ext)
		{
			var page = await one.GetPage(pageId, OneNote.PageDetail.BinaryData);
			if (page == null)
			{
				logger.WriteLine($"export skipped, page not found [{pageId}]");
				return;
			}

			var title = await GetBestTitle(one, page, path, ext, true);
			// cleaned, sized, and ready go; now make sure it's unique!
			var filename = PathHelper.GetUniqueQualifiedFileName(path, title, ext);

			var archivist = new Archivist(one);

			if (format == OneNote.ExportFormat.HTML)
			{
				await archivist.Export(pageId, filename, OneNote.ExportFormat.HTML);
			}
			else if (format == OneNote.ExportFormat.XML)
			{
				archivist.ExportXML(page.Root, filename, withAttachments: false);
			}
			else if (format == OneNote.ExportFormat.Markdown)
			{
				archivist.ExportMarkdown(page, filename, withAttachments: false);
			}
			else
			{
				await archivist.Export(pageId, filename, format);
			}

			logger.WriteLine($"exported '{title}' → {filename}");
		}


		private static void SaveDefaultPath(string path)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("Export");
			settings.Add("path", path);
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
