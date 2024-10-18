//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Export one or more selected pages, optionally with attachments, to a folder
	/// </summary>
	internal class ExportCommand : Command
	{
		private OneNote one;
		private int quickCount = 0;


		public ExportCommand()
		{
		}


		public override async Task Execute(params object[] args)
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

					if (page.Title == null)
					{
						page.SetTitle(quickCount == 0
							? Resx.phrase_QuickNote
							: $"{Resx.phrase_QuickNote} ({quickCount})");

						quickCount++;
					}

					var title = page.Title.Trim();

					if (title.Length == 0)
					{
						var pageinfo = await one.GetPageInfo(pageID);
						var sectinfo = await one.GetSectionInfo(pageinfo.SectionId);
						title = $"{PathHelper.CleanFileName(sectinfo.Name)} Untitled Page";
					}

					if (useUnderscores)
					{
						title = title.Replace(' ', '_');
					}

					// cleaned, sized, and ready go!
					var filename = PathHelper.GetUniqueQualifiedFileName(path, ref title, ext);
					if (filename is not null)
					{
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

			SaveDefaultPath(path);

			ShowMessage(string.Format(Resx.SaveAsMany_Success, savedCount, pageIDs.Count, path));
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
