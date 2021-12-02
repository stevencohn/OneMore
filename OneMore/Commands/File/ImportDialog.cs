//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.IO;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ImportDialog : UI.LocalizableForm
	{
		public enum Formats
		{
			Word,
			PowerPoint,
			Xml,
			OneNote,
			Markdown
		}


		private readonly bool wordInstalled;
		private readonly bool powerPointInstalled;


		public ImportDialog()
		{
			InitializeComponent();

			wordInstalled = Office.IsInstalled("Word");
			powerPointInstalled = Office.IsInstalled("Powerpoint");

			wordGroup.Visible = false;
			powerGroup.Visible = false;
			notInstalledLabel.Visible = false;

			browseButton.Top = pathBox.Top;
			browseButton.Height = pathBox.Height;

			if (NeedsLocalizing())
			{
				Text = Resx.ImportDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"fileLabel",
					"wordGroup",
					"wordAppendButton",
					"wordCreateButton",
					"powerGroup",
					"powerAppendButton",
					"powerCreateButton",
					"powerSectionButton",
					"notInstalledLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			pathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}


		public string FilePath => pathBox.Text;

		public Formats Format { get; private set; }

		public bool AppendToPage
		{
			get
			{
				if (Format == Formats.Xml) return false;
				return Format == Formats.Word ? wordAppendButton.Checked : powerAppendButton.Checked;
			}
		}

		public bool CreateSection => powerSectionButton.Checked;


		private void ChangePath(object sender, EventArgs e)
		{
			try
			{
				var ext = Path.GetExtension(pathBox.Text);

				switch (ext)
				{
					case ".doc":
					case ".docx":
						wordGroup.Visible = wordInstalled;
						powerGroup.Visible = false;
						notInstalledLabel.Visible = !wordInstalled;
						okButton.Enabled = wordInstalled;
						Format = Formats.Word;
						break;

					case ".md":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						notInstalledLabel.Visible = false;
						okButton.Enabled = true;
						Format = Formats.Markdown;
						break;

					case ".ppt":
					case ".pptx":
						wordGroup.Visible = false;
						powerGroup.Visible = powerPointInstalled;
						notInstalledLabel.Visible = !powerPointInstalled;
						okButton.Enabled = powerPointInstalled;
						Format = Formats.PowerPoint;
						break;

					case ".xml":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						notInstalledLabel.Visible = false;
						okButton.Enabled = true;
						Format = Formats.Xml;
						break;

					case ".one":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						notInstalledLabel.Visible = false;
						okButton.Enabled = true;
						Format = Formats.OneNote;
						break;

					default:
						wordGroup.Visible = powerGroup.Visible = false;
						notInstalledLabel.Visible = false;
						okButton.Enabled = false;
						break;
				}
			}
			catch
			{
				okButton.Enabled = false;
			}
		}


		private async void BrowseFile(object sender, EventArgs e)
		{
			try
			{
				// OpenFileDialog must run in an STA thread
				var path = await SingleThreaded.Invoke(() =>
				{
					using (var dialog = new OpenFileDialog()
					{
						AddExtension = true,
						CheckFileExists = true,
						DefaultExt = ".docx",
						Filter = Resx.ImportDialog_OpenFileFilter,
						InitialDirectory = pathBox.Text,
						Multiselect = false,
						Title = Resx.ImportDialog_OpenFileTitle
					})
					{
						// cannot use owner parameter here or it will hang! cross-threading
						if (dialog.ShowDialog() == DialogResult.OK)
						{
							return dialog.FileName;
						}
					}

					return null;
				});

				if (path != null)
				{
					pathBox.Text = path;
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error running OpenFileDialog", exc);
			}
		}
	}
}
