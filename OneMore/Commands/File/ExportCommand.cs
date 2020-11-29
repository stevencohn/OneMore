//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ExportCommand : Command
	{

		public ExportCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				var section = one.GetSection();
				var ns = one.GetNamespace(section);

				var pageIDs = section.Elements(ns + "Page")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Attribute("ID").Value)
					.ToList();

				if (pageIDs.Count > 1)
				{
					ExportMany(one, pageIDs);
				}
				else
				{
					var page = one.GetPage();
					ExportOne(one, page);
				}
			}
		}


		private void ExportMany(OneNote one, List<string> pageIDs)
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
			}

			string formatName = format.ToString();

			using (var progress = new Dialogs.ProgressDialog())
			{
				progress.SetMaximum(pageIDs.Count);
				progress.Show(owner);

				foreach (var pageID in pageIDs)
				{
					var page = one.GetPage(pageID);
					var filename = Path.Combine(path, page.Title.Replace(' ', '_') + ext);

					progress.SetMessage(filename);
					progress.Increment();

					if (format == OneNote.ExportFormat.XML)
					{
						SaveAsXML(page.Root, filename);
					}
					else
					{
						SaveAs(one, page.PageId, filename, format, formatName);
					}
				}
			}

			UIHelper.ShowMessage(string.Format(Resx.SaveAsMany_Success, pageIDs.Count, path));
		}


		private void ExportOne(OneNote one, Page page)
		{
			var filename = page.Title.Replace(' ', '_');

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

			var format = OneNote.ExportFormat.XML;
			var ext = Path.GetExtension(filename).ToLower();

			if (ext == ".xml")
			{
				SaveAsXML(page.Root, filename);
				UIHelper.ShowMessage(string.Format(Resx.SaveAs_Success, filename));
				return;
			}

			switch (ext)
			{
				case ".htm": format = OneNote.ExportFormat.HTML; break;
				case ".pdf": format = OneNote.ExportFormat.PDF; break;
				case ".docx": format = OneNote.ExportFormat.Word; break;
				case ".xml": format = OneNote.ExportFormat.XML; break;

				default:
					UIHelper.ShowError(Resx.SaveAs_Invalid_Type);
					break;
			}

			SaveAs(one, page.PageId, filename, format, format.ToString());
			UIHelper.ShowMessage(string.Format(Resx.SaveAs_Success, filename));
		}


		private void SaveAs(
			OneNote one, string pageId, string filename,
			OneNote.ExportFormat format, string formatName)
		{
			logger.WriteLine($"publishing page {pageId} to {filename}");

			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				one.Export(pageId, filename, format);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error publishig page as {formatName}", exc);
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
				logger.WriteLine("error publishig page as XML", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, "XML") + "\n\n" + exc.Message);
			}
		}
	}
}
