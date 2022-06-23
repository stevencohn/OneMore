//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Export one or more selected pages, optionally with attachments, to a folder
	/// </summary>
	internal class ExportCommand : Command
	{
		private OneNote one;


		public ExportCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote())
			{
				var section = one.GetSection();
				var ns = one.GetNamespace(section);

				var pageIDs = section.Elements(ns + "Page")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Attribute("ID").Value)
					.ToList();

				if (pageIDs.Count == 0)
				{
					pageIDs.Add(one.CurrentPageId);
					Export(pageIDs);
				}
				else
				{
					Export(pageIDs);
				}
			}

			await Task.Yield();
		}


		private void Export(List<string> pageIDs)
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

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(pageIDs.Count);
				progress.Show(owner);

				var archivist = new Archivist(one);

				foreach (var pageID in pageIDs)
				{
					var page = one.GetPage(pageID, OneNote.PageDetail.BinaryData);

					var title = useUnderscores
						? PathFactory.CleanFileName(page.Title).Replace(' ', '_')
						: page.Title;

					var filename = Path.Combine(path, title + ext);

					progress.SetMessage(filename);
					progress.Increment();

					if (format == OneNote.ExportFormat.HTML)
					{
						if (withAttachments)
						{
							archivist.ExportHTML(page, ref filename);
						}
						else
						{
							archivist.Export(page.PageId, filename, OneNote.ExportFormat.HTML);
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
						archivist.Export(page.PageId, filename, format, withAttachments, embedded);
					}
				}
			}

			SaveDefaultPath(path);

			UIHelper.ShowMessage(string.Format(Resx.SaveAsMany_Success, pageIDs.Count, path));
		}


		private void SaveDefaultPath(string path)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("Export");
			settings.Add("path", path);
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
