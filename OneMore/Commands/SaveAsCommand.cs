//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Models;
	using System;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class SaveAsCommand : Command
	{
		public SaveAsCommand()
		{
		}


		public void Execute()
		{
			Page page;
			using (var manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage());
			}

			var titles = page.Root
				.Elements(page.Namespace + "Title")
				.Elements(page.Namespace + "OE")
				.Elements(page.Namespace + "T")
				.Select(e => e.GetCData().GetWrapper().Value.Replace(' ', '_'));

			var filename = titles == null ? string.Empty : string.Concat(titles);

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

			switch (Path.GetExtension(filename))
			{
				case ".htm":
					SaveAs(page.PageId, filename, PublishFormat.pfHTML, "HTML");
					break;

				case ".pdf":
					SaveAs(page.PageId, filename, PublishFormat.pfPDF, "PDF");
					break;

				case ".docx":
					SaveAs(page.PageId, filename, PublishFormat.pfWord, "DOCX");
					break;

				case ".xml":
					SaveAsXML(page.Root, filename);
					break;

				default:
					UIHelper.ShowError(Resx.SaveAs_Invalid_Type);
					break;
			}
		}


		private void SaveAs(string pageId, string filename, PublishFormat format, string fname)
		{
			logger.WriteLine($"publishing page {pageId} to {filename}");

			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				using (var manager = new ApplicationManager())
				{
					manager.Application.Publish(pageId, filename, format, string.Empty);
				}

				UIHelper.ShowMessage(string.Format(Resx.SaveAs_Success, filename));
			}
			catch (Exception exc)
			{
				logger.WriteLine($"ERROR publishig page as {fname}", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, fname) + "\n\n" + exc.Message);
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

				UIHelper.ShowMessage(string.Format(Resx.SaveAs_Success, filename));
			}
			catch (Exception exc)
			{
				logger.WriteLine("ERROR publishig page as XML", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, "XML") + "\n\n" + exc.Message);
			}
		}
	}
}
