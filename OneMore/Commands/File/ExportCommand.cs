//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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

			using (var dialog = new ExportDialog(pageIDs.Count))
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				path = dialog.FolderPath;
				format = dialog.Format;
			}

			string ext = null;
			switch (format)
			{
				case OneNote.ExportFormat.HTML: ext = ".htm"; break;
				case OneNote.ExportFormat.PDF: ext = ".pdf"; break;
				case OneNote.ExportFormat.Word: ext = ".docx"; break;
				case OneNote.ExportFormat.XML: ext = ".xml"; break;
				case OneNote.ExportFormat.OneNote: ext = ".one"; break;
			}

			bool archive = false;
			if (format == OneNote.ExportFormat.HTML)
			{
				var result = PromptToArchive();
				if (result == DialogResult.Cancel)
				{
					return;
				}

				archive = result == DialogResult.Yes;
			}

			string formatName = format.ToString();

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(pageIDs.Count);
				progress.Show(owner);

				var archivist = new Archivist(one);

				foreach (var pageID in pageIDs)
				{
					var page = one.GetPage(pageID, OneNote.PageDetail.BinaryData);
					var filename = Path.Combine(path, page.Title.Replace(' ', '_') + ext);

					progress.SetMessage(filename);
					progress.Increment();

					if (format == OneNote.ExportFormat.HTML)
					{
						archivist.SaveAsHTML(page, ref filename, archive);
					}
					else if (format == OneNote.ExportFormat.XML)
					{
						archivist.SaveAsXML(page.Root, filename);
					}
					else
					{
						archivist.SaveAs(page.PageId, filename, format, formatName);
					}
				}
			}

			UIHelper.ShowMessage(string.Format(Resx.SaveAsMany_Success, pageIDs.Count, path));
		}


		private DialogResult PromptToArchive()
		{
			return MessageBox.Show(
				Resx.ExportCommand_ArchivePrompt,
				Resx.ExportCommand_ArchiveHTML,
				MessageBoxButtons.YesNoCancel,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1,
				MessageBoxOptions.DefaultDesktopOnly);
		}
	}
}
