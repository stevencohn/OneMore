//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;
	using Microsoft.Office.Interop.OneNote;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class SaveAsCommand : Command
	{
		public SaveAsCommand()
		{
		}


		public void Execute()
		{
			using (var dialog = new SaveFileDialog
			{
				Filter = Resx.SaveAs_Filter,
				DefaultExt = ".htm",
				Title = Resx.SaveAs_Title,
				AddExtension = true
			})
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					switch (Path.GetExtension(dialog.FileName))
					{
						case ".htm":
							SaveAsHTML(dialog.FileName);
							break;

						case ".xml":
							SaveAsXML(dialog.FileName);
							break;

						default:
							UIHelper.ShowError(Resx.SaveAs_Invalid_Type);
							break;
					}
				}
			}
		}


		private void SaveAsHTML(string filename)
		{
			using (var manager = new ApplicationManager())
			{
				var root = manager.CurrentPage();

				var pageId = root.Attribute("ID").Value;
				logger.WriteLine($"publishing page {pageId} to {filename}");

				try
				{
					if (File.Exists(filename))
					{
						File.Delete(filename);
					}

					manager.Application.Publish(
						pageId,
						filename,
						PublishFormat.pfHTML,
						string.Empty
					);
				}
				catch (Exception exc)
				{
					logger.WriteLine("ERROR publishig page as HTML", exc);
					UIHelper.ShowError(string.Format(Resx.SaveAs_Error, "HTML") + "\n\n" + exc.Message);
					return;
				}
			}

			if (filename != null)
			{
				UIHelper.ShowMessage(string.Format(Resx.SaveAs_Success, filename));
			}
		}


		public void SaveAsXML(string filename)
		{
			using (var manager = new ApplicationManager())
			{
				var root = manager.CurrentPage(PageInfo.piAll);

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
					return;
				}
			}

			if (filename != null)
			{
				UIHelper.ShowMessage(string.Format(Resx.SaveAs_Success, filename));
			}
		}
	}
}
