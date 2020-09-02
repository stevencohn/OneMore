//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;
	using Microsoft.Office.Interop.OneNote;
	using System.Windows.Forms;


	internal enum SaveAsFormat
	{
		HTML,
		XML
	}


	internal class SaveAsCommand : Command
	{
		public SaveAsCommand()
		{
		}


		public void Execute(SaveAsFormat format)
		{
			switch (format)
			{
				case SaveAsFormat.HTML:
					SaveAsHTML();
					break;

				case SaveAsFormat.XML:
					SaveAsXML();
					break;
			}
		}


		private void SaveAsHTML()
		{
			using (var dialog = new SaveFileDialog
			{
				Filter = "HTML File|.htm",
				DefaultExt = ".htm",
				Title = "Save Page as HTML",
				AddExtension = true
			})
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					using (var manager = new ApplicationManager())
					{
						var root = manager.CurrentPage();

						var pageId = root.Attribute("ID").Value;
						logger.WriteLine($"publishing page {pageId} to {dialog.FileName}");

						try
						{
							if (File.Exists(dialog.FileName))
							{
								File.Delete(dialog.FileName);
							}

							manager.Application.Publish(
								pageId,
								dialog.FileName,
								PublishFormat.pfHTML,
								string.Empty
							);
						}
						catch (Exception exc)
						{
							logger.WriteLine("ERROR publishig page as HTML", exc);

							MessageBox.Show(owner, "Error saving page as HTML", "OneMore",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
								MessageBoxOptions.DefaultDesktopOnly);

							return;
						}
					}

					UIHelper.ShowMessage(owner, $"Page saved to {dialog.FileName}");
				}
			}
		}


		public void SaveAsXML()
		{
			using (var dialog = new SaveFileDialog
			{
				Filter = "XML File|.xml",
				DefaultExt = ".xml",
				Title = "Save Page as XML",
				AddExtension = true
			})
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					using (var manager = new ApplicationManager())
					{
						var root = manager.CurrentPage();

						try
						{
							if (File.Exists(dialog.FileName))
							{
								File.Delete(dialog.FileName);
							}

							root.Save(dialog.FileName);
						}
						catch (Exception exc)
						{
							logger.WriteLine("ERROR publishig page as XML", exc);

							MessageBox.Show(owner, "Error saving page as XML", "OneMore",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
								MessageBoxOptions.DefaultDesktopOnly);

							return;
						}
					}
				}

				UIHelper.ShowMessage(owner, $"Page saved to {dialog.FileName}");
			}
		}
	}
}
