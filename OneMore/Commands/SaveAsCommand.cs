//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class SaveAsCommand : Command
	{

		public enum ExportFormat
		{
			HTML = PublishFormat.pfHTML,
			PDF = PublishFormat.pfPDF,
			Word = PublishFormat.pfWord,
			XML = 100
		}


		public SaveAsCommand()
		{
		}


		public void Execute()
		{
			try
			{
				using (var manager = new ApplicationManager())
				{
					var section = manager.CurrentSection();
					var ns = section.GetNamespaceOfPrefix("one");
					var pageIDs = section.Elements(ns + "Page")
						.Where(e => e.Attribute("selected")?.Value == "all")
						.Select(e => e.Attribute("ID").Value)
						.ToList();

					if (pageIDs.Count > 1)
					{
						ExportMany(manager, pageIDs);
					}
					else
					{
						var page = new Page(manager.CurrentPage());
						ExportOne(manager, page);
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error saving page(s)", exc);
				UIHelper.ShowError("Error exporting page(s). See log file for more information.");
			}
		}


		private void ExportMany(ApplicationManager manager, List<string> pageIDs)
		{
			ExportFormat format;
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
				case ExportFormat.HTML: ext = ".htm"; break;
				case ExportFormat.PDF: ext = ".pdf"; break;
				case ExportFormat.Word: ext = ".docx"; break;
				case ExportFormat.XML: ext = ".xml"; break;
			}

			string formatName = format.ToString();

			using (var progress = new ProgressDialog())
			{
				progress.SetMaximum(pageIDs.Count);
				progress.Show(owner);

				foreach (var pageID in pageIDs)
				{
					var page = new Page(manager.GetPage(pageID));
					var filename = Path.Combine(path, page.PageName.Replace(' ', '_') + ext);

					progress.SetMessage(filename);
					progress.Increment();

					if (format == ExportFormat.XML)
					{
						SaveAsXML(page.Root, filename);
					}
					else
					{
						SaveAs(manager, page.PageId, filename, (PublishFormat)format, formatName);
					}
				}
			}

			UIHelper.ShowMessage(string.Format(Resx.SaveAsMany_Success, pageIDs.Count, path));
		}


		private void ExportOne(ApplicationManager manager, Page page)
		{
			var filename = page.PageName.Replace(' ', '_');

			using (var dialog = new SaveFileDialog
			{
				FileName = filename,
				Filter = Resx.SaveAs_Filter,
				DefaultExt = ".htm",
				Title = Resx.SaveAs_Title,
				AddExtension = false
			})
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				filename = dialog.FileName;
			}

			ExportFormat format = ExportFormat.XML;
			var ext = Path.GetExtension(filename).ToLower();

			if (ext == ".xml")
			{
				SaveAsXML(page.Root, filename);
				UIHelper.ShowMessage(string.Format(Resx.SaveAs_Success, filename));
				return;
			}

			switch (ext)
			{
				case ".htm": format = ExportFormat.HTML; break;
				case ".pdf": format = ExportFormat.PDF; break;
				case ".docx": format = ExportFormat.Word; break;
				case ".xml": format = ExportFormat.XML; break;

				default:
					UIHelper.ShowError(Resx.SaveAs_Invalid_Type);
					break;
			}

			SaveAs(manager, page.PageId, filename, (PublishFormat)format, format.ToString());
			UIHelper.ShowMessage(string.Format(Resx.SaveAs_Success, filename));
		}


		private void SaveAs(
			ApplicationManager manager, string pageId, string filename,
			PublishFormat format, string formatName)
		{
			logger.WriteLine($"publishing page {pageId} to {filename}");

			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				manager.Application.Publish(pageId, filename, format, string.Empty);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"ERROR publishig page as {formatName}", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, formatName) + "\n\n" + exc.Message);
			}
		}


		private void SaveAsXML(XElement root, string filename)
		{
			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				root.Save(filename);
			}
			catch (Exception exc)
			{
				logger.WriteLine("ERROR publishig page as XML", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, "XML") + "\n\n" + exc.Message);
			}
		}
	}
}
